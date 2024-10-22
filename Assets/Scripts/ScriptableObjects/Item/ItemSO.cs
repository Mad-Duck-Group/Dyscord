using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.ScriptableObjects.Overtime;
using SerializeReferenceEditor;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.ScriptableObjects.Item
{
	[CreateAssetMenu(menuName = "Item/ItemSO", fileName = "ItemSO")]
	public class ItemSO : ScriptableObject
	{
		[SerializeField] private Sprite icon;
		[SerializeField] private string itemName;
		[SerializeField] private string description;
		[SerializeReference, SR] List<OvertimeTemplate> overtimeTemplates;
		
		public Sprite Icon => icon;
		public string ItemName => itemName;
		public string Description => description;
		public List<OvertimeTemplate> OvertimeTemplates => overtimeTemplates;
	}
}