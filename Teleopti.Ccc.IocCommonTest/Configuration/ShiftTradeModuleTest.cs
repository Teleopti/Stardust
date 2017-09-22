using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class ShiftTradeModuleTest
	{
		private ContainerBuilder builder;

		[SetUp]
		public void Setup()
		{
			builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());
		
		}

		[Test]
		public void ShouldRegisterShiftTradeSpecifications()
		{
			const int noOfSpecs = 6;
			using (var ioc = builder.Build())
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
			using (var ioc = builder.Build())
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
			using (var ioc = builder.Build())
			{
				ioc.Resolve<IShiftTradeValidator>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void CanCreateShiftTradeLightValidator()
		{
			using (var ioc = builder.Build())
			{
				ioc.Resolve<IShiftTradeLightValidator>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void CanCreateShiftTradeRequestSetChecksum()
		{
			using (var ioc = builder.Build())
			{
				ioc.Resolve<IShiftTradeRequestSetChecksum>().Should().Not.Be.Null();
			}		
		}

		[Test]
		public void CanCreateShiftTradeRequestStatusChecker()
		{
			//just a dummy impl used here
			builder.RegisterType<personRequestCheckAuthorization>().As<IPersonRequestCheckAuthorization>();
			using (var ioc = builder.Build())
			{
				ioc.Resolve<IShiftTradeRequestStatusChecker>().Should().Be.InstanceOf<ShiftTradeRequestStatusChecker>();
			}
		}

		private class personRequestCheckAuthorization : IPersonRequestCheckAuthorization
		{
			public void VerifyEditRequestPermission(IPersonRequest personRequest)
			{
			}

			public bool HasEditRequestPermission(IPersonRequest personRequest)
			{
				return false;
			}

			public bool HasViewRequestPermission(IPersonRequest personRequest)
			{
				return false;
			}

			public bool HasCancelRequestPermission (IPersonRequest personRequest)
			{
				return false;
			}
		}
	}
}