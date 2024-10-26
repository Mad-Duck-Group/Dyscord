using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.ScriptableObjects.Action;
using Dyscord.ScriptableObjects.Cyberware;
using Dyscord.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class HackCyberwareButtonUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text cyberwareName;
		
		private CyberwareSO cyberware;
		private Button button;
		private Tooltip tooltip;
		
		private void Awake()
		{
			tooltip = GetComponent<Tooltip>();
			button = GetComponent<Button>();
		}
		
		public Button Setup(CyberwareSO cyberware)
		{
			if (!tooltip) tooltip = GetComponent<Tooltip>();
			if (!button) button = GetComponent<Button>();
			this.cyberware = cyberware;
			cyberwareName.text = cyberware.CyberwareName;
			tooltip.TooltipObject = cyberware;
			return button;
		}
	}
}