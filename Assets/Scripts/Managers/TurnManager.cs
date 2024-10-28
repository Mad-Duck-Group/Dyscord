using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dyscord.Characters;
using Dyscord.Characters.Player;
using Dyscord.ScriptableObjects.Action;
using Dyscord.UI;
using NaughtyAttributes;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
	/// <summary>
	/// Manages the turn order and general state of the game.
	/// </summary>
	public class TurnManager : MonoSingleton<TurnManager>
	{
		[Header("Prefabs")]
		[SerializeField] private Character playerPrefab;
		[SerializeField] private Transform playerParent;
		[SerializeField] private List<Character> enemyPrefabs = new List<Character>();
		[SerializeField] private Transform enemyParent;
		[SerializeField] private Animator vfxPrefab;

		[Header("UI")]
		[SerializeField] private TurnOrderUI turnOrderUIPrefab;
		[SerializeField] private ScrollRect turnOrderScrollView;

		private bool gameEnded;
		private Character playerInstance;
		private List<Character> enemyInstances = new List<Character>();
		private LinkedList<TurnOrderUI> turnOrderUIs = new LinkedList<TurnOrderUI>();
		
		public Animator VfxPrefab => vfxPrefab;

		public TurnOrder CurrentTurnOrder => turnOrderUIs.First?.Value.TurnOrder;
		public Character PlayerInstance => playerInstance;
		public List<Character> EnemyInstances => enemyInstances;

		private void Start()
		{
			if (!ProgressionManager.Instance.PlayVN(VNTypes.SouthStreet))
			{
				Initialize();
				return;
			}
			VNManager.OnVNFinished += VNManagerOnOnVNFinished();
		}

		private VNManager.VNFinished VNManagerOnOnVNFinished()
		{
			return _ =>
			{
				Initialize();
			};
		}

		private void Initialize()
		{
			InitializeCharacters();
			Subscribe();
			PanelManager.Instance.InitializePanel();
			PanelManager.Instance.InitializeActionUI();
			InitializeTurnOrder();
			GlobalSoundManager.Instance.PlayBGM(BGMTypes.Gameplay);
			PanelManager.Instance.UpdateButtonUI();
			DOVirtual.DelayedCall(1.5f, ManageTurn, false);
		}

		/// <summary>
		/// Subscribes to character events.
		/// </summary>
		private void Subscribe()
		{
			playerInstance.OnCharacterDeath += OnCharacterDeath;
			enemyInstances.ForEach(enemyInstance => enemyInstance.OnCharacterDeath += OnCharacterDeath);
		}
		
		/// <summary>
		/// Unsubscribes from character events.
		/// </summary>
		private void OnDisable()
		{
			playerInstance.OnCharacterDeath -= OnCharacterDeath;
			enemyInstances.ForEach(enemyInstance => enemyInstance.OnCharacterDeath -= OnCharacterDeath);
		}

		
		/// <summary>
		/// Handles character death events.
		/// </summary>
		/// <param name="character">The character that died.</param>
		private void OnCharacterDeath(Character character)
		{
			GlobalSoundManager.Instance.PlayEffectClip(character.CharacterSO.DeathSound);
			if (character is Player)
			{
				RemoveTurnOrder(character);
				gameEnded = true;
				PanelManager.Instance.ShowLosePanel();
			}
			else
			{
				RemoveTurnOrder(character);
				PanelManager.Instance.SetStatsText(PlayerInstance);
				if (enemyInstances.Count == 0)
				{
					gameEnded = true;
					PanelManager.Instance.ShowWinPanel();
				}
			}
			
		}

		/// <summary>
		/// Initializes the characters by instantiating the player and enemies.
		/// </summary>
		private void InitializeCharacters()
		{
			playerInstance = Instantiate(playerPrefab, playerParent);
			playerInstance.InitializeCharacter();
			playerInstance.ReequipCyberware(InventoryManager.Instance.EquippedCyberware);
			enemyPrefabs.ForEach(enemyPrefab =>
			{
				Character enemyInstance = Instantiate(enemyPrefab, enemyParent);
				enemyInstance.InitializeCharacter();
				enemyInstances.Add(enemyInstance);
			});
		}

		/// <summary>
		/// Initializes the turn order by instantiating the turn order UIs.
		/// </summary>
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

		
		/// <summary>
		/// Recalculates the turn order based on the action values and speed of the characters.
		/// </summary>
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
		
		/// <summary>
		/// Removes the character from the turn order.
		/// </summary>
		/// <param name="character">The character to be removed.</param>
		private void RemoveTurnOrder(Character character)
		{
			character.OnCharacterDeath -= OnCharacterDeath;
			if (character != playerInstance)
				character.gameObject.SetActive(false);
			TurnOrderUI toRemove = turnOrderUIs.First(node => node.TurnOrder.character == character);
			enemyInstances.Remove(character);
			turnOrderUIs.Remove(toRemove);
			Destroy(toRemove.gameObject);
			RecalculateTurnOrder();
		}
		
		/// <summary>
		/// Adds the character to the turn order.
		/// </summary>
		/// <param name="character">The character to be added.</param>
		private void AddTurnOrder(Character character)
		{
			TurnOrder newTurnOrder = character.GetRawTurnOrder();
			enemyInstances.Add(character);
			turnOrderUIs.AddLast(Instantiate(turnOrderUIPrefab, turnOrderScrollView.content));
			turnOrderUIs.Last.Value.Setup(newTurnOrder);
			RecalculateTurnOrder();
		}

		/// <summary>
		/// Manages the turn by regenerating RAM and playing the AI.
		/// </summary>
		private void ManageTurn()
		{
			if (gameEnded) return;
			//UpdateButtonUI(); 
			CurrentTurnOrder.character.UpdateOvertime();
			DOVirtual.DelayedCall(1f, () => CurrentTurnOrder.character.RegenRam(), false);
			if (CurrentTurnOrder.character is not Player)
			{
				PanelManager.Instance.UpdateButtonUI();
				DOVirtual.DelayedCall(1.5f, () =>
				{
					CurrentTurnOrder.character.AIPlay();
				}, false);
			}
		}
		
		/// <summary>
		/// Move the first character in the turn order to the end and then go to the next turn.
		/// </summary>
		public void NextTurn()
		{
			LinkedListNode<TurnOrderUI> currentTurn = turnOrderUIs.First;
			turnOrderUIs.RemoveFirst();
			// int difference = Mathf.Abs(turnOrderUIs.Last.Value.TurnOrder.character.GetRawTurnOrder().actionValue -
			//                            currentTurn.Value.TurnOrder.character.GetRawTurnOrder().actionValue);
			// currentTurn.Value.TurnOrder.actionValue = turnOrderUIs.Last.Value.TurnOrder.actionValue + difference;
			currentTurn.Value.TurnOrder.actionValue = currentTurn.Value.TurnOrder.character.GetRawTurnOrder().actionValue;
			turnOrderUIs.AddLast(currentTurn);
			turnOrderUIs.Last.Value.TurnOrder.currentTurn = false;
			turnOrderUIs.Last.Value.UpdateValue();
			RecalculateTurnOrder();
			ManageTurn();
		}

		private void OnDestroy()
		{
			VNManager.OnVNFinished -= VNManagerOnOnVNFinished();
			DOTween.Kill(this);
		}
	}
}