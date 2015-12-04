using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
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