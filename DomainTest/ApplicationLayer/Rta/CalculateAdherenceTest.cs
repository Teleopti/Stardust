using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class CalculateAdherenceTest
	{
		[Test]
		public void ShouldProduceAModel()
		{
			var model = new AdherencePercentageReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = TimeSpan.FromMinutes(1)
			};
			var target = new CalculateAdherence(new ThisIsNow(new DateTime(2014, 12, 24, 15, 0, 0)), new FakeAdherencePercentageReadModelPersister(model), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForToday(model.PersonId);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResultIn50PercentWhenInAndOutOfAdherenceIsTheSame()
		{
			var now = new DateTime(2014, 12, 24, 15, 0, 0);
			var model = new AdherencePercentageReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = TimeSpan.FromMinutes(74),
				TimeOutOfAdherence = TimeSpan.FromMinutes(74),
				LastTimestamp = now,
			};
			var target = new CalculateAdherence(new ThisIsNow(now), new FakeAdherencePercentageReadModelPersister(model), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForToday(model.PersonId);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		[Test]
		public void ShouldRestultIn100PercentIfOnlyInAdherence()
		{
			var now = new DateTime(2014, 12, 24, 15, 0, 0);
			var model = new AdherencePercentageReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = TimeSpan.FromMinutes(12),
				TimeOutOfAdherence = TimeSpan.FromMinutes(0),
				LastTimestamp = now,
			};
			var target = new CalculateAdherence(new ThisIsNow(now), new FakeAdherencePercentageReadModelPersister(model), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForToday(model.PersonId);

			result.AdherencePercent.Should().Be.EqualTo(100);
		}

		[Test]
		public void ShouldResultIn0PercentInOnlyOutOfAdherence()
		{
			var now = new DateTime(2014, 12, 24, 15, 0, 0);
			var model = new AdherencePercentageReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = TimeSpan.FromMinutes(0),
				TimeOutOfAdherence = TimeSpan.FromMinutes(12),
				LastTimestamp = now,
			};
			var target = new CalculateAdherence(new ThisIsNow(now), new FakeAdherencePercentageReadModelPersister(model), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForToday(model.PersonId);

			result.AdherencePercent.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnNullIfNoDataIsFound()
		{
			var target = new CalculateAdherence(new Now(), new FakeAdherencePercentageReadModelPersister(null), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForToday(Guid.NewGuid());

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldResultInNullWhenNoAdherenceData()
		{
			var model = new AdherencePercentageReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-12-24".Utc(),
				TimeInAdherence = TimeSpan.Zero,
				TimeOutOfAdherence = TimeSpan.Zero
			};
			var target = new CalculateAdherence(new ThisIsNow("2014-12-24 11:00".Utc()), new FakeAdherencePercentageReadModelPersister(model), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForToday(model.PersonId);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldAddTimeInAdherenceBasedOnCurrentTime()
		{
			var model = new AdherencePercentageReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-12-24".Utc(),
				TimeInAdherence = TimeSpan.FromMinutes(0),
				TimeOutOfAdherence = TimeSpan.FromMinutes(60),
				LastTimestamp = "2014-12-24 10:00".Utc(),
				IsLastTimeInAdherence = true
			};
			var target = new CalculateAdherence(new ThisIsNow("2014-12-24 11:00"), new FakeAdherencePercentageReadModelPersister(model), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForToday(model.PersonId);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		[Test]
		public void ShouldAddTimeOutOfAdherenceBasedOnCurrentTime()
		{
			var model = new AdherencePercentageReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-12-24".Utc(),
				TimeInAdherence = TimeSpan.FromMinutes(60),
				TimeOutOfAdherence = TimeSpan.FromMinutes(0),
				LastTimestamp = "2014-12-24 10:00".Utc(),
				IsLastTimeInAdherence = false
			};
			var target = new CalculateAdherence(new ThisIsNow("2014-12-24 11:00"), new FakeAdherencePercentageReadModelPersister(model), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForToday(model.PersonId);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		[Test]
		public void ShouldNotAddTimeAfterShiftHasEnded()
		{
			var model = new AdherencePercentageReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-12-24".Utc(),
				TimeInAdherence = TimeSpan.FromHours(1),
				TimeOutOfAdherence = TimeSpan.FromHours(1),
				ShiftHasEnded = true,
				LastTimestamp = null,
				IsLastTimeInAdherence = null
			};
			var target = new CalculateAdherence(new ThisIsNow("2014-12-24 11:00"), new FakeAdherencePercentageReadModelPersister(model), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForToday(model.PersonId);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}


		[Test]
		public void ShouldFormatTimestamp()
		{
			var model = new AdherencePercentageReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-12-24".Utc(),
				TimeInAdherence = TimeSpan.FromMinutes(1),
				LastTimestamp = "2014-12-24 14:00".Utc()
			};
			var target = new CalculateAdherence(new ThisIsNow("2014-12-24 15:00"), new FakeAdherencePercentageReadModelPersister(model), new CatalanCulture(), new UtcTimeZone());

			var result = target.ForToday(model.PersonId);

			result.LastTimestamp.Should().Be(model.LastTimestamp.Value.ToShortTimeString(new CatalanCulture().GetCulture()));
		}

		[Test]
		public void ShouldAdjustTimestampToUserTimeZone()
		{
			var model = new AdherencePercentageReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-12-24".Utc(),
				TimeInAdherence = TimeSpan.FromMinutes(1),
				LastTimestamp = "2014-12-24 14:00".Utc()
			};
			var target = new CalculateAdherence(new ThisIsNow("2014-12-24 14:00"), new FakeAdherencePercentageReadModelPersister(model), new ThreadCulture(), new HawaiiTimeZone());

			var result = target.ForToday(model.PersonId);

			result.LastTimestamp.Should().Be(TimeZoneInfo.ConvertTimeFromUtc(model.LastTimestamp.Value, new HawaiiTimeZone().TimeZone()).ToShortTimeString());
		}

	}
}