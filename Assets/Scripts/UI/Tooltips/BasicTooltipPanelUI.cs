using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class BasicTooltipPanelUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text label;
		[SerializeField] private TMP_Text description;
		
		public void SetTooltip(string label, string description)
		{
			this.label.text = label;
			this.description.text = description;
		}
	}
}