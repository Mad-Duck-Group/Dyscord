using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Characters;
using Dyscord.Managers;
using Dyscord.ScriptableObjects.Action.Hack;
using Dyscord.ScriptableObjects.Cyberware;
using Dyscord.ScriptableObjects.Overtime;
using NaughtyAttributes;
using Redcode.Extensions;
using SerializeReferenceEditor;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.ScriptableObjects.Action
{
	public enum TargetTypes
	{
		Self,
		Single,
		Burst,
		Multi,
		All
	}
	public enum TargetSides
	{
		Self,
		Other
	}
	/// <summary>
	/// Base class for character actions.
	/// Target selection and usage are handled here.
	/// Inherit and implement the UseAction method for specific actions.
	/// NOTE: If the stats of the action can be modified during runtime, we have to change the design of the action system. Praying to god that it won't happen.🙏
	/// </summary>
	public abstract class CharacterActionSO : ScriptableObject
	{
		[Header("General Configs")]
		[SerializeField] protected string actionName;
		[SerializeField][TextArea] protected string description;
		[SerializeField] protected bool playAnimation;
		[SerializeField][ShowIf(nameof(playAnimation))] protected float playerAnimationSize = 0.33f;
		[SerializeField][ShowIf(nameof(playAnimation))] protected AnimatorOverrideController animatorController;
		[SerializeField] protected bool playSound;
		[SerializeField] protected TargetSides targetSide = TargetSides.Other;
		[SerializeField][ValidateInput(nameof(ValidateTargetType), "Hack Action can only target single target")] 
		protected TargetTypes targetType;
		[SerializeField][ShowIf(nameof(targetType), TargetTypes.Multi)][MinValue(1)] protected int maxTargets;
		[SerializeField][Min(0)] protected int ramCost;
		[SerializeField] protected TargetSides overtimeTargetSide = TargetSides.Other;
		[SerializeReference, SR] protected List<OvertimeTemplate> overtimeTemplates;

		protected Stack<Character> _targets = new Stack<Character>();
		public string ActionName => actionName;
		public string Description => description;
		public AnimatorOverrideController AnimatorController => animatorController;
		public TargetSides TargetSide => targetSide;
		public TargetTypes TargetType => targetType;
		public int MaxTargets => maxTargets;
		public int RamCost => ramCost;
		public CyberwareSO Cyberware { get; private set; }
		public Character Owner { get; private set; }
		public bool PlayerSelecting { get; protected set; }
		public bool PlayerHacking { get; protected set; }
		public List<Character> Targets => _targets.ToList();

		protected virtual bool ValidateTargetType()
		{
			if (GetType() == typeof(HackAction) && targetType is not TargetTypes.Single)
				return false;
			return true;
		}

		/// <summary>
		/// Initialize the action with the owner.
		/// </summary>
		/// <param name="owner">Owner of the action</param>
		public virtual void Initialize(Character owner, CyberwareSO cyberware = null)
		{
			Owner = owner;
			Cyberware = cyberware;
		}

		/// <summary>
		/// Use the action with the selected targets.
		/// </summary>
		protected abstract void UseAction();

		/// <summary>
		/// Select the target for the action.
		/// </summary>
		public virtual void SelectTarget()
		{
			Debug.Log(actionName);
			if (!Owner.HasEnoughRam(RamCost)) return;
			Owner.CurrentAction = this;
			if (TargetType == TargetTypes.Self)
			{
				AddTarget(Owner);
				return;
			}

			switch (TargetType)
			{
				case TargetTypes.All when TargetSide == TargetSides.Self && Owner is Characters.Player.Player:
				case TargetTypes.All when TargetSide == TargetSides.Other && Owner is not Characters.Player.Player:
					AddTarget(TurnManager.Instance.PlayerInstance);
					return;
				case TargetTypes.All when TargetSide == TargetSides.Other && Owner is Characters.Player.Player:
				case TargetTypes.All when TargetSide == TargetSides.Self && Owner is not Characters.Player.Player:
					TurnManager.Instance.EnemyInstances.ForEach(x => AddTarget(x));
					return;
			}

			if (Owner is Characters.Player.Player)
			{
				PlayerSelecting = true;
				PanelManager.Instance.UpdateButtonUI();
				return;
			}
			switch (TargetType)
			{
				case TargetTypes.Single when TargetSide == TargetSides.Self && Owner is Characters.Player.Player:
				case TargetTypes.Single when TargetSide == TargetSides.Other && Owner is not Characters.Player.Player:
					AddTarget(TurnManager.Instance.PlayerInstance);
					return;
				case TargetTypes.Single when TargetSide == TargetSides.Other && Owner is Characters.Player.Player:
				case TargetTypes.Single when TargetSide == TargetSides.Self && Owner is not Characters.Player.Player:
					AddTarget(TurnManager.Instance.EnemyInstances.GetRandomElement());
					return;
				case TargetTypes.Burst when TargetSide == TargetSides.Self && Owner is Characters.Player.Player:
				case TargetTypes.Burst when TargetSide == TargetSides.Other && Owner is not Characters.Player.Player:
					AddTarget(TurnManager.Instance.PlayerInstance);
					return;
				case TargetTypes.Burst when TargetSide == TargetSides.Other && Owner is Characters.Player.Player:
				case TargetTypes.Burst when TargetSide == TargetSides.Self && Owner is not Characters.Player.Player:
				{
					Character target = TurnManager.Instance.EnemyInstances.GetRandomElement();
					AddTarget(target, true);
					return;
				}
				case TargetTypes.Multi when TargetSide == TargetSides.Self && Owner is Characters.Player.Player:
				case TargetTypes.Multi when TargetSide == TargetSides.Other && Owner is not Characters.Player.Player:
				{
					for (int i = 0; i < MaxTargets; i++)
					{
						AddTarget(TurnManager.Instance.PlayerInstance);
					}
					return;
				}
				case TargetTypes.Multi when TargetSide == TargetSides.Other && Owner is Characters.Player.Player:
				case TargetTypes.Multi when TargetSide == TargetSides.Self && Owner is not Characters.Player.Player:
				{
					for (int i = 0; i < MaxTargets; i++)
					{
						AddTarget(TurnManager.Instance.EnemyInstances.GetRandomElement());
					}
					return;
				}
			}
		}
		
		/// <summary>
		/// Add a target to the action.
		/// </summary>
		/// <param name="target">Target to be added</param>
		/// <param name="burst">Whether the action is a burst action</param>
		public virtual void AddTarget(Character target, bool burst = false)
		{
			_targets.Push(target);
			if (burst)
			{
				int targetIndex = TurnManager.Instance.EnemyInstances.IndexOf(target);
				int leftIndex = targetIndex - 1;
				int rightIndex = targetIndex + 1;
				if (leftIndex >= 0)
				{
					_targets.Push(TurnManager.Instance.EnemyInstances[leftIndex]);
				}
				if (rightIndex < TurnManager.Instance.EnemyInstances.Count)
				{
					_targets.Push(TurnManager.Instance.EnemyInstances[rightIndex]);
				}
			}
			CheckTargetRequirement();
		}
		
		/// <summary>
		/// Check if the target requirement is met.
		/// </summary>
		protected virtual void CheckTargetRequirement()
		{
			if (_targets.Count == 0)
			{
				Cancel();
				return;
			}
			bool pass = false;
			switch (TargetType)
			{
				case TargetTypes.Self:
					pass = _targets.Count == 1 && _targets.Peek() == Owner;
					break;
				case TargetTypes.Single:
					pass = _targets.Count == 1;
					break;
				case TargetTypes.Burst:
				{
					pass = _targets.Count > 0;
					break;
				}
				case TargetTypes.Multi:
				{
					pass = _targets.Count == MaxTargets;
					break;
				}
				case TargetTypes.All when TargetSide == TargetSides.Self && Owner is Characters.Player.Player:
				case TargetTypes.All when TargetSide == TargetSides.Other && Owner is not Characters.Player.Player:
					pass = _targets.Count == 1;
					break;
				case TargetTypes.All when TargetSide == TargetSides.Other && Owner is Characters.Player.Player:
				case TargetTypes.All when TargetSide == TargetSides.Self && Owner is not Characters.Player.Player:
					pass = _targets.Count == TurnManager.Instance.EnemyInstances.Count;
					break;
			}
			if (pass)
			{
				UseAction();
			}
		}
		
		/// <summary>
		/// Cancel the action.
		/// </summary>
		public virtual void Cancel()
		{
			Debug.Log("Action canceled");
			PlayerSelecting = false;
			Owner.CurrentAction = null;
			_targets.ForEach(x => x.SelectionCount = 0);
			_targets.Clear();
			PanelManager.Instance.UpdateButtonUI();
		}
		
		/// <summary>
		/// Undo the latest target selection.
		/// </summary>
		public virtual void UndoTarget()
		{
			Debug.Log("Undo target");
			if (_targets.Count > 0)
			{
				Character pop = _targets.Pop();
				pop.SelectionCount--;
			}
			CheckTargetRequirement();
		}
	}
}