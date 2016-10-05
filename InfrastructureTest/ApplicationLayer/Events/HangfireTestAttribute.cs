using System;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	public class HangfireTestAttribute : AnalyticsDatabaseTestAttribute
	{
		public HangfireClientStarter Starter;
		public Lazy<HangfireUtilities> Hangfire;

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			system.AddService<HangfireEventClient>();// register over the fake, which already registeres over the common registration ;)
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();

			Starter.Start();
			Hangfire.Value.CleanQueue();
		}
		
	}
}