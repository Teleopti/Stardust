using System.Collections.Generic;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.IocCommon.Configuration;
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
			containerBuilder.RegisterModule<UnitOfWorkModule>();
			containerBuilder.RegisterModule<RepositoryModule>();
			containerBuilder.RegisterModule<AuthenticationModule>();
		}

		[Test]
		public void ShouldGetScheduleRangePersister()
		{
			containerBuilder.RegisterModule(new SchedulePersistModule(null, null, false));
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IScheduleRangePersister>()
								 .Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldGetScheduleDictionaryPersister()
		{
			containerBuilder.RegisterModule(new SchedulePersistModule(null, null, false));
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IScheduleDictionaryPersister>()
				         .Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldGetConflictChecker()
		{
			containerBuilder.RegisterModule(new SchedulePersistModule(null, null, true));
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IScheduleRangeConflictCollector>()
				         .Should().Be.OfType<ScheduleRangeConflictCollector>();
			}
		}

		[Test]
		public void ShouldGetNonConflictChecker()
		{
			containerBuilder.RegisterModule(new SchedulePersistModule(null, null, false));
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IScheduleRangeConflictCollector>()
								 .Should().Be.OfType<NoScheduleRangeConflictCollector>();
			}
		}

		//rk: don't like
		[Test]
		public void ShouldGetExplicitSetOwnMessage()
		{
			var ownMessage = new ownMessageImpl();
			containerBuilder.RegisterModule(new SchedulePersistModule(null, ownMessage, false));
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IReassociateDataForSchedules>()
								 .Should().Be.OfType<ownMessageImpl>();
			}
		}

		//rk: don't like
		[Test]
		public void ShouldGetExplicitSeMessageBrokerIdentifier()
		{
			containerBuilder.RegisterModule(new SchedulePersistModule(new FakeMessageBrokerIdentifier(), null, false));
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IMessageBrokerIdentifier>()
								 .Should().Be.OfType<FakeMessageBrokerIdentifier>();
			}
		}

		private class ownMessageImpl : IReassociateDataForSchedules
		{
			public void ReassociateDataForAllPeople()
			{
				throw new System.NotImplementedException();
			}

			public IEnumerable<IAggregateRoot>[] DataToReassociate(IPerson personToReassociate)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}