using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class AdherenceDetailsReadModelUpdaterTest
	{
		#region starttimes
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
		public void ShouldPersistActivitiesStartTimeWhenStateChangedBeforeShiftStarted()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 7:55".Utc(),
				InAdherence = false
			});
			target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});

			persister.Rows.Single().ActualStartTime.Should().Be("2014-11-17 7:55".Utc());
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
				Timestamp = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			persister.Rows.Single().ActualStartTime.Should().Be("2014-11-17 8:00".Utc());
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenLater()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone" , InAdherence = false});

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:05".Utc(), InAdherence = true});

			persister.Rows.Single().ActualStartTime.Should().Be("2014-11-17 8:05".Utc());
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenEarlier()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone" , InAdherence = false});
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc() ,InAdherence = true});

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc(), InAdherence = false});

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Break", InAdherence = true});

			persister.Rows.Last().ActualStartTime.Should().Be("2014-11-17 8:55".Utc());
		}

		[Test]
		public void ShouldNotPersistActualStartTimeWhenNeverStarted()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false});

			persister.Rows.Single().ActualStartTime.Should().Be(null);
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenRedundantInAdherenceEvents()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false});

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc(), InAdherence = true});

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:10".Utc(), InAdherence = true});

			persister.Rows.Single().ActualStartTime.Should().Be("2014-11-17 8:02".Utc());
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenAdherenceDoesNotChangeWhenActivityChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone" , InAdherence = false});
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc() , InAdherence = true});
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Phone 2", InAdherence = true });

			persister.Rows.Last().ActualStartTime.Should().Be("2014-11-17 9:00".Utc());
		}

		[Test]
		public void ShouldNotPersistActualStartTimeWhenSecondActivityNeverStarted()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc(), InAdherence = true});
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Lunch" , InAdherence = false});

			persister.Rows.Last().ActualStartTime.Should().Be(null);
		}

		#endregion starttimes

		#region inadherence
		[Test]
		public void ShouldPersistTimeInAdherenceWhenActivityStarts()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 7:55".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });

			persister.Rows.Single().TimeInAdherence.Should().Be(TimeSpan.Zero);
		}

		[Test]
		public void ShouldPersistTimeInAdherenceWhenStateChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 7:55".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 9:00".Utc(), InAdherence = false });
			persister.Rows.Single().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeInAdherenceWhenActivityChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 7:55".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Lunch", InAdherence = false });
			persister.Rows.First().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeInAdherenceWhenOutAdherenceBeforeActivityChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 7:55".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Lunch", InAdherence = false });
			persister.Rows.First().TimeInAdherence.Should().Be(TimeSpan.FromMinutes(55));
		}

		[Test]
		public void ShouldSummarizeAllInAdherenceforOneActivity()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:05".Utc(), InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:30".Utc(), InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:40".Utc(), InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:45".Utc(), InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc(), InAdherence = false });
			persister.Rows.First().TimeInAdherence.Should().Be(TimeSpan.FromMinutes(25+10+10));
		}

		#endregion inadherence

		#region outofadherence
	

		[Test]
		public void ShouldPersistTimeOutAdherenceWhenStateChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 9:00".Utc(), InAdherence = true });
			persister.Rows.Single().TimeOutAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeOutAdherenceWhenActivityChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Lunch", InAdherence = false });
			persister.Rows.First().TimeOutAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeOutOfAdherenceWhenInAdherenceBeforeActivityChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:00".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Lunch", InAdherence = false });
			persister.Rows.First().TimeOutAdherence.Should().Be(TimeSpan.FromMinutes(5));
		}

		[Test]
		public void ShouldSummarizeAllOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:10".Utc(), InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:20".Utc(), InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:25".Utc(), InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:45".Utc(), InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc(), InAdherence = false });
			persister.Rows.First().TimeOutAdherence.Should().Be(TimeSpan.FromMinutes(10+5+10));
		}
		#endregion outofadherence

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
				Date = new DateOnly(@event.StartTime),
				IsInAdherence = @event.InAdherence
			};
			var previous = _persister.Get(@event.PersonId, new DateOnly(@event.StartTime)).LastOrDefault();
			if (previous != null)
			{
				if (@event.InAdherence && previous.IsInAdherence)
				{
					model.ActualStartTime = @event.StartTime;
				}
				if (@event.InAdherence && !previous.IsInAdherence)
				{
					model.ActualStartTime = previous.LastStateChangedTime;
				}
				if (previous.Name == null)
					_persister.Remove(model.PersonId, model.Date);
				else
				{
					if (previous.IsInAdherence)
					{
						if (previous.LastStateChangedTime != null)
						{
							previous.TimeInAdherence += @event.StartTime - previous.LastStateChangedTime.Value;
						}
						else
						{
							previous.TimeInAdherence += @event.StartTime - previous.StartTime;
						}
					}
					else
					{
						if (previous.LastStateChangedTime != null)
						{
							previous.TimeOutAdherence += @event.StartTime - previous.LastStateChangedTime.Value;
						}
						else
						{
							previous.TimeOutAdherence += @event.StartTime - previous.StartTime;
						}
					}
					_persister.Update(previous);
				}
			}

			_persister.Add(model);	
		}

		public void Handle(PersonStateChangedEvent @event)
		{
			var existingModel = _persister.Get(@event.PersonId, new DateOnly(@event.Timestamp)).LastOrDefault();
			if (existingModel != null)
			{
				if (existingModel.IsInAdherence)
				{
					if (!existingModel.LastStateChangedTime.HasValue)
						existingModel.TimeInAdherence += @event.Timestamp - existingModel.StartTime;
					else
						existingModel.TimeInAdherence += @event.Timestamp - existingModel.LastStateChangedTime.Value;
				}
				else
				{
					if (!existingModel.LastStateChangedTime.HasValue)
						existingModel.TimeOutAdherence += @event.Timestamp - existingModel.StartTime;
					else
						existingModel.TimeOutAdherence += @event.Timestamp - existingModel.LastStateChangedTime.Value;
				}

				existingModel.LastStateChangedTime = @event.Timestamp;
				if (@event.InAdherence && existingModel.ActualStartTime == null)
					existingModel.ActualStartTime = @event.Timestamp;
				existingModel.IsInAdherence = @event.InAdherence;
				_persister.Update(existingModel);
			}
			else
			{
				var model = new AdherenceDetailsReadModel
				{
					PersonId = @event.PersonId,
					Date = new DateOnly(@event.Timestamp),
					LastStateChangedTime = @event.Timestamp,
				};
				_persister.Add(model);
			}
		}

		public void Handle(PersonInAdherenceEvent @event)
		{
			
		}

		public void Handle(PersonOutOfAdherenceEvent @event)
		{
			
		}
	}

	public class FakeAdherenceDetailsReadModelPersister : IAdherenceDetailsReadModelPersister
	{
		public IList<AdherenceDetailsReadModel> Rows = new List<AdherenceDetailsReadModel>();
		public AdherenceDetailsReadModel PersistedModel { get; set; }

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

		public void Remove(Guid personId, DateOnly date)
		{
			var existing = from m in Rows
						   where m.PersonId == personId &&
								 m.Date == date
						   select m;
			Rows.Remove(existing.Single());
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
					   IsInAdherence = m.IsInAdherence,
					   TimeInAdherence = m.TimeInAdherence,
					   TimeOutAdherence = m.TimeOutAdherence
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
		public TimeSpan TimeInAdherence { get; set; }
		public TimeSpan TimeOutAdherence { get; set; }
	}

	public interface IAdherenceDetailsReadModelPersister
	{
		void Add(AdherenceDetailsReadModel model);
		void Update(AdherenceDetailsReadModel model);
		IEnumerable<AdherenceDetailsReadModel> Get(Guid personId, DateOnly date);
		void Remove(Guid personId, DateOnly date);
	}
}