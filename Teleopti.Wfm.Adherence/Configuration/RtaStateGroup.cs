using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Configuration.Events;

namespace Teleopti.Wfm.Adherence.Configuration
{
	public class RtaStateGroup : VersionedAggregateRootWithBusinessUnitIdWithoutChangeInfo, IRtaStateGroup
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

		public virtual void SetBusinessUnit(IBusinessUnit businessUnit)
	    {
		    BusinessUnit = businessUnit.Id.Value;
	    }

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        // Todo: Make sure only one State Group can be default. Maybe this can be done from the GUI?
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

		public virtual IRtaStateGroup AddState(string stateCode)
		{
			IRtaState state = new RtaState(stateCode);
			AddStateInternal(state);
			return this;
		}

		public virtual IRtaStateGroup AddState(string stateCode, string stateName)
        {
            IRtaState state = new RtaState(stateName, stateCode);
            AddStateInternal(state);
            return this;
        }

        protected internal virtual void AddStateInternal(IRtaState state)
        {
            _stateCollection.Add(state);
			AddEvent(new RtaStateGroupChangedEvent());
			state.Parent = this;
        }

        public virtual IRtaState MoveStateTo(IRtaStateGroup target, IRtaState state)
        {
            _stateCollection.Remove(state);
            RtaStateGroup internalAdd = target as RtaStateGroup;
            if (internalAdd==null) return null;
            if (state==null) return null;
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