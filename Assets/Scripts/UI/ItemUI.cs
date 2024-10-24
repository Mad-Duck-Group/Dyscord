using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.ScriptableObjects.Item;
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
		public delegate void UseItem(ItemSO item);
		public static event UseItem OnUseItem;

		public void Setup(ItemSO item)
		{
			icon = GetComponent<Image>();
			transform.SetAsFirstSibling();
			icon.sprite = item.Icon;
			this.item = item;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left) return;
			OnUseItem?.Invoke(item);
		}
	}
}