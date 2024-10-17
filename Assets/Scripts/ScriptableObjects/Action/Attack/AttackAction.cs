using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dyscord.Characters;
using Dyscord.Managers;
using NaughtyAttributes;
using Redcode.Extensions;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.ScriptableObjects.Action.Attack
{
	/// <summary>
	/// Attack action that deals damage to the target based on the Attack stat of the owner.
	/// </summary>
	[CreateAssetMenu(menuName = "Action/Attack", fileName = "Attack")]
	public class AttackAction : CharacterActionSO
	{
		[Header("Attack Configs")]
		[SerializeField] protected float damageMultiplier = 1f;
		[SerializeField][ShowIf(nameof(targetType), TargetTypes.Burst)] protected float adjacentPenalty = 0.5f;
		[SerializeField][ShowIf(nameof(targetType), TargetTypes.Multi)] protected float spamPenalty = 0.1f;
		public float DamageMultiplier => damageMultiplier;
		public float AdjacentPenalty => adjacentPenalty;
		public float SpamPenalty => spamPenalty;
		protected override void UseAction()
		{
			PlayerSelecting = false;
			Owner.ChangeRam(-RamCost);
			Owner.Attack(this);
			DOVirtual.DelayedCall(0.5f, () =>
			{
				_targets.ForEach(x => x.SelectionCount = 0);
				_targets.Clear();
			});
			Owner.CurrentAction = null;
			TurnManager.Instance.NextTurn();
		}
	}
}