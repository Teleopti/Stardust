using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class AdherencePercentageReadModelUpdaterTest
	{
		[Test]
		public void ShouldPersist()
		{
			var persister = new FakeAdherencePercentageReadModelRepository();
			var target = new AdherencePercentageReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent
			{
				PersonId = Guid.NewGuid(),
				Timestamp = new DateTime(2014, 10, 13, 8, 0, 0)
			});

			persister.PersistedModel.MinutesInAdherence.Should().Be(0);
		}

		[Test]
		public void ShouldUpdateMinutesInAdherenceWhenPersonOutOfAdherence()
		{
			var persister = new FakeAdherencePercentageReadModelRepository();
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

			persister.PersistedModel.MinutesInAdherence.Should().Be(10);
		}

		[Test]
		public void ShouldUpdateMinutesOutOfAdherenceWhenPersonInAdherence()
		{
			var persister = new FakeAdherencePercentageReadModelRepository();
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

			persister.PersistedModel.MinutesOutOfAdherence.Should().Be(20);
		}

		[Test]
		public void ShouldUpdateMinutesInAdherenceWhenPersonInAdherence()
		{
			var persister = new FakeAdherencePercentageReadModelRepository();
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

			persister.PersistedModel.MinutesInAdherence.Should().Be(60);
		}

		[Test]
		public void ShouldUpdateMinutesOutOfAdherenceWhenPersonOutOfAdherence()
		{
			var persister = new FakeAdherencePercentageReadModelRepository();
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

			persister.PersistedModel.MinutesOutOfAdherence.Should().Be(70);
		}

		[Test]
		public void ShouldUpdateMinutesOutOfAdherenceWhenPersonInAndOutOfAdherence()
		{
			var persister = new FakeAdherencePercentageReadModelRepository();
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

			persister.PersistedModel.MinutesOutOfAdherence.Should().Be(15);
		}

		[Test]
		public void ShouldUpdateMinutesInAdherenceWhenPersonOutAndInAdherence()
		{
			var persister = new FakeAdherencePercentageReadModelRepository();
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

			persister.PersistedModel.MinutesInAdherence.Should().Be(20);
		}


	}

	public interface IAdherencePercentageReadModelPersister
	{
		void Persist(AdherencePercentageReadModel model);
		AdherencePercentageReadModel Get(DateOnly date, Guid personId);
	}

	public class FakeAdherencePercentageReadModelRepository : IAdherencePercentageReadModelPersister
	{
		public void Persist(AdherencePercentageReadModel model)
		{
			PersistedModel = model;
		}

		public AdherencePercentageReadModel Get(DateOnly date, Guid personId)
		{
			if (PersistedModel == null)
				return null;
			if (PersistedModel.Date == date && PersistedModel.PersonId.Equals(personId))
				return PersistedModel;
			return null;
		}

		public AdherencePercentageReadModel PersistedModel { get; private set; }
	}

	public class AdherencePercentageReadModel
	{
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
		public int MinutesInAdherence { get; set; }
		public int MinutesOutOfAdherence { get; set; }
		public DateTime LastTimestamp { get; set; }
		public bool IsLastTimeInAdherence { get; set; }
	}

	public class AdherencePercentageReadModelUpdater :
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonOutOfAdherenceEvent>
	{
		private readonly IAdherencePercentageReadModelPersister _persister;

		public AdherencePercentageReadModelUpdater(IAdherencePercentageReadModelPersister persister)
		{
			_persister = persister;
		}

		public void Handle(PersonInAdherenceEvent @event)
		{
			handleEvent(@event.PersonId, @event.Timestamp, true);
		}

		public void Handle(PersonOutOfAdherenceEvent @event)
		{
			handleEvent(@event.PersonId, @event.Timestamp, false);
		}

		private void handleEvent(Guid personId, DateTime timestamp, bool isInAdherence)
		{
			var model = getModel(personId, timestamp, isInAdherence);
			incrementMinutes(model, timestamp);
			model.IsLastTimeInAdherence = isInAdherence;
			_persister.Persist(model);
		}

		private static void incrementMinutes(AdherencePercentageReadModel model, DateTime timestamp)
		{
			if (model.IsLastTimeInAdherence)
				model.MinutesInAdherence += Convert.ToInt32((timestamp - model.LastTimestamp).TotalMinutes);
			else
				model.MinutesOutOfAdherence += Convert.ToInt32((timestamp - model.LastTimestamp).TotalMinutes);
			model.LastTimestamp = timestamp;
		}

		private AdherencePercentageReadModel getModel(Guid personId, DateTime timestamp, bool currentlyInAdherence)
		{
			var model = _persister.Get(new DateOnly(timestamp), personId);
			if (model == null)
				model = makeModel(personId, timestamp, currentlyInAdherence);
			return model;
		}

		private static AdherencePercentageReadModel makeModel(Guid personId, DateTime timestamp, bool currentlyInAdherence)
		{
			return new AdherencePercentageReadModel
			{
				Date = new DateOnly(timestamp),
				PersonId = personId,
				MinutesInAdherence = 0,
				LastTimestamp = timestamp,
				IsLastTimeInAdherence = currentlyInAdherence
			};
		}

	}
}