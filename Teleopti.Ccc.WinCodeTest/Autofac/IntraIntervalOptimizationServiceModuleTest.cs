﻿using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.WinCode.Autofac;

namespace Teleopti.Ccc.WinCodeTest.Autofac
{
	[TestFixture]
	class IntraIntervalOptimizationServiceModuleTest
	{
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void Setup()
		{
			var configuration = new IocConfiguration(new IocArgs(), null);
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(new CommonModule(configuration));
			_containerBuilder.RegisterModule(new SchedulingCommonModule(configuration));
		}

		[Test]
		public void ShouldResolve()
		{
			_containerBuilder.RegisterModule<IntraIntervalOptimizationServiceModule>();
			using (var container = _containerBuilder.Build())
			{
				container.Resolve<IIntraIntervalOptimizationService>().Should().Not.Be.Null();
			}
		}
	}
}
