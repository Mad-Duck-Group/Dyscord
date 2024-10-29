using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Managers;
using Dyscord.ScriptableObjects.Overtime;
using Dyscord.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class OvertimeTemplateUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text countdownText;
		[SerializeField] private Image icon;
		
		private OvertimeTemplate overtime;
		private Tooltip tooltip;

		private void Awake()
		{
			tooltip = GetComponent<Tooltip>();
		}

		public void Setup(OvertimeTemplate overtime)
		{
			if (!tooltip) tooltip = GetComponent<Tooltip>();
			this.overtime = overtime;
			countdownText.text = overtime.Infinite ? "∞" : this.overtime.RemainingTurns.ToString();
			icon.sprite = this.overtime.Icon;
			tooltip.TooltipObject = this.overtime;
		}
	}
}