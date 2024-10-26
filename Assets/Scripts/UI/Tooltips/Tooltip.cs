using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = System.Object;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI.Tooltips
{
	public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[Header("Tooltip")]
		[SerializeField] private float tooltipDelay = 0.5f;
		
		private bool pointerOver;
		private bool tooltipActive;
		private float tooltipTimer;
		
		public Object TooltipObject { get; set; }
		
		private void Update()
		{
			UpdateTooltip();
		}

		public void UpdateTooltip()
		{
			if (!pointerOver) return;
			if (tooltipActive) return;
			tooltipTimer += Time.deltaTime;
			if (tooltipTimer >= tooltipDelay)
			{
				tooltipActive = true;
				TooltipManager.Instance.ShowTooltip(TooltipObject);
			}
		}
		
		public void OnPointerEnter(PointerEventData eventData)
		{
			pointerOver = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			pointerOver = false;
			tooltipTimer = 0;
			if (!tooltipActive) return;
			TooltipManager.Instance.DestroyTooltip();
			tooltipActive = false;
		}

		private void OnDestroy()
		{
			if (!tooltipActive) return;
			TooltipManager.Instance.DestroyTooltip();
		}
	}
}