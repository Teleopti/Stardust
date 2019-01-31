

using System;


namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface ISkillStaffPeriodView
    {
        DateTimePeriod Period { get; set; }
        double ForecastedIncomingDemand { get; set; }
        double CalculatedResource { get; set; }
        double FStaff { get; set; }
	    double ForecastedTasks { get; set; }
	    Percent EstimatedServiceLevel { get; set; }
	    Percent EstimatedServiceLevelShrinkage { get; set; }
		TimeSpan AverageHandlingTaskTime { get; set; }
		Percent PercentAnswered { get; set; }
		double AnsweredWithinSeconds { get; set; }
	}
}
