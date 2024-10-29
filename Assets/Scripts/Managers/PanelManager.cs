using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dyscord.Characters;
using Dyscord.Characters.Player;
using Dyscord.ScriptableObjects.Action;
using Dyscord.ScriptableObjects.Action.Hack;
using Dyscord.ScriptableObjects.Cyberware;
using Dyscord.ScriptableObjects.Overtime;
using Dyscord.UI;
using Microlight.MicroAudio;
using Microlight.MicroBar;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
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
		HackCyberware,
		Item
	}
	public class PanelManager : MonoSingleton<PanelManager>
	{
		[Header("Raycaster")]
		[SerializeField] private GraphicRaycaster gameplayGraphicRaycaster;
		[SerializeField] private GraphicRaycaster overlayGraphicRaycaster;
		
		[Header("Stats Panel")]
		[SerializeField] private CanvasGroup statsPanel;
		[SerializeField] private Image thumbnail;
		[SerializeField] private TMP_Text nameText;
		[SerializeField] private TMP_Text statsText;
		[SerializeField] private MicroBar healthBar;
		[SerializeField] private MicroBar shieldBar;
		[SerializeField] private GameObject overtimeTemplateUIParent;
		[SerializeField] private OvertimeTemplateUI overtimeTemplateUIPrefab;
		
		[Header("Attack Panel")]
		[SerializeField] private CanvasGroup attackPanel;
		
		[Header("Skill Panel")]
		[SerializeField] private CanvasGroup skillPanel;
		
		[Header("Hack Panel")]
		[SerializeField] private CanvasGroup hackSelectionPanel;
		[SerializeField] private CanvasGroup hackCyberwarePanel;
		
		[Header("Item Panel")]
		[SerializeField] private CanvasGroup itemPanel;
		[SerializeField] private ScrollRect itemScrollRect;
		[SerializeField] private ItemSlotUI itemSlotUIPrefab;
		
		[Header("Pause Panel")]
		[SerializeField] private CanvasGroup pausePanel;
		[SerializeField] private Button pauseButton;
		[SerializeField] private Button resumeButton;
		[SerializeField] private Button quitButton;
		[SerializeField] private Slider masterVolumeSlider;
		[SerializeField] private Button muteButton;
		
		[Header("Win Panel")]
		[SerializeField] private CanvasGroup winPanel;
		[SerializeField] private Button toMainMenuButton;
		
		[Header("Lose Panel")]
		[SerializeField] private CanvasGroup losePanel;
		[SerializeField] private Button toHQButton;

		[Header("RAM Slot")]
		[SerializeField] private Transform ramSlotRack;
		[SerializeField] private Image ramSlotPrefab;
		[SerializeField] private Sprite ramSlotEmptyPrefab;
		[SerializeField] private Sprite ramSlotFilledPrefab;

		[Header("Button Prefabs")]
		[SerializeField] private ActionButtonUI actionButtonPrefab;
		[SerializeField] private HackCyberwareButtonUI cyberwareButtonPrefab;
		
		[Header("Scene Buttons")]
		[SerializeField] private Button attackButton;
		[SerializeField] private Button skillButton;
		[SerializeField] private Button hackButton;
		[SerializeField] private Button itemButton;
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
		private List<Button> ActionPanelButtons => new List<Button> {attackButton, skillButton, hackButton, itemButton};
		private List<CanvasGroup> AllPanels => new List<CanvasGroup>
		{
			statsPanel, attackPanel, skillPanel, hackSelectionPanel, hackCyberwarePanel, itemPanel, pausePanel,
			winPanel, losePanel
		};
		private List<ItemSlotUI> itemSlotUIs = new List<ItemSlotUI>();
		private List<OvertimeTemplateUI> overtimeTemplateUIs = new List<OvertimeTemplateUI>();
		private bool paused;
		private bool mute;
		private float beforeMute;
		private Tween overlayPanelFadeTween;
		private Tween panelTween;

		protected override void Awake()
		{
			base.Awake();
			InitializePanel();
			InitializeActionPanelButton();
			InitializeOverlayPanelButton();
		}
		
		private void InitializePanel()
		{
			AllPanels.ForEach(panel => panel.gameObject.SetActive(panel == statsPanel));
		}
		
		private void InitializeActionPanelButton()
		{
			attackButton.onClick.AddListener(() => SetActivePanel(PanelTypes.Attack, true));
			skillButton.onClick.AddListener(() => SetActivePanel(PanelTypes.Skill, true));
			hackCyberSecurityButton.onClick.AddListener(() =>
			{
				OnHackActionButtonPressed?.Invoke(false);
				TooltipManager.Instance.DestroyTooltip();
			});
			hackCyberwareButton.onClick.AddListener(() => OnHackActionButtonPressed?.Invoke(true));
			itemButton.onClick.AddListener(() => SetActivePanel(PanelTypes.Item, true));
		}
		
		private void InitializeOverlayPanelButton()
		{
			pauseButton.onClick.AddListener(TogglePausePanel);
			resumeButton.onClick.AddListener(TogglePausePanel);
			quitButton.onClick.AddListener(() =>
			{
				gameplayGraphicRaycaster.enabled = false;
				Time.timeScale = 1;
				SceneManagerPersistent.Instance.LoadNextScene(SceneTypes.HQ, LoadSceneMode.Additive, false);
			});
			muteButton.onClick.AddListener(ToggleMute);
			masterVolumeSlider.value = MicroAudio.MasterVolume;
			masterVolumeSlider.onValueChanged.AddListener(_ => OnMasterVolumeChange());
			toMainMenuButton.onClick.AddListener(() =>
			{
				gameplayGraphicRaycaster.enabled = false;
				overlayGraphicRaycaster.enabled = false;
				Time.timeScale = 1;
				ProgressionManager.Instance.ResetProgression();
				SceneManagerPersistent.Instance.LoadNextScene(SceneTypes.MainMenu, LoadSceneMode.Additive, false);
			});
			toHQButton.onClick.AddListener(() =>
			{
				gameplayGraphicRaycaster.enabled = false;
				overlayGraphicRaycaster.enabled = false;
				Time.timeScale = 1;
				SceneManagerPersistent.Instance.LoadNextScene(SceneTypes.HQ, LoadSceneMode.Additive, false);
			});
		}

		

		private void OnInventoryUpdated(bool _) => PopulateItemPanel();

		private void OnEnable()
		{
			
			InventoryManager.OnInventoryUpdated += OnInventoryUpdated;
		}
		
		private void OnDisable()
		{
			InventoryManager.OnInventoryUpdated -= OnInventoryUpdated;
		}

		public void InitializeStatsPanel()
		{
			healthBar.Initialize(PlayerInstance.CharacterSO.Health);
			shieldBar.Initialize(PlayerInstance.CharacterSO.Shield);
			SetStatsText(PlayerInstance);
			UpdateRamSlotUI(PlayerInstance);
		}
		
		/// <summary>
		/// Initializes the player UI by instantiating the action buttons.
		/// </summary>
		public void InitializeActionUI()
		{
			PlayerInstance.AllAttacks.ForEach(attack =>
			{
				var attackButtonInstance = Instantiate(actionButtonPrefab, attackPanel.transform).Setup(attack);
				attackButtonInstance.onClick.AddListener(() =>
				{
					attack.SelectTarget();
					TooltipManager.Instance.DestroyTooltip();
				});
				actionButtons.Add(attackButtonInstance);
			});
			PlayerInstance.AllSkills.ForEach(skill =>
			{
				var attackButtonInstance = Instantiate(actionButtonPrefab, skillPanel.transform).Setup(skill);
				attackButtonInstance.onClick.AddListener(() =>
				{
					skill.SelectTarget();
					TooltipManager.Instance.DestroyTooltip();
				});
				actionButtons.Add(attackButtonInstance);
			});
			hackButton.onClick.AddListener(() =>
			{
				PlayerInstance.HackAction.SelectTarget();
				TooltipManager.Instance.DestroyTooltip();
				ActionPanelButtons.ForEach(button => SetButtonInteractable(button, false));
			});
			actionButtons.Add(hackButton);
			var hackCyberwareSecurityButtonUI = hackCyberSecurityButton.GetComponent<ActionButtonUI>();
			hackCyberwareSecurityButtonUI.Setup(PlayerInstance.HackAction);
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
				Button cyberwareButtonInstance =
					Instantiate(cyberwareButtonPrefab, hackCyberwarePanel.transform).Setup(cyberware);
				cyberwareButtonInstance.onClick.AddListener(() => OnCyberwareButtonPressed?.Invoke(cyberware));
				cyberwareButtonInstance.GetComponentInChildren<TMP_Text>().text = cyberware.CyberwareName;
				cyberwareButtonInstance.interactable = cyberware.HackAccessLevel <= hackAccessLevel;
				cyberwareButtons.Add(cyberwareButtonInstance);
			}
		}

		public void PopulateItemPanel()
		{
			foreach (var itemSlotUI in itemSlotUIs)
			{
				Destroy(itemSlotUI.gameObject);
			}
			itemSlotUIs.Clear();
			for (int i = 0; i < InventoryManager.Instance.MaxItemSlots; i++)
			{
				ItemSlotUI itemSlotUI = Instantiate(itemSlotUIPrefab, itemScrollRect.content);
				itemSlotUIs.Add(itemSlotUI);
			}
			for (int i = 0; i < InventoryManager.Instance.ItemList.Count; i++)
			{
				itemSlotUIs[i].Setup(InventoryManager.Instance.ItemList[i]);
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
			if (CurrentTurnOrder == null)
			{
				actionButtons.ForEach(button => button.interactable = false);
				ActionPanelButtons.ForEach(button => button.interactable = false);
				return;
			}
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
					if (button == itemButton && currentPanel == PanelTypes.Item) return;
					button.interactable = true;
				});
				List<CharacterActionSO> allActions = CurrentTurnOrder.character.AllActions;
				for (int i = 0; i < allActions.Count; i++)
				{
					actionButtons[i].interactable = CurrentTurnOrder.character.HasEnoughRam(allActions[i].RamCost);
				}
			}
		}

		public void UpdateStatsText(Character character, bool playAnimation = true)
		{
			if (currentCharacterStats != character) return;
			thumbnail.sprite = character.CharacterSO.CharacterThumbnail;
			nameText.text = character.CharacterSO.CharacterName;
			statsText.text = $"ATK: {character.CurrentAttack}\n" +
			                 $"DEF: {character.CurrentDefense}\n" +
			                 $"SPD: {character.CurrentSpeed}\n";
			healthBar.SetNewMaxHP(character.CharacterSO.Health, true);
			healthBar.UpdateBar(character.CurrentHealth, !playAnimation, UpdateAnim.Damage);
			shieldBar.SetNewMaxHP(character.CharacterSO.Shield, true);
			shieldBar.UpdateBar(character.CurrentShield, !playAnimation, UpdateAnim.Damage);
			PopulateOvertimeTemplateUI(character);
		}

		public void SetStatsText(Character character, bool playAnimation = false)
		{
			if (currentCharacterStats == character) return;
			currentCharacterStats = character;
			UpdateStatsText(character, playAnimation);
		}

		private void PopulateOvertimeTemplateUI(Character character)
		{
			foreach (var overtimeTemplateUI in overtimeTemplateUIs)
			{
				Destroy(overtimeTemplateUI.gameObject);
			}
			overtimeTemplateUIs.Clear();
			List<OvertimeTemplate> availableOvertimes = character.CurrentOvertimes.Where(x => x.Infinite || x.RemainingTurns > 0).ToList();
			foreach (var overtime in availableOvertimes)
			{
				OvertimeTemplateUI overtimeTemplateUI = Instantiate(overtimeTemplateUIPrefab, overtimeTemplateUIParent.transform);
				overtimeTemplateUI.Setup(overtime);
				overtimeTemplateUIs.Add(overtimeTemplateUI);
			}
			TooltipManager.Instance.DestroyTooltip();
		}
		
		public void UpdateRamSlotUI(Character character)
		{
			if (character is not Player) return;
			foreach (Transform child in ramSlotRack)
			{
				Destroy(child.gameObject);
			}
			for (int i = 0; i < character.CharacterSO.Ram; i++)
			{
				Image ramSlot = Instantiate(ramSlotPrefab, ramSlotRack);
				ramSlot.sprite = i < character.CurrentRam ? ramSlotFilledPrefab : ramSlotEmptyPrefab;
			}
		}

		public void SetActivePanel(PanelTypes panelType, bool active)
		{
			if (active && currentPanel == panelType) return;
			//if (!active && currentPanel != panelType) return;
			if (active && currentPanel != panelType)
			{
				Debug.Log("Fade out Previous Panel");
				FadePanel(currentPanel, false);
			}
			if (active)
			{
				currentPanel = panelType;
				FadePanel(currentPanel, true);
			}
			else
			{
				FadePanel(currentPanel, false);
			}
				
			TooltipManager.Instance.DestroyTooltip();
			ActionPanelButtons.ForEach(button => SetButtonInteractable(button, false));
			switch (panelType)
			{
				case PanelTypes.Attack:
					if (!PlayerSelecting)
					{
						ActionPanelButtons.ForEach(button => SetButtonInteractable(button, button != attackButton));
					}
					if (!active) SetActivePanel(PanelTypes.Stats, true);
					break;
				case PanelTypes.Skill:
					if (!PlayerSelecting)
					{
						ActionPanelButtons.ForEach(button => SetButtonInteractable(button, button != skillButton));
					}
					if (!active) SetActivePanel(PanelTypes.Stats, true);
					break;
				case PanelTypes.Stats:
					if (PlayerSelecting) return;
					ActionPanelButtons.ForEach(button => SetButtonInteractable(button, active));
					break;
				case PanelTypes.HackSelection:
					SetButtonInteractable(hackButton, !active);
					if (!active) SetActivePanel(PanelTypes.Stats, true);
					break;
				case PanelTypes.HackCyberware:
					SetButtonInteractable(hackButton, !active);
					if (!active) SetActivePanel(PanelTypes.Stats, true);
					break;
				case PanelTypes.Item:
					SetButtonInteractable(itemButton, !active);
					PopulateItemPanel();
					if (!PlayerSelecting)
					{
						ActionPanelButtons
							.Except(new List<Button> {itemButton}).ToList()
							.ForEach(button => SetButtonInteractable(button, active));
					}
					if (!active) SetActivePanel(PanelTypes.Stats, true);
					break;
			}
		}
		
		private void FadePanel(PanelTypes panelType, bool active)
		{
			//if (panelTween.IsActive()) panelTween.Kill();
			Dictionary<PanelTypes, CanvasGroup> panelDictionary = new Dictionary<PanelTypes, CanvasGroup>()
			{
				{PanelTypes.Stats, statsPanel},
				{PanelTypes.Attack, attackPanel},
				{PanelTypes.Skill, skillPanel},
				{PanelTypes.HackSelection, hackSelectionPanel},
				{PanelTypes.HackCyberware, hackCyberwarePanel},
				{PanelTypes.Item, itemPanel}
			};
			panelDictionary[panelType].gameObject.SetActive(true);
			panelDictionary[panelType].blocksRaycasts = active;
			panelDictionary[panelType].alpha = active ? 0 : 1;
			float targetAlpha = active ? 1 : 0;
			panelTween = panelDictionary[panelType].DOFade(targetAlpha, 0.1f)
				.OnComplete(() => panelDictionary[panelType].gameObject.SetActive(active));
		}

		private void SetButtonInteractable(Button button, bool interactable)
		{
			DOVirtual.DelayedCall(0.1f, () => button.interactable = interactable);
			//button.image.sprite = interactable ? button.spriteState.pressedSprite : button.spriteState.disabledSprite;
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
			if (paused)
			{
				TogglePausePanel();
				return;
			}
			if (currentCharacterStats != PlayerInstance)
			{
				TooltipManager.Instance.DestroyTooltip();
				SetStatsText(PlayerInstance);
			}
			if (CurrentTurnOrder?.character is not Player) return;
			//Undo target selection
			if (PlayerSelecting)
			{
				CurrentTurnOrder.character.CurrentAction.UndoTarget();
				return;
			}
			if (CurrentTurnOrder.character.CurrentAction is HackAction)
			{
				CurrentTurnOrder.character.CurrentAction.Cancel();
				TooltipManager.Instance.DestroyTooltip();
				SetActivePanel(currentPanel, false);
				return;
			}
			//Go back to the stats panel
			if (currentPanel is not PanelTypes.Stats)
			{
				TooltipManager.Instance.DestroyTooltip();
				SetActivePanel(currentPanel, false);
			}
		}
		
		private void TogglePausePanel()
		{
			if (overlayPanelFadeTween.IsActive()) return;
			if (paused)
			{
				paused = false;
				Time.timeScale = 1;
				overlayPanelFadeTween = pausePanel.DOFade(0, 0.2f).OnComplete(() => pausePanel.gameObject.SetActive(false));
			}
			else
			{
				paused = true;
				Time.timeScale = 0;
				pausePanel.gameObject.SetActive(true);
				pausePanel.alpha = 0;
				overlayPanelFadeTween = pausePanel.DOFade(1, 0.2f).SetUpdate(true);
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

		public void GameOver()
		{
			gameplayGraphicRaycaster.enabled = false;
		}
		
		public void ShowWinPanel()
		{
			Time.timeScale = 0;
			winPanel.gameObject.SetActive(true);
			winPanel.alpha = 0;
			overlayPanelFadeTween = winPanel.DOFade(1, 0.2f).SetUpdate(true);
		}
		
		public void ShowLosePanel()
		{
			Time.timeScale = 0;
			losePanel.gameObject.SetActive(true);
			losePanel.alpha = 0;
			overlayPanelFadeTween = losePanel.DOFade(1, 0.2f).SetUpdate(true);
		}
	}
}