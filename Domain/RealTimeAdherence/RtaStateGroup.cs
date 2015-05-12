using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
	public class RtaStateGroup : VersionedAggregateRootWithBusinessUnitWithoutChangeInfo, IRtaStateGroup
    {
        private readonly IList<IRtaState> _stateCollection = new List<IRtaState>();
        private bool _available;
        private bool _defaultStateGroup;
        private string _name;
        private bool _isLogOutState;
        private bool _isDeleted;

        protected RtaStateGroup()
        {

        }

        public RtaStateGroup(string name, bool isDefaultStateGroup, bool isAvailable)
            : this()
        {
            _name = name;
            _defaultStateGroup = isDefaultStateGroup;
            _available = isAvailable;
        }

	    public virtual void SetBusinessUnit(IBusinessUnit businessUnit)
	    {
		    BusinessUnit = businessUnit;
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

        public virtual ReadOnlyCollection<IRtaState> StateCollection
        {
            get { return new ReadOnlyCollection<IRtaState>(_stateCollection.ToList()); }
        }

        public virtual bool IsLogOutState
        {
            get { return _isLogOutState; }
            set { _isLogOutState = value; }
        }

        public virtual IRtaState AddState(string stateName, string stateCode, Guid platformTypeId)
        {
            IRtaState state = new RtaState(stateName, stateCode, platformTypeId);
            AddStateInternal(state);
            return state;
        }

        protected internal virtual void AddStateInternal(IRtaState state)
        {
            _stateCollection.Add(state);
            state.SetParent(this);
        }

        public virtual IRtaState MoveStateTo(IRtaStateGroup target, IRtaState state)
        {
            _stateCollection.Remove(state);
            RtaStateGroup internalAdd = target as RtaStateGroup;
            if (internalAdd==null) return null;
            if (state==null) return null;
	        var rtaState = new RtaState(state.Name, state.StateCode, state.PlatformTypeId);
	        internalAdd.AddStateInternal(rtaState);
	        return rtaState;
        }

		public virtual void DeleteState(IRtaState state)
    	{
    		_stateCollection.Remove(state);
    	}

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}