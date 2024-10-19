using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Managers;
using Dyscord.ScriptableObjects.Action;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Characters.Enemy
{
	public class Enemy : Character, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] private GameObject selectionIcon;
		[SerializeField] private GameObject selectionIconParent;
		private Character CurrentCharacter => TurnManager.Instance.CurrentTurnOrder.character;
		
		private List<GameObject> selectionIcons = new List<GameObject>();
		
		private int selectionCount;

		public override int SelectionCount
		{
			get => selectionCount;
			set
			{
				selectionCount = value;
				UpdateSelectionUI();
			}
		}
		
		public void OnPointerEnter(PointerEventData eventData)
		{
			PanelManager.Instance.SetStatsText(this);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			PanelManager.Instance.SetStatsText(TurnManager.Instance.PlayerInstance);
		}

		/// <summary>
		/// Handles pointer click event.
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left) return;
			if (CurrentCharacter is not Player.Player) return;
			if (!CurrentCharacter.CurrentAction) return;
			if (!CurrentCharacter.CurrentAction.PlayerSelecting) return;
			SelectionCount++;
			CurrentCharacter.CurrentAction.AddTarget(this, CurrentCharacter.CurrentAction.TargetType == TargetTypes.Burst);
		}

		/// <summary>
		/// Updates the selection UI.
		/// </summary>
		private void UpdateSelectionUI()
		{
			while (selectionIcons.Count < selectionCount)
			{
				selectionIcons.Add(Instantiate(selectionIcon, selectionIconParent.transform));
			}
			List<int> removeIndexes = new List<int>();
			//Remove excess icons
			for (int i = 0; i < selectionIcons.Count; i++)
			{
				if (i < selectionCount) continue;
				Destroy(selectionIcons[i]);
				removeIndexes.Add(i);
			}
			removeIndexes.Reverse();
			foreach (var index in removeIndexes)
			{
				selectionIcons.RemoveAt(index);
			}
		}
	}
}