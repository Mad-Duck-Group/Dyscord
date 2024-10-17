using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Characters.Enemy
{
	public class EnemyTest : Character, IPointerClickHandler
	{
		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left) return;
			if (TurnManager.Instance.CurrentTurnOrder.character is not Player.Player) return;
			TurnManager.Instance.CurrentTurnOrder.character.Attack(this);
			TurnManager.Instance.NextTurn();
		}
	}
}