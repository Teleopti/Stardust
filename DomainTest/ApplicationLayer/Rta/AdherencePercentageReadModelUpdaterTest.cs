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

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[AdherenceTest]
	[TestFixture]
	public class AdherencePercentageReadModelUpdaterTest : IRegisterInContainer
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

			Persister.PersistedModel.TimeInAdherence.Should().Be(TimeSpan.FromMinutes(10));
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

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(20));
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

			Persister.PersistedModel.TimeInAdherence.Should().Be(TimeSpan.FromMinutes(60));
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

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(70));
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

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(15));
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

			Persister.PersistedModel.TimeInAdherence.Should().Be(TimeSpan.FromMinutes(20));
		}

		[Test]
		public void TimeInAdherence_WhenLoggingInAndOutInShortPeriods_ShouldBeTheSumOfThePeriodsInAdherence()
		{
			var personId = Guid.NewGuid();
			var startTime = "2014-10-13 8:00".Utc();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime
			});

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime.AddSeconds(12)
			});

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime.AddSeconds(37)
			});

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime.AddMinutes(1).AddSeconds(45)
			});

			var expected = TimeSpan.FromSeconds(12)
				.Add(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(45)))
				.Subtract(TimeSpan.FromSeconds(37));

			Persister.PersistedModel.TimeInAdherence.Should().Be(expected);
		}

		[Test]
		public void TimeOutOfAdherence_WhenLoggingInAndOutInShortPeriods_ShouldBeTheSumOfThePeriodsOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			var startTime = "2014-10-13 8:00".Utc();

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime
			});

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime.AddMinutes(3).AddSeconds(12)
			});

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime.AddMinutes(4)
			});

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime.AddMinutes(4).AddSeconds(1)
			});

			var expected = TimeSpan
				.FromMinutes(3).Add(TimeSpan.FromSeconds(12))
				.Add(TimeSpan.FromSeconds(1));

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be(expected);
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

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(60));
			Persister.PersistedModel.TimeInAdherence.Should().Be(TimeSpan.FromMinutes(60));
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
			Persister.PersistedModel.TimeInAdherence.Should().Be(TimeSpan.FromMinutes(60));
		}

	}
}