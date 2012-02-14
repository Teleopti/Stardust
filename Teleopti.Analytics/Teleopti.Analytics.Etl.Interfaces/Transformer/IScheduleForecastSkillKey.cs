using System;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    public interface IScheduleForecastSkillKey
    {
        DateTime StartDateTime { get; }
        int IntervalId { get; }
        Guid SkillCode { get; }
        Guid ScenarioCode { get; }
    }
}