using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class SchedulePersistModuleTest
	{
		private ContainerBuilder containerBuilder;

		[SetUp]
		public void Setup()
		{
			containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(CommonModule.ForTest());
		}

		[Test]
		public void ShouldGetScheduleRangePersister()
		{
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IScheduleRangePersister>()
								 .Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldGetScheduleDictionaryPersister()
		{
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IScheduleDictionaryPersister>()
				         .Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldGetConflictChecker()
		{
			containerBuilder.RegisterType<ScheduleRangeConflictCollector>().As<IScheduleRangeConflictCollector>().SingleInstance();
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IScheduleRangeConflictCollector>()
				         .Should().Be.OfType<ScheduleRangeConflictCollector>();
			}
		}

		[Test]
		public void ShouldGetNonConflictChecker()
		{
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IScheduleRangeConflictCollector>()
								 .Should().Be.OfType<NoScheduleRangeConflictCollector>();
			}
		}

		[Test]
		public void ShouldGetExplicitSetOwnMessage()
		{
			containerBuilder.RegisterType<reassociater>().As<IReassociateDataForSchedules>();
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IReassociateDataForSchedules>()
								 .Should().Be.OfType<reassociater>();
			}
		}

		[Test]
		public void ShouldGetExplicitSeMessageBrokerIdentifier()
		{
			containerBuilder.RegisterType<FakeInitiatorIdentifier>().As<IInitiatorIdentifier>();
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IInitiatorIdentifier>()
								 .Should().Be.OfType<FakeInitiatorIdentifier>();
			}
		}

		private class reassociater : IReassociateDataForSchedules
		{
			public void ReassociateDataForAllPeople()
			{
				throw new System.NotImplementedException();
			}

			public void ReassociateDataFor(IPerson person)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}