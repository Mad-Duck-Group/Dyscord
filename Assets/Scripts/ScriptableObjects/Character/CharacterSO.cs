using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
		[SerializeField] private string characterName;
		[SerializeField] private int health;
		[SerializeField] private int attack;
		[SerializeField] private int defense;
		[SerializeField] private int speed;
		[SerializeField] private int actionValueNumerator = 10000;
		[SerializeField] private int ram;
		[SerializeField] private int ramRegen;
		
		public string CharacterName => characterName;
		public int Health => health;
		public int Attack => attack;
		public int Defense => defense;
		public int Speed => speed;
		public int ActionValueNumerator => actionValueNumerator;
		public int Ram => ram;
		public int RamRegen => ramRegen;
	}
}