using System;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Transformer
{
    public class ScheduleForecastSkillKey : IScheduleForecastSkillKey
    {
        public ScheduleForecastSkillKey(DateTime startDateTime, int intervalId, Guid skillCode, Guid scenarioCode)
        {
            StartDateTime = startDateTime;
            IntervalId = intervalId;
            SkillCode = skillCode;
            ScenarioCode = scenarioCode;
        }

        public DateTime StartDateTime { get; private set; }
        public int IntervalId { get; private set; }
        public Guid SkillCode { get; private set; }
        public Guid ScenarioCode { get; private set; }
    }
}