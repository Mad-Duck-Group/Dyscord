using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
		[SerializeField] private Image background;
		[SerializeField] private Sprite currentTurnBackground;
		[SerializeField] private Sprite waitingTurnBackground;
		[SerializeField] private Image thumbnail;
		private TurnOrder _turnOrder;
		
		public TurnOrder TurnOrder => _turnOrder;

		private void Awake()
		{
			nameText.text = "";
			actionValueText.text = "";
		}

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
			background.sprite = _turnOrder.currentTurn ? currentTurnBackground : waitingTurnBackground;
			thumbnail.sprite = _turnOrder.character.CharacterSO.CharacterThumbnail;
			//nameText.text = _turnOrder.character.CharacterSO.CharacterName;
			//actionValueText.text = _turnOrder.actionValue.ToString();
			//Color textColor = _turnOrder.currentTurn ? Color.green : Color.black;
			//nameText.color = textColor;
			//actionValueText.color = textColor;
		}
	}
}