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
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class ActionButtonUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text actionName;
		
		private CharacterActionSO action;
		private Button button;
		private Tooltip tooltip;
		
		private void Awake()
		{
			tooltip = GetComponent<Tooltip>();
			button = GetComponent<Button>();
		}
		
		public Button Setup(CharacterActionSO action)
		{
			if (!tooltip) tooltip = GetComponent<Tooltip>();
			if (!button) button = GetComponent<Button>();
			this.action = action;
			if (actionName) actionName.text = action.ActionName;
			tooltip.TooltipObject = action;
			return button;
		}

	}
}