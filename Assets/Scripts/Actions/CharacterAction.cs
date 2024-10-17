using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Actions
{
	public enum ActionTypes
	{
		Attack,
		Defend,
		Heal
	}
	public enum TargetTypes
	{
		Single,
		Burst,
		Multi,
		All
	}
	public class CharacterAction : MonoBehaviour
	{
		[SerializeField] private string actionName;
		[SerializeField] private ActionTypes actionType;
		[SerializeField][ShowIf(nameof(actionType), ActionTypes.Attack)] private TargetTypes targetType;
		[SerializeField][ShowIf(nameof(targetType), TargetTypes.Multi)] private int maxTargets;
		[SerializeField] private int ramCost;
		
		public string ActionName => actionName;
		public ActionTypes ActionType => actionType;
		public TargetTypes TargetType => targetType;
		public int MaxTargets => maxTargets;
		public int RamCost => ramCost;
	}
}