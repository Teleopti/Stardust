using System;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[RealHangfire]
	public class HangfireTestAttribute : AnalyticsDatabaseTestAttribute
	{
		public HangfireClientStarter Starter;
		public Lazy<HangfireUtilities> Hangfire;
		
		protected override void BeforeTest()
		{
			base.BeforeTest();

			Starter.Start();
			Hangfire.Value.CleanQueue();
		}
		
	}
}