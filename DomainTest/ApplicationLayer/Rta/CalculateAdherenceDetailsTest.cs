using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class CalculateAdherenceDetailsTest
	{
		[Test]
		public void ShouldReturnCalculatedResult()
		{
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-11-20".Utc(),
				StartTime = "2014-11-20 9:00".Utc(),
				TimeInAdherence = TimeSpan.FromMinutes(10)
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow("2014-11-20 9:00".Utc()),
				new FakeAdherenceDetailsReadModelPersister(new[] {model}), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForDetails(model.PersonId);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResultIn100PercenWithOnlyInAdherence()
		{
			var now = "2014-11-20 9:00".Utc();
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-11-20".Utc(),
				StartTime = "2014-11-20 8:00".Utc(),
				TimeInAdherence = TimeSpan.FromMinutes(60),
				TimeOutOfAdherence = TimeSpan.FromMinutes(0),
				LastStateChangedTime = now
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow(now),
				new FakeAdherenceDetailsReadModelPersister(new[] {model}), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForDetails(model.PersonId);

			result.Single().AdherencePercent.Should().Be(100);
		}

		[Test]
		public void ShouldResultIn80PercenWhenHaveInAndOutOfAdherence()
		{
			var now = "2014-11-20 9:00".Utc();
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-11-20".Utc(),
				StartTime = "2014-11-20 8:00".Utc(),
				TimeInAdherence = TimeSpan.FromMinutes(30),
				TimeOutOfAdherence = TimeSpan.FromMinutes(30),
				LastStateChangedTime = now
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow(now),
				new FakeAdherenceDetailsReadModelPersister(new[] {model}), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForDetails(model.PersonId);

			result.Single().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldResultIn0PercenWhenOnlyOutOfAdherence()
		{
			var now = "2014-11-20 9:00".Utc();
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-11-20".Utc(),
				StartTime = "2014-11-20 8:00".Utc(),
				TimeInAdherence = TimeSpan.FromMinutes(0),
				TimeOutOfAdherence = TimeSpan.FromMinutes(60),
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow(now),
				new FakeAdherenceDetailsReadModelPersister(new[] {model}), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForDetails(model.PersonId);

			result.Single().AdherencePercent.Should().Be(0);
		}

		[Test]
		public void ShouldCalculateForTwoActivities()
		{
			var now = "2014-11-20 9:00".Utc();
			var personId = Guid.NewGuid();
			var date = "2014-11-20".Utc();
			var model1 = new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = date,
				StartTime = "2014-11-20 8:00".Utc(),
				TimeInAdherence = TimeSpan.FromMinutes(0),
				TimeOutOfAdherence = TimeSpan.FromMinutes(60)
			};
			var model2 = new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = date,
				StartTime = "2014-11-20 9:00".Utc(),
				TimeInAdherence = TimeSpan.FromMinutes(30),
				TimeOutOfAdherence = TimeSpan.FromMinutes(30),
				LastStateChangedTime = now
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow(now),
				new FakeAdherenceDetailsReadModelPersister(new[] { model1, model2 }), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForDetails(personId);

			result.First().AdherencePercent.Should().Be(0);
			result.Last().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldReturnEmptyResultIfNoDataIsFound()
		{
			var target = new CalculateAdherenceDetails(new ThisIsNow("2014-11-20 9:00".Utc()),
				new FakeAdherenceDetailsReadModelPersister(null), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForDetails(Guid.NewGuid());

			result.Count().Should().Be(0);
		}

		[Test]
		public void ShouldResultInEmptyWhenNoAdherenceData()
		{
			var now = "2014-11-20 9:00".Utc();
			var personId = Guid.NewGuid();
			var date = "2014-11-20".Utc();
			var model = new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = date,
				StartTime = "2014-11-20 8:00".Utc(),
				TimeInAdherence = TimeSpan.Zero,
				TimeOutOfAdherence = TimeSpan.Zero
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow(now),
				new FakeAdherenceDetailsReadModelPersister(new[] { model }), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForDetails(model.PersonId);

			result.Count().Should().Be(0);
		}

		[Test]
		public void ShouldAddTimeInAdherenceBasedOnCurrentTime()
		{
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-11-20".Utc(),
				StartTime ="2014-11-20 8:00".Utc(),
				TimeInAdherence = TimeSpan.FromMinutes(0),
				TimeOutOfAdherence = TimeSpan.FromMinutes(30),
				LastStateChangedTime = "2014-11-20 8:30".Utc(),
				IsInAdherence = true
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow("2014-11-20 9:00".Utc()),
				new FakeAdherenceDetailsReadModelPersister(new[] { model }), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForDetails(model.PersonId);

			result.Single().AdherencePercent.Should().Be(50);
		}

		[Test]
		public void ShouldAddTimeOutOfAdherenceBasedOnCurrentTime()
		{
			var model = new AdherenceDetailsReadModel
			{
				PersonId = Guid.NewGuid(),
				Date = "2014-11-20".Utc(),
				StartTime = "2014-11-20 8:00".Utc(),
				TimeInAdherence = TimeSpan.FromMinutes(0),
				TimeOutOfAdherence = TimeSpan.FromMinutes(30),
				LastStateChangedTime = "2014-11-20 8:30".Utc(),
				IsInAdherence = false
			};
			var target = new CalculateAdherenceDetails(new ThisIsNow("2014-11-20 9:00".Utc()),
				new FakeAdherenceDetailsReadModelPersister(new[] { model }), new ThreadCulture(), new UtcTimeZone());

			var result = target.ForDetails(model.PersonId);

			result.Single().AdherencePercent.Should().Be(0);
		}


		public class FakeAdherenceDetailsReadModelPersister : IAdherenceDetailsReadModelPersister
		{
			private readonly IList<AdherenceDetailsReadModel> _models;

			public FakeAdherenceDetailsReadModelPersister(IList<AdherenceDetailsReadModel> models)
			{
				_models = models;
			}

			public void Add(AdherenceDetailsReadModel model)
			{
				throw new NotImplementedException();
			}

			public void Update(AdherenceDetailsReadModel model)
			{
				throw new NotImplementedException();
			}

			public IEnumerable<AdherenceDetailsReadModel> Get(Guid personId, DateOnly date)
			{
				var readModels = new List<AdherenceDetailsReadModel>();
				if (_models == null)
					return readModels;
				_models.ForEach(m =>
				{
					if (date.Equals(m.BelongsToDate) && m.PersonId == personId) readModels.Add(m);
				});
				return readModels;
			}

			public void Remove(Guid personId, DateOnly date)
			{
				throw new NotImplementedException();
			}
		}
	}

	public class CalculateAdherenceDetails
	{
		private readonly INow _now;
		private readonly IAdherenceDetailsReadModelPersister _readModelPersister;
		private readonly IUserCulture _userCulture;
		private readonly IUserTimeZone _timeZone;
		private readonly CalculateAdherencePercent _calculateAdherencePercent;

		public CalculateAdherenceDetails(INow now, IAdherenceDetailsReadModelPersister readModelPersister, IUserCulture userCulture, IUserTimeZone timeZone)
		{
			_now = now;
			_readModelPersister = readModelPersister;
			_userCulture = userCulture;
			_timeZone = timeZone;
			_calculateAdherencePercent = new CalculateAdherencePercent(_now);
		}

		public IEnumerable<AdherenceDetailsPercentageModel> ForDetails(Guid personId)
		{
			var readModels = _readModelPersister.Get(personId, new DateOnly(_now.UtcDateTime()));
			var result = new List<AdherenceDetailsPercentageModel>();
			readModels.ForEach(m =>
			{
				if (m == null || !isValid(m))
					return;
				result.Add(new AdherenceDetailsPercentageModel
				{
					AdherencePercent = (int)_calculateAdherencePercent.ForActivity(m).ValueAsPercent()
				});
			});
			return result;
		}


		private static bool isValid(AdherenceDetailsReadModel readModel)
		{
			return !(readModel.TimeInAdherence == TimeSpan.Zero && readModel.TimeOutOfAdherence == TimeSpan.Zero);
		}
	}

	public class AdherenceDetailsPercentageModel
	{
		public int AdherencePercent { get; set; }
	}
}
