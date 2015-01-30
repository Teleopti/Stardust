using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherenceDetails
{
	[TestFixture]
	public class OutOfAdherenceTest
	{
		[Test]
		public void ShouldPersistTimeOutAdherenceWhenStateChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var liteUnitOfWorkSync = MockRepository.GenerateMock<ILiteTransactionSyncronization>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new AdherenceDetailsReadModelUpdater(persister, liteUnitOfWorkSync, performanceCounter);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 9:00".Utc(), InAdherence = true });
			persister.Details.Single().TimeOutOfAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeOutAdherenceWhenActivityChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var liteUnitOfWorkSync = MockRepository.GenerateMock<ILiteTransactionSyncronization>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new AdherenceDetailsReadModelUpdater(persister, liteUnitOfWorkSync, performanceCounter);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Lunch", InAdherence = false });
			persister.Details.First().TimeOutOfAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeOutOfAdherenceWhenInAdherenceBeforeActivityChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var liteUnitOfWorkSync = MockRepository.GenerateMock<ILiteTransactionSyncronization>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new AdherenceDetailsReadModelUpdater(persister, liteUnitOfWorkSync, performanceCounter);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:00".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Lunch", InAdherence = false });
			persister.Details.First().TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(5));
		}

		[Test]
		public void ShouldSummarizeAllOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var liteUnitOfWorkSync = MockRepository.GenerateMock<ILiteTransactionSyncronization>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new AdherenceDetailsReadModelUpdater(persister, liteUnitOfWorkSync, performanceCounter);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:10".Utc(), InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:20".Utc(), InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:25".Utc(), InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:45".Utc(), InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc(), InAdherence = false });
			persister.Details.First().TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(10 + 5 + 10));
		}
	}
}