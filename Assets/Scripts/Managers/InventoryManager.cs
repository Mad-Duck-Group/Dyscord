using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.ScriptableObjects;
using Dyscord.ScriptableObjects.Cyberware;
using Dyscord.ScriptableObjects.Item;
using Dyscord.ScriptableObjects.Overtime;
using Dyscord.UI;
using NaughtyAttributes;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Managers
{
	[Serializable]
	public class ItemList
	{
		public ItemSO item;
		[Min(0)] public int amount;
	}
	public class InventoryManager : PersistentMonoSingleton<InventoryManager>
	{
		[SerializeField] private int maxCyberwareSlots = 20;
		[SerializeField] private int maxItemSlots = 20;
		[SerializeField] private List<ItemList> itemList = new List<ItemList>();
		[SerializeField] private List<CyberwareSO> cyberwareList = new List<CyberwareSO>();
		[SerializeField][ReadOnly] private List<CyberwareSO> equippedCyberware = new List<CyberwareSO>();
		
		private Dictionary<CyberwareSO, string> equippedCyberwareDictionary = new Dictionary<CyberwareSO, string>();
		public int MaxCyberwareSlots => maxCyberwareSlots;
		public int MaxItemSlots => maxItemSlots;
		public List<ItemList> ItemList => itemList;
		public List<CyberwareSO> CyberwareList => cyberwareList;
		public List<CyberwareSO> EquippedCyberware => equippedCyberware;
		public Dictionary<CyberwareSO, string> EquippedCyberwareDictionary => equippedCyberwareDictionary;
		
		public delegate void InventoryUpdated(bool isCyberware);
		public static InventoryUpdated OnInventoryUpdated;
		
		public delegate void EquippedCyberwareUpdated();
		public static EquippedCyberwareUpdated OnEquippedCyberwareUpdated;
		
		public void AddItem(ItemSO item, int amount)
		{
			if (itemList.Any(i => i.item == item))
			{
				Debug.LogWarning("Item already in inventory");
				return;
			}
			itemList.Add(new ItemList { item = item, amount = amount });
			OnInventoryUpdated?.Invoke(false);
		}
		
		public void RemoveItem(ItemSO item, int amount)
		{
			if (itemList.All(i => i.item != item))
			{
				Debug.LogWarning("Item not in inventory");
				return;
			}
			var itemToRemove = itemList.First(i => i.item == item);
			if (itemToRemove.amount < amount)
			{
				Debug.LogWarning("Not enough items to remove");
				return;
			}
			itemToRemove.amount -= amount;
			if (itemToRemove.amount < 1)
			{
				itemList.Remove(itemToRemove);
			}
			OnInventoryUpdated?.Invoke(false);
		}
		
		public void AddCyberware(CyberwareSO cyberware)
		{
			if (cyberwareList.Contains(cyberware))
			{
				Debug.LogWarning("Cyberware already in inventory");
				return;
			}
			cyberwareList.Add(cyberware);
			OnInventoryUpdated?.Invoke(true);
		}
		
		public void RemoveCyberware(CyberwareSO cyberware)
		{
			if (!cyberwareList.Contains(cyberware))
			{
				Debug.LogWarning("Cyberware not in inventory");
				return;
			}
			cyberwareList.Remove(cyberware);
			OnInventoryUpdated?.Invoke(true);
		}
		
		public void EquipCyberware(CyberwareSO cyberware)
		{
			if (equippedCyberware.Contains(cyberware))
			{
				Debug.LogWarning("Cyberware already equipped");
				return;
			}
			equippedCyberware.Add(cyberware);
			OnEquippedCyberwareUpdated?.Invoke();
		}
		
		public void UnequipCyberware(CyberwareSO cyberware)
		{
			if (!equippedCyberware.Contains(cyberware))
			{
				Debug.LogWarning("Cyberware not equipped");
				return;
			}
			equippedCyberware.Remove(cyberware);
			OnEquippedCyberwareUpdated?.Invoke();
		}

		public void SaveEquippedCyberware(CyberwareSO cyberware, string id)
		{
			equippedCyberwareDictionary.Add(cyberware, id);
		}
		
	

		[Button("Gameplay")]
		private void GoToGameplay()
		{
			SceneManager.LoadScene("Gameplay");
		}
		
		[Button("Go to HQ")]
		private void GoToHQ()
		{
			SceneManager.LoadScene("HQ");
		}
	} 
}