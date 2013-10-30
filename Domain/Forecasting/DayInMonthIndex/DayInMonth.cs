using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.DayInMonthIndex
{
    public class DayInMonth : VolumeYear
    {
        public DayInMonth(ITaskOwnerPeriod taskOwnerPeriod, IDayInMonthCreator creator) : base(taskOwnerPeriod)
        {
            SetTaskOwnerDaysCollection(taskOwnerPeriod);
            creator.Create(this);
        }

        public override void ReloadHistoricalDataDepth(ITaskOwnerPeriod taskOwnerPeriod)
        {
            //throw new System.NotImplementedException();
        }

        public override double TaskIndex(DateOnly dateTime)
        {
            return PeriodTypeCollection[DayInMonthHelper.DayIndex(dateTime)].TaskIndex;
        }

        public override double TalkTimeIndex(DateOnly dateTime)
        {
            return PeriodTypeCollection[DayInMonthHelper.DayIndex(dateTime)].TalkTimeIndex;
        }

        public override double AfterTalkTimeIndex(DateOnly dateTime)
        {
            return PeriodTypeCollection[DayInMonthHelper.DayIndex(dateTime)].AfterTalkTimeIndex;
        }
    }
}