using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.ScriptableObjects.Action;
using Dyscord.ScriptableObjects.Cyberware;
using Gamelogic.Extensions;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.ScriptableObjects
{
	/// <summary>
	/// ScriptableObject for a character.
	/// Contains base stats that shouldn't be modified during runtime.
	/// NOTE: If you want to modify stats during runtime, use Character class instead.
	/// </summary>
	public abstract class CharacterSO : ScriptableObject
	{
		[Header("Base Stats")]
		[SerializeField][ShowAssetPreview(128, 128)] private Sprite characterSprite;
		[SerializeField] private string characterName;
		[SerializeField][Min(1)] private int health;
		[SerializeField][Min(0)] private int attack;
		[SerializeField][Range(0f, 1f)] private float hitRate = 1;
		[SerializeField][Range(0f, 1f)]  private float criticalRate;
		[SerializeField][Min(1)] private float criticalMultiplier = 2;
		[SerializeField][Min(0)] private int defense;
		[SerializeField][Min(0)] private int shield;
		[SerializeField][Range(0f, 1f)]  private float maxShieldReduction;
		[SerializeField][Min(1)] private int speed;
		[SerializeField][Min(1)] private int actionValueNumerator = 10000;
		[SerializeField][Min(0)] private int ram;
		[SerializeField][Min(0)] private int ramRegen;
		
		[Header("Default Actions")]
		[FormerlySerializedAs("attacks")][SerializeField] protected List<CharacterActionSO> defaultAttacks = new List<CharacterActionSO>();
		[FormerlySerializedAs("skills")][SerializeField] protected List<CharacterActionSO> defaultSkills = new List<CharacterActionSO>();
		[SerializeField] protected CharacterActionSO hackAction;

		public Sprite CharacterSprite => characterSprite;
		public string CharacterName => characterName;
		public int Health => health;
		public int Attack => attack;
		public float HitRate => hitRate;
		public float CriticalRate => criticalRate;
		public float CriticalMultiplier => criticalMultiplier;
		public int Defense => defense;
		public int Shield => shield;
		public float MaxShieldReduction => maxShieldReduction;
		public int Speed => speed;
		public int ActionValueNumerator => actionValueNumerator;
		public int Ram => ram;
		public int RamRegen => ramRegen;
		public List<CharacterActionSO> DefaultAttacks => defaultAttacks;
		public List<CharacterActionSO> DefaultSkills => defaultSkills;
		public CharacterActionSO HackAction => hackAction;
	}
}