using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.ScriptableObjects.Overtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class OvertimeTemplateUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text countdownText;
		
		private Image icon;
		private OvertimeTemplate overtime;

		public void Setup(OvertimeTemplate overtime)
		{
			icon = GetComponent<Image>();
			this.overtime = overtime;
			countdownText.text = overtime.Infinite ? "∞" : this.overtime.TurnCount.ToString();
			icon.sprite = this.overtime.Icon;
		}
	}
}