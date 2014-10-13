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
		public void ShouldPersistOnFirstEvent()
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
			var model = getModel(@event.PersonId, @event.Timestamp);
			model.MinutesOutOfAdherence += Convert.ToInt32((@event.Timestamp - model.LastTimestamp).TotalMinutes);
			_persister.Persist(model);
		}

		public void Handle(PersonOutOfAdherenceEvent @event)
		{
			var model = getModel(@event.PersonId, @event.Timestamp);
			model.MinutesInAdherence += Convert.ToInt32((@event.Timestamp - model.LastTimestamp).TotalMinutes);
			_persister.Persist(model);
		}

		private AdherencePercentageReadModel getModel(Guid personId, DateTime timestamp)
		{
			var model = _persister.Get(new DateOnly(timestamp), personId);
			if (model == null)
				model = makeModel(personId, timestamp);
			return model;
		}

		private static AdherencePercentageReadModel makeModel(Guid personId, DateTime timestamp)
		{
			return new AdherencePercentageReadModel
			{
				Date = new DateOnly(timestamp),
				PersonId = personId,
				MinutesInAdherence = 0,
				LastTimestamp = timestamp
			};
		}

	}
}