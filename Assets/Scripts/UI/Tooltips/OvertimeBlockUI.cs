using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dyscord.ScriptableObjects.Overtime;
using TMPro;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class OvertimeBlockUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text overtimeName;
		[SerializeField] private TMP_Text overtimeEffectsPrefab;

		private Dictionary<TemporalStatTypes, string> temporalText = new Dictionary<TemporalStatTypes, string>
		{
			{ TemporalStatTypes.Attack, "Attack"},
			{ TemporalStatTypes.HitRate, "Hit Rate"},
			{ TemporalStatTypes.CriticalRate, "Critical Rate"},
			{ TemporalStatTypes.CriticalMultiplier, "Critical Multiplier"},
			{ TemporalStatTypes.Defense, "Defense"},
			{ TemporalStatTypes.Speed, "Speed"},
			{ TemporalStatTypes.RamRegen, "Ram Regen"}
		};
		
		private Dictionary<PermanentStatTypes, string> permanentText = new Dictionary<PermanentStatTypes, string>
		{
			{ PermanentStatTypes.Health, "Health"},
			{ PermanentStatTypes.Shield, "Shield"},
			{ PermanentStatTypes.Ram, "RAM"}
		};

		public void SetOvertimeBlock(OvertimeTemplate template)
		{
			string delay = template is DelayedOvertime ? "Activate in " : "";
			string s = template.Duration > 1 ? "s" : "";
			string duration = template.Infinite ? "Passive" : template.Duration < 1 ? "Instant" : $"{delay}{template.Duration} turn{s}";
			overtimeName.text = $"{template.OvertimeName}: {duration}";
			foreach (var effect in template.Effects)
			{
				var overtimeEffect = Instantiate(overtimeEffectsPrefab, transform);
				overtimeEffect.text = FormatEffectText(effect);
			}
		}

		private string FormatEffectText(OvertimeEffect effect)
		{
			string effectText = "";
			string prefix = effect.value > 0 ? "Increase" : "Decrease";
			string valueType = effect.permanent ? permanentText[effect.permanentStatType] : temporalText[effect.temporalStatType];
			float value = Mathf.Abs(effect.value);
			string effectString = "";
			string percentage;
			switch (effect.operatorType)
			{
				case OperatorTypes.Add:
					percentage = (value * 100).ToString(CultureInfo.CurrentCulture);
					if (!effect.permanent && (effect.temporalStatType is TemporalStatTypes.HitRate or TemporalStatTypes.CriticalRate or TemporalStatTypes.CriticalMultiplier))
					{
						effectString = $"by {percentage}%";
					}
					else
					{
						effectString = $"by {value}";
					}
					break;
				case OperatorTypes.Multiply:
					percentage = (value * 100).ToString(CultureInfo.CurrentCulture);
					effectString = $"by {percentage}%";
					break;
				case OperatorTypes.MultiplyAdd:
					percentage = (value * 100).ToString(CultureInfo.CurrentCulture);
					string permanent = effect.permanent ? "current" : "max";
					effectString = $"by {percentage}% of {permanent} {valueType}";
					break;
			}
			effectText += $"{prefix} {valueType} {effectString}\n";
			return effectText;
		}
	}
}