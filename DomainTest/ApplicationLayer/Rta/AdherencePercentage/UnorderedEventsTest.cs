using System;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherencePercentage
{
	[AdherenceTest]
	[TestFixture]
	public class UnorderedEventsTest: IRegisterInContainer
	{
		public FakeAdherencePercentageReadModelPersister Persister;
		public AdherencePercentageReadModelUpdater Target;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<AdherencePercentageReadModelUpdater>().AsSelf();
		}

		[Test]
		public void ShouldUpdateInAdherenceWhenInAdherenceOccuredBeforeOutOfAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:01".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:00".Utc()
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be("1".Seconds());
		}


		[Test]
		public void ShouldUpdateOutOfAdherenceWhenInAdherenceOccuredBeforeOutOfAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:01".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:00".Utc()
			});

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("0".Seconds());
		}

		[Test]
		public void ShouldUpdateOutOfAdherenceWhenOutOfAdherenceOccuredBeforeInAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:01".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:00".Utc()
			});

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("1".Seconds());
		}

		[Test]
		public void ShouldUpdateInAdherenceWhenOutOfAdherenceOccuredBeforeInAdhernce()
		{

			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:01".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:00".Utc()
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be("0".Seconds());
		}

		[Test]
		public void ShouldUpdateInAdherenceWhenInAdherenceOccuredBetweenOutOfAherenceEvents()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:00".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:02".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:01".Utc()
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be("1".Seconds());
		}

		[Test]
		public void ShouldUpdateOutOfAdherenceWhenInAdherenceOccuredBetweenOutOfAherenceEvents()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:00".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:02".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:01".Utc()
			});

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("1".Seconds());
		}

		[Test]
		public void ShouldUpdateInAdherenceWhenOutOfAdherenceOccuredBetweenInAdherenceEvents()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:00".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:02".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:01".Utc()
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be("1".Seconds());
		}

		[Test]
		public void ShouldUpdateOutOfAdherenceWhenOutOfAdherenceOccuredBetweenInAdherenceEvents()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:00".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:02".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:01".Utc()
			});

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("1".Seconds());
		}

		[Test]
		public void ShouldUpdateAdherenceWhenOutOfAdherenceOccuredBetweenInAdherenceEventsButExecutedFirst()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:01".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:00".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:02".Utc()
			});

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("1".Seconds());
			Persister.PersistedModel.TimeInAdherence.Should().Be("1".Seconds());
		}

		[Test]
		public void ShouldUpdateWhenShiftEndOccuredAfterOutOfAdherenceEvents()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:00".Utc()
			});
			Target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftEndTime = "2015-02-09 09:00:00".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:59:00".Utc()
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be("59".Minutes()); 
			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("1".Minutes());
		}


		[Test]
		public void ShouldUpdateWhenShiftEndOccuredAfterOutOfAdherenceEventsButExecutedFirst()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftEndTime = "2015-02-09 09:00:00".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:00:00".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-09 08:59:00".Utc()
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be("59".Minutes());
			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("1".Minutes());
		}



		
		// One test to rule them all
		[Test]
		public void ShouldHandleRandomEventOrder([Range(0, 23)] int a)
		{
			var personId = Guid.NewGuid();
			var events = new IEvent[]
			{
				new PersonInAdherenceEvent
				{
					PersonId = personId,
					Timestamp = "2015-02-09 08:00:00".Utc()
				},
				new PersonOutOfAdherenceEvent
				{
					PersonId = personId,
					Timestamp = "2015-02-09 08:00:01".Utc()
				},
				new PersonInAdherenceEvent
				{
					PersonId = personId,
					Timestamp = "2015-02-09 08:00:02".Utc()
				},
				new PersonShiftEndEvent
				{
					PersonId = personId,
					ShiftEndTime= "2015-02-09 08:00:03".Utc()
				}
			}.Randomize();
			events.ForEach(e =>
			{
				if (e.GetType() == typeof(PersonInAdherenceEvent))
					Target.Handle((PersonInAdherenceEvent)e);
				if (e.GetType() == typeof(PersonOutOfAdherenceEvent))
					Target.Handle((PersonOutOfAdherenceEvent)e);
				if (e.GetType() == typeof(PersonShiftEndEvent))
					Target.Handle((PersonShiftEndEvent)e);
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be("2".Seconds());
			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("1".Seconds());
		}


	}
}