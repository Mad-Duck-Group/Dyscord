using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Managers;
using TMPro;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	/// <summary>
	/// UI that displays the TurnOrder.
	/// </summary>
	public class TurnOrderUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text nameText;
		[SerializeField] private TMP_Text actionValueText;
		private TurnOrder _turnOrder;
		
		public TurnOrder TurnOrder => _turnOrder;
		
		
		/// <summary>
		/// Sets up the TurnOrderUI with the TurnOrder.
		/// </summary>
		/// <param name="turnOrder">The TurnOrder to setup the TurnOrderUI with.</param>
		public void Setup(TurnOrder turnOrder)
		{
			_turnOrder = turnOrder;
			UpdateValue();
		}

		/// <summary>
		/// Updates the TurnOrderUI with the TurnOrder values.
		/// </summary>
		public void UpdateValue()
		{
			nameText.text = _turnOrder.character.CharacterSO.CharacterName;
			actionValueText.text = _turnOrder.actionValue.ToString();
			Color textColor = _turnOrder.currentTurn ? Color.green : Color.black;
			nameText.color = textColor;
			actionValueText.color = textColor;
		}
	}
}