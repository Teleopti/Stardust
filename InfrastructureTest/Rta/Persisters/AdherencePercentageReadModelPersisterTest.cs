using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[ReadModelUnitOfWorkTest]
	public class AdherencePercentageReadModelPersisterTest
	{
		public IAdherencePercentageReadModelPersister Target;
		public IAdherencePercentageReadModelReader Reader;

		[Test]
		public void ShouldSaveReadModelForPerson()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = new DateTime(2012,08,29),
				PersonId = personId,
				TimeInAdherence = "17".Minutes(),
				TimeOutOfAdherence = "28".Minutes(),
				IsLastTimeInAdherence = true,
				LastTimestamp = "2012-08-29 8:00".Utc(),
				ShiftHasEnded = true,
				State = new[] { new AdherencePercentageReadModelState { Timestamp = "2012-08-29 8:00".Utc() } }
			});

			var model = Target.Get(new DateOnly(2012, 8, 29), personId);
			model.PersonId.Should().Be.EqualTo(model.PersonId);
			model.BelongsToDate.Should().Be.EqualTo("2012-08-29".Date());
			model.TimeOutOfAdherence.Should().Be.EqualTo("28".Minutes());
			model.TimeInAdherence.Should().Be.EqualTo("17".Minutes());
			model.IsLastTimeInAdherence.Should().Be.EqualTo(true);
			model.LastTimestamp.Should().Be.EqualTo("2012-08-29 8:00".Utc());
			model.ShiftHasEnded.Should().Be.EqualTo(true);
			model.State.Single().Timestamp.Should().Be("2012-08-29 8:00".Utc());
		}

		[Test]
		public void ShouldSaveReadModelOnDifferentDaysForSamePerson()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = new DateTime(2012, 08, 29),
				PersonId = personId
			});
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = new DateTime(2012, 08, 30),
				PersonId = personId
			});

			var model1 = Target.Get("2012-08-29".Date(), personId);
			model1.PersonId.Should().Be.EqualTo(personId);
			model1.BelongsToDate.Should().Be.EqualTo("2012-08-29".Date());

			var model2 = Target.Get("2012-08-30".Date(), personId);
			model2.PersonId.Should().Be.EqualTo(personId);
			model2.BelongsToDate.Should().Be.EqualTo("2012-08-30".Date());
		}

		[Test]
		public void ShouldUpdateExistingReadModel()
		{
			var personId = Guid.NewGuid();
			var model = new AdherencePercentageReadModel
			{
				Date = new DateTime(2012, 08, 29),
				PersonId = personId,
				LastTimestamp = "2012-08-29 8:00".Utc(),
				TimeInAdherence = "0".Minutes(),
				TimeOutOfAdherence = "10".Minutes()
			};
			Target.Persist(model);
			model.Date = new DateTime(2012, 08, 29);
			model.PersonId = personId;
			model.LastTimestamp = "2012-08-29 8:15".Utc();
			model.TimeInAdherence = "17".Minutes();
			model.TimeOutOfAdherence = "28".Minutes();
			model.State = new[] {new AdherencePercentageReadModelState {Timestamp = "2012-08-29 8:15".Utc()}};
			Target.Persist(model);

			var actual = Target.Get("2012-08-29".Date(), personId);
			actual.LastTimestamp.Should().Be("2012-08-29 8:15".Utc());
			actual.BelongsToDate.Should().Be.EqualTo("2012-08-29".Date());
			actual.TimeOutOfAdherence.Should().Be.EqualTo("28".Minutes());
			actual.TimeInAdherence.Should().Be.EqualTo("17".Minutes());
			actual.State.Single().Timestamp.Should().Be("2012-08-29 8:15".Utc());
		}

		[Test]
		public void ShouldDelete()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = "2015-12-08".Utc(),
				PersonId = personId
			});

			Target.Delete(personId);

			Target.Get("2015-12-08".Date(), personId).Should().Be.Null();
		}

		[Test]
		public void ShouldSaveWithNullables()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = new DateTime(2012, 08, 29),
				PersonId = personId
			});

			var model = Target.Get("2012-08-29".Date(), personId);
			model.IsLastTimeInAdherence.Should().Be(null);
			model.LastTimestamp.Should().Be(null);
		}

		[Test]
		public void ShouldUpdateToNullables()
		{
			var personId = Guid.NewGuid();
			var model = new AdherencePercentageReadModel
			{
				Date = new DateTime(2012, 08, 29),
				PersonId = personId,
				IsLastTimeInAdherence = false,
				LastTimestamp = "2012-08-29 8:00".Utc()
			};
			Target.Persist(model);
			model.IsLastTimeInAdherence = null;
			model.LastTimestamp = null;
			Target.Persist(model);
			
			var actual = Target.Get("2012-08-29".Date(), personId);
			actual.IsLastTimeInAdherence.Should().Be(null);
			actual.LastTimestamp.Should().Be(null);
		}

		[Test]
		public void ShouldKnowIfThereIsData()
		{
			Target.Persist(new AdherencePercentageReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2015-01-19".Utc()
			});

			Target.HasData().Should().Be.True();
		}

		[Test]
		public void ShouldKnowIfThereIsNoData()
		{
			Target.HasData().Should().Be.False();
		}

		[Test]
		public void ShouldRead()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = new DateTime(2012, 08, 29),
				PersonId = personId,
				TimeInAdherence = "17".Minutes(),
				TimeOutOfAdherence = "28".Minutes(),
				IsLastTimeInAdherence = true,
				LastTimestamp = "2012-08-29 8:00".Utc(),
				ShiftHasEnded = true
			});

			var model = Reader.Read("2012-08-29".Date(), personId);

			model.PersonId.Should().Be.EqualTo(model.PersonId);
			model.BelongsToDate.Should().Be.EqualTo("2012-08-29".Date());
			model.TimeOutOfAdherence.Should().Be.EqualTo("28".Minutes());
			model.TimeInAdherence.Should().Be.EqualTo("17".Minutes());
			model.IsLastTimeInAdherence.Should().Be.EqualTo(true);
			model.LastTimestamp.Should().Be.EqualTo("2012-08-29 8:00".Utc());
			model.ShiftHasEnded.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldSaveReadModelWithVeryLongStateString()
		{
			var personId = Guid.NewGuid();
			var states = new List<AdherencePercentageReadModelState>();
			states.AddRange(from i in Enumerable.Range(1, 400) select new AdherencePercentageReadModelState());
			
			Assert.DoesNotThrow(() => Target.Persist(new AdherencePercentageReadModel
			{
				Date = new DateTime(2012, 08, 29),
				PersonId = personId,
				TimeInAdherence = "17".Minutes(),
				TimeOutOfAdherence = "28".Minutes(),
				IsLastTimeInAdherence = true,
				LastTimestamp = "2012-08-29 8:00".Utc(),
				ShiftHasEnded = true,
				State = states
			}));
		}
		
	}
}

