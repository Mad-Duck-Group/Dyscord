using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Characters;
using NaughtyAttributes;
using SerializeReferenceEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.ScriptableObjects.Overtime
{
	public enum OperatorTypes
	{
		Multiply = 0,
		Add = 1,
		MultiplyAdd = 2
	}
	public enum TemporalStatTypes
	{
		Attack,
		HitRate,
		CriticalRate,
		CriticalMultiplier,
		Defense,
		Speed,
		RamRegen
	}
	public enum PermanentStatTypes
	{
		Health,
		Shield,
		Ram
	}
	[Serializable]
	public struct OvertimeEffect
	{
		public bool permanent;
		[AllowNesting][HideIf(nameof(permanent))] public TemporalStatTypes temporalStatType;
		[AllowNesting][ShowIf(nameof(permanent))] public PermanentStatTypes permanentStatType;
		public OperatorTypes operatorType;
		public float value;
	}
	[Serializable]
	public abstract class OvertimeTemplate
	{
		[AllowNesting][InfoBox("Instant Overtime will apply the temporal effect and permanent instantly.\n" +
		                       "Delayed Overtime will apply the temporal effect instantly but the permanent effect will be applied once the duration is over.")]
		[Header("Overtime Configs")]
		[SerializeField] protected Sprite icon;
		[SerializeField] protected string overtimeName;
		[SerializeField] protected string description;
		[SerializeField] protected bool infinite;
		[SerializeField] [AllowNesting] [HideIf(nameof(infinite))] [Min(0)] protected int duration = 0;
		[SerializeField] protected List<OvertimeEffect> effects;
		
		protected int turnCount;
		protected Character owner;
		
		public delegate void OnOvertimeEnd();
		public OnOvertimeEnd onOvertimeEnd;
		
		public int TurnCount => turnCount;
		public int RemainingTurns => duration - turnCount;
		public Sprite Icon => icon;
		public string OvertimeName => overtimeName;
		public string Description => description;
		public int Duration => duration;
		public List<OvertimeEffect> Effects => effects;
		public bool Infinite => infinite;

		public virtual void ApplyOvertime(Character character)
		{
			character.AddOvertimeEffect(this, effects);
			owner = character;
			owner.CalculatePermanentEffect();
			if (Duration == 0 && !infinite)
				RemoveOvertime();
		}
		
		public virtual void IncreaseTurnCount()
		{
			if (infinite) return;
			turnCount++;
			CheckDuration();
		}
		
		protected virtual void CheckDuration()
		{
			if (infinite) return;
			if (turnCount >= duration)
			{
				turnCount = 0;
				RemoveOvertime();
			}
		}
		
		public virtual void RemoveOvertime()
		{
			onOvertimeEnd?.Invoke();
			owner.RemoveOvertime(this);
		}
		
		public virtual OvertimeTemplate Clone()
		{
			return (OvertimeTemplate)MemberwiseClone(); // Shallow copy
		}
	}

	[Serializable]
	[SRName("Instant Overtime")]
	public class InstantOvertime : OvertimeTemplate
	{
		
	}
	
	[Serializable]
	[SRName("Delayed Overtime")]
	public class DelayedOvertime : OvertimeTemplate
	{
		public override void ApplyOvertime(Character character)
		{
			character.AddOvertimeEffect(this, effects);
			owner = character;
			if (Duration == 0 && !infinite)
				RemoveOvertime();
		}
		
		public override void RemoveOvertime()
		{
			onOvertimeEnd?.Invoke();
			owner.CalculatePermanentEffect(true);
			owner.RemoveOvertime(this);
		}
	}
}