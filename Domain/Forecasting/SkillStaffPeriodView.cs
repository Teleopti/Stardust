using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class SkillStaffPeriodView : ISkillStaffPeriodView
    {
        public DateTimePeriod Period { get; set; }
        public double ForecastedIncomingDemand { get; set; }
        public double CalculatedResource { get; set; }
        public double FStaff { get; set; }
	    public double ForecastedTasks { get; set; }
	    public Percent EstimatedServiceLevel { get; set; }
	    public Percent EstimatedServiceLevelShrinkage { get; set; }
		public TimeSpan AverageHandlingTaskTime { get; set; }
		public Percent PercentAnswered { get; set; }
		public double AnsweredWithinSeconds { get; set; }
	}
}
