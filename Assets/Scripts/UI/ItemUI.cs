using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.ScriptableObjects.Item;
using UnityEngine;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class ItemUI : MonoBehaviour
	{
		private Image icon;
		
		public void Setup(ItemSO item)
		{
			icon = GetComponent<Image>();
			icon.sprite = item.Icon;
		}
	}
}