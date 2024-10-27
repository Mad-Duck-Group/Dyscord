using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Managers;
using Dyscord.ScriptableObjects.Item;
using Dyscord.UI;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Characters.Player
{
	public class Player : Character
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
			InventoryManager.Instance.RemoveItem(item, 1);
			PanelManager.Instance.UpdateRamSlotUI(this);
			PanelManager.Instance.UpdateStatsText(this);
		}
	}
}