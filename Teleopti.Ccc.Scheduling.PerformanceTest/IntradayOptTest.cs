using System;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver.CoypuImpl;

namespace Teleopti.Ccc.Scheduling.PerformanceTest
{
	public class IntradayOptTest
	{
		[Test]
		[Category("IntradayOptimization")]
		public void MeasurePerformance()
		{
			using (var browserActivator = new CoypuChromeActivator())
			{
				browserActivator.Start(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(500));
				var browserInteractions = browserActivator.GetInteractions();

				WebAction.Logon(browserInteractions, AppConfigs.BusinessUnitName, AppConfigs.UserName, AppConfigs.Password);

				doIntraday(browserInteractions, AppConfigs.PlanningPeriodId);

				using (new TimeoutScope(browserActivator, TimeSpan.FromDays(1)))
				{
				}
			}
		}

		private static void doIntraday(IBrowserInteractions browserInteractions, string planningPeriodId)
		{
			
		}
	}
}