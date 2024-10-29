using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dyscord.Characters;
using Dyscord.Managers;
using Dyscord.ScriptableObjects.Overtime;
using Gamelogic.Extensions;
using NaughtyAttributes;
using Redcode.Extensions;
using SerializeReferenceEditor;
using UnityEngine;
using UnityEngine.Serialization;
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
		[SerializeField][Min(0)] protected float damageMultiplier = 1f;
		[SerializeField][Range(0f, 1f)][ShowIf(nameof(targetType), TargetTypes.Burst)] protected float adjacentPenalty = 0.5f;
		[SerializeField][Range(0f, 1f)][ShowIf(nameof(targetType), TargetTypes.Multi)] protected float spamPenalty = 0.1f;
		[SerializeField][ShowIf(nameof(playSound))] protected AudioClip attackSound;
		public float DamageMultiplier => damageMultiplier;
		public float AdjacentPenalty => adjacentPenalty;
		public float SpamPenalty => spamPenalty;
		protected override void UseAction()
		{
			PlayerSelecting = false;
			Owner.ChangeRam(-RamCost);
			Owner.Attack(this);
			if (playAnimation)
			{
				_targets.ForEach(x =>
				{
					var anim = Instantiate(TurnManager.Instance.VfxPrefab, x.transform.position, Quaternion.identity, x.transform);
					if (x == TurnManager.Instance.PlayerInstance)
						anim.transform.localScale *= playerAnimationSize;
					anim.runtimeAnimatorController = animatorController;
					float animationDuration = anim.runtimeAnimatorController.animationClips[0].length;
					Destroy(anim.gameObject, animationDuration);
				});
			}
			if (playSound)
				GlobalSoundManager.Instance.PlayEffectClip(attackSound);
			switch (overtimeTargetSide)
			{
				case TargetSides.Other:
				{
					foreach (var overtimeTemplate in overtimeTemplates)
					{
						_targets.Distinct().ForEach(x => x.AddOvertime(overtimeTemplate));
					}
					break;
				}
				case TargetSides.Self:
					Owner.AddOvertime(overtimeTemplates);
					break;
			}
			DOVirtual.DelayedCall(0.5f, () =>
			{
				_targets.ForEach(x => x.SelectionCount = 0);
				_targets.Clear();
			});
			Owner.CurrentAction = null;
			if (Owner is Characters.Player.Player)
				PanelManager.Instance.SetActivePanel(PanelTypes.Stats, true);
			TurnManager.Instance.NextTurn();
		}
	}
}