using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Managers;
using Dyscord.ScriptableObjects.Cyberware;
using Dyscord.UI.Tooltips;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class CyberwareUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
	{
		private Image icon;
		public CyberwareSO Cyberware { get; set; }
		public Vector3 OriginalPosition { get; set; }
		public Transform Parent { get; set; }
		
		private CyberwareSlotUI cyberwareSlot;
		private bool equipped;
		private Tooltip tooltip;

		private void Awake()
		{
			tooltip = GetComponent<Tooltip>();
		}
		
		public void Setup(CyberwareSO cyberware, Transform parent, bool equipped, CyberwareSlotUI cyberwareSlot = null)
		{
			icon = GetComponent<Image>();
			Cyberware = cyberware;
			icon.sprite = cyberware.Icon;
			OriginalPosition = transform.localPosition;
			Parent = parent;
			transform.SetSiblingIndex(0);
			this.equipped = equipped;
			this.cyberwareSlot = cyberwareSlot;
			tooltip.TooltipObject = cyberware;
		}
		
		private void ReturnToOriginalPosition()
		{
			transform.SetParent(Parent);
			transform.SetSiblingIndex(0);
			transform.localPosition = OriginalPosition;
			icon.raycastTarget = true;
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
				transform.localPosition = Vector3.zero;
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
			if (eventData.button != PointerEventData.InputButton.Right) return;
			if (!equipped) return;
			cyberwareSlot.RemoveCyberware();
			InventoryManager.Instance.AddCyberware(Cyberware);
			InventoryManager.Instance.UnequipCyberware(Cyberware);
			Destroy(gameObject);
		}
	}
}