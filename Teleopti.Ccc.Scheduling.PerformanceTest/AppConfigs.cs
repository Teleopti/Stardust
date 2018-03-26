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
			PlanningGroupId = new Guid(ConfigurationManager.AppSettings["PlanningGroupId"]);
			PlanningPeriodId = new Guid(ConfigurationManager.AppSettings["PlanningPeriodId"]);
		}

		public static Guid PlanningGroupId { get; }
		public static Guid PlanningPeriodId { get; }
		public static string BusinessUnitName { get; }
		public static string Password { get; }
		public static string UserName { get; }
	}
}