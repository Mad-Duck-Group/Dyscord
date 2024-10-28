using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Microlight.MicroAudio;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.EventSystems;
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
		Clinic
	}
	public enum SubPanelTypes
	{
		Null,
		Equipment,
		Mission,
		Store,
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
		[SerializeField] private GraphicRaycaster hqGraphicRaycaster;
		[SerializeField] private GameObject hqPanel;
		[SerializeField] private GameObject clinicPanel;
		[SerializeField] private CanvasGroup equipmentPanel;
		[SerializeField] private CanvasGroup missionPanel;
		[SerializeField] private CanvasGroup storePanel;
		[SerializeField] private CanvasGroup pausePanel;

		[Header("Locations")]
		[SerializeField] private GameObject southStreet;
		[SerializeField] private GameObject hectCorp67;
		[SerializeField] private GameObject hectCorpRnD;
		
		[Header("Buttons")]
		[SerializeField] private Button closeEquipmentButton;
		[SerializeField] private Button closeMapButton;
		[SerializeField] private Button closeMissionButton;
		[SerializeField] private Button closeStoreButton;
		[SerializeField] private Button gameplayButton;
		[SerializeField] private Button[] settingsButtons;
		[SerializeField] private Button resumeButton;
		[SerializeField] private Button quitButton;
		[SerializeField] private Slider masterVolumeSlider;
		[SerializeField] private Button muteButton;
		
		private HQPanelTypes currentPanel = HQPanelTypes.HQ;
		private SubPanelTypes currentSubPanel;
		private MissionLocations currentMissionLocation;
		private Sequence mainPanelSequence;
		private Sequence subPanelSequence;
		private Tween locationTween;
		private bool mute;
		private float beforeMute;

		private void Start()
		{
			hqPanel.SetActive(true);
			clinicPanel.SetActive(true);
			equipmentPanel.gameObject.SetActive(false);
			missionPanel.gameObject.SetActive(false);
			pausePanel.gameObject.SetActive(false);
			southStreet.SetActive(false);
			hectCorp67.SetActive(false);
			hectCorpRnD.SetActive(false);
			closeEquipmentButton.onClick.AddListener(() => ChangeSubPanel(SubPanelTypes.Equipment, false));
			closeMapButton.onClick.AddListener(() => ChangeSubPanel(SubPanelTypes.Mission, false));
			closeStoreButton.onClick.AddListener(() => ChangeSubPanel(SubPanelTypes.Store, false));
			closeMissionButton.onClick.AddListener(() => ChangeMissionLocation(currentMissionLocation, false));
			gameplayButton.onClick.AddListener(() =>
				SceneManagerPersistent.Instance.LoadNextScene(SceneTypes.GamePlay, LoadSceneMode.Additive, false));
			settingsButtons.ToList().ForEach(button => button.onClick.AddListener(() => ChangeSubPanel(SubPanelTypes.Pause, true)));
			resumeButton.onClick.AddListener(() => ChangeSubPanel(SubPanelTypes.Pause, false));
			quitButton.onClick.AddListener(() => SceneManagerPersistent.Instance.LoadNextScene(SceneTypes.MainMenu, LoadSceneMode.Additive, false));
			closeMissionButton.gameObject.SetActive(false);
			masterVolumeSlider.value = MicroAudio.MasterVolume;
			masterVolumeSlider.onValueChanged.AddListener(_ => OnMasterVolumeChange());
			muteButton.onClick.AddListener(ToggleMute);
			GlobalSoundManager.Instance.PlayBGM(BGMTypes.HQ);
			ProgressionManager.Instance.PlayVN(VNTypes.HQ);
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
			if (type == HQPanelTypes.Clinic) ProgressionManager.Instance.PlayVN(VNTypes.Shop);
			int move = type == HQPanelTypes.HQ ? -1 : 1;
			currentPanel = type;
			Sequence sequence = DOTween.Sequence();
			RectTransform hqPanelRect = hqPanel.transform as RectTransform;
			//convert ui width to world space
			float hqPanelsize = hqPanelRect.rect.width * hqPanelRect.lossyScale.x;
			//move camera according to the panel width
			sequence.Append(Camera.main.transform.DOMoveX(hqPanelsize * move, 0.25f).SetRelative(true));
			sequence.AppendCallback(() => hqGraphicRaycaster.enabled = true);
			mainPanelSequence = sequence;
			hqGraphicRaycaster.enabled = false;
			GlobalSoundManager.Instance.PlayUISFX(UISFXTypes.Swoosh);
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
					if (active) GlobalSoundManager.Instance.PlayUISFX(UISFXTypes.Popup);
					equipmentPanel.gameObject.SetActive(true);
					equipmentPanel.alpha = active ? 0 : 1;
					sequence.Append(equipmentPanel.DOFade(alpha, 0.2f));
					sequence.AppendCallback(() =>
					{
						if (!active) TooltipManager.Instance.DestroyTooltip();
						equipmentPanel.gameObject.SetActive(active);
					});
					if (!active) TooltipManager.Instance.DestroyTooltip();
					break;
				case SubPanelTypes.Mission:
					if (!ProgressionManager.Instance.ShopVNPlayed)
					{
						GlobalSoundManager.Instance.PlayUISFX(UISFXTypes.Unavailable);
						return;
					}
					if (active) GlobalSoundManager.Instance.PlayUISFX(UISFXTypes.Popup);
					missionPanel.gameObject.SetActive(true);
					missionPanel.alpha = active ? 0 : 1;
					sequence.Append(missionPanel.DOFade(alpha, 0.2f));
					sequence.AppendCallback(() => missionPanel.gameObject.SetActive(active));
					break;
				case SubPanelTypes.Store:
					if (active) GlobalSoundManager.Instance.PlayUISFX(UISFXTypes.Popup);
					storePanel.gameObject.SetActive(true);
					storePanel.alpha = active ? 0 : 1;
					sequence.Append(storePanel.DOFade(alpha, 0.2f));
					sequence.AppendCallback(() => storePanel.gameObject.SetActive(active));
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
			if (active) GlobalSoundManager.Instance.PlayUISFX(UISFXTypes.Popup);
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
		
		public void OnMasterVolumeChange()
		{
			MicroAudio.MasterVolume = masterVolumeSlider.value;
			MicroAudio.SaveSettings();
		}

		public void ToggleMute()
		{
			if (mute)
			{
				mute = false;
				MicroAudio.MasterVolume = beforeMute;
				masterVolumeSlider.gameObject.SetActive(true);
				masterVolumeSlider.value = beforeMute;
				muteButton.image.sprite = muteButton.spriteState.pressedSprite;
			}
			else
			{
				mute = true;
				beforeMute = MicroAudio.MasterVolume;
				MicroAudio.MasterVolume = 0;
				masterVolumeSlider.gameObject.SetActive(false);
				muteButton.image.sprite = muteButton.spriteState.disabledSprite;
			}
		}
		
	}
}