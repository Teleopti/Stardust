using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class AdherenceDetailsReadModelUpdaterTest
	{
		[Test]
		public void ShouldPersist()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent
			{
				PersonId = Guid.NewGuid(),
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc()
			});

			persister.Rows.Single().Name.Should().Be("Phone");
		}

		[Test]
		public void ShouldPersistEachActivityStarted()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent
			{
				PersonId = Guid.NewGuid(),
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc()
			});
			target.Handle(new PersonActivityStartEvent
			{
				PersonId = Guid.NewGuid(),
				Name = "Break",
				StartTime = "2014-11-17 9:00".Utc()
			});

			persister.Rows.Select(x => x.Name).Should().Have.SameValuesAs("Phone", "Break");
		}

		[Test]
		public void ShouldPersistActivitiesStartTime()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent
			{
				PersonId = Guid.NewGuid(),
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc()
			});

			persister.Rows.Single().StartTime.Should().Be("2014-11-17 8:00".Utc());
		}

		[Test]
		public void ShouldPersistActualStartTime()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc()
			});
			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:00".Utc()
			});
			target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:00".Utc()
			});

			persister.Rows.Single().ActualStartTime.Should().Be("2014-11-17 8:00".Utc());
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenLater()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone" });
			target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 8:00".Utc() });

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:05".Utc() });
			target.Handle(new PersonInAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 8:05".Utc() });

			persister.Rows.Single().ActualStartTime.Should().Be("2014-11-17 8:05".Utc());
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenEarlier()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone" });
			target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 8:00".Utc() });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc() });
			target.Handle(new PersonInAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc() });

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc() });
			target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc() });

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Break" });
			target.Handle(new PersonInAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 9:00".Utc() });

			persister.Rows.Last().ActualStartTime.Should().Be("2014-11-17 8:55".Utc());
		}

		[Test]
		public void ShouldNotPersistActualStartTimeWhenNeverStarted()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone" });
			target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 8:00".Utc() });

			persister.Rows.Single().ActualStartTime.Should().Be(null);
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenRedundantInAdherenceEvents()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone" });
			target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 8:00".Utc() });

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc() });
			target.Handle(new PersonInAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc() });

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:10".Utc() });
			target.Handle(new PersonInAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 8:15".Utc() });

			persister.Rows.Single().ActualStartTime.Should().Be("2014-11-17 8:02".Utc());
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenAdherenceDoesNotChangeWhenActivityChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone" });
			target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 8:00".Utc() });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc() });
			target.Handle(new PersonInAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc() });

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Phone 2" });

			persister.Rows.Last().ActualStartTime.Should().Be("2014-11-17 9:00".Utc());
		}

		[Test]
		public void ShouldNotPersistActualStartTimeWhenSecondActivityNeverStarted()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone" });
			target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 8:00".Utc() });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc() });
			target.Handle(new PersonInAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc() });

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Lunch" });
			target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, Timestamp = "2014-11-17 9:00".Utc() });

			persister.Rows.Last().ActualStartTime.Should().Be(null);
		}
	}

	public class AdherenceDetailsReadModelUpdater :
		IHandleEvent<PersonActivityStartEvent>,
		IHandleEvent<PersonStateChangedEvent>,
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonOutOfAdherenceEvent>
	{
		private readonly IAdherenceDetailsReadModelPersister _persister;

		public AdherenceDetailsReadModelUpdater(IAdherenceDetailsReadModelPersister persister)
		{
			_persister = persister;
		}

		public void Handle(PersonActivityStartEvent @event)
		{
			var model = new AdherenceDetailsReadModel
			{
				Name = @event.Name,
				StartTime = @event.StartTime,
				PersonId = @event.PersonId,
				Date = new DateOnly(@event.StartTime)
			};
			var previous = _persister.Get(@event.PersonId, new DateOnly(@event.StartTime)).LastOrDefault();
			if (previous != null)
			{
				model.LastStateChangedTime = previous.LastStateChangedTime;
				model.IsInAdherence = previous.IsInAdherence;
				if (previous.IsInAdherence)
					model.ActualStartTime = @event.StartTime;
			}
			_persister.Add(model);
		}

		public void Handle(PersonStateChangedEvent @event)
		{
			var existingModel = _persister.Get(@event.PersonId, new DateOnly(@event.Timestamp)).Last();
			existingModel.LastStateChangedTime = @event.Timestamp;
			_persister.Update(existingModel);
		}

		public void Handle(PersonInAdherenceEvent @event)
		{
			var existingModel = _persister.Get(@event.PersonId, new DateOnly(@event.Timestamp)).Last();
			if (!existingModel.ActualStartTime.HasValue)
				existingModel.ActualStartTime = existingModel.LastStateChangedTime;
			existingModel.IsInAdherence = true;
			_persister.Update(existingModel);
		}

		public void Handle(PersonOutOfAdherenceEvent @event)
		{
			var existingModel = _persister.Get(@event.PersonId, new DateOnly(@event.Timestamp)).Last();
			existingModel.IsInAdherence = false;
			if (personWentOutOfAdherenceWhenTheActivityStarted(@event, existingModel))
				existingModel.ActualStartTime = null;
			_persister.Update(existingModel);
		}

		private static bool personWentOutOfAdherenceWhenTheActivityStarted(PersonOutOfAdherenceEvent @event, AdherenceDetailsReadModel existingModel)
		{
			return existingModel.StartTime == @event.Timestamp;
		}
	}

	public class FakeAdherenceDetailsReadModelPersister : IAdherenceDetailsReadModelPersister
	{
		public IList<AdherenceDetailsReadModel> Rows = new List<AdherenceDetailsReadModel>();

		public void Add(AdherenceDetailsReadModel model)
		{
			Rows.Add(model);
			Rows = Rows.OrderBy(x => x.StartTime).ToList();
		}

		public void Update(AdherenceDetailsReadModel model)
		{
			var existing = from m in Rows
						   where m.PersonId == model.PersonId &&
								 m.StartTime == model.StartTime
						   select m;
			Rows.Remove(existing.Single());
			Rows.Add(model);
			Rows = Rows.OrderBy(x => x.StartTime).ToList();
		}

		public IEnumerable<AdherenceDetailsReadModel> Get(Guid personId, DateOnly date)
		{
			return from m in Rows
				   where m.PersonId == personId &&
						 m.Date == date
				   select new AdherenceDetailsReadModel
				   {
					   PersonId = m.PersonId,
					   Date = m.Date,
					   StartTime = m.StartTime,
					   Name = m.Name,
					   ActualStartTime = m.ActualStartTime,
					   LastStateChangedTime = m.LastStateChangedTime,
					   IsInAdherence = m.IsInAdherence
				   };
		}
	}

	public class AdherenceDetailsReadModel
	{
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
		public string Name { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime? ActualStartTime { get; set; }

		public DateTime? LastStateChangedTime { get; set; }
		public bool IsInAdherence { get; set; }
	}

	public interface IAdherenceDetailsReadModelPersister
	{
		void Add(AdherenceDetailsReadModel model);
		void Update(AdherenceDetailsReadModel model);
		IEnumerable<AdherenceDetailsReadModel> Get(Guid personId, DateOnly date);
	}
}