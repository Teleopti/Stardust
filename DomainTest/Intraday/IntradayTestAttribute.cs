using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	class IntradayTestAttribute : DomainTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			//system.AddModule(new WebModule(configuration, null));
			//system.AddModule(new IntradayAreaModule());
			system.UseTestDouble<IntradaySkillStatusService>().For<IIntradaySkillStatusService>();
			system.UseTestDouble<SkillForecastedTasksProvider>().For<ISkillForecastedTasksProvider>();
			system.UseTestDouble<SkillActualTasksProvider>().For<ISkillActualTasksProvider>();
		}
	}
}
