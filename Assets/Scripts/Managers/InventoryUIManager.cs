using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Characters.Player;
using Dyscord.ScriptableObjects;
using Dyscord.ScriptableObjects.Cyberware;
using Dyscord.ScriptableObjects.Overtime;
using Dyscord.UI;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Managers
{
	public enum InventoryPanelTypes
	{
		Stats,
		Skill,
		Inventory
	}
	
	public enum ItemPanelTypes
	{
		Cyberware,
		Item
	}

	public class InventoryUIManager : MonoSingleton<InventoryUIManager>
	{
		[SerializeField] private Player playerCharacter;

		[Header("UI")] 
		[SerializeField] private TMP_Text statsText;
		[SerializeField] private GameObject skillUIPrefab;
		[SerializeField] private ItemSlotUI itemSlotUIPrefab;
		[SerializeField] private CyberwareUI cyberwareUIPrefab;
		[SerializeField] private List<CyberwareSlotUI> cyberwareSlotUIs;

		[Header("Button")] 
		[SerializeField] private Button statsButton;
		[SerializeField] private Button skillButton;
		[SerializeField] private Button inventoryButton;
		[SerializeField] private Button cyberwareButton;
		[SerializeField] private Button itemButton;

		[Header("Scroll Rects")] 
		[SerializeField] private ScrollRect statsPanel;
		[SerializeField] private ScrollRect skillPanel;
		[SerializeField] private ScrollRect inventoryPanel;
		
		private InventoryPanelTypes currentPanel = InventoryPanelTypes.Stats;
		private ItemPanelTypes currentItemPanel = ItemPanelTypes.Cyberware;

		private void OnEnable()
		{
			InventoryManager.OnInventoryUpdated += InventoryUpdateHandler;
			InventoryManager.OnEquippedCyberwareUpdated += EquipCyberwareHandler;
		}
		
		private void OnDisable()
		{
			InventoryManager.OnInventoryUpdated -= InventoryUpdateHandler;
			InventoryManager.OnEquippedCyberwareUpdated -= EquipCyberwareHandler;
		}
		
		private void InventoryUpdateHandler(bool isCyberware)
		{
			if (isCyberware)
			{
				if (currentPanel == InventoryPanelTypes.Inventory)
				{
					UpdateCyberware();
				}
			}
			else
			{
				if (currentPanel == InventoryPanelTypes.Inventory)
				{
					UpdateItems();
				}
			}
		}
		
		private void EquipCyberwareHandler()
		{
			playerCharacter.ReequipCyberware(InventoryManager.Instance.EquippedCyberware);
			UpdateStats();
			UpdateSkills();
		}
		
		private void Start()
		{
			Initialize();
		}

		private void Initialize()
		{
			statsButton.onClick.AddListener(() => ChangePanel(InventoryPanelTypes.Stats));
			skillButton.onClick.AddListener(() => ChangePanel(InventoryPanelTypes.Skill));
			inventoryButton.onClick.AddListener(() => ChangePanel(InventoryPanelTypes.Inventory));
			cyberwareButton.onClick.AddListener(() => ChangeItemPanel(ItemPanelTypes.Cyberware));
			itemButton.onClick.AddListener(() => ChangeItemPanel(ItemPanelTypes.Item));
			LoadEquippedCyberware();
			playerCharacter.InitializeCharacter(true);
			EquipCyberwareHandler();
			ChangePanel(InventoryPanelTypes.Stats);
		}
		
		private void LoadEquippedCyberware()
		{
			foreach (var pair in InventoryManager.Instance.EquippedCyberwareDictionary)
			{
				CyberwareSO cyberware = pair.Key;
				string id = pair.Value;
				CyberwareSlotUI cyberwareSlot = cyberwareSlotUIs.Find(slot => slot.Id == id);
				if (cyberwareSlot == null)
				{
					Debug.LogWarning("Cyberware slot not found");
					continue;
				}
				var cyberwareUI = Instantiate(cyberwareUIPrefab, cyberwareSlot.transform);
				cyberwareUI.Setup(cyberware, cyberwareSlot.transform, true, cyberwareSlot);
				cyberwareSlot.AssignCyberware(cyberware);
			}
			InventoryManager.Instance.EquippedCyberwareDictionary.Clear();
		}

		private void ChangePanel(InventoryPanelTypes panelTypes)
		{
			statsPanel.gameObject.SetActive(false);
			skillPanel.gameObject.SetActive(false);
			inventoryPanel.gameObject.SetActive(false);
			currentPanel = panelTypes;
			switch (panelTypes)
			{
				case InventoryPanelTypes.Stats:
					statsPanel.gameObject.SetActive(true);
					cyberwareButton.gameObject.SetActive(false);
					itemButton.gameObject.SetActive(false);
					UpdateStats();
					break;
				case InventoryPanelTypes.Skill:
					skillPanel.gameObject.SetActive(true);
					cyberwareButton.gameObject.SetActive(false);
					itemButton.gameObject.SetActive(false);
					UpdateSkills();
					break;
				case InventoryPanelTypes.Inventory:
					inventoryPanel.gameObject.SetActive(true);
					cyberwareButton.gameObject.SetActive(true);
					itemButton.gameObject.SetActive(true);
					ChangeItemPanel(currentItemPanel);
					break;
			}
		}

		private void UpdateStats()
		{
			statsText.text = $"Health: {playerCharacter.CurrentHealth}\n" +
			                 $"Attack: {playerCharacter.CurrentAttack}\n" +
			                 $"Defense: {playerCharacter.CurrentDefense}\n" +
			                 $"Shield: {playerCharacter.CurrentShield}\n" +
			                 $"Speed: {playerCharacter.CurrentSpeed}\n" +
			                 $"Hit Rate: {playerCharacter.CurrentHitRate}\n" +
			                 $"Critical Rate: {playerCharacter.CurrentCriticalRate}\n" +
			                 $"Critical Multiplier: {playerCharacter.CurrentCriticalMultiplier}\n" +
			                 $"Ram: {playerCharacter.CurrentRam}\n" +
			                 $"Ram Regen: {playerCharacter.CurrentRamRegen}";
		}

		private void UpdateSkills()
		{
			foreach (Transform child in skillPanel.content)
			{
				Destroy(child.gameObject);
			}

			foreach (var skill in playerCharacter.AllActions)
			{
				GameObject skillUI = Instantiate(skillUIPrefab, skillPanel.content);
				skillUI.GetComponentInChildren<TMP_Text>().text = skill.ActionName;
			}
		}
		
		private void ChangeItemPanel(ItemPanelTypes itemPanelTypes)
		{
			currentItemPanel = itemPanelTypes;
			switch (itemPanelTypes)
			{
				case ItemPanelTypes.Cyberware:
					UpdateCyberware();
					break;
				case ItemPanelTypes.Item:
					UpdateItems();
					break;
			}
		}
		
		private void UpdateCyberware()
		{
			List<ItemSlotUI> itemSlotUIs = new List<ItemSlotUI>();
			foreach (Transform child in inventoryPanel.content)
			{
				Destroy(child.gameObject);
			}

			for (int i = 0; i < InventoryManager.Instance.MaxCyberwareSlots; i++)
			{
				ItemSlotUI itemSlotUI = Instantiate(itemSlotUIPrefab, inventoryPanel.content);
				itemSlotUIs.Add(itemSlotUI);
			}
			
			for (int i = 0; i < InventoryManager.Instance.CyberwareList.Count; i++)
			{
				itemSlotUIs[i].Setup(InventoryManager.Instance.CyberwareList[i]);
			}
		}
		
		private void UpdateItems()
		{
			List<ItemSlotUI> itemSlotUIs = new List<ItemSlotUI>();
			foreach (Transform child in inventoryPanel.content)
			{
				Destroy(child.gameObject);
			}

			for (int i = 0; i < InventoryManager.Instance.MaxItemSlots; i++)
			{
				ItemSlotUI itemSlotUI = Instantiate(itemSlotUIPrefab, inventoryPanel.content);
				itemSlotUIs.Add(itemSlotUI);
			}
			
			for (int i = 0; i < InventoryManager.Instance.ItemList.Count; i++)
			{
				itemSlotUIs[i].Setup(InventoryManager.Instance.ItemList[i]);
			}
		}
	}
}