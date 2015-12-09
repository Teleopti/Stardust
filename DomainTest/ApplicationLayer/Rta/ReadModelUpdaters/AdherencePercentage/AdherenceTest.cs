using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherencePercentage
{
	[ReadModelUpdaterTest]
	[TestFixture]
	public class AdherenceTest
	{
		public FakeAdherencePercentageReadModelPersister Persister;
		public AdherencePercentageReadModelUpdater Target;
		
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
		public void ShouldUpdateWithSecondResolutionOfTimeInAdherence()
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
		public void ShouldUpdateWithSecondResolutionOfTimeOutOfAdherence()
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
	}
}