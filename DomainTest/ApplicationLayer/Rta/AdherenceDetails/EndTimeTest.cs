using System;
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
	public class EndTimeTest
	{
		[Test]
		public void ShouldPersistFirstOutOfAdherenceStateChangeInLastActivity()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var liteUnitOfWorkSync = MockRepository.GenerateMock<ILiteTransactionSyncronization>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new AdherenceDetailsReadModelUpdater(persister, liteUnitOfWorkSync, performanceCounter);
			var personId = Guid.NewGuid();
			target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:30".Utc(),
				InAdherence = false
			});

			target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftStartTime = "2014-11-17 8:00".Utc(),
				ShiftEndTime = "2014-11-17 9:00".Utc(),
			});

			persister.Model.ActualEndTime.Should().Be("2014-11-17 8:30".Utc());
		}

		[Test]
		public void ShouldPersistFirstOutOfAdherenceStateChangeInLastActivity_WhenOnlyOutOfAdherenceStates()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var liteUnitOfWorkSync = MockRepository.GenerateMock<ILiteTransactionSyncronization>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new AdherenceDetailsReadModelUpdater(persister, liteUnitOfWorkSync, performanceCounter);
			var personId = Guid.NewGuid();
			target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:30".Utc(),
				InAdherence = false
			});

			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:37".Utc(),
				InAdherence = false
			});

			target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftStartTime = "2014-11-17 8:00".Utc(),
				ShiftEndTime = "2014-11-17 9:00".Utc(),
			});

			persister.Model.ActualEndTime.Should().Be("2014-11-17 8:30".Utc());
		}

		[Test]
		public void ShouldPersistLastStateChangeThatCausedOutOfAdherence()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var liteUnitOfWorkSync = MockRepository.GenerateMock<ILiteTransactionSyncronization>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new AdherenceDetailsReadModelUpdater(persister, liteUnitOfWorkSync, performanceCounter);
			var personId = Guid.NewGuid();
			target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:47".Utc(),
				InAdherence = false
			});
			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:52".Utc(),
				InAdherence = true
			});
			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:53".Utc(),
				InAdherence = false
			});
			target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftStartTime = "2014-11-17 8:00".Utc(),
				ShiftEndTime = "2014-11-17 9:00".Utc(),
			});
			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 9:13".Utc(),
				InAdherence = false
			});

			persister.Model.ActualEndTime.Should().Be("2014-11-17 8:53".Utc());
		}

		[Test]
		public void ShouldNotSetActualEndTimeIfActualStartNeverOccured()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var liteUnitOfWorkSync = MockRepository.GenerateMock<ILiteTransactionSyncronization>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new AdherenceDetailsReadModelUpdater(persister, liteUnitOfWorkSync, performanceCounter);
			var personId = Guid.NewGuid();

			target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:55".Utc(),
				InAdherence = false
			});

			target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				Name = "Phone",
				StartTime = "2014-11-17 9:00".Utc(),
				InAdherence = false
			});
			target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftStartTime = "2014-11-17 8:00".Utc(),
				ShiftEndTime = "2014-11-17 10:00".Utc(),
			});

			persister.Model.ActualEndTime.Should().Be(null);

		}

		[Test]
		public void ShouldPersistLastStateChangeAfterShiftEnds()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var liteUnitOfWorkSync = MockRepository.GenerateMock<ILiteTransactionSyncronization>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new AdherenceDetailsReadModelUpdater(persister, liteUnitOfWorkSync, performanceCounter);
			var personId = Guid.NewGuid();
			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 7:30".Utc(),
				InAdherence = false
			});
			target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftStartTime = "2014-11-17 8:00".Utc(),
				ShiftEndTime = "2014-11-17 9:00".Utc(),
			});
			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 9:22".Utc(),
				InAdherence = false,
				InAdherenceWithPreviousActivity = true,
			});
			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 9:25".Utc(),
				InAdherence = false,
				InAdherenceWithPreviousActivity = true,
			});
			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 9:30".Utc(),
				InAdherence = false,
				InAdherenceWithPreviousActivity = false,
			});
			persister.Model.ActualEndTime.Should().Be("2014-11-17 9:30".Utc());
		}
		
		[Test]
		public void ShouldPersistShiftEndTime()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var liteUnitOfWorkSync = MockRepository.GenerateMock<ILiteTransactionSyncronization>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new AdherenceDetailsReadModelUpdater(persister, liteUnitOfWorkSync, performanceCounter);
			var personId = Guid.NewGuid();
			target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftStartTime = "2014-11-17 8:00".Utc(),
				ShiftEndTime = "2014-11-17 9:00".Utc(),
			});

			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 9:30".Utc(),
				InAdherence = false
			});

			persister.Model.ShiftEndTime.Should().Be("2014-11-17 9:00".Utc());
		}
	}
}
