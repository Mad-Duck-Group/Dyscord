using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.ScriptableObjects.Overtime;
using TMPro;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class OvertimeTooltipPanelUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text title;
		[SerializeField] private OvertimeBlockUI overtimeBlockUIPrefab;
		
		public void SetTooltip(string title, List<OvertimeTemplate> overtimeTemplates)
		{
			this.title.text = title;
			foreach (var overtimeTemplate in overtimeTemplates)
			{
				var overtimeBlock = Instantiate(overtimeBlockUIPrefab, transform);
				overtimeBlock.SetOvertimeBlock(overtimeTemplate);
			}
		}
	}
}