using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Characters;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Managers
{
	public class GameManager : MonoSingleton<GameManager>
	{
		[SerializeField] private List<Character> allCharacters;

		protected override void Awake()
		{
			base.Awake();
			foreach (var character in allCharacters)
			{
				character.InitializeCharacter();
			}
		}
	}
}