using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Managers
{
	public enum HQPanelTypes
	{
		Null,
		HQ,
		Shop,
	}
	public enum SubPanelTypes
	{
		Null,
		Equipment,
		Mission,
		Pause
	}
	public enum MissionLocations
	{
		Null,
		SouthStreet,
		HectCorp67,
		HectCorpRnD
	}
	public class HQManager : MonoSingleton<HQManager>
	{
		[Header("Panels")]
		[SerializeField] private GameObject hqPanel;
		[SerializeField] private GameObject shopPanel;
		[SerializeField] private CanvasGroup equipmentPanel;
		[SerializeField] private CanvasGroup missionPanel;
		[SerializeField] private CanvasGroup pausePanel;
		
		[Header("Locations")]
		[SerializeField] private GameObject southStreet;
		[SerializeField] private GameObject hectCorp67;
		[SerializeField] private GameObject hectCorpRnD;
		
		[Header("Buttons")]
		[SerializeField] private Button closeEquipmentButton;
		[SerializeField] private Button closeMapButton;
		[SerializeField] private Button closeMissionButton;
		[SerializeField] private Button gameplayButton;
		[SerializeField] private Button[] settingsButtons;
		[SerializeField] private Button resumeButton;
		[SerializeField] private Button quitButton;
		
		private HQPanelTypes currentPanel = HQPanelTypes.HQ;
		private SubPanelTypes currentSubPanel;
		private MissionLocations currentMissionLocation;
		private Sequence mainPanelSequence;
		private Sequence subPanelSequence;
		private Tween locationTween;
		private Camera mainCamera;
		
		private void Start()
		{
			hqPanel.SetActive(true);
			shopPanel.SetActive(true);
			equipmentPanel.gameObject.SetActive(false);
			missionPanel.gameObject.SetActive(false);
			pausePanel.gameObject.SetActive(false);
			southStreet.SetActive(false);
			hectCorp67.SetActive(false);
			hectCorpRnD.SetActive(false);
			closeEquipmentButton.onClick.AddListener(() => ChangeSubPanel(SubPanelTypes.Equipment, false));
			closeMapButton.onClick.AddListener(() => ChangeSubPanel(SubPanelTypes.Mission, false));
			closeMissionButton.onClick.AddListener(() => ChangeMissionLocation(currentMissionLocation, false));
			gameplayButton.onClick.AddListener(() =>
				SceneManagerPersistent.Instance.LoadNextScene(SceneTypes.GamePlay, LoadSceneMode.Additive, false));
			settingsButtons.ToList().ForEach(button => button.onClick.AddListener(() => ChangeSubPanel(SubPanelTypes.Pause, true)));
			resumeButton.onClick.AddListener(() => ChangeSubPanel(SubPanelTypes.Pause, false));
			quitButton.onClick.AddListener(() => SceneManagerPersistent.Instance.LoadNextScene(SceneTypes.MainMenu, LoadSceneMode.Additive, false));
			closeMissionButton.gameObject.SetActive(false);
			ProgressionManager.Instance.PlayVN(VNTypes.HQ);
			mainCamera = Camera.main;
		}

		private void Update()
		{
			UndoHandler();
		}

		private void UndoHandler()
		{
			if (!Input.GetMouseButtonDown(1)) return;
			if (currentMissionLocation != MissionLocations.Null)
			{
				ChangeMissionLocation(currentMissionLocation, false);
				return;
			}
			if (currentSubPanel != SubPanelTypes.Null)
			{
				ChangeSubPanel(currentSubPanel, false);
				return;
			}
			if (currentPanel != HQPanelTypes.HQ)
			{
				ChangePanel(HQPanelTypes.HQ);
			}
		}

		public void ChangePanel(HQPanelTypes type)
		{
			if (mainPanelSequence.IsActive()) return;
			int move = type == HQPanelTypes.HQ ? -1 : 1;
			currentPanel = type;
			Sequence sequence = DOTween.Sequence();
			RectTransform hqPanelRect = hqPanel.transform as RectTransform;
			//convert ui width to world space
			float hqPanelsize = hqPanelRect.rect.width * hqPanelRect.lossyScale.x;
			//move camera according to the panel width
			sequence.Append(mainCamera.transform.DOMoveX(hqPanelsize * move, 0.25f).SetRelative(true));
			mainPanelSequence = sequence;
		}

		public void ChangeSubPanel(SubPanelTypes type, bool active)
		{
			if (subPanelSequence.IsActive()) return;
			currentSubPanel = active ? type : SubPanelTypes.Null;
			float alpha = active ? 1 : 0;
			Sequence sequence = DOTween.Sequence();
			switch (type)
			{
				case SubPanelTypes.Equipment:
					equipmentPanel.gameObject.SetActive(true);
					equipmentPanel.alpha = active ? 0 : 1;
					sequence.Append(equipmentPanel.DOFade(alpha, 0.2f));
					sequence.AppendCallback(() => equipmentPanel.gameObject.SetActive(active));
					break;
				case SubPanelTypes.Mission:
					missionPanel.gameObject.SetActive(true);
					missionPanel.alpha = active ? 0 : 1;
					sequence.Append(missionPanel.DOFade(alpha, 0.2f));
					sequence.AppendCallback(() => missionPanel.gameObject.SetActive(active));
					break;
				case SubPanelTypes.Pause:
					pausePanel.gameObject.SetActive(true);
					pausePanel.alpha = active ? 0 : 1;
					sequence.Append(pausePanel.DOFade(alpha, 0.2f));
					sequence.AppendCallback(() => pausePanel.gameObject.SetActive(active));
					break;
			}
		}
		
		public void ChangeMissionLocation(MissionLocations location, bool active)
		{
			if (locationTween.IsActive()) return;
			currentMissionLocation = active ? location : MissionLocations.Null;
			closeMissionButton.gameObject.SetActive(active);
			switch (location)
			{
				case MissionLocations.SouthStreet:
					southStreet.SetActive(true);
					southStreet.transform.localScale = active ? Vector3.zero : Vector3.one;
					locationTween = southStreet.transform.DOScale(active ? Vector3.one : Vector3.zero, 0.2f).OnComplete(() =>
					{
						southStreet.SetActive(active);
					});
					break;
				case MissionLocations.HectCorp67:
					hectCorp67.SetActive(true);
					hectCorp67.transform.localScale = active ? Vector3.zero : Vector3.one;
					locationTween = hectCorp67.transform.DOScale(active ? Vector3.one : Vector3.zero, 0.2f).OnComplete(() =>
					{
						hectCorp67.SetActive(active);
					});
					break;
				case MissionLocations.HectCorpRnD:
					hectCorpRnD.SetActive(true);
					hectCorpRnD.transform.localScale = active ? Vector3.zero : Vector3.one;
					locationTween = hectCorpRnD.transform.DOScale(active ? Vector3.one : Vector3.zero, 0.2f).OnComplete(() =>
					{
						hectCorpRnD.SetActive(active);
					});
					break;
			}
		}
		
	}
}