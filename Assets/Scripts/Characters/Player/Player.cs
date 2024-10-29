using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Managers;
using Dyscord.ScriptableObjects.Item;
using Dyscord.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Characters.Player
{
	public class Player : Character, IPointerClickHandler
	{
		private void OnEnable()
		{
			ItemUI.OnUseItem += UseItem;
		}
		
		private void OnDisable()
		{
			ItemUI.OnUseItem -= UseItem;
		}

		private void UseItem(ItemSO item)
		{
			if (_fromInventory) return;
			AddOvertime(item.OvertimeTemplates);
			// var allEffects = item.OvertimeTemplates.SelectMany(template => template.Effects).ToList();
			// PermanentStatHandler(allEffects);
			TooltipManager.Instance.DestroyTooltip();
			InventoryManager.Instance.RemoveItem(item, 1);
			PanelManager.Instance.UpdateRamSlotUI(this);
			PanelManager.Instance.UpdateStatsText(this);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left) return;
			PanelManager.Instance.SetStatsText(this);
		}
	}
}