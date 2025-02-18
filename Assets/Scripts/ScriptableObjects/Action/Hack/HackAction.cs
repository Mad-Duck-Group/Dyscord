using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dyscord.Characters;
using Dyscord.Managers;
using Dyscord.ScriptableObjects.Cyberware;
using NaughtyAttributes;
using Redcode.Extensions;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.ScriptableObjects.Action.Hack
{
	[CreateAssetMenu(menuName = "Action/HackAction", fileName = "HackAction")]
	public class HackAction : CharacterActionSO
	{
		[Header("Hack Configs")] 
		[SerializeField][Min(0)] protected int hackDamage;
		[SerializeField] protected CyberwareSO[] cyberwares;
		[SerializeField][ShowIf(nameof(playSound))] protected AudioClip hackCyberSecuritySound;
		[SerializeField][ShowIf(nameof(playSound))] protected AudioClip hackCyberwareSound;
		[SerializeField][ShowIf(nameof(playAnimation))] protected AnimatorOverrideController hackCyberwareAnimatorController;
		
		
		protected override void UseAction()
		{
			Character target = _targets.Peek();
			PlayerSelecting = false;
			PlayerHacking = true;
			Owner.ChangeRam(-RamCost);
			if (Owner is not Characters.Player.Player)
			{
				HandleAIHack();
				return;
			}
			PanelManager.Instance.SetActivePanel(PanelTypes.HackSelection, true);
			PanelManager.OnHackActionButtonPressed += HandlePlayerHack;
			PanelManager.Instance.UpdateHackSelectionButton(target.CanHackCyberSecurity(),
				target.CanHackAnyCyberware(Owner.HackAccessChip.HackAccessLevel, out _));
		}
		
		protected virtual void HandleAIHack()
		{
			Character target = _targets.Peek();
			if (target.CanHackAnyCyberware(Owner.HackAccessChip.HackAccessLevel, out var cyberware))
			{
				HackCyberware(cyberware.GetRandomElement());
				return;
			}
			HackCyberSecurity();
		}

		protected virtual void HandlePlayerHack(bool isCyberware)
		{
			Character target = _targets.Peek();
			if (isCyberware)
			{
				PanelManager.Instance.PopulateCyberwareUI(target.Cyberwares, Owner.HackAccessChip.HackAccessLevel);
				PanelManager.Instance.SetActivePanel(PanelTypes.HackCyberware, true);
				PanelManager.OnCyberwareButtonPressed += HackCyberware;
				return;
			}
			HackCyberSecurity();
		}
		
		protected virtual void HackCyberSecurity()
		{
			Character target = _targets.Peek();
			target.ChangeShield(-Mathf.RoundToInt(hackDamage * Owner.HackAccessChip.HackDamageModifier));
			if (playSound) GlobalSoundManager.Instance.PlayEffectClip(hackCyberSecuritySound);
			if (playAnimation)
			{
				var anim = Instantiate(TurnManager.Instance.VfxPrefab, target.transform.position, Quaternion.identity,
					target.transform);
				if (target == TurnManager.Instance.PlayerInstance)
					anim.transform.localScale *= playerAnimationSize;
				anim.runtimeAnimatorController = animatorController;
				float animationDuration = anim.runtimeAnimatorController.animationClips[0].length;
				Destroy(anim.gameObject, animationDuration);
			}
			TooltipManager.Instance.DestroyTooltip();
			EndHack();
		}
		protected virtual void HackCyberware(CyberwareSO cyberware)
		{
			Character target = _targets.Peek();
			target.AddOvertime(cyberware.HackedOvertimeTemplates, cyberware);
			cyberware.Hacked = true;
			if (playSound) GlobalSoundManager.Instance.PlayEffectClip(hackCyberwareSound);
			if (playAnimation)
			{
				var anim = Instantiate(TurnManager.Instance.VfxPrefab, target.transform.position, Quaternion.identity,
					target.transform);
				if (target == TurnManager.Instance.PlayerInstance)
					anim.transform.localScale *= playerAnimationSize;
				anim.runtimeAnimatorController = hackCyberwareAnimatorController;
				float animationDuration = anim.runtimeAnimatorController.animationClips[0].length;
				Destroy(anim.gameObject, animationDuration);
			}
			TooltipManager.Instance.DestroyTooltip();
			EndHack();
		}

		protected virtual void EndHack()
		{
			Owner.CurrentAction = null;
			PlayerHacking = false;
			if (Owner is Characters.Player.Player)
			{
				PanelManager.OnHackActionButtonPressed -= HandlePlayerHack;
				PanelManager.OnCyberwareButtonPressed -= HackCyberware;
				PanelManager.Instance.SetActivePanel(PanelTypes.Stats, true);
			}
			DOVirtual.DelayedCall(0.5f, () =>
			{
				_targets.ForEach(x => x.SelectionCount = 0);
				_targets.Clear();
			});
			TurnManager.Instance.NextTurn();
		}
		
		public override void Cancel()
		{
			base.Cancel();
			PlayerHacking = false;
			PanelManager.OnHackActionButtonPressed -= HandlePlayerHack;
			PanelManager.OnCyberwareButtonPressed -= HackCyberware;
		}
	}
}