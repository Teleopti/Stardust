using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public class RtaState : AggregateEntity,
                            IRtaState
    {
        private string _stateCode;
        private string _name;
        private readonly Guid _platformTypeId;

        protected RtaState() { }

        internal RtaState(string name, string stateCode, Guid platformTypeId)
            : this()
        {
            _name = name;
            _stateCode = stateCode;
            _platformTypeId = platformTypeId;
        }

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual string StateCode
        {
            get { return _stateCode; }
            set { _stateCode = value; }
        }

        public virtual IRtaStateGroup StateGroup
        {
            get { return Parent as IRtaStateGroup; }
        }

        public virtual Guid PlatformTypeId
        {
            get { return _platformTypeId; }
        }

	    public virtual object Clone()
	    {
		    return MemberwiseClone();
	    }

		public virtual IRtaState NoneEntityClone()
	    {
		    var clone = (IRtaState)MemberwiseClone();
		    clone.SetId(null);
		    return clone;
	    }

		public virtual IRtaState EntityClone()
	    {
		    return (IRtaState) MemberwiseClone();
	    }
    }
}