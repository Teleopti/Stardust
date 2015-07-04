﻿using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.MultipleConfig;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class OutboundScheduledResourcesProviderModuleTest
	{
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void Setup()
		{
			var configuration = new IocConfiguration(new IocArgs(new AppConfigReader()), null);
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(new CommonModule(configuration));
			_containerBuilder.RegisterModule(new OutboundScheduledResourcesProviderModule());
			_containerBuilder.RegisterModule(new SchedulingCommonModule());
			
		}

		[Test]
		public void ShouldResolveOutboundScheduledResourcesProvider()
		{
			using (var ioc = _containerBuilder.Build())
			{
				var provider = ioc.Resolve<OutboundScheduledResourcesProvider>();
				provider.Should().Not.Be.Null();
			}
		}
	}
}