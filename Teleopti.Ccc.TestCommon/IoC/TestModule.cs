using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class TestModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(typeof(IUserDataSetup<>).Assembly)
				.Where(t => t
					.GetInterfaces()
					.Where(i => i.IsGenericType)
					.Any(i => i.GetGenericTypeDefinition() == typeof(IUserDataSetup<>))
				)
				.As(t => t
					.GetInterfaces()
					.Where(i => i.IsGenericType)
					.Where(i => i.GetGenericTypeDefinition() == typeof(IUserDataSetup<>))
					.Select(i => i)
				)
				.SingleInstance();
			builder.RegisterAssemblyTypes(typeof(IUserSetup<>).Assembly)
				.Where(t => t
					.GetInterfaces()
					.Where(i => i.IsGenericType)
					.Any(i => i.GetGenericTypeDefinition() == typeof(IUserSetup<>))
				)
				.As(t => t
					.GetInterfaces()
					.Where(i => i.IsGenericType)
					.Where(i => i.GetGenericTypeDefinition() == typeof(IUserSetup<>))
					.Select(i => i)
				)
				.SingleInstance();

			builder.RegisterType<TestDataFactory>().SingleInstance();
			builder.RegisterType<AutofacSetupResolver>().As<ISetupResolver>().SingleInstance();

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