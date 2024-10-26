using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Managers;
using Dyscord.ScriptableObjects.Cyberware;
using Dyscord.ScriptableObjects.Item;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class ItemSlotUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text amountText;
		[SerializeField] private CyberwareUI cyberwareUIPrefab;
		[SerializeField] private ItemUI itemUIPrefab;

		private void Awake()
		{
			amountText.text = "";
		}

		public void Setup(ItemList item)
		{
			amountText.text = $"x{item.amount}";
			var itemUI = Instantiate(itemUIPrefab, transform);
			itemUI.Setup(item.item);
		}
		public void Setup(CyberwareSO cyberware)
		{
			amountText.text = "";
			var cyberwareUI = Instantiate(cyberwareUIPrefab, transform);
			cyberwareUI.Setup(cyberware, transform, false);
		}
	}
}