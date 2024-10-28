using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dyscord.Managers;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public enum SelectableTypes
	{
		MainPanel,
		SubPanel,
		MissionLocation
	}
	public class SelectableArea : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] private Image decorationImage;
		[SerializeField] private float scaleMultiplier = 1.1f;
		[SerializeField] private float scaleDuration = 0.2f;
		[SerializeField] private SelectableTypes selectableType;
		[SerializeField][ShowIf(nameof(selectableType), SelectableTypes.MainPanel)] private HQPanelTypes panelType;
		[SerializeField][ShowIf(nameof(selectableType), SelectableTypes.SubPanel)] private SubPanelTypes subPanelType;
		[SerializeField][ShowIf(nameof(selectableType), SelectableTypes.MissionLocation)] private MissionLocations missionLocation;
		
		private Tween scaleTween;
		private Vector3 originalScale;
		
		
		private void Awake()
		{
			originalScale = decorationImage.transform.localScale;
		}
		
		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left) return;
			GlobalSoundManager.Instance.PlayUISFX(UISFXTypes.SoftMouseClick);
			switch (selectableType)
			{
				case SelectableTypes.MissionLocation:
					HQManager.Instance.ChangeMissionLocation(missionLocation, true);
					break;
				case SelectableTypes.SubPanel:
					HQManager.Instance.ChangeSubPanel(subPanelType, true);
					break;
				default:
					HQManager.Instance.ChangePanel(panelType);
					break;
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (scaleTween.IsActive())
				scaleTween.Kill();
			scaleTween = decorationImage.transform.DOScale(scaleMultiplier, scaleDuration);
			GlobalSoundManager.Instance.PlayUISFX(UISFXTypes.SoftMouseHover);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (scaleTween.IsActive())
				scaleTween.Kill();
			scaleTween = decorationImage.transform.DOScale(originalScale, scaleDuration);
		}
	}
}