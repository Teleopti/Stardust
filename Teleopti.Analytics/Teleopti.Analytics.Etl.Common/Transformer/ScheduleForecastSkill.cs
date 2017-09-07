using System;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ScheduleForecastSkill : IScheduleForecastSkill
	{
		public ScheduleForecastSkill(DateTime startDateTime, int intervalId, Guid skillCode, Guid scenarioCode)
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
		public double ForecastedResourcesMinutes { get; set; }
		public double ForecastedResources { get; set; }
		public double ForecastedResourcesIncludingShrinkageMinutes { get; set; }
		public double ForecastedResourcesIncludingShrinkage { get; set; }
		public double ScheduledResourcesMinutes { get; set; }
		public double ScheduledResources { get; set; }
		public double ScheduledResourcesIncludingShrinkageMinutes { get; set; }
		public double ScheduledResourcesIncludingShrinkage { get; set; }
		public Guid BusinessUnitCode { get; set; }
		public string BusinessUnitName { get; set; }
		public int DataSourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public double EstimatedTasksAnsweredWithinSL { get; set; }
		public double ForecastedTasks { get; set; }
		public double EstimatedTasksAnsweredWithinSLIncludingShrinkage { get; set; }
		public double ForecastedTasksIncludingShrinkage { get; set; }
	}
}