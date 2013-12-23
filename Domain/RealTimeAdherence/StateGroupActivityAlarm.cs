using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public class StateGroupActivityAlarm : VersionedAggregateRootWithBusinessUnit, IStateGroupActivityAlarm

	{
        private readonly IRtaStateGroup _stateGroup;
        private readonly IActivity _activity;
        private IAlarmType _alarmType;

        public StateGroupActivityAlarm(IRtaStateGroup stateGroup, IActivity activity)
        {
            _stateGroup = stateGroup;
            _activity = activity;
        }

        protected StateGroupActivityAlarm()
        {}

        public virtual IActivity Activity
        {
            get { return _activity; }
        }

        public virtual IRtaStateGroup StateGroup
        {
            get { return _stateGroup; }
        }

        public virtual IAlarmType AlarmType
        {
            get { return _alarmType; }
            set { _alarmType = value; }
        }

		public virtual object Clone()
		{
			return EntityClone();
		}

		public virtual IStateGroupActivityAlarm NoneEntityClone()
		{
			var clone = (IStateGroupActivityAlarm)MemberwiseClone();
			clone.SetId(null);
			return clone;
		}

		public virtual IStateGroupActivityAlarm EntityClone()
		{
			return (IStateGroupActivityAlarm) MemberwiseClone();
		}
	}
}