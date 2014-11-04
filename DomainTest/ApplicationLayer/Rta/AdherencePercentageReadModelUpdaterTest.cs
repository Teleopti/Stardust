using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class AdherencePercentageReadModelUpdaterTest
	{
		[Test]
		public void ShouldPersist()
		{
			var persister = new FakeAdherencePercentageReadModelPersister();
			var target = new AdherencePercentageReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent
			{
				PersonId = Guid.NewGuid(),
				Timestamp = new DateTime(2014, 10, 13, 8, 0, 0)
			});

			persister.PersistedModel.TimeInAdherence.Should().Be(TimeSpan.Zero);
		}

		[Test]
		public void ShouldUpdateTimeInAdherenceWhenPersonOutOfAdherence()
		{
			var persister = new FakeAdherencePercentageReadModelPersister();
			var target = new AdherencePercentageReadModelUpdater(persister);
			var personId = Guid.NewGuid();

			target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = new DateTime(2014, 10, 13, 8, 0, 0)
			});
			target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = new DateTime(2014, 10, 13, 8, 10, 0)
			});

			persister.PersistedModel.TimeInAdherence.Should().Be(TimeSpan.FromMinutes(10));
		}

		[Test]
		public void ShouldUpdateTimeOutOfAdherenceWhenPersonInAdherence()
		{
			var persister = new FakeAdherencePercentageReadModelPersister();
			var target = new AdherencePercentageReadModelUpdater(persister);
			var personId = Guid.NewGuid();

			target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = new DateTime(2014, 10, 13, 8, 0, 0)
			});
			target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = new DateTime(2014, 10, 13, 8, 20, 0)
			});

			persister.PersistedModel.TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(20));
		}

		[Test]
		public void ShouldUpdateTimeInAdherenceWhenPersonInAdherence()
		{
			var persister = new FakeAdherencePercentageReadModelPersister();
			var target = new AdherencePercentageReadModelUpdater(persister);
			var personId = Guid.NewGuid();

			target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = new DateTime(2014, 10, 13, 8, 0, 0)
			});
			target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = new DateTime(2014, 10, 13, 9, 0, 0)
			});

			persister.PersistedModel.TimeInAdherence.Should().Be(TimeSpan.FromMinutes(60));
		}

		[Test]
		public void ShouldUpdateTimeOutOfAdherenceWhenPersonOutOfAdherence()
		{
			var persister = new FakeAdherencePercentageReadModelPersister();
			var target = new AdherencePercentageReadModelUpdater(persister);
			var personId = Guid.NewGuid();

			target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = new DateTime(2014, 10, 13, 8, 0, 0)
			});
			target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = new DateTime(2014, 10, 13, 9, 10, 0)
			});

			persister.PersistedModel.TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(70));
		}

		[Test]
		public void ShouldUpdateTimeOutOfAdherenceWhenPersonInAndOutOfAdherence()
		{
			var persister = new FakeAdherencePercentageReadModelPersister();
			var target = new AdherencePercentageReadModelUpdater(persister);
			var personId = Guid.NewGuid();

			target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = new DateTime(2014, 10, 13, 8, 0, 0)
			});
			target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = new DateTime(2014, 10, 13, 8, 15, 0)
			});
			target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = new DateTime(2014, 10, 13, 8, 30, 0)
			});

			persister.PersistedModel.TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(15));
		}

		[Test]
		public void ShouldUpdateTimeInAdherenceWhenPersonOutAndInAdherence()
		{
			var persister = new FakeAdherencePercentageReadModelPersister();
			var target = new AdherencePercentageReadModelUpdater(persister);
			var personId = Guid.NewGuid();

			target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = new DateTime(2014, 10, 13, 8, 0, 0)
			});
			target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = new DateTime(2014, 10, 13, 8, 20, 0)
			});
			target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = new DateTime(2014, 10, 13, 8, 40, 0)
			});

			persister.PersistedModel.TimeInAdherence.Should().Be(TimeSpan.FromMinutes(20));
		}

		[Test]
		public void TimeInAdherence_WhenLoggingInAndOutInShortPeriods_ShouldBeTheSumOfThePeriodsInAdherence()
		{
			var persister = new FakeAdherencePercentageReadModelPersister();
			var target = new AdherencePercentageReadModelUpdater(persister);
			var personId = Guid.NewGuid();
			var startTime = new DateTime(2014, 10, 13, 8, 0, 0);

			target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime
			});

			target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime.AddSeconds(12)
			});

			target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime.AddSeconds(37)
			});

			target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime.AddMinutes(1).AddSeconds(45)
			});

			var expected = TimeSpan.FromSeconds(12)
				.Add(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(45)))
				.Subtract(TimeSpan.FromSeconds(37));

			persister.PersistedModel.TimeInAdherence.Should().Be(expected);
		}

		[Test]
		public void TimeOutOfAdherence_WhenLoggingInAndOutInShortPeriods_ShouldBeTheSumOfThePeriodsOutOfAdherence()
		{
			var persister = new FakeAdherencePercentageReadModelPersister();
			var target = new AdherencePercentageReadModelUpdater(persister);
			var personId = Guid.NewGuid();
			var startTime = new DateTime(2014, 10, 13, 8, 0, 0);

			target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime
			});

			target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime.AddMinutes(3).AddSeconds(12)
			});

			target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime.AddMinutes(4)
			});

			target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = startTime.AddMinutes(4).AddSeconds(1)
			});

			var expected = TimeSpan
				.FromMinutes(3).Add(TimeSpan.FromSeconds(12))
				.Add(TimeSpan.FromSeconds(1));

			persister.PersistedModel.TimeOutOfAdherence.Should().Be(expected);
		}


	}

	

	public class FakeAdherencePercentageReadModelPersister : IAdherencePercentageReadModelPersister
	{
		public void Persist(AdherencePercentageReadModel model)
		{
			PersistedModel = model;
		}

		public AdherencePercentageReadModel Get(DateOnly date, Guid personId)
		{
			if (PersistedModel == null)
				return null;
			if (PersistedModel.BelongsToDate == date && PersistedModel.PersonId.Equals(personId))
				return PersistedModel;
			return null;
		}

		public AdherencePercentageReadModel PersistedModel { get; private set; }
	}
}