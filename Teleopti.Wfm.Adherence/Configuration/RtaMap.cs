using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Configuration.Events;

namespace Teleopti.Wfm.Adherence.Configuration
{
    public class RtaMap : VersionedAggregateRootWithBusinessUnit, IRtaMap
	{
        private readonly IRtaStateGroup _stateGroup;
        private readonly IActivity _activity;
        private IRtaRule _rtaRule;

        public RtaMap(IRtaStateGroup stateGroup, IActivity activity)
        {
            _stateGroup = stateGroup;
            _activity = activity;
        }

        protected RtaMap()
        {}

		public override void NotifyTransactionComplete(DomainUpdateType operation)
		{
			base.NotifyTransactionComplete(operation);
			AddEvent(new RtaMapChangedEvent());
		}

		public virtual IActivity Activity
        {
            get { return _activity; }
        }

        public virtual IRtaStateGroup StateGroup
        {
            get { return _stateGroup; }
        }

        public virtual IRtaRule RtaRule
        {
            get { return _rtaRule; }
            set { _rtaRule = value; }
        }

		public virtual object Clone()
		{
			return EntityClone();
		}

		public virtual IRtaMap NoneEntityClone()
		{
			var clone = (IRtaMap)MemberwiseClone();
			clone.SetId(null);
			return clone;
		}

		public virtual IRtaMap EntityClone()
		{
			return (IRtaMap) MemberwiseClone();
		}
	}
}