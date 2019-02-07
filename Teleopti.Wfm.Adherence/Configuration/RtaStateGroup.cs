using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Configuration.Events;

namespace Teleopti.Wfm.Adherence.Configuration
{
	public class RtaStateGroup : AggregateRoot_Events_Versioned_BusinessUnitId, IRtaStateGroup
	{
		private readonly IList<IRtaState> _stateCollection = new List<IRtaState>();
		private bool _available;
		private bool _defaultStateGroup;
		private string _name;
		private bool _isLogOutState;

		protected RtaStateGroup()
		{
		}

		public RtaStateGroup(string name, bool isDefaultStateGroup, bool isAvailable) : this()
		{
			_name = name;
			_defaultStateGroup = isDefaultStateGroup;
			_available = isAvailable;
		}

		public RtaStateGroup(string name) : this()
		{
			_name = name;
		}

		public override void NotifyTransactionComplete(DomainUpdateType operation)
		{
			base.NotifyTransactionComplete(operation);
			AddEvent(new RtaStateGroupChangedEvent());
		}

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual bool DefaultStateGroup
		{
			get { return _defaultStateGroup; }
			set { _defaultStateGroup = value; }
		}

		public virtual bool Available
		{
			get { return _available; }
			set { _available = value; }
		}

		public virtual IList<IRtaState> StateCollection => _stateCollection.ToList();

		public virtual bool IsLogOutState
		{
			get { return _isLogOutState; }
			set { _isLogOutState = value; }
		}

		public virtual void AddState(string stateCode) =>
			AddStateInternal(new RtaState(stateCode));

		public virtual void AddState(string stateCode, string stateName) =>
			AddStateInternal(new RtaState(stateName, stateCode));

		protected virtual void AddStateInternal(IRtaState state)
		{
			state.BusinessUnit = BusinessUnit;
			state.Parent = this;
			_stateCollection.Add(state);
			AddEvent(new RtaStateGroupChangedEvent());
		}

		public virtual IRtaState MoveStateTo(IRtaStateGroup target, IRtaState state)
		{
			_stateCollection.Remove(state);
			var internalAdd = target as RtaStateGroup;
			if (internalAdd == null) return null;
			if (state == null) return null;
			var rtaState = new RtaState(state.Name, state.StateCode);
			internalAdd.AddStateInternal(rtaState);
			return rtaState;
		}

		public virtual void DeleteState(IRtaState state)
		{
			_stateCollection.Remove(state);
		}

		public virtual void ClearStates()
		{
			_stateCollection.Clear();
		}
	}
}