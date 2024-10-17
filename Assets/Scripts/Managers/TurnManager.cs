using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dyscord.Characters;
using Dyscord.Characters.Player;
using Dyscord.UI;
using Gamelogic.Extensions.Algorithms;
using NaughtyAttributes;
using Redcode.Extensions;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
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
		[SerializeField] private Character playerPrefab;
		[SerializeField] private List<Character> enemyPrefabs = new List<Character>();
		[SerializeField] private TurnOrderUI turnOrderUIPrefab;
		[SerializeField] private ScrollRect turnOrderScrollView;
		[SerializeField] private Transform enemyParent;
		[SerializeField] private Transform playerParent;
		private Character playerInstance;
		private List<Character> enemyInstances = new List<Character>();

		private LinkedList<TurnOrderUI> turnOrderUIs = new LinkedList<TurnOrderUI>();
		public TurnOrder CurrentTurnOrder => turnOrderUIs.First.Value.TurnOrder;

		private void Start()
		{
			InitializeCharacters();
			InitializeTurnOrder();
			Subscribe();
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
			Debug.Log($"current action value: {minActionValue}");
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

		private void Update()
		{
			
		}

		private void ManageTurn()
		{
			if (CurrentTurnOrder.character is Player)
			{
				Debug.Log("Player's turn");
			}
			else
			{
				DOVirtual.DelayedCall(1f, () =>
				{
					CurrentTurnOrder.character.Attack(playerInstance);
					NextTurn();
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

	}
}