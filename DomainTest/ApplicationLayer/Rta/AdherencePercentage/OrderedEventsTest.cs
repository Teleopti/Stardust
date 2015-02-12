using System;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherencePercentage
{
	[AdherenceTest]
	[TestFixture]
	public class OrderedEventsTest : IRegisterInContainer
	{
		public FakeAdherencePercentageReadModelPersister Persister;
		public AdherencePercentageReadModelUpdater Target;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<AdherencePercentageReadModelUpdater>().AsSelf();
		}
		
		[Test]
		public void ShouldPersist()
		{
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = Guid.NewGuid(),
				Timestamp =  "2014-10-13 8:00".Utc()
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be(TimeSpan.Zero);
		}

		[Test]
		public void ShouldUpdateTimeInAdherenceWhenPersonOutOfAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:00".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:10".Utc()
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be("10".Minutes());
		}

		[Test]
		public void ShouldUpdateTimeOutOfAdherenceWhenPersonInAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:00".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:20".Utc()
			});

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("20".Minutes());
		}

		[Test]
		public void ShouldUpdateTimeInAdherenceWhenPersonInAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:00".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 9:00".Utc()
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be("60".Minutes());
		}

		[Test]
		public void ShouldUpdateTimeOutOfAdherenceWhenPersonOutOfAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:00".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 9:10".Utc()
			});

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("70".Minutes());
		}

		[Test]
		public void ShouldUpdateTimeOutOfAdherenceWhenPersonInAndOutOfAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:00".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:15".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:30".Utc()
			});

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("15".Minutes());
		}

		[Test]
		public void ShouldUpdateTimeInAdherenceWhenPersonOutAndInAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:00".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:20".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:40".Utc()
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be("20".Minutes());
		}

		[Test]
		public void TimeInAdherence_WhenLoggingInAndOutInShortPeriods_ShouldBeTheSumOfThePeriodsInAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:00:00".Utc()
			});

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:00:01".Utc()
			});

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:00:02".Utc()
			});

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:00:03".Utc()
			});
			
			Persister.PersistedModel.TimeInAdherence.Should().Be("2".Seconds());
		}

		[Test]
		public void TimeOutOfAdherence_WhenLoggingInAndOutInShortPeriods_ShouldBeTheSumOfThePeriodsOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:00:00".Utc()
			});

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:00:01".Utc()
			});

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:00:02".Utc()
			});

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 8:00:03".Utc()
			});

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("2".Seconds());
		}

		[Test]
		public void ShouldUpdateAdherenceTimesWhenShiftEnds()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 15:00".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 16:00".Utc()
			});
			Target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftEndTime = "2014-10-13 17:00".Utc()
			});

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("60".Minutes());
			Persister.PersistedModel.TimeInAdherence.Should().Be("60".Minutes());
		}

		[Test]
		public void ShouldStopUpdatingAdherenceTimesWhenShiftEnded()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 16:00".Utc()
			});
			Target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftEndTime = "2014-10-13 17:00".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 17:30".Utc()
			});

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be(TimeSpan.Zero);
			Persister.PersistedModel.TimeInAdherence.Should().Be("60".Minutes());
		}

		[Test]
		public void ShouldHaveOneReadModelForOneScheduleDay()
		{
			var personId = Guid.NewGuid();
			var shiftStartTime = "2014-10-06 23:00".Utc();
			var date = new DateOnly(shiftStartTime);
			Target.Handle(new PersonInAdherenceEvent
			{
				ScheduleDate = date,
				PersonId = personId,
				Timestamp = shiftStartTime
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				ScheduleDate = date,
				PersonId = personId,
				Timestamp = "2014-10-07 03:00".Utc()
			});
			Target.Handle(new PersonShiftEndEvent
			{
				ScheduleDate = date,
				PersonId = personId,
				ShiftEndTime = "2014-10-07 07:00".Utc()
			});
			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("4".Hours());
			Persister.PersistedModel.TimeInAdherence.Should().Be("4".Hours());
		}
	}
}