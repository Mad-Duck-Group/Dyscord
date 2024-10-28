using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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
	public class ItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		private Image icon;
		private ItemSO item;
		private Tooltip tooltip;
		private Tween scaleTween;
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
			icon.SetNativeSize();
			this.item = item;
			tooltip.TooltipObject = item;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left) return;
			OnUseItem?.Invoke(item);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (scaleTween.IsActive()) scaleTween.Kill();
			scaleTween = transform.DOScale(1.1f, 0.2f);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (scaleTween.IsActive()) scaleTween.Kill();
			scaleTween = transform.DOScale(Vector3.one, 0.2f);
		}
	}
}