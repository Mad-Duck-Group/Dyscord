using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.ScriptableObjects.Action;
using Dyscord.ScriptableObjects.Overtime;
using NaughtyAttributes;
using SerializeReferenceEditor;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.ScriptableObjects.Cyberware
{
	public enum CyberwareTypes
	{
		Chip,
		Eye,
		Arm
	}
	[CreateAssetMenu(menuName = "Cyberware/CyberwareSO", fileName = "CyberwareSO")]
	public class CyberwareSO : ScriptableObject
	{
		[SerializeField] protected CyberwareTypes cyberwareType;
		[SerializeField][ShowAssetPreview(128, 128)] protected Sprite icon;
		[SerializeField] protected string cyberwareName;
		[SerializeField][TextArea] protected string description;
		[SerializeField][HideIf(nameof(cyberwareType), CyberwareTypes.Chip)] protected int powerCost;
		[SerializeField][Min(1)] protected int hackAccessLevel = 1;
		[SerializeField][ShowIf(nameof(cyberwareType), CyberwareTypes.Chip)] 
		protected float hackDamageModifier = 1f;
		[SerializeField][HideIf(nameof(cyberwareType), CyberwareTypes.Chip)]  
		protected List<CharacterActionSO> attacks;
		[SerializeField][HideIf(nameof(cyberwareType), CyberwareTypes.Chip)]  
		protected List<CharacterActionSO> skills;
		[SerializeReference, SR][HideIf(nameof(cyberwareType), CyberwareTypes.Chip)]  
		protected List<OvertimeTemplate> overtimeTemplates;
		[SerializeReference, SR][HideIf(nameof(cyberwareType), CyberwareTypes.Chip)]  
		protected List<OvertimeTemplate> hackedOvertimeTemplates;

		public Sprite Icon => icon;
		public string CyberwareName => cyberwareName;
		public string Description => description;
		public int PowerCost => powerCost;
		public int HackAccessLevel => hackAccessLevel;
		public CyberwareTypes CyberwareType => cyberwareType;
		public float HackDamageModifier => hackDamageModifier;
		public List<CharacterActionSO> Attacks => attacks;
		public List<CharacterActionSO> Skills => skills;
		public List<CharacterActionSO> AllActions => attacks.Concat(skills).ToList();
		public List<OvertimeTemplate> OvertimeTemplates => overtimeTemplates;
		public List<OvertimeTemplate> HackedOvertimeTemplates => hackedOvertimeTemplates;
	}
}