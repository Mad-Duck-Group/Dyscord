using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dyscord.Characters;
using Dyscord.Characters.Player;
using Dyscord.ScriptableObjects.Action;
using Dyscord.UI;
using Gamelogic.Extensions.Algorithms;
using NaughtyAttributes;
using Redcode.Extensions;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Managers
{
	public class TurnOrder
	{
		public bool currentTurn;
		public int actionValue;
		public Character character;
	}
	public class TurnManager : MonoSingleton<TurnManager>
	{
		[Header("Prefabs")]
		[SerializeField] private Character playerPrefab;
		[SerializeField] private List<Character> enemyPrefabs = new List<Character>();
		[SerializeField] private Transform enemyParent;
		[SerializeField] private Transform playerParent;

		[Header("UI")]
		[SerializeField] private TurnOrderUI turnOrderUIPrefab;
		[SerializeField] private ScrollRect turnOrderScrollView;
		[SerializeField] private GameObject actionButtonPrefab;
		[SerializeField] private Transform actionPanel;
		
		private Character playerInstance;
		private List<Character> enemyInstances = new List<Character>();
		private List<Button> actionButtons = new List<Button>();

		private LinkedList<TurnOrderUI> turnOrderUIs = new LinkedList<TurnOrderUI>();
		public TurnOrder CurrentTurnOrder => turnOrderUIs.First.Value.TurnOrder;
		
		public Transform ActionPanel => actionPanel;
		public Character PlayerInstance => playerInstance;
		public List<Character> EnemyInstances => enemyInstances;

		private void Start()
		{
			InitializeCharacters();
			Subscribe();
			InitializePlayerUI();
			InitializeTurnOrder();
			ManageTurn();
		}
		
		private void Subscribe()
		{
			playerInstance.OnCharacterDeath += OnCharacterDeath;
			enemyInstances.ForEach(enemyInstance => enemyInstance.OnCharacterDeath += OnCharacterDeath);
		}
		
		private void OnDisable()
		{
			playerInstance.OnCharacterDeath -= OnCharacterDeath;
			enemyInstances.ForEach(enemyInstance => enemyInstance.OnCharacterDeath -= OnCharacterDeath);
		}

		private void OnCharacterDeath(Character character)
		{
			if (character is Player)
			{
				RemoveTurnOrder(character);
				Debug.Log("Player died");
			}
			else
			{
				RemoveTurnOrder(character);
				if (enemyInstances.Count == 0)
				{
					Debug.Log("Player won");
				}
			}
		}

		private void InitializeCharacters()
		{
			playerInstance = Instantiate(playerPrefab, playerParent);
			enemyPrefabs.ForEach(enemyPrefab =>
			{
				Character enemyInstance = Instantiate(enemyPrefab, enemyParent);
				enemyInstances.Add(enemyInstance);
			});
		}

		private void InitializePlayerUI()
		{
			Button basicAttackButton = Instantiate(actionButtonPrefab, actionPanel).GetComponent<Button>();
			basicAttackButton.onClick.AddListener(() =>
			{
				playerInstance.BasicAttack.SelectTarget();
			});
			basicAttackButton.GetComponentInChildren<TMP_Text>().text = playerInstance.BasicAttack.ActionName;
			actionButtons.Add(basicAttackButton);
			playerInstance.Skills.ForEach(skill =>
			{
				Button skillButton = Instantiate(actionButtonPrefab, actionPanel).GetComponent<Button>();
				skillButton.onClick.AddListener(() =>
				{
					skill.SelectTarget();
				});
				skillButton.GetComponentInChildren<TMP_Text>().text = skill.ActionName;
				actionButtons.Add(skillButton);
			});
		}

		private void InitializeTurnOrder()
		{
			TurnOrder playerActionValue = playerInstance.GetRawTurnOrder();
			turnOrderUIs.AddLast(Instantiate(turnOrderUIPrefab, turnOrderScrollView.content));
			turnOrderUIs.Last.Value.Setup(playerActionValue);
			enemyInstances.ForEach(enemyInstance =>
			{
				TurnOrder enemyActionValue = enemyInstance.GetRawTurnOrder();
				turnOrderUIs.AddLast(Instantiate(turnOrderUIPrefab, turnOrderScrollView.content));
				turnOrderUIs.Last.Value.Setup(enemyActionValue);
			});
			RecalculateTurnOrder();
		}

		private void RecalculateTurnOrder()
		{
			int minActionValue = turnOrderUIs.Min(actionValue => actionValue.TurnOrder.actionValue);
			foreach (var actionValue in turnOrderUIs)
			{
				actionValue.TurnOrder.actionValue -= minActionValue;
			}
			//Sort based on action value in ascending order, if the action values are the same, sort based on speed in descending order
			List<TurnOrderUI> sorted = turnOrderUIs.OrderBy(actionValue => actionValue.TurnOrder.actionValue)
				.ThenByDescending(actionValue => actionValue.TurnOrder.character.CurrentSpeed).ToList();
			
			for (int i = 0; i < turnOrderUIs.Count; i++)
			{
				sorted[i].transform.SetSiblingIndex(i); // Reorder the hierarchy
				sorted[i].TurnOrder.currentTurn = i == 0;
				sorted[i].UpdateValue();
			}
			
			turnOrderUIs = new LinkedList<TurnOrderUI>(sorted);
		}
		
		private void RemoveTurnOrder(Character character)
		{
			character.OnCharacterDeath -= OnCharacterDeath;
			character.gameObject.SetActive(false);
			TurnOrderUI toRemove = turnOrderUIs.First(node => node.TurnOrder.character == character);
			enemyInstances.Remove(character);
			turnOrderUIs.Remove(toRemove);
			Destroy(toRemove.gameObject);
			RecalculateTurnOrder();
		}
		
		private void AddTurnOrder(Character character)
		{
			TurnOrder newTurnOrder = character.GetRawTurnOrder();
			enemyInstances.Add(character);
			turnOrderUIs.AddLast(Instantiate(turnOrderUIPrefab, turnOrderScrollView.content));
			turnOrderUIs.Last.Value.Setup(newTurnOrder);
			RecalculateTurnOrder();
		}

		public void UpdateButtonUI()
		{
			if (CurrentTurnOrder.character is not Player || (CurrentTurnOrder.character.CurrentAction &&
			                                                 CurrentTurnOrder.character.CurrentAction.PlayerSelecting))
			{
				actionButtons.ForEach(button => button.interactable = false);
			}
			else
			{
				List<CharacterActionSO> allActions = CurrentTurnOrder.character.AllActions;
				for (int i = 0; i < allActions.Count; i++)
				{
					actionButtons[i].interactable = CurrentTurnOrder.character.HasEnoughRam(allActions[i].RamCost);
				}
			}
		}

		private void ManageTurn()
		{
			//UpdateButtonUI();
			DOVirtual.DelayedCall(1f, () => CurrentTurnOrder.character.RegenRam());
			if (CurrentTurnOrder.character is Player)
			{
				
			}
			else
			{
				DOVirtual.DelayedCall(1.5f, () =>
				{
					CurrentTurnOrder.character.AIPlay();
				});
			}
		}
		
		public void NextTurn()
		{
			LinkedListNode<TurnOrderUI> currentTurn = turnOrderUIs.First;
			turnOrderUIs.RemoveFirst();
			RecalculateTurnOrder();
			int difference = Mathf.Abs(turnOrderUIs.Last.Value.TurnOrder.character.GetRawTurnOrder().actionValue -
			                           currentTurn.Value.TurnOrder.character.GetRawTurnOrder().actionValue);
			currentTurn.Value.TurnOrder.actionValue = turnOrderUIs.Last.Value.TurnOrder.actionValue + difference;
			turnOrderUIs.AddLast(currentTurn);
			turnOrderUIs.Last.Value.TurnOrder.currentTurn = false;
			turnOrderUIs.Last.Value.UpdateValue();
			ManageTurn();
		}
		
		private void Update()
		{
			UndoHandler();
		}

		private void UndoHandler()
		{
			if (CurrentTurnOrder.character is not Player) return;
			if (!CurrentTurnOrder.character.CurrentAction) return;
			if (!CurrentTurnOrder.character.CurrentAction.PlayerSelecting) return;
			if (Input.GetMouseButtonDown(1))
			{
				CurrentTurnOrder.character.CurrentAction.UndoTarget();
			}
		}

	}
}