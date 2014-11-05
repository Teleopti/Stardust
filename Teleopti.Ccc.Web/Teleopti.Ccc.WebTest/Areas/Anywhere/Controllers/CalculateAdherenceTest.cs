using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class CalculateAdherenceTest
	{
		[Test]
		public void ForToday_ShouldGetTheReadModelForSuppliedAgentWithTodaysDate()
		{
			var model = new AdherencePercentageReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = TimeSpan.FromMinutes(1)
			};
			var readModelRepo = new FakeAdherencePercentageReadModelPersister(model);
			var target = new CalculateAdherence(readModelRepo, new ThisIsNow(new DateTime(2014, 12, 24, 15, 0, 0)));

			var result = target.ForToday(model.PersonId);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void HistoricalAdherence_WhenInAdherenceAndOutOfAdherenceAreSame_ShouldBeFiftyPercent()
		{
			var now = new DateTime(2014, 12, 24, 15, 0, 0);
			var model = new AdherencePercentageReadModel()
			{
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = TimeSpan.FromMinutes(74),
				TimeOutOfAdherence = TimeSpan.FromMinutes(74),
				LastTimestamp = now,
			};
			var target = new CalculateAdherence(new FakeAdherencePercentageReadModelPersister(model), new ThisIsNow(now));

			var result = target.ForToday(model.PersonId);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		[Test]
		public void HistoricalAdherence_WhenOnlyInAdherence_ShouldBeHundredPercent()
		{
			var now = new DateTime(2014, 12, 24, 15, 0, 0);
			var model = new AdherencePercentageReadModel()
			{
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = TimeSpan.FromMinutes(12),
				TimeOutOfAdherence = TimeSpan.FromMinutes(0),
				LastTimestamp = now,
			};
			var target = new CalculateAdherence(new FakeAdherencePercentageReadModelPersister(model), new ThisIsNow(now));

			var result = target.ForToday(model.PersonId);

			result.AdherencePercent.Should().Be.EqualTo(100);
		}

		[Test]
		public void HistoricalAdherence_WhenOnlyOutOfAdherence_ShouldBeZeroPercent()
		{
			var now = new DateTime(2014, 12, 24, 15, 0, 0);
			var model = new AdherencePercentageReadModel()
			{
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = TimeSpan.FromMinutes(0),
				TimeOutOfAdherence = TimeSpan.FromMinutes(12),
				LastTimestamp = now,
			};
			var target = new CalculateAdherence(new FakeAdherencePercentageReadModelPersister(model), new ThisIsNow(now));

			var result = target.ForToday(model.PersonId);

			result.AdherencePercent.Should().Be.EqualTo(0);
		}

		[Test]
		public void HistoricalAdherence_WhenThereIsNoReadmodel_ShouldReturnData()
		{
			var target = new CalculateAdherence(new FakeAdherencePercentageReadModelPersister(null), new Now());

			var result = target.ForToday(Guid.NewGuid());

			result.Should().Be.Null();
		}

		[Test]
		public void HistoricalAdherence_WhenAgentIsInAdherence_ShouldCalculateThatTimeAsInAdherenceBasedOnTheCurrentTime()
		{
			var now = new DateTime(2014, 12, 24, 11, 0, 0);
			var model = new AdherencePercentageReadModel()
			{
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = TimeSpan.FromMinutes(0),
				TimeOutOfAdherence = TimeSpan.FromMinutes(60),
				LastTimestamp = now.AddMinutes(-60),
				IsLastTimeInAdherence = true,
				ShiftEnd = now.AddHours(2)
			};
			var target = new CalculateAdherence(new FakeAdherencePercentageReadModelPersister(model), new ThisIsNow(now));

			var result = target.ForToday(model.PersonId);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		[Test]
		public void HistoricalAdherence_WhenAgentIsOutOfAdherence_ShouldCalculateThatTimeAsInAdherenceBasedOnTheCurrentTime()
		{
			var now = new DateTime(2014, 12, 24, 11, 0, 0);
			var model = new AdherencePercentageReadModel()
			{
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = TimeSpan.FromMinutes(60),
				TimeOutOfAdherence = TimeSpan.FromMinutes(0),
				LastTimestamp = now.AddMinutes(-60),
				IsLastTimeInAdherence = false,
				ShiftEnd = now.AddHours(2)
			};
			var target = new CalculateAdherence(new FakeAdherencePercentageReadModelPersister(model), new ThisIsNow(now));

			var result = target.ForToday(model.PersonId);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		[Test]
		public void HistoricalAdherence_WhenTimeIsAfterTheShiftHasEnded_ShouldNotBeIncludedInTheCalculation()
		{
			var now = new DateTime(2014, 12, 24, 11, 0, 0);
			var model = new AdherencePercentageReadModel()
			{
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = TimeSpan.FromHours(1),
				TimeOutOfAdherence = TimeSpan.Zero,
				LastTimestamp = now.AddHours(-2),
				IsLastTimeInAdherence = false,
				ShiftEnd = now.AddHours(-1)
			};
			var target = new CalculateAdherence(new FakeAdherencePercentageReadModelPersister(model), new ThisIsNow(now));

			var result = target.ForToday(model.PersonId);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		[Test]
		public void HistoricalAdherence_WhenBothInAdherenceAndOutOfAdherenceIsZero_ShouldNotReturnData()
		{
			var now = new DateTime(2014, 12, 24, 11, 0, 0);
			var model = new AdherencePercentageReadModel()
			{
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2014, 12, 24),
				TimeInAdherence = TimeSpan.Zero,
				TimeOutOfAdherence = TimeSpan.Zero
			};
			var target = new CalculateAdherence(new FakeAdherencePercentageReadModelPersister(model), new ThisIsNow(now));

			var result = target.ForToday(model.PersonId);

			result.Should().Be.Null();
		}

		public class FakeAdherencePercentageReadModelPersister : IAdherencePercentageReadModelPersister
		{
			private readonly AdherencePercentageReadModel _model;

			public FakeAdherencePercentageReadModelPersister(AdherencePercentageReadModel model)
			{
				_model = model;
			}

			public void Persist(AdherencePercentageReadModel model)
			{
				throw new NotImplementedException();
			}

			public AdherencePercentageReadModel Get(DateOnly date, Guid personId)
			{
				if (_model == null)
					return null;
				return date.Equals(_model.BelongsToDate) && _model.PersonId == personId ? 
					_model : 
					null;
			}
		}
	}
}