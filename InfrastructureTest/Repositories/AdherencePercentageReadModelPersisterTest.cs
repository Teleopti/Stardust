﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	[ReadModelTest]
	public class AdherencePercentageReadModelPersisterTest
	{
		public IAdherencePercentageReadModelPersister Target { get; set; }

		[Test]
		public void ShouldSaveReadModelForPerson()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = "2012-08-29".Date(),
				PersonId = personId,
				TimeInAdherence = "17".Minutes(),
				TimeOutOfAdherence = "28".Minutes(),
				IsLastTimeInAdherence = true,
				LastTimestamp = "2012-08-29 8:00".Utc(),
				ShiftHasEnded = true,
				ShiftEndTime = "2012-08-29 10:00".Utc(),
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
			model.ShiftEndTime.Should().Be.EqualTo("2012-08-29 10:00".Utc());
			model.State.Single().Timestamp.Should().Be("2012-08-29 8:00".Utc());
		}

		[Test]
		public void ShouldSaveReadModelOnDifferentDaysForSamePerson()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = "2012-08-29".Date(),
				PersonId = personId,
				ShiftEndTime = "2012-08-29".Date()
			});
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = "2012-08-30".Date(),
				PersonId = personId,
				ShiftEndTime = "2012-08-29".Date()
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
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = "2012-08-29".Date(),
				PersonId = personId,
				LastTimestamp = "2012-08-29 8:00".Utc(),
				TimeInAdherence = "0".Minutes(),
				TimeOutOfAdherence = "10".Minutes(),
				ShiftEndTime = "2012-08-29".Date()
			});
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = "2012-08-29".Date(),
				PersonId = personId,
				LastTimestamp = "2012-08-29 8:15".Utc(),
				TimeInAdherence = "17".Minutes(),
				TimeOutOfAdherence = "28".Minutes(),
				ShiftEndTime = "2012-08-29 10:00".Utc(),
				State = new[] { new AdherencePercentageReadModelState { Timestamp = "2012-08-29 8:15".Utc() } }
			});

			var model = Target.Get("2012-08-29".Date(), personId);
			model.LastTimestamp.Should().Be("2012-08-29 8:15".Utc());
			model.BelongsToDate.Should().Be.EqualTo("2012-08-29".Date());
			model.ShiftEndTime.Should().Be.EqualTo("2012-08-29 10:00".Utc());
			model.TimeOutOfAdherence.Should().Be.EqualTo("28".Minutes());
			model.TimeInAdherence.Should().Be.EqualTo("17".Minutes());
			model.State.Single().Timestamp.Should().Be("2012-08-29 8:15".Utc());
		}

		[Test]
		public void ShouldSaveWithNullables()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = "2012-08-29".Date(),
				PersonId = personId,
				ShiftEndTime = "2012-08-29".Date()
			});

			var model = Target.Get("2012-08-29".Date(), personId);
			model.IsLastTimeInAdherence.Should().Be(null);
			model.LastTimestamp.Should().Be(null);
		}

		[Test]
		public void ShouldUpdateToNullables()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = "2012-08-29".Date(),
				PersonId = personId,
				IsLastTimeInAdherence = false,
				LastTimestamp = "2012-08-29 8:00".Utc(),
				ShiftEndTime = "2012-08-29".Date()
			});
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = "2012-08-29".Date(),
				PersonId = personId,
				ShiftEndTime = "2012-08-29".Date()
			});

			var model = Target.Get("2012-08-29".Date(), personId);
			model.IsLastTimeInAdherence.Should().Be(null);
			model.LastTimestamp.Should().Be(null);
		}

		[Test]
		public void ShouldKnowIfThereIsData()
		{
			Target.Persist(new AdherencePercentageReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2015-01-19".Utc(),
				ShiftEndTime = "2012-08-29".Date()
			});

			Target.HasData().Should().Be.True();
		}

		[Test]
		public void ShouldKnowIfThereIsNoData()
		{
			Target.HasData().Should().Be.False();
		}

		[Test]
		public void ShouldGetCorrectModel()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = "2012-08-29".Date(),
				PersonId = personId,
				ShiftEndTime = "2012-08-30 02:00".Utc()
			});
			Target.Persist(new AdherencePercentageReadModel
			{
				Date = "2012-08-30".Date(),
				PersonId = personId,
				ShiftEndTime = "2012-08-30 17:00".Utc()
			});

			var model1 = Target.Get("2012-08-30 02:00".Utc(), personId);
			model1.BelongsToDate.Should().Be("2012-08-29".Date());

			var model2 = Target.Get("2012-08-30 03:00".Utc(), personId);
			model2.BelongsToDate.Should().Be("2012-08-30".Date());
		}
	}
}

