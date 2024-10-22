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
		[SerializeField] private string id;
		[SerializeField] private CyberwareTypes cyberwareType;
		
		private bool isOccupied;
		
		public string Id => id;
		
		public bool AssignCyberware(CyberwareSO cyberware)
		{
			if (isOccupied)
				return false;
			if (cyberware.CyberwareType != cyberwareType)
				return false;
			InventoryManager.Instance.EquipCyberware(cyberware);
			InventoryManager.Instance.RemoveCyberware(cyberware);
			isOccupied = true;
			return true;
		}
		
		public void RemoveCyberware()
		{
			isOccupied = false;
		}
	}
}