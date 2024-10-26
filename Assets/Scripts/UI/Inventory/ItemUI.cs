using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Managers;
using Dyscord.ScriptableObjects.Item;
using Dyscord.UI.Tooltips;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class ItemUI : MonoBehaviour, IPointerClickHandler
	{
		private Image icon;
		private ItemSO item;
		private Tooltip tooltip;
		public delegate void UseItem(ItemSO item);
		public static event UseItem OnUseItem;

		private void Awake()
		{
			tooltip = GetComponent<Tooltip>();
		}
		public void Setup(ItemSO item)
		{
			if (!tooltip) tooltip = GetComponent<Tooltip>();
			icon = GetComponent<Image>();
			transform.SetAsFirstSibling();
			icon.sprite = item.Icon;
			this.item = item;
			tooltip.TooltipObject = item;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left) return;
			OnUseItem?.Invoke(item);
		}
		
	}
}