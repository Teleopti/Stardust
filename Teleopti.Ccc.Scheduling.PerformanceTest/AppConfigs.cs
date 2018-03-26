using System;
using System.Configuration;

namespace Teleopti.Ccc.Scheduling.PerformanceTest
{
	public static class AppConfigs
	{
		static AppConfigs()
		{
			UserName = ConfigurationManager.AppSettings["UserName"];
			Password = ConfigurationManager.AppSettings["Password"];
			BusinessUnitName = ConfigurationManager.AppSettings["BusinessUnitName"];
			if(Guid.TryParse(ConfigurationManager.AppSettings["PlanningGroupId"], out var planningGroupId))
			{
				PlanningGroupId = planningGroupId;
			}
			if(Guid.TryParse(ConfigurationManager.AppSettings["PlanningPeriodId"], out var planningPeriodId))
			{
				PlanningPeriodId = planningPeriodId;
			}
		}

		public static Guid PlanningGroupId { get; }
		public static Guid PlanningPeriodId { get; }
		public static string BusinessUnitName { get; }
		public static string Password { get; }
		public static string UserName { get; }
	}
}