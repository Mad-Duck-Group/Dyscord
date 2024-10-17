using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dyscord.Managers;
using Dyscord.ScriptableObjects;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Characters
{
	public class Character : MonoBehaviour
	{
		[SerializeField][Expandable] private CharacterSO characterSO;
		[SerializeField] private TMP_Text infoText;
		[SerializeField][ReadOnly] private int currentHealth;
		[SerializeField][ReadOnly] private int currentAttack;
		[SerializeField][ReadOnly] private int currentDefense;
		[SerializeField][ReadOnly] private int currentSpeed;
		
		public delegate void CharacterDeath(Character character);
		public event CharacterDeath OnCharacterDeath;
		
		public int CurrentSpeed => currentSpeed;
		public CharacterSO CharacterSO => characterSO;

		private void Awake()
		{
			Initialize();
		}

		protected virtual void Initialize()
		{
			currentHealth = characterSO.Health;
			currentAttack = characterSO.Attack;
			currentDefense = characterSO.Defense;
			currentSpeed = characterSO.Speed;
			UpdateInfoText();
		}
		
		public virtual void Attack(Character target)
		{
			infoText.DOColor(Color.green, 0.2f).SetLoops(2, LoopType.Yoyo);
			target.TakeDamage(currentAttack);
		}
		
		public virtual void Attack(List<Character> targets)
		{
			infoText.DOColor(Color.green, 0.2f).SetLoops(2, LoopType.Yoyo);
			targets.ForEach(target => target.TakeDamage(currentAttack));
		}

		public virtual void TakeDamage(int damage)
		{
			int damageTaken = damage - currentDefense;
			currentHealth -= damageTaken;
			currentHealth = Mathf.Clamp(currentHealth, 0, characterSO.Health);
			if (currentHealth == 0)
			{
				OnCharacterDeath?.Invoke(this);
			}

			infoText.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo);
			UpdateInfoText();
		}

		public virtual TurnOrder GetRawTurnOrder()
		{
			int actionValue = Mathf.RoundToInt(characterSO.ActionValueNumerator / (float)currentSpeed);
			Debug.Log($"{characterSO.CharacterName} action value: {actionValue}");
			return new TurnOrder
			{
				actionValue = actionValue,
				character = this
			};
		}
		
		private void UpdateInfoText()
		{
			infoText.text = $"{characterSO.CharacterName}\n" +
			                $"HP: {currentHealth}\n" +
			                $"ATK: {currentAttack}\n" +
			                $"DEF: {currentDefense}\n" +
			                $"SPD: {currentSpeed}";
		}
	}
}