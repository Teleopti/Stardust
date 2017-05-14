using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AdherencePercentage
{
	[TestFixture]
	[ReadModelUnitOfWorkTest]
	public class AdherencePercentageReadModelReaderTest
	{
		public IAdherencePercentageReadModelPersister Persister;
		public IAdherencePercentageReadModelReader Target;
		public MutableNow Now;

		[Test]
		public void ShouldReadCurrent()
		{
			var personId = Guid.NewGuid();
			Persister.Persist(new AdherencePercentageReadModel
			{
				Date = "2012-08-29".Utc(),
				ShiftStartTime = "2012-08-29 08:00".Utc(),
				ShiftEndTime = "2012-08-29 17:00".Utc(),
				PersonId = personId,
				TimeInAdherence = "17".Minutes(),
				TimeOutOfAdherence = "28".Minutes(),
				IsLastTimeInAdherence = true,
				LastTimestamp = "2012-08-29 8:00".Utc(),
				ShiftHasEnded = true
			});
			Now.Is("2012-08-29 09:00");

			var model = Target.ReadCurrent(personId);

			model.PersonId.Should().Be.EqualTo(model.PersonId);
			model.BelongsToDate.Should().Be.EqualTo("2012-08-29".Date());
			model.ShiftStartTime.Should().Be.EqualTo("2012-08-29 08:00".Utc());
			model.ShiftEndTime.Should().Be.EqualTo("2012-08-29 17:00".Utc());
			model.TimeOutOfAdherence.Should().Be.EqualTo("28".Minutes());
			model.TimeInAdherence.Should().Be.EqualTo("17".Minutes());
			model.IsLastTimeInAdherence.Should().Be.EqualTo(true);
			model.LastTimestamp.Should().Be.EqualTo("2012-08-29 8:00".Utc());
			model.ShiftHasEnded.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReadForNightShift()
		{
			var personId = Guid.NewGuid();
			Persister.Persist(new AdherencePercentageReadModel
			{
				Date = "2016-06-23".Utc(),
				ShiftStartTime = "2016-06-23 22:00".Utc(),
				ShiftEndTime = "2016-06-24 06:00".Utc(),
				PersonId = personId,
				TimeInAdherence = "17".Minutes(),
				TimeOutOfAdherence = "28".Minutes(),
				IsLastTimeInAdherence = true,
				LastTimestamp = "2016-06-23 8:00".Utc(),
				ShiftHasEnded = true
			});
			Now.Is("2016-06-24 05:00");

			var model = Target.ReadCurrent(personId);

			model.PersonId.Should().Be.EqualTo(model.PersonId);
			model.BelongsToDate.Should().Be.EqualTo("2016-06-23".Date());
		}

		[Test]
		public void ShouldReadPastNightShift()
		{
			var personId = Guid.NewGuid();
			Persister.Persist(new AdherencePercentageReadModel
			{
				Date = "2016-06-23".Utc(),
				ShiftStartTime = "2016-06-23 22:00".Utc(),
				ShiftEndTime = "2016-06-24 06:00".Utc(),
				PersonId = personId,
				TimeInAdherence = "17".Minutes(),
				TimeOutOfAdherence = "28".Minutes(),
				IsLastTimeInAdherence = true,
				LastTimestamp = "2016-06-23 8:00".Utc(),
				ShiftHasEnded = true
			});
			Now.Is("2016-06-24 07:00");

			var model = Target.ReadCurrent(personId);

			model.PersonId.Should().Be.EqualTo(model.PersonId);
			model.BelongsToDate.Should().Be.EqualTo("2016-06-23".Date());
		}
	}
}