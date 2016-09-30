using System;
using Autofac;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class IntegrationIoCTest
	{
		public static IContainer Container { get; private set; }

		public static void Setup()
		{
			Setup(null, null);
		}

		public static void Setup(Action<ContainerBuilder> registrations, object injectTo)
		{
			var builder = new ContainerBuilder();
			var args = new IocArgs(new ConfigReader())
			{
				AllEventPublishingsAsSync = true,
				FeatureToggle = TestSiteConfigurationSetup.URL.ToString()
			};
			var configuration = new IocConfiguration(args, CommonModule.ToggleManagerForIoc(args));

			builder.RegisterModule(new CommonModule(configuration));

			builder.RegisterModule(new TenantServerModule(configuration));

			builder.RegisterType<MutableNow>().AsSelf().As<INow>().SingleInstance();
			builder.RegisterType<DefaultDataCreator>().SingleInstance();
			builder.RegisterType<DefaultAnalyticsDataCreator>().SingleInstance();
			builder.RegisterType<Http>().SingleInstance();
			builder.RegisterType<NoMessageSender>().As<IMessageSender>().SingleInstance();
			builder.RegisterType<Database>().AsSelf().AsImplementedInterfaces().SingleInstance().ApplyAspects();
			builder.RegisterType<AnalyticsDatabase>().AsSelf().AsImplementedInterfaces().SingleInstance().ApplyAspects();

			registrations?.Invoke(builder);

			Container = builder.Build();

			if (injectTo != null)
				IoCTestService.InjectTo(Container, injectTo);
		}

		public static void Dispose()
		{
			Container?.Dispose();
			Container = null;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public class IntegrationIoCTestAttribute : Attribute, ITestAction
	{
		private IoCTestService _service;

		public ActionTargets Targets => ActionTargets.Test;

		protected virtual void BeforeTest()
		{
		}

		protected virtual void AfterTest()
		{
		}

		public void BeforeTest(ITest testDetails)
		{
			_service = new IoCTestService(testDetails, this);
			_service.InjectFrom(IntegrationIoCTest.Container);
			BeforeTest();
		}

		public void AfterTest(ITest testDetails)
		{
			AfterTest();
			_service = null;
		}

	}
}