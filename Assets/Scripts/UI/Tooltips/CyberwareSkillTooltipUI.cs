using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.ScriptableObjects.Action;
using TMPro;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class CyberwareSkillTooltipUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text title;
		[SerializeField] private TMP_Text skillNamePrefab;
		
		public void SetTooltip(string title, List<CharacterActionSO> actions)
		{
			this.title.text = title;
			foreach (var action in actions)
			{
				var skillNameText = Instantiate(skillNamePrefab, transform);
				skillNameText.text = action.ActionName;
			}
		}

	}
}