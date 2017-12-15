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
			PlanningGroupId = ConfigurationManager.AppSettings["PlanningGroupId"];
			PlanningPeriodId = ConfigurationManager.AppSettings["PlanningPeriodId"];
		}

		public static string PlanningGroupId { get; private set; }
		public static string PlanningPeriodId { get; private set; }
		public static string BusinessUnitName { get; private set; }
		public static string Password { get; private set; }
		public static string UserName { get; private set; }
	}
}