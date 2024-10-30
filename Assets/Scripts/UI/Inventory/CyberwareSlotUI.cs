using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Managers;
using Dyscord.ScriptableObjects.Cyberware;
using UnityEngine;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class CyberwareSlotUI : MonoBehaviour
	{
		[SerializeField] private bool disabled;
		[SerializeField] private string id;
		[SerializeField] private CyberwareTypes cyberwareType;
		[SerializeField] private Sprite emptySprite;
		[SerializeField] private Sprite occupiedSprite;
		[SerializeField] private Sprite disabledSprite;
		
		private Image image;
		
		private bool isOccupied;
		
		public string Id => id;
		private void Awake()
		{
			image = GetComponent<Image>();
			image.sprite = disabled ? disabledSprite : emptySprite;
			if (isOccupied)
				image.sprite = occupiedSprite;
		}
		
		public bool AssignCyberware(CyberwareSO cyberware)
		{
			if (disabled)
				return false;
			if (isOccupied)
				return false;
			if (cyberware.CyberwareType != cyberwareType)
				return false;
			if (!InventoryManager.Instance.EquipCyberware(cyberware, id)) 
				return false;
			InventoryManager.Instance.RemoveCyberware(cyberware);
			isOccupied = true;
			image.sprite = occupiedSprite;
			return true;
		}
		
		public void ReassignCyberware(CyberwareSO cyberware)
		{
			InventoryManager.Instance.RemoveCyberware(cyberware);
			InventoryManager.Instance.EquipCyberware(cyberware, id);
			isOccupied = true;
			if (!image) image = GetComponent<Image>();
			image.sprite = occupiedSprite;
		}
		
		public void RemoveCyberware()
		{
			isOccupied = false;
			image.sprite = emptySprite;
		}
	}
}