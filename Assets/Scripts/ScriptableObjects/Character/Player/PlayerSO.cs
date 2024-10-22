using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.ScriptableObjects.Player
{
	[CreateAssetMenu(menuName = "Character/PlayerSO", fileName = "PlayerSO")]
	public class PlayerSO : CharacterSO
	{
		[Header("Base Stats")]
		[SerializeField] private int maxPowerCapacity;
		public int MaxPowerCapacity => maxPowerCapacity;
	}
}