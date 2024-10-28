using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dyscord.Managers;
using Dyscord.ScriptableObjects.Cyberware;
using Dyscord.UI.Tooltips;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class CyberwareUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		private Image icon;
		public CyberwareSO Cyberware { get; set; }
		public Vector3 OriginalPosition { get; set; }
		public Transform Parent { get; set; }
		
		private CyberwareSlotUI cyberwareSlot;
		private bool equipped;
		private Tooltip tooltip;
		private Tween scaleTween;
		private Tween locationTween;

		private void Awake()
		{
			tooltip = GetComponent<Tooltip>();
		}
		
		public void Setup(CyberwareSO cyberware, Transform parent, bool equipped, CyberwareSlotUI cyberwareSlot = null)
		{
			icon = GetComponent<Image>();
			Cyberware = cyberware;
			icon.sprite = cyberware.Icon;
			icon.SetNativeSize();
			OriginalPosition = transform.localPosition;
			Parent = parent;
			transform.SetSiblingIndex(0);
			this.equipped = equipped;
			this.cyberwareSlot = cyberwareSlot;
			if (!tooltip) tooltip = GetComponent<Tooltip>();
			tooltip.TooltipObject = cyberware;
		}
		
		private void ReturnToOriginalPosition()
		{
			if (locationTween.IsActive()) locationTween.Kill();
			Vector3 originalPositionInWorld = Parent.TransformPoint(OriginalPosition);
			locationTween = transform.DOMove(originalPositionInWorld, 0.2f).OnComplete(() =>
			{
				transform.SetParent(Parent);
				transform.SetSiblingIndex(0);
			});
			icon.raycastTarget = true;
			GlobalSoundManager.Instance.PlayUISFX(UISFXTypes.Unavailable);
		}
		
		public void OnBeginDrag(PointerEventData eventData)
		{
			if (equipped) return;
			transform.SetParent(transform.root);
			icon.raycastTarget = false;
		}
		
		public void OnDrag(PointerEventData eventData)
		{
			if (equipped) return;
			transform.position = Input.mousePosition;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (eventData.hovered.Count == 0)
			{
				ReturnToOriginalPosition();
				return;
			}

			if (!equipped && eventData.hovered[^1].TryGetComponent(out CyberwareSlotUI cyberwareSlot))
			{
				if (!cyberwareSlot.AssignCyberware(Cyberware))
				{
					ReturnToOriginalPosition();
					return;
				}
				this.cyberwareSlot = cyberwareSlot;
				Parent = cyberwareSlot.transform;
				transform.SetParent(Parent);
				if (locationTween.IsActive()) locationTween.Kill();
				locationTween = transform.DOLocalMove(Vector3.zero, 0.2f);
				OriginalPosition = transform.localPosition;
				icon.raycastTarget = true;
				equipped = true;
			}
			else
			{
				ReturnToOriginalPosition();
			}
		}
		
		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left) return;
			if (!equipped) return;
			cyberwareSlot.RemoveCyberware();
			InventoryManager.Instance.AddCyberware(Cyberware);
			InventoryManager.Instance.UnequipCyberware(Cyberware);
			TooltipManager.Instance.DestroyTooltip();
			Destroy(gameObject);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (scaleTween.IsActive()) scaleTween.Kill();
			scaleTween = transform.DOScale(1.1f, 0.2f);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (scaleTween.IsActive()) scaleTween.Kill();
			scaleTween = transform.DOScale(1f, 0.2f);
		}
	}
}