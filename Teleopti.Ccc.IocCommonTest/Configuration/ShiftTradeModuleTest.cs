using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class ShiftTradeModuleTest
	{
		private ContainerBuilder containerBuilder;

		[SetUp]
		public void Setup()
		{
			containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<ShiftTradeModule>();
			containerBuilder.RegisterModule<DateAndTimeModule>();
			containerBuilder.RegisterModule<AuthenticationModule>();
			containerBuilder.RegisterType<SchedulingResultStateHolder>().As<ISchedulingResultStateHolder>();
		}

		[Test]
		public void ShouldRegisterShiftTradeSpecifications()
		{
			const int noOfSpecs = 4;
			using (var ioc = containerBuilder.Build())
			{
				var specs = ioc.Resolve<IEnumerable<IShiftTradeSpecification>>().ToArray();
				specs.Count().Should().Be.EqualTo(noOfSpecs);
				var uniqueTypes = new HashSet<Type>();
				specs.ForEach(spec => uniqueTypes.Add(spec.GetType()));
				uniqueTypes.Count.Should().Be.EqualTo(noOfSpecs);
			}
		}

		[Test]
		public void ShouldRegisterShiftTradeLightSpecifications()
		{
			const int noOfSpecs = 2;
			using (var ioc = containerBuilder.Build())
			{
				var specs = ioc.Resolve<IEnumerable<IShiftTradeLightSpecification>>().ToArray();
				specs.Count().Should().Be.EqualTo(noOfSpecs);
				var uniqueTypes = new HashSet<Type>();
				specs.ForEach(spec => uniqueTypes.Add(spec.GetType()));
				uniqueTypes.Count.Should().Be.EqualTo(noOfSpecs);
			}
		}

		[Test]
		public void CanCreateShiftTradeValidator()
		{
			using (var ioc = containerBuilder.Build())
			{
				ioc.Resolve<IShiftTradeValidator>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void CanCreateShiftTradeLightValidator()
		{
			using (var ioc = containerBuilder.Build())
			{
				ioc.Resolve<IShiftTradeLightValidator>().Should().Not.Be.Null();
			}
		}
	}
}