using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.ScriptableObjects
{
	//[CreateAssetMenu(menuName = "Configs/CharacterSO", fileName = "CharacterSO")]
	public abstract class CharacterSO : ScriptableObject
	{
		[SerializeField] private string characterName;
		[SerializeField] private int health;
		[SerializeField] private int attack;
		[SerializeField] private int defense;
		[SerializeField] private int speed;
		[SerializeField] private int actionValueNumerator = 10000;
		
		public string CharacterName => characterName;
		public int Health => health;
		public int Attack => attack;
		public int Defense => defense;
		public int Speed => speed;
		public int ActionValueNumerator => actionValueNumerator;
	}
}