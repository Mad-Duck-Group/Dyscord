using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dyscord.Managers;
using Dyscord.ScriptableObjects;
using Dyscord.ScriptableObjects.Action;
using Dyscord.ScriptableObjects.Action.Attack;
using NaughtyAttributes;
using Redcode.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Characters
{
	public abstract class Character : MonoBehaviour
	{
		[Header("Character Configs")]	
		[SerializeField][Expandable] protected CharacterSO characterSO;
		
		[Header("Actions")]	
		[SerializeField] protected CharacterActionSO basicAttack;
		[SerializeField] protected List<CharacterActionSO> skills = new List<CharacterActionSO>();
		
		[Header("UI")]	
		[SerializeField] protected TMP_Text infoText;
		
		[Header("Debug Info")]	
		[SerializeField][NaughtyAttributes.ReadOnly] protected int currentHealth;
		[SerializeField][NaughtyAttributes.ReadOnly] protected int currentAttack;
		[SerializeField][NaughtyAttributes.ReadOnly] protected int currentDefense;
		[SerializeField][NaughtyAttributes.ReadOnly] protected int currentSpeed;
		[SerializeField][NaughtyAttributes.ReadOnly] protected int currentRam;
		[SerializeField][NaughtyAttributes.ReadOnly] protected int currentRamRegen;
		[SerializeField][NaughtyAttributes.ReadOnly] protected CharacterActionSO currentAction;
		
		private bool _firstTurn = true;
		
		public virtual int SelectionCount { get; set; }

		public CharacterActionSO CurrentAction
		{
			get => currentAction;
			set => currentAction = value;
		}

		public delegate void CharacterDeath(Character character);
		public event CharacterDeath OnCharacterDeath;
		
		public int CurrentSpeed => currentSpeed;
		public CharacterSO CharacterSO => characterSO;
		public CharacterActionSO BasicAttack => basicAttack;
		public List<CharacterActionSO> Skills => skills;
		
		public List<CharacterActionSO> AllActions => new List<CharacterActionSO> {basicAttack}.Concat(skills).ToList();

		private void Awake()
		{
			InitializeStats();
			InitializeAction();
		}

		protected virtual void InitializeStats()
		{
			currentHealth = characterSO.Health;
			currentAttack = characterSO.Attack;
			currentDefense = characterSO.Defense;
			currentSpeed = characterSO.Speed;
			currentRam = characterSO.Ram;
			currentRamRegen = characterSO.RamRegen;
			UpdateInfoText();
		}
		
		protected virtual void InitializeAction()
		{
			basicAttack = Instantiate(basicAttack);
			basicAttack.Initialize(this);
			skills = skills.Select(skill => Instantiate(skill)).ToList();
			skills.ForEach(skill => skill.Initialize(this));
		}

		public virtual void AIPlay()
		{
			List<CharacterActionSO> availableActions = AllActions
				.Where(action => HasEnoughRam(action.RamCost)).ToList();
			CharacterActionSO selectedAction = availableActions.GetRandomElement();
			if (selectedAction)
				selectedAction.SelectTarget();
			else
			{
				TurnManager.Instance.NextTurn();
			}
		}
		
		// public virtual void Attack(Character target)
		// {
		// 	infoText.DOColor(Color.green, 0.2f).SetLoops(2, LoopType.Yoyo);
		// 	target.TakeDamage(currentAttack);
		// }
		
		public virtual void Attack(AttackAction attackAction)
		{
			infoText.DOColor(Color.green, 0.2f).SetLoops(2, LoopType.Yoyo);
			int rawDamage = 0;
			int finalDamage = 0;
			switch (attackAction.TargetType)
			{
				case TargetTypes.Single:
				case TargetTypes.All:
					rawDamage = Mathf.RoundToInt(currentAttack * attackAction.DamageMultiplier);
					finalDamage = rawDamage - currentDefense;
					attackAction.Targets.ForEach(target => target.TakeDamage(finalDamage));
					break;
				case TargetTypes.Burst:
					rawDamage = Mathf.RoundToInt(currentAttack * attackAction.DamageMultiplier);
					for (int i = 0; i < attackAction.Targets.Count; i++)
					{
						int burstDamage = Mathf.RoundToInt(rawDamage * (i == attackAction.Targets.Count - 1 ? 1 : 1 - attackAction.AdjacentPenalty));
						finalDamage = burstDamage - currentDefense;
						attackAction.Targets[i].TakeDamage(finalDamage);
					}
					break;
				case TargetTypes.Multi:
					rawDamage = Mathf.RoundToInt(currentAttack * attackAction.DamageMultiplier);
					var targetFrequency = attackAction.Targets
						.GroupBy(target => target)
						.ToDictionary(group => group.Key, group => group.Count());
					foreach (var target in targetFrequency)
					{
						float multiDamage = rawDamage * (target.Value == 1 ? 1 : 1 - attackAction.SpamPenalty * (target.Value - 1));
						finalDamage = Mathf.FloorToInt((multiDamage - currentDefense) / target.Value);
						for (int i = 0; i < target.Value; i++)
							target.Key.TakeDamage(finalDamage);
					}
					break;
			}
		}

		public virtual void TakeDamage(int damage)
		{
			currentHealth -= damage;
			currentHealth = Mathf.Clamp(currentHealth, 0, characterSO.Health);
			if (currentHealth == 0)
			{
				OnCharacterDeath?.Invoke(this);
			}
			infoText.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo);
			DynamicTextManager.CreateText2D(transform.position, damage.ToString(), DynamicTextManager.damageData);
			UpdateInfoText();
		}
		
		public virtual void RegenRam()
		{
			if (_firstTurn)
			{
				_firstTurn = false;
				return;
			}
			ChangeRam(currentRamRegen);
		}
		
		public virtual void ChangeRam(int amount)
		{
			currentRam += amount;
			if (amount != 0)
				DynamicTextManager.CreateText2D(transform.position, amount.ToString(), DynamicTextManager.ramData);
			currentRam = Mathf.Clamp(currentRam, 0, characterSO.Ram);
			TurnManager.Instance.UpdateButtonUI();
			UpdateInfoText();
		}
		
		public virtual bool HasEnoughRam(int amount)
		{
			return currentRam >= amount;
		}

		public virtual TurnOrder GetRawTurnOrder()
		{
			int actionValue = Mathf.RoundToInt(characterSO.ActionValueNumerator / (float)currentSpeed);
			return new TurnOrder
			{
				actionValue = actionValue,
				character = this
			};
		}
		
		protected void UpdateInfoText()
		{
			infoText.text = $"{characterSO.CharacterName}\n" +
			                $"HP: {currentHealth}\n" +
			                $"ATK: {currentAttack}\n" +
			                $"DEF: {currentDefense}\n" +
			                $"SPD: {currentSpeed}\n" +
			                $"RAM: {currentRam}";
		}
	}
}