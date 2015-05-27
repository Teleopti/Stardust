using System;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
    public interface IScheduleForecastSkillKey
    {
        DateTime StartDateTime { get; }
        int IntervalId { get; }
        Guid SkillCode { get; }
        Guid ScenarioCode { get; }
    }
}