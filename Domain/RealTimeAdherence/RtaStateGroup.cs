using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public class RtaStateGroup : VersionedAggregateRootWithBusinessUnit,
                                 IRtaStateGroup, IDeleteTag
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
            get { return new ReadOnlyCollection<IRtaState>(_stateCollection); }
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

    	public virtual Description ConfidentialDescription(IPerson assignedPerson, DateOnly assignedDate)
        {
            return new Description(_name);
        }

		public virtual Color ConfidentialDisplayColor(IPerson assignedPerson, DateOnly assignedDate)
        {
            return Color.Empty;
        }

        public virtual bool InContractTime
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual ITracker Tracker
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual IPayload UnderlyingPayload
        {
            get { return this; }
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