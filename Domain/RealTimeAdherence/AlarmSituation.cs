using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public class AlarmSituation : VisualLayer
    {
        public AlarmSituation(IAlarmType alarmType, DateTimePeriod period, IPerson person) : base(alarmType,period,new Activity("Dummy"),person)
        {
        }
    }
}