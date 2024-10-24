using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dyscord.Managers;
using Dyscord.ScriptableObjects;
using Dyscord.ScriptableObjects.Action;
using Dyscord.ScriptableObjects.Action.Attack;
using Dyscord.ScriptableObjects.Action.Hack;
using Dyscord.ScriptableObjects.Cyberware;
using Dyscord.ScriptableObjects.Overtime;
using JetBrains.Annotations;
using NaughtyAttributes;
using Redcode.Extensions;
using SerializeReferenceEditor;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Characters
{
	/// <summary>
	/// Runtime character class that holds character stats and actions.
	/// </summary>
	public abstract class Character : MonoBehaviour
	{
		[Header("Character Configs")]	
		[SerializeField][Expandable] protected CharacterSO characterSO;
		[SerializeReference, SR] protected List<OvertimeTemplate> shieldBreakOvertimes;
		
		[Header("Cyberware")]	
		[SerializeField] protected CyberwareSO hackAccessChip;
		[SerializeField] protected List<CyberwareSO> cyberwares = new List<CyberwareSO>();

		[Header("UI")]	
		[SerializeField] protected TMP_Text infoText;
		
		[Header("Debug Info")]	
		[SerializeField][NaughtyAttributes.ReadOnly] protected int currentHealth;
		[SerializeField][NaughtyAttributes.ReadOnly] protected int currentAttack;
		[SerializeField][NaughtyAttributes.ReadOnly] protected float currentHitRate;
		[SerializeField][NaughtyAttributes.ReadOnly] protected float currentCriticalRate;
		[SerializeField][NaughtyAttributes.ReadOnly] protected float currentCriticalMultiplier;
		[SerializeField][NaughtyAttributes.ReadOnly] protected int currentDefense;
		[SerializeField][NaughtyAttributes.ReadOnly] protected int currentShield; 
		[SerializeField][NaughtyAttributes.ReadOnly] protected int currentSpeed;
		[SerializeField][NaughtyAttributes.ReadOnly] protected int currentRam;
		[SerializeField][NaughtyAttributes.ReadOnly] protected int currentRamRegen;
		[SerializeField][NaughtyAttributes.ReadOnly] protected CharacterActionSO currentAction;
			
		protected List<CharacterActionSO> defaultAttacks = new List<CharacterActionSO>();
		protected List<CharacterActionSO> defaultSkills = new List<CharacterActionSO>();
		protected CharacterActionSO hackAction;

		protected List<CharacterActionSO> cyberwareAttacks = new List<CharacterActionSO>();
		protected List<CharacterActionSO> cyberwareSkills = new List<CharacterActionSO>();
		protected List<OvertimeTemplate> currentOvertimes = new List<OvertimeTemplate>();
		protected List<OvertimeTemplate> overtimeToRemove = new List<OvertimeTemplate>();
		protected List<KeyValuePair<OvertimeTemplate, OvertimeEffect>> overtimeEffects = new List<KeyValuePair<OvertimeTemplate, OvertimeEffect>>();
		
		protected bool _firstTurn = true;
		protected bool _fromInventory;
		
		public virtual int SelectionCount { get; set; }

		public CharacterActionSO CurrentAction
		{
			get => currentAction;
			set => currentAction = value;
		}

		public delegate void CharacterDeath(Character character);
		public event CharacterDeath OnCharacterDeath;
		
		public int CurrentHealth => currentHealth;
		public int CurrentAttack => currentAttack;
		public float CurrentHitRate => currentHitRate;
		public float CurrentCriticalRate => currentCriticalRate;
		public float CurrentCriticalMultiplier => currentCriticalMultiplier;
		public int CurrentDefense => currentDefense;
		public int CurrentShield => currentShield;
		public int CurrentSpeed => currentSpeed;
		public int CurrentRam => currentRam;
		public int CurrentRamRegen => currentRamRegen;
		public List<OvertimeTemplate> CurrentOvertimes => currentOvertimes;
		public CharacterSO CharacterSO => characterSO;
		public CyberwareSO HackAccessChip => hackAccessChip;
		public List<CyberwareSO> Cyberwares => cyberwares;
		public List<CharacterActionSO> DefaultAttacks => defaultAttacks;
		public List<CharacterActionSO> DefaultSkills => defaultSkills;
		public List<CharacterActionSO> CyberwareAttacks => cyberwareAttacks;
		public List<CharacterActionSO> CyberwareSkills => cyberwareSkills;
		
		public List<CharacterActionSO> AllAttacks => new List<CharacterActionSO>()
			.Concat(defaultAttacks)
			.Concat(cyberwareAttacks)
			.ToList();
		
		public List<CharacterActionSO> AllSkills => new List<CharacterActionSO>()
			.Concat(defaultSkills)
			.Concat(cyberwareSkills)
			.ToList();
		
		public CharacterActionSO HackAction => hackAction;

		public List<CharacterActionSO> AllActions => new List<CharacterActionSO>()
			.Concat(AllAttacks)
			.Concat(AllSkills)
			.Concat(new []{HackAction})
			.ToList();


		public void InitializeCharacter(bool fromInventory = false)
		{
			_fromInventory = fromInventory;
			InitializeStats();
			InitializeDefaultAction();
			InitializeCyberware();
		}

		/// <summary>
		/// Sets the character stats to the values defined in the CharacterSO.
		/// </summary>
		protected virtual void InitializeStats()
		{
			currentHealth = characterSO.Health;
			currentAttack = characterSO.Attack;
			currentHitRate = characterSO.HitRate;
			currentCriticalRate = characterSO.CriticalRate;
			currentCriticalMultiplier = characterSO.CriticalMultiplier;
			currentDefense = characterSO.Defense;
			currentShield = characterSO.Shield;
			currentSpeed = characterSO.Speed;
			currentRam = characterSO.Ram;
			currentRamRegen = characterSO.RamRegen;
			UpdateInfoText();
		}
		
		/// <summary>
		/// Clone the actions and initialize them.
		/// </summary>
		protected virtual void InitializeDefaultAction()
		{
			defaultAttacks = characterSO.DefaultAttacks.Select(attack => Instantiate(attack)).ToList();
			defaultAttacks.ForEach(attack => attack.Initialize(this));
			defaultSkills = characterSO.DefaultSkills.Select(skill => Instantiate(skill)).ToList();
			defaultSkills.ForEach(skill => skill.Initialize(this));
	
		}
		
		protected virtual void InitializeCyberware()
		{
			cyberwares = cyberwares.Select(cyberware => Instantiate(cyberware)).ToList();
			cyberwareAttacks.Clear();
			cyberwareSkills.Clear();
			cyberwares.ForEach(cyberware =>
	        {
	            cyberwareAttacks.AddRange(cyberware.Attacks.Select(attack => Instantiate(attack)));
	            cyberwareSkills.AddRange(cyberware.Skills.Select(skill => Instantiate(skill)));
	        });
	        cyberwareAttacks.ForEach(attack => attack.Initialize(this));
	        cyberwareSkills.ForEach(skill => skill.Initialize(this));
	        hackAccessChip = Instantiate(hackAccessChip);
	        hackAction = Instantiate(characterSO.HackAction);
	        hackAction.Initialize(this);
	        cyberwares.ForEach(x => AddOvertime(x.OvertimeTemplates));
		}

		public virtual void ReequipCyberware(List<CyberwareSO> equip)
		{
			cyberwares.ForEach(x => RemoveOvertime(x.OvertimeTemplates));
			cyberwares = equip;
			InitializeCyberware();
		}

		/// <summary>
		/// AI plays a random action that the character has enough RAM for.
		/// </summary>
		public virtual void AIPlay()
		{
			List<CharacterActionSO> availableActions = AllActions
				.Where(action =>
				{
					bool hasEnoughRam = HasEnoughRam(action.RamCost);
					if (action is HackAction)
						return hasEnoughRam && TurnManager.Instance.PlayerInstance.CanHack(hackAccessChip.HackAccessLevel);
					return hasEnoughRam;
				}).ToList();
			CharacterActionSO selectedAction = availableActions.GetRandomElement();
			if (selectedAction)
				selectedAction.SelectTarget();
			else
			{
				TurnManager.Instance.NextTurn();
			}
		}

		/// <summary>
		/// Attack the targets
		/// </summary>
		/// <param name="attackAction">The attack action to be used</param>
		public virtual void Attack(AttackAction attackAction)
		{
			infoText.DOColor(Color.green, 0.2f).SetLoops(2, LoopType.Yoyo);
    
			// Call the combined damage calculation method
			List<int> rawDamage = CalculateRawDamage(attackAction);
			List<int> damageFromType = CalculateDamageType(attackAction, rawDamage);

			// Apply the calculated damage to each target
			for (int i = 0; i < attackAction.Targets.Count; i++)
			{
				int finalDamage = attackAction.Targets[i].ApplyDamageReduction(damageFromType[i]);
				attackAction.Targets[i].ChangeHealth(-finalDamage);
			}
		}
		
		/// <summary>
		/// Calculate the raw damage and apply the critical multiplier if the attack is critical.
		/// </summary>
		/// <param name="attackAction">The attack action</param>
		/// <returns>Raw damage value</returns>
		protected virtual List<int> CalculateRawDamage(AttackAction attackAction)
		{
			List<int> rawDamages = new List<int>();
			foreach (var target in attackAction.Targets)
			{
				bool hit = currentHitRate > 0f && UnityRandom.Range(0f, 1f) <= currentHitRate;
				if (!hit)
				{
					DynamicTextManager.CreateText2D(target.transform.position, "Miss", DynamicTextManager.damageData);
					rawDamages.Add(0);
					continue;
				}
				int rawDamage = Mathf.RoundToInt(currentAttack * attackAction.DamageMultiplier);
				bool crit = currentCriticalRate > 0f && UnityRandom.Range(0f, 1f) <= currentCriticalRate;
				if (crit)
				{
					rawDamage = Mathf.RoundToInt(rawDamage * currentCriticalMultiplier);
					DynamicTextManager.CreateText2D(target.transform.position, "Crit", DynamicTextManager.damageData);
				}
				rawDamages.Add(rawDamage);
			}
			return rawDamages;
		}
		
		/// <summary>
		/// Calculate the damage type based on the attack action.
		/// </summary>
		/// <param name="attackAction">Attack action</param>
		/// <param name="rawDamage">Raw damage value</param>
		/// <returns>List of damage values for each target</returns>
		protected virtual List<int> CalculateDamageType(AttackAction attackAction, List<int> rawDamage)
		{
			var finalDamages = new List<int>();
			for (int i = 0; i < attackAction.Targets.Count; i++)
			{
				switch (attackAction.TargetType)
				{
					case TargetTypes.Self:
					case TargetTypes.Single:
					case TargetTypes.All:
						// All targets get the same damage (e.g. single target, self-target)
						attackAction.Targets.ForEach(_ => finalDamages.Add(rawDamage[i]));
						break;

					case TargetTypes.Burst:
						// Burst damage: middle target full, adjacent targets reduced
						for (var j = 0; j < attackAction.Targets.Count; j++)
						{
							var isMiddle = j == attackAction.Targets.Count - 1;
							var burstDamage =
								Mathf.RoundToInt(rawDamage[i] * (isMiddle ? 1 : 1 - attackAction.AdjacentPenalty));
							finalDamages.Add(burstDamage);
						}
						break;

					case TargetTypes.Multi:
						// Multi-damage calculation
						var targetFrequency = attackAction.Targets
							.GroupBy(target => target)
							.ToDictionary(group => group.Key, group => group.Count());

						foreach (var target in targetFrequency)
						{
							var frequency = target.Value;
							var multiDamage = 
								Mathf.RoundToInt(rawDamage[i] * (frequency == 1 ? 1 : 1 - attackAction.SpamPenalty * (frequency - 1)));
							var baseDamage = multiDamage / frequency;
							var remainder = multiDamage % frequency;
							for (var k = 0; k < frequency; k++)
							{
								var finalDamage = baseDamage + (k < remainder ? 1 : 0);
								finalDamages.Add(finalDamage);
							}
						}
						break;
				}
			}
			return finalDamages;
		}
		
		/// <summary>
		/// Apply the damage reduction based on shield and defense.
		/// </summary>
		/// <param name="damage">The damage value</param>
		/// <returns>Damage value after reduction</returns>
		protected virtual int ApplyDamageReduction(int damage)
		{
			float currentShieldPercent = currentShield / (float)characterSO.Shield;
			Debug.Log($"Current shield percent: {currentShieldPercent}");
			Debug.Log(1 - Mathf.Lerp(0, characterSO.MaxShieldReduction, currentShieldPercent));
			int reducedByShield =
				Mathf.RoundToInt(damage * (1 - Mathf.Lerp(0, characterSO.MaxShieldReduction, currentShieldPercent)));
			int reducedByDefense = Mathf.Max(reducedByShield - currentDefense, 0);
			return reducedByDefense;
		}
		
		public virtual void ChangeShield(int value)
		{
			currentShield += value;
			currentShield = Mathf.Clamp(currentShield, 0, characterSO.Shield);
			if (currentShield == 0)
			{
				AddOvertime(shieldBreakOvertimes);
			}
			DynamicTextManager.CreateText2D(transform.position, value.ToString(), DynamicTextManager.damageData);
			PanelManager.Instance.UpdateStatsText(this);
			UpdateInfoText();
		}

		/// <summary>
		/// Change the health value by the amount.
		/// </summary>
		/// <param name="value">amount of health</param>
		public virtual void ChangeHealth(int value)
		{
			currentHealth += value;
			currentHealth = Mathf.Clamp(currentHealth, 0, characterSO.Health);
			if (currentHealth == 0)
			{
				OnCharacterDeath?.Invoke(this);
			}
			if (value < 0)
			{
				infoText.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo);
			}
			DynamicTextManager.CreateText2D(transform.position, value.ToString(), DynamicTextManager.damageData);
			PanelManager.Instance.UpdateStatsText(this);
			UpdateInfoText();
		}
		
		/// <summary>
		/// Change the RAM value by the amount.
		/// </summary>
		/// <param name="amount"></param>
		public virtual void ChangeRam(int amount)
		{
			currentRam += amount;
			if (amount != 0)
				DynamicTextManager.CreateText2D(transform.position, amount.ToString(), DynamicTextManager.ramData);
			currentRam = Mathf.Clamp(currentRam, 0, characterSO.Ram);
			PanelManager.Instance.UpdateButtonUI();
			UpdateInfoText();
		}
		
		/// <summary>
		/// Regenerate RAM at the start of the turn except for the first turn.
		/// </summary>
		public virtual void RegenRam()
		{
			if (_firstTurn)
			{
				_firstTurn = false;
				PanelManager.Instance.UpdateButtonUI();
				return;
			}
			ChangeRam(currentRamRegen);
		}

		/// <summary>
		/// Check if the character has enough RAM for the amount.
		/// </summary>
		/// <param name="amount">amount of RAM</param>
		/// <returns>True if the character has enough RAM, false otherwise</returns>
		public virtual bool HasEnoughRam(int amount)
		{
			return currentRam >= amount;
		}
		
		public virtual bool CanHack(int hackAccess)
		{
			return CanHackCyberSecurity() || CanHackAnyCyberware(hackAccess, out _);
		}
		
		public virtual bool CanHackCyberSecurity()
		{
			return currentShield > 0;
		}

		public virtual bool CanHackAnyCyberware(int hackAccess, out List<CyberwareSO> hackable)
		{
			hackable = new List<CyberwareSO>();
			if (cyberwares.Count == 0 || currentShield > 0)
				return false;
			hackable = cyberwares.Where(cyberware => cyberware.HackAccessLevel <= hackAccess).ToList();
			return hackable.Count > 0;
		}

		/// <summary>
		/// Get the raw turn order of the character.
		/// </summary>
		/// <returns>Raw turn order</returns>
		public virtual TurnOrder GetRawTurnOrder()
		{
			int actionValue = Mathf.RoundToInt(characterSO.ActionValueNumerator / (float)currentSpeed);
			return new TurnOrder
			{
				actionValue = actionValue,
				character = this
			};
		}
		
		public void AddOvertime(OvertimeTemplate overtimeTemplate)
		{
			var overtimeInstance = overtimeTemplate.Clone();
			currentOvertimes.Add(overtimeInstance);
			overtimeInstance.ApplyOvertime(this);
			CalculateTemporalEffect();
		}
		
		public void AddOvertime(List<OvertimeTemplate> overtimeTemplates)
		{
			foreach (var overtimeTemplate in overtimeTemplates)
			{
				var overtimeInstance = overtimeTemplate;
				currentOvertimes.Add(overtimeInstance);
				overtimeInstance.ApplyOvertime(this);
			}
			CalculateTemporalEffect();
		}

		public void AddOvertimeEffect(OvertimeTemplate overtimeTemplate, List<OvertimeEffect> effect)
		{
			foreach (var overtimeEffect in effect)
			{
				overtimeEffects.Add(new KeyValuePair<OvertimeTemplate, OvertimeEffect>(overtimeTemplate, overtimeEffect));
			}
		}

		
		public void UpdateOvertime()
		{
			foreach (var overtime in currentOvertimes)
			{
				overtime.IncreaseTurnCount();
			}
			foreach (var overtime in overtimeToRemove)
			{
				currentOvertimes.Remove(overtime);
			}
			overtimeToRemove.Clear();
		}
		
		public void RemoveOvertime(OvertimeTemplate overtimeTemplate)
		{
			overtimeEffects.RemoveAll(pair => pair.Key == overtimeTemplate);
			overtimeToRemove.Add(overtimeTemplate);
			CalculateTemporalEffect();
		}
		
		public void RemoveOvertime(List<OvertimeTemplate> overtimeTemplates)
		{
			foreach (var overtimeTemplate in overtimeTemplates)
			{
				overtimeEffects.RemoveAll(pair => pair.Key == overtimeTemplate);
				overtimeToRemove.Add(overtimeTemplate);
			}
			CalculateTemporalEffect();
		}

		protected virtual void CalculateTemporalEffect()
		{
			//sort by operator type, multiply first, then add, sort this by converting enum to int
			List<OvertimeEffect> temporal = overtimeEffects
				.Where(x => !x.Value.permanent)
				.OrderBy(pair => (int)pair.Value.operatorType)
				.Select(pair => pair.Value)
				.ToList();
			TemporalStatHandler(temporal);
		}
		
		public virtual void CalculatePermanentEffect()
		{
			List<OvertimeEffect> permanent = overtimeEffects
				.Where(x => x.Value.permanent)
				.OrderBy(pair => (int)pair.Value.operatorType)
				.Select(pair => pair.Value)
				.ToList();
			PermanentStatHandler(permanent);
		}

		protected virtual void TemporalStatHandler(List<OvertimeEffect> temporal)
		{
			// Initialize a dictionary of stats and their respective current values
			Dictionary<TemporalStatTypes, (float currentValue, Func<float, float> clampFunc)> statMap = new()
			{
				//{ StatTypes.Health, (characterSO.Health, val => Mathf.Clamp(Mathf.RoundToInt(val), 0, characterSO.Health)) },
				{ TemporalStatTypes.Attack, (characterSO.Attack, val => Mathf.Clamp(Mathf.RoundToInt(val), 0, int.MaxValue)) },
				{ TemporalStatTypes.HitRate, (characterSO.HitRate, val => Mathf.Clamp(val, 0f, 1f)) },
				{ TemporalStatTypes.CriticalRate, (characterSO.CriticalRate, val => Mathf.Clamp(val, 0f, 1f)) },
				{ TemporalStatTypes.CriticalMultiplier, (characterSO.CriticalMultiplier, val => Mathf.Clamp(val, 0f, float.MaxValue)) },
				{ TemporalStatTypes.Defense, (characterSO.Defense, val => Mathf.Clamp(Mathf.RoundToInt(val), 0, int.MaxValue)) },
				//{ StatTypes.Shield, (currentShield, val => Mathf.Clamp(Mathf.RoundToInt(val), 0, int.MaxValue)) },
				{ TemporalStatTypes.Speed, (characterSO.Speed, val => Mathf.Clamp(Mathf.RoundToInt(val), 1, int.MaxValue)) },
				//{ StatTypes.Ram, (currentRam, val => Mathf.Clamp(Mathf.RoundToInt(val), 0, int.MaxValue)) },
				{ TemporalStatTypes.RamRegen, (characterSO.RamRegen, val => Mathf.Clamp(Mathf.RoundToInt(val), 0, int.MaxValue)) }
			};

			// Apply effects to the corresponding stats
			foreach (var effect in temporal)
				if (statMap.TryGetValue(effect.temporalStatType, out var stat))
				{
					// Calculate new value
					var updatedValue = CalculateStatOperator(effect, stat.currentValue);
					// Update with clamped value
					statMap[effect.temporalStatType] = (updatedValue, stat.clampFunc);
				}

			// Assign clamped values back to character stats
			//currentHealth = (int)statMap[StatTypes.Health].clampFunc(statMap[StatTypes.Health].currentValue);
			currentAttack = (int)statMap[TemporalStatTypes.Attack].clampFunc(statMap[TemporalStatTypes.Attack].currentValue);
			currentHitRate = statMap[TemporalStatTypes.HitRate].clampFunc(statMap[TemporalStatTypes.HitRate].currentValue);
			currentCriticalRate =
				statMap[TemporalStatTypes.CriticalRate].clampFunc(statMap[TemporalStatTypes.CriticalRate].currentValue);
			currentCriticalMultiplier = statMap[TemporalStatTypes.CriticalMultiplier]
				.clampFunc(statMap[TemporalStatTypes.CriticalMultiplier].currentValue);
			currentDefense = (int)statMap[TemporalStatTypes.Defense].clampFunc(statMap[TemporalStatTypes.Defense].currentValue);
			//currentShield = (int)statMap[StatTypes.Shield].clampFunc(statMap[StatTypes.Shield].currentValue);
			currentSpeed = (int)statMap[TemporalStatTypes.Speed].clampFunc(statMap[TemporalStatTypes.Speed].currentValue);
			//currentRam = (int)statMap[StatTypes.Ram].clampFunc(statMap[StatTypes.Ram].currentValue);
			currentRamRegen = (int)statMap[TemporalStatTypes.RamRegen].clampFunc(statMap[TemporalStatTypes.RamRegen].currentValue);
			if (!_fromInventory) PanelManager.Instance.UpdateStatsText(this);
			UpdateInfoText();
		}

		protected virtual void PermanentStatHandler(List<OvertimeEffect> permanent)
		{
			// Initialize a dictionary of stats and their respective current values
			Dictionary<PermanentStatTypes, (float currentValue, Func<float, float> clampFunc)> statMap = new()
			{
				{ PermanentStatTypes.Health, (currentHealth, val => Mathf.Clamp(Mathf.RoundToInt(val), 0, characterSO.Health)) },
				//{ StatTypes.Attack, (currentAttack, val => Mathf.Clamp(Mathf.RoundToInt(val), 0, int.MaxValue)) },
				//{ StatTypes.HitRate, (currentHitRate, val => Mathf.Clamp(val, 0f, 1f)) },
				//{ StatTypes.CriticalRate, (currentCriticalRate, val => Mathf.Clamp(val, 0f, 1f)) },
				//{ StatTypes.CriticalMultiplier, (currentCriticalMultiplier, val => Mathf.Clamp(val, 0f, float.MaxValue)) },
				//{ StatTypes.Defense, (currentDefense, val => Mathf.Clamp(Mathf.RoundToInt(val), 0, int.MaxValue)) },
				{ PermanentStatTypes.Shield, (currentShield, val => Mathf.Clamp(Mathf.RoundToInt(val), 0, characterSO.Shield)) },
				//{ StatTypes.Speed, (currentSpeed, val => Mathf.Clamp(Mathf.RoundToInt(val), 1, int.MaxValue)) },
				{ PermanentStatTypes.Ram, (currentRam, val => Mathf.Clamp(Mathf.RoundToInt(val), 0, characterSO.Ram)) },
				//{ StatTypes.RamRegen, (currentRamRegen, val => Mathf.Clamp(Mathf.RoundToInt(val), 0, int.MaxValue)) }
			};

			// Apply effects to the corresponding stats
			foreach (var effect in permanent)
				if (statMap.TryGetValue(effect.permanentStatType, out var stat))
				{
					// Calculate new value
					var updatedValue = CalculateStatOperator(effect, stat.currentValue);
					// Update with clamped value
					statMap[effect.permanentStatType] = (updatedValue, stat.clampFunc);
				}

			// Assign clamped values back to character stats
			currentHealth = (int)statMap[PermanentStatTypes.Health].clampFunc(statMap[PermanentStatTypes.Health].currentValue);
			currentShield = (int)statMap[PermanentStatTypes.Shield].clampFunc(statMap[PermanentStatTypes.Shield].currentValue);
			currentRam = (int)statMap[PermanentStatTypes.Ram].clampFunc(statMap[PermanentStatTypes.Ram].currentValue);
			if (!_fromInventory) PanelManager.Instance.UpdateStatsText(this);
			UpdateInfoText();
		}

		protected virtual float CalculateStatOperator(OvertimeEffect overtimeEffect, float value)
		{
			switch (overtimeEffect.operatorType)
			{
				case OperatorTypes.Multiply:
					return value * overtimeEffect.value;
				case OperatorTypes.Add:
					return value + overtimeEffect.value;
				case OperatorTypes.MultiplyAdd:
					return value + (value * overtimeEffect.value);
			}
			return value;
		}

		/// <summary>
		/// Update the info text of the current stats.
		/// </summary>
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