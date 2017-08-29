using Autofac;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class TestModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PerformanceTest.PerformanceTest>().SingleInstance();
			builder.RegisterType<MutableNow>().AsSelf().As<INow>().SingleInstance();
			builder.RegisterType<DefaultDataCreator>().SingleInstance();
			builder.RegisterType<DefaultAnalyticsDataCreator>().SingleInstance();
			builder.RegisterType<Http>().SingleInstance();
			builder.RegisterType<Database>().AsSelf().AsImplementedInterfaces().SingleInstance().ApplyAspects();
			builder.RegisterType<AnalyticsDatabase>().AsSelf().AsImplementedInterfaces().SingleInstance().ApplyAspects();
		}
	}
}