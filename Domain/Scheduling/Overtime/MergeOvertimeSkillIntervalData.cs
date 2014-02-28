namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface IMergeOvertimeSkillIntervalData
    {
        IOvertimeSkillIntervalData MergeSkillIntervalData(IOvertimeSkillIntervalData interval1, IOvertimeSkillIntervalData interval2);
    }

    public class MergeOvertimeSkillIntervalData : IMergeOvertimeSkillIntervalData
    {
        public IOvertimeSkillIntervalData MergeSkillIntervalData(IOvertimeSkillIntervalData interval1,
                                                                 IOvertimeSkillIntervalData interval2)
        {
            return new OvertimeSkillIntervalData(interval1.Period, interval1.ForecastedDemand + interval2.ForecastedDemand, interval1.CurrentDemand + interval2.CurrentDemand);
        }
    }
}