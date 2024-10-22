using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Characters;
using Dyscord.Characters.Player;
using Dyscord.ScriptableObjects.Action;
using Dyscord.ScriptableObjects.Action.Hack;
using Dyscord.ScriptableObjects.Cyberware;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Managers
{
	public enum PanelTypes
	{
		Stats,
		Attack,
		Skill,
		HackSelection,
		HackCyberware
		
	}
	public class PanelManager : MonoSingleton<PanelManager>
	{
		[Header("Panel")]
		[SerializeField] private GameObject statsPanel;
		[SerializeField] private GameObject attackPanel;
		[SerializeField] private GameObject skillPanel;
		[SerializeField] private GameObject hackSelectionPanel;
		[SerializeField] private GameObject hackCyberwarePanel;
		
		[Header("Button")]
		[SerializeField] private TMP_Text statsText;
		[SerializeField] private GameObject actionButtonPrefab;
		[SerializeField] private GameObject cyberwareButtonPrefab;
		[SerializeField] private Button attackButton;
		[SerializeField] private Button skillButton;
		[SerializeField] private Button hackButton;
		[SerializeField] private Button hackCyberSecurityButton;
		[SerializeField] private Button hackCyberwareButton;
		
		public delegate void HackActionButtonPressed(bool isCyberware);
		public static event HackActionButtonPressed OnHackActionButtonPressed;
		
		public delegate void CyberwareButtonPressed(CyberwareSO cyberware);
		public static event CyberwareButtonPressed OnCyberwareButtonPressed;
		
		private List<Button> actionButtons = new List<Button>();
		private List<Button> cyberwareButtons = new List<Button>();
		private TurnOrder CurrentTurnOrder => TurnManager.Instance.CurrentTurnOrder;
		private Character PlayerInstance => TurnManager.Instance.PlayerInstance;

		private bool PlayerSelecting => CurrentTurnOrder.character.CurrentAction &&
		                                CurrentTurnOrder.character.CurrentAction.PlayerSelecting;

		private PanelTypes currentPanel = PanelTypes.Stats;
		private Character currentCharacterStats;
		private List<Button> ActionPanelButtons => new List<Button> {attackButton, skillButton, hackButton};
		
		protected override void Awake()
		{
			base.Awake();
			statsPanel.SetActive(true);
			attackPanel.SetActive(false);
			skillPanel.SetActive(false);
			hackSelectionPanel.SetActive(false);
			hackCyberwarePanel.SetActive(false);
			attackButton.onClick.AddListener(() => SetActivePanel(PanelTypes.Attack, true));
			skillButton.onClick.AddListener(() => SetActivePanel(PanelTypes.Skill, true));
			hackCyberSecurityButton.onClick.AddListener(() => OnHackActionButtonPressed?.Invoke(false));
			hackCyberwareButton.onClick.AddListener(() => OnHackActionButtonPressed?.Invoke(true));
		}

		public void InitializePanel()
		{
			SetStatsText(PlayerInstance);
		}
		
		/// <summary>
		/// Initializes the player UI by instantiating the action buttons.
		/// </summary>
		public void InitializeActionUI()
		{
			PlayerInstance.AllAttacks.ForEach(attack =>
			{
				Button attackButtonInstance = Instantiate(actionButtonPrefab, attackPanel.transform).GetComponent<Button>();
				attackButtonInstance.onClick.AddListener(attack.SelectTarget);
				attackButtonInstance.GetComponentInChildren<TMP_Text>().text = attack.ActionName;
				actionButtons.Add(attackButtonInstance);
			});
			PlayerInstance.AllSkills.ForEach(skill =>
			{
				Button skillButtonInstance = Instantiate(actionButtonPrefab, skillPanel.transform).GetComponent<Button>();
				skillButtonInstance.onClick.AddListener(skill.SelectTarget);
				skillButtonInstance.GetComponentInChildren<TMP_Text>().text = skill.ActionName;
				actionButtons.Add(skillButtonInstance);
			});
			hackButton.onClick.AddListener(() =>
			{
				PlayerInstance.HackAction.SelectTarget();
				hackButton.interactable = false;
				attackButton.interactable = false;
				skillButton.interactable = false;
			});
			actionButtons.Add(hackButton);
		}

		public void PopulateCyberwareUI(List<CyberwareSO> cyberwares, int hackAccessLevel)
		{
			foreach (var button in cyberwareButtons)
			{
				Destroy(button.gameObject);
			}
			cyberwareButtons.Clear();
			foreach (CyberwareSO cyberware in cyberwares)
			{
				Button cyberwareButtonInstance = Instantiate(cyberwareButtonPrefab, hackCyberwarePanel.transform)
					.GetComponent<Button>();
				cyberwareButtonInstance.onClick.AddListener(() => OnCyberwareButtonPressed?.Invoke(cyberware));
				cyberwareButtonInstance.GetComponentInChildren<TMP_Text>().text = cyberware.CyberwareName;
				cyberwareButtonInstance.interactable = cyberware.HackAccessLevel <= hackAccessLevel;
				cyberwareButtons.Add(cyberwareButtonInstance);
			}
		}

		public void UpdateHackSelectionButton(bool security, bool cyberware)
		{
			hackCyberSecurityButton.interactable = security;
			hackCyberwareButton.interactable = cyberware;
		}

		/// <summary>
		/// Updates the button UI based on the current turn order and RAM of the player.
		/// </summary>
		public void UpdateButtonUI()
		{
			if (CurrentTurnOrder.character is not Player)
			{
				actionButtons.ForEach(button => button.interactable = false);
				ActionPanelButtons.ForEach(button => button.interactable = false);
			}
			else if (PlayerSelecting)
			{
				if (currentPanel is not PanelTypes.Stats)
					SetActivePanel(currentPanel, false);
			}
			else
			{
				ActionPanelButtons.ForEach(button =>
				{
					if (currentPanel == PanelTypes.Stats)
					{
						button.interactable = true;
						return;
					}
					if (button == attackButton && currentPanel == PanelTypes.Attack) return;
					if (button == skillButton && currentPanel == PanelTypes.Skill) return;
					button.interactable = true;
				});
				List<CharacterActionSO> allActions = CurrentTurnOrder.character.AllActions;
				for (int i = 0; i < allActions.Count; i++)
				{
					actionButtons[i].interactable = CurrentTurnOrder.character.HasEnoughRam(allActions[i].RamCost);
				}
			}
		}

		public void UpdateStatsText(Character character)
		{
			if (currentCharacterStats != character) return;
			statsText.text = $"{character.CharacterSO.CharacterName}\n" +
			                 $"HP: {character.CurrentHealth} / {character.CharacterSO.Health}\n" +
			                 $"ATK: {character.CurrentAttack}\n" +
			                 $"SHD: {character.CurrentShield} / {character.CharacterSO.Shield}\n" +
			                 $"DEF: {character.CurrentDefense}\n" +
			                 $"SPD: {character.CurrentSpeed}\n" +
			                 $"RAM: {character.CurrentRam} / {character.CharacterSO.Ram}";
		}

		public void SetStatsText(Character character)
		{
			if (currentCharacterStats == character) return;
			currentCharacterStats = character;
			UpdateStatsText(character);
		}

		public void SetActivePanel(PanelTypes panelType, bool active)
		{
			if (active && currentPanel == panelType) return;
			if (!active && currentPanel != panelType) return;
			if (active) currentPanel = panelType;
			attackPanel.SetActive(false);
			skillPanel.SetActive(false);
			statsPanel.SetActive(false);
			hackSelectionPanel.SetActive(false);
			hackCyberwarePanel.SetActive(false);
			ActionPanelButtons.ForEach(button => button.interactable = false);
			switch (panelType)
			{
				case PanelTypes.Attack:
					attackPanel.SetActive(active);
					if (!PlayerSelecting)
					{
						attackButton.interactable = !active;
						skillButton.interactable = active;
						hackButton.interactable = active;
					}
					if (!active) SetActivePanel(PanelTypes.Stats, true);
					break;
				case PanelTypes.Skill:
					skillPanel.SetActive(active);
					if (!PlayerSelecting)
					{
						skillButton.interactable = !active;
						attackButton.interactable = active;
						hackButton.interactable = active;
					}
					if (!active) SetActivePanel(PanelTypes.Stats, true);
					break;
				case PanelTypes.Stats:
					statsPanel.SetActive(active);
					if (PlayerSelecting) return;
					attackButton.interactable = active;
					skillButton.interactable = active;
					hackButton.interactable = active;
					break;
				case PanelTypes.HackSelection:
					hackSelectionPanel.SetActive(active);
					hackButton.interactable = !active;
					if (!active) SetActivePanel(PanelTypes.Stats, true);
					break;
				case PanelTypes.HackCyberware:
					hackCyberwarePanel.SetActive(active);
					hackButton.interactable = !active;
					if (!active) SetActivePanel(PanelTypes.Stats, true);
					break;
			}
		}
		
		private void Update()
		{
			UndoHandler();
		}

		/// <summary>
		/// Handle right click to undo various actions such as target selection, panel switching, etc.
		/// </summary>
		private void UndoHandler()
		{
			if (!Input.GetMouseButtonDown(1)) return;
			if (CurrentTurnOrder.character is not Player) return;
			//Undo target selection
			if (PlayerSelecting)
			{
				CurrentTurnOrder.character.CurrentAction.UndoTarget();
				return;
			}
			if (CurrentTurnOrder.character.CurrentAction is HackAction)
			{
				CurrentTurnOrder.character.CurrentAction.Cancel();
				SetActivePanel(currentPanel, false);
				return;
			}
			//Go back to the stats panel
			if (currentPanel is not PanelTypes.Stats)
			{
				SetActivePanel(currentPanel, false);
			}
		}


	}
}