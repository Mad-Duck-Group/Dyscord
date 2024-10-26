using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Managers;
using Dyscord.ScriptableObjects.Action;
using Dyscord.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	
	public class CharacterActionUI : MonoBehaviour
	{
		[Header("UI")]
		[SerializeField] private TMP_Text actionName;

		private CharacterActionSO action;
		private Tooltip tooltip;
		
		private void Awake()
		{
			tooltip = GetComponent<Tooltip>();
		}
		
		public void Setup(CharacterActionSO action)
		{
			if (!tooltip) tooltip = GetComponent<Tooltip>();
			this.action = action;
			actionName.text = action.ActionName;
			tooltip.TooltipObject = action;
		}
	}
}