using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public class StateGroupActivityAlarm : AggregateRootWithBusinessUnit, IStateGroupActivityAlarm, IDeleteTag
    {
        private readonly IRtaStateGroup _stateGroup;
        private readonly IActivity _activity;
        private IAlarmType _alarmType;
        private bool _isDeleted;

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