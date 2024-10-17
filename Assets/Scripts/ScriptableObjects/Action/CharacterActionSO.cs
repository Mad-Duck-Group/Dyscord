using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.Characters;
using Dyscord.Managers;
using NaughtyAttributes;
using Redcode.Extensions;
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
	public abstract class CharacterActionSO : ScriptableObject
	{
		[SerializeField] protected string actionName;
		[SerializeField] protected TargetSides targetSide;
		[SerializeField] protected TargetTypes targetType;
		[SerializeField][ShowIf(nameof(targetType), TargetTypes.Multi)][MinValue(1)] protected int maxTargets;
		[SerializeField] protected int ramCost;
		
		protected Stack<Character> _targets = new Stack<Character>();
		public string ActionName => actionName;
		public TargetSides TargetSide => targetSide;
		public TargetTypes TargetType => targetType;
		public int MaxTargets => maxTargets;
		public int RamCost => ramCost;
		public Character Owner { get; private set; }
		public bool PlayerSelecting { get; protected set; }
		
		public List<Character> Targets => _targets.ToList();

		public virtual void Initialize(Character owner)
		{
			Owner = owner;
		}

		protected abstract void UseAction();

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
				TurnManager.Instance.UpdateButtonUI();
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

		public virtual void Cancel()
		{
			Debug.Log("Action canceled");
			PlayerSelecting = false;
			Owner.CurrentAction = null;
			_targets.Clear();
			TurnManager.Instance.UpdateButtonUI();
		}
		
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