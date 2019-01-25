using System.Web;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.IoC;
using ConfigReader = Teleopti.Ccc.Domain.Config.ConfigReader;

namespace Teleopti.Ccc.WebTest.Core.IoC
{
	[TestFixture]
	public class WebModuleTest
	{
		[SetUp]
		public void Setup()
		{
			HttpContext.Current = null;
			webContainer = buildContainer();
			SignalRConfiguration.Configure(SignalRSettings.Load(), () => { });
		}

		[TearDown]
		public void Teardown()
		{
			if (webContainer != null)
			{
				webContainer.Dispose();
				webContainer = null;
			}
		}

		private ILifetimeScope buildContainer()
		{
			var args = new IocArgs(new ConfigReader());
			return buildContainer(CommonModule.ToggleManagerForIoc(args));
		}

		private ILifetimeScope buildContainer(IToggleManager toggleManager)
		{
			var builder = new ContainerBuilder();
			var args = new IocArgs(new ConfigReader());
			var iocConfig = new IocConfiguration(args, toggleManager);
			builder.RegisterModule(new IntradayWebModule(iocConfig));
			builder.RegisterModule(new StaffingModule(iocConfig));
			builder.RegisterModule(new WebModule(iocConfig, null));
			builder.RegisterType<MutableNow>().As<INow>().As<IMutateNow>().SingleInstance();
			builder.RegisterInstance(MockRepository.GenerateMock<ISkillCombinationResourceRepository>()).As<ISkillCombinationResourceRepository>();
			builder.RegisterInstance(MockRepository.GenerateMock<IStaffingSettingsReader>()).As<IStaffingSettingsReader>();
			builder.RegisterInstance(MockRepository.GenerateMock<IDisableDeletedFilter>()).As<IDisableDeletedFilter>();
			builder.RegisterInstance(MockRepository.GenerateMock<IPersonSkillProvider>()).As<IPersonSkillProvider>();
			builder.RegisterInstance(MockRepository.GenerateMock<ISkillRepository>()).As<ISkillRepository>();
			
			return builder.Build();
		}

		private ILifetimeScope webContainer;

		

		[Test]
		public void ShouldResolveScheduleDayDifferenceSaver()
		{
			var saver = webContainer.Resolve<IScheduleDayDifferenceSaver>();
			saver.Should().Not.Be.Null();
			saver.GetType().Should().Be(typeof(ScheduleDayDifferenceSaver));
		}
	}
}