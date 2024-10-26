using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.ScriptableObjects.Action;
using Dyscord.ScriptableObjects.Cyberware;
using Dyscord.ScriptableObjects.Item;
using Dyscord.ScriptableObjects.Overtime;
using Dyscord.UI;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = System.Object;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Managers
{
	public class TooltipManager : PersistentMonoSingleton<TooltipManager>
	{
		[SerializeField] private GameObject tooltipPrefab;
		[SerializeField] private BasicTooltipPanelUI basicTooltipPanelUIPrefab;
		[SerializeField] private OvertimeTooltipPanelUI overtimeTooltipPanelUIPrefab;
		[SerializeField] private CyberwareSkillTooltipUI cyberwareSkillTooltipUIPrefab;
		[SerializeField] private Vector3 defaultOffset;
		
		private RectTransform currentTooltip;
		
		private Vector3 CurrentMousePosition => Input.mousePosition;

		public void ShowTooltip(Object tooltipObject)
		{
			switch (tooltipObject)
			{
				case CharacterActionSO action:
					ShowTooltip(action);
					break;
				case ItemSO item:
					ShowTooltip(item);
					break;
				case CyberwareSO cyberware:
					ShowTooltip(cyberware);
					break;
				case OvertimeTemplate overtime:
					ShowTooltip(overtime);
					break;
			}
		}
		
		private void ShowTooltip(CharacterActionSO action)
		{
			DestroyTooltip();
			currentTooltip = Instantiate(tooltipPrefab, new Vector3(10000, 10000, 0), Quaternion.identity, transform)
				.GetComponent<RectTransform>();
			var basicTooltipPanelUI = Instantiate(basicTooltipPanelUIPrefab, currentTooltip.transform);
			basicTooltipPanelUI.SetTooltip(action.ActionName, action.Description);
			StartCoroutine(AdjustTooltipPosition());
		}
		
		private void ShowTooltip(ItemSO item)
		{
			DestroyTooltip();
			currentTooltip = Instantiate(tooltipPrefab, new Vector3(10000, 10000, 0), Quaternion.identity, transform)
				.GetComponent<RectTransform>();
			var basicTooltipPanelUI = Instantiate(basicTooltipPanelUIPrefab, currentTooltip.transform);
			basicTooltipPanelUI.SetTooltip(item.ItemName, item.Description);
			if (item.OvertimeTemplates.Count > 0)
			{
				var overtimeTooltipPanelUI = Instantiate(overtimeTooltipPanelUIPrefab, currentTooltip.transform);
				overtimeTooltipPanelUI.SetTooltip("Effects", item.OvertimeTemplates);
			}
			StartCoroutine(AdjustTooltipPosition());
		}
		
		private void ShowTooltip(CyberwareSO cyberware)
		{
			DestroyTooltip();
			currentTooltip = Instantiate(tooltipPrefab, new Vector3(10000, 10000, 0), Quaternion.identity, transform)
				.GetComponent<RectTransform>();
			var basicTooltipPanelUI = Instantiate(basicTooltipPanelUIPrefab, currentTooltip.transform);
			basicTooltipPanelUI.SetTooltip(cyberware.CyberwareName, cyberware.Description);
			if (cyberware.OvertimeTemplates.Count > 0)
			{
				var overtimeTooltipPanelUI = Instantiate(overtimeTooltipPanelUIPrefab, currentTooltip.transform);
				overtimeTooltipPanelUI.SetTooltip("When Worn", cyberware.OvertimeTemplates);
			}
			if (cyberware.HackedOvertimeTemplates.Count > 0)
			{
				var hackedOvertimeTooltipPanelUI = Instantiate(overtimeTooltipPanelUIPrefab, currentTooltip.transform);
				hackedOvertimeTooltipPanelUI.SetTooltip("When Hacked", cyberware.HackedOvertimeTemplates);
			}
			if (cyberware.AllActions.Count > 0)
			{
				var cyberwareSkillTooltipUI = Instantiate(cyberwareSkillTooltipUIPrefab, currentTooltip.transform);
				cyberwareSkillTooltipUI.SetTooltip("Additional Skills", cyberware.AllActions);
			}
			StartCoroutine(AdjustTooltipPosition());
		}

		private void ShowTooltip(OvertimeTemplate overtime)
		{
			DestroyTooltip();
			currentTooltip = Instantiate(tooltipPrefab, new Vector3(10000, 10000, 0), Quaternion.identity, transform)
				.GetComponent<RectTransform>();
			var overtimeTooltipPanelUI = Instantiate(overtimeTooltipPanelUIPrefab, currentTooltip.transform);
			overtimeTooltipPanelUI.SetTooltip(overtime.OvertimeName, new List<OvertimeTemplate> {overtime});
			StartCoroutine(AdjustTooltipPosition());
		}

		private IEnumerator AdjustTooltipPosition()
		{
			if (!currentTooltip) yield break;
			yield return new WaitForEndOfFrame();
			
			Canvas canvas = GetComponent<Canvas>();
			RectTransform canvasRect = canvas.GetComponent<RectTransform>();
			currentTooltip.anchorMin = new Vector2(0.5f, 0.5f);
			currentTooltip.anchorMax = new Vector2(0.5f, 0.5f);
			currentTooltip.pivot = new Vector2(0.5f, 0.5f);
			
			RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, canvas.worldCamera, out Vector2 localMousePosition);
			
			currentTooltip.anchoredPosition = localMousePosition + (Vector2)defaultOffset;
			
			Vector3[] corners = new Vector3[4];
			currentTooltip.GetWorldCorners(corners);

			for (int i = 0; i < corners.Length; i++)
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, corners[i], canvas.worldCamera, out Vector2 localPoint);
				corners[i] = localPoint;
			}
			
			float tooltipLeft = corners[0].x;
			float tooltipBottom = corners[0].y;
			float tooltipRight = corners[2].x;
			float tooltipTop = corners[2].y;
			
			float canvasLeft = canvasRect.rect.xMin;
			float canvasRight = canvasRect.rect.xMax;
			float canvasTop = canvasRect.rect.yMax;
			float canvasBottom = canvasRect.rect.yMin;
			
			Vector2 finalPosition = currentTooltip.anchoredPosition;

			if (tooltipBottom < canvasBottom)
				finalPosition.y += (canvasBottom - tooltipBottom);
			if (tooltipTop > canvasTop)
				finalPosition.y -= (tooltipTop - canvasTop);
			if (tooltipLeft < canvasLeft)
				finalPosition.x += (canvasLeft - tooltipLeft);
			if (tooltipRight > canvasRight)
				finalPosition.x -= (tooltipRight - canvasRight);
			
			currentTooltip.anchoredPosition = finalPosition;
		}

		public void DestroyTooltip()
		{
			if (!currentTooltip) return;
			Destroy(currentTooltip.gameObject);
		}
	}
}