using Autofac;
using NUnit.Framework;
using SharpTestsEx;
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
			containerBuilder.RegisterModule(SchedulePersistModule.ForOtherModules());
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IScheduleRangePersister>()
								 .Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldGetScheduleDictionaryPersister()
		{
			containerBuilder.RegisterModule(SchedulePersistModule.ForOtherModules());
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IScheduleDictionaryPersister>()
				         .Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldGetConflictChecker()
		{
			containerBuilder.RegisterModule(SchedulePersistModule.ForScheduler(null, null));
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IScheduleRangeConflictCollector>()
				         .Should().Be.OfType<ScheduleRangeConflictCollector>();
			}
		}

		[Test]
		public void ShouldGetNonConflictChecker()
		{
			containerBuilder.RegisterModule(SchedulePersistModule.ForOtherModules());
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IScheduleRangeConflictCollector>()
								 .Should().Be.OfType<NoScheduleRangeConflictCollector>();
			}
		}

		[Test]
		public void ShouldGetExplicitSetOwnMessage()
		{
			var reassociater = new reassociater();
			containerBuilder.RegisterModule(SchedulePersistModule.ForScheduler(null, reassociater));
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IReassociateDataForSchedules>()
								 .Should().Be.OfType<reassociater>();
			}
		}

		[Test]
		public void ShouldGetExplicitSeMessageBrokerIdentifier()
		{
			containerBuilder.RegisterModule(SchedulePersistModule.ForScheduler(new FakeInitiatorIdentifier(), null));
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