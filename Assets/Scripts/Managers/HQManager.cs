using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		Mission
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
		[SerializeField] private GameObject equipmentPanel;
		[SerializeField] private GameObject missionPanel;
		
		[Header("Locations")]
		[SerializeField] private GameObject southStreet;
		[SerializeField] private GameObject hectCorp67;
		[SerializeField] private GameObject hectCorpRnD;
		
		[Header("Buttons")]
		[SerializeField] private Button closeEquipmentButton;
		[SerializeField] private Button closeMapButton;
		[SerializeField] private Button closeMissionButton;
		[SerializeField] private Button gameplayButton;
		
		private HQPanelTypes currentPanel = HQPanelTypes.HQ;
		private SubPanelTypes currentSubPanel;
		private MissionLocations currentMissionLocation;
		
		private void Start()
		{
			hqPanel.SetActive(false);
			shopPanel.SetActive(false);
			equipmentPanel.SetActive(false);
			missionPanel.SetActive(false);
			southStreet.SetActive(false);
			hectCorp67.SetActive(false);
			hectCorpRnD.SetActive(false);
			closeEquipmentButton.onClick.AddListener(() => ChangeSubPanel(SubPanelTypes.Equipment, false));
			closeMapButton.onClick.AddListener(() => ChangeSubPanel(SubPanelTypes.Mission, false));
			closeMissionButton.onClick.AddListener(() => ChangeMissionLocation(currentMissionLocation, false));
			gameplayButton.onClick.AddListener(() => SceneManager.LoadScene("Gameplay"));
			closeMissionButton.gameObject.SetActive(false);
			ChangePanel(HQPanelTypes.HQ, true);
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
				ChangePanel(HQPanelTypes.HQ, true);
			}
		}

		public void ChangePanel(HQPanelTypes type, bool active)
		{
			hqPanel.SetActive(false);
			shopPanel.SetActive(false);
			currentPanel = active ? type : HQPanelTypes.Null;
			switch (type)
			{
				case HQPanelTypes.HQ:
					hqPanel.SetActive(active);
					break;
				case HQPanelTypes.Shop:
					shopPanel.SetActive(active);
					break;
			}
		}

		public void ChangeSubPanel(SubPanelTypes type, bool active)
		{
			equipmentPanel.SetActive(false);
			missionPanel.SetActive(false);
			currentSubPanel = active ? type : SubPanelTypes.Null;
			switch (type)
			{
				case SubPanelTypes.Equipment:
					equipmentPanel.SetActive(active);
					break;
				case SubPanelTypes.Mission:
					missionPanel.SetActive(active);
					break;
			}
		}
		
		public void ChangeMissionLocation(MissionLocations location, bool active)
		{
			southStreet.SetActive(false);
			hectCorp67.SetActive(false);
			hectCorpRnD.SetActive(false);
			currentMissionLocation = active ? location : MissionLocations.Null;
			closeMissionButton.gameObject.SetActive(active);
			switch (location)
			{
				case MissionLocations.SouthStreet:
					southStreet.SetActive(active);
					break;
				case MissionLocations.HectCorp67:
					hectCorp67.SetActive(active);
					break;
				case MissionLocations.HectCorpRnD:
					hectCorpRnD.SetActive(active);
					break;
			}
		}
		
	}
}