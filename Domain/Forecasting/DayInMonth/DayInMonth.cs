using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.DayInMonth
{
    public class DayInMonth : VolumeYear
    {
        public DayInMonth(ITaskOwnerPeriod taskOwnerPeriod) : base(taskOwnerPeriod)
        {
        }

        public override void ReloadHistoricalDataDepth(ITaskOwnerPeriod taskOwnerPeriod)
        {
            throw new System.NotImplementedException();
        }

        public override double TaskIndex(DateOnly dateTime)
        {
            throw new System.NotImplementedException();
        }

        public override double TalkTimeIndex(DateOnly dateTime)
        {
            throw new System.NotImplementedException();
        }

        public override double AfterTalkTimeIndex(DateOnly dateTime)
        {
            throw new System.NotImplementedException();
        }
    }
}