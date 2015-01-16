using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherenceDetails
{
	[TestFixture]
	public class ActivityEndedTest
	{
		[Test]
		public void ShouldNotPersistTimeInOfAdherenceWhenInAdherenceBeforeShiftStarts()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var liteUnitOfWorkSync = MockRepository.GenerateMock<ILiteTransactionSyncronization>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new AdherenceDetailsReadModelUpdater(persister, liteUnitOfWorkSync, performanceCounter);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 7:00".Utc(), InAdherence = true });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(new PersonShiftEndEvent { PersonId = personId, ShiftStartTime = "2014-11-17 8:00".Utc(), ShiftEndTime = "2014-11-17 9:00".Utc() });
			persister.Details.First().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
		}


		[Test]
		public void ShouldNotPersistTimeInAdherenceWhenInAdherenceAfterShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var liteUnitOfWorkSync = MockRepository.GenerateMock<ILiteTransactionSyncronization>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new AdherenceDetailsReadModelUpdater(persister, liteUnitOfWorkSync, performanceCounter);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:05".Utc(), InAdherence = true });
			target.Handle(new PersonShiftEndEvent { PersonId = personId, ShiftEndTime = "2014-11-17 9:00".Utc(), ShiftStartTime = "2014-11-17 8:00".Utc() });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 9:05".Utc(), InAdherence = true });

			persister.Details.First().TimeInAdherence.Should().Be(TimeSpan.FromMinutes(55));
			persister.Details.First().TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(5));
		}

		[Test]
		public void ShouldPersistInAdherenceWhenShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var liteUnitOfWorkSync = MockRepository.GenerateMock<ILiteTransactionSyncronization>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new AdherenceDetailsReadModelUpdater(persister, liteUnitOfWorkSync, performanceCounter);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(new PersonShiftEndEvent { PersonId = personId,ShiftStartTime = "2014-11-17 8:00".Utc(), ShiftEndTime = "2014-11-17 9:00".Utc() });
			persister.Details.First().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
		}
	}
}