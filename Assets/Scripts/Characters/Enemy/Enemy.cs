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
	public class Enemy : Character, IPointerClickHandler
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


		public void OnPointerClick(PointerEventData eventData)
		{
			if (CurrentCharacter is not Player.Player) return;
			if (!CurrentCharacter.CurrentAction) return;
			if (!CurrentCharacter.CurrentAction.PlayerSelecting) return;
			switch (eventData.button)
			{
				case PointerEventData.InputButton.Left:
					SelectionCount++;
					CurrentCharacter.CurrentAction.AddTarget(this, CurrentCharacter.CurrentAction.TargetType == TargetTypes.Burst);
					return;
			}
		}

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