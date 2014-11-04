using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class AdherenceControllerTest
	{
		[Test]
		public void ForToday_WhenAdherenceInfoIsValid_ShouldGenerateResultBasedOnAdherenceInfo()
		{
			var calculateAdherence = MockRepository.GenerateStub<ICalculateAdherence>();
			var personId = Guid.NewGuid();
			calculateAdherence.Expect(h => h.ForToday(personId)).Return(new AdherenceInfo()
			                                               {
															   AdherencePercent = 70,
															   IsValid = true
			                                               });

			var target = new AdherenceController(calculateAdherence);

			var result = target.ForToday(personId);

			((AdherenceInfo) result.Data).AdherencePercent.Should().Be.EqualTo(70);
		}

		[Test]
		public void ForToday_WhenAdherenceInfoIsNotValid_ShouldHaveFalsyAdherencePercentage()
		{
			var calculateAdherence = MockRepository.GenerateStub<ICalculateAdherence>();
			var personId = Guid.NewGuid();
			calculateAdherence.Expect(h => h.ForToday(personId)).Return(new AdherenceInfo()
			{
				AdherencePercent = 70,
				IsValid = false
			});

			var target = new AdherenceController(calculateAdherence);

			var result = target.ForToday(personId);

			Assert.That(isFalsy(result.Data));
		}

		[Test]
		public void ForToday_ShouldGetTheReadModelForSuppliedAgentWithTodaysDate()
		{
			var thisIsNow = new DateTime(2014, 12, 24, 15, 0, 0);
			var expectedMinutesInAdherence = TimeSpan.FromMinutes(12);
			var expectedminutesOutOfAdherence = TimeSpan.FromMinutes(36);
			var expectedLastTimeStamp = thisIsNow.AddMinutes(-5);
			var personId = Guid.NewGuid();
			var now = new ThisIsNow(thisIsNow);
			var readModelRepo = MockRepository.GenerateStub<IAdherencePercentageReadModelPersister>();

			readModelRepo.Stub(r => r.Get(new DateOnly(now.UtcDateTime()), personId)).Return(new AdherencePercentageReadModel()
			                                                                                 {
				                                                                                 TimeInAdherence =
					                                                                                 expectedMinutesInAdherence,
				                                                                                 TimeOutOfAdherence =
					                                                                                 expectedminutesOutOfAdherence,
				                                                                                 LastTimestamp =
					                                                                                 expectedLastTimeStamp
			                                                                                 });

			var target = new CalculateAdherence(readModelRepo, now);
			var result = target.ForToday(personId);

			(result.TimeInAdherence).Should().Be.EqualTo(expectedMinutesInAdherence);
			(result.TimeOutOfAdherence).Should().Be.EqualTo(expectedminutesOutOfAdherence);
			(result.LastTimestamp).Should().Be.EqualTo(expectedLastTimeStamp);
		}

		[Test]
		public void HistoricalAdherence_WhenInAdherenceAndOutOfAdherenceAreSame_ShouldBeFiftyPercent()
		{
			var now = new ThisIsNow(DateTime.Now);
			var data = new AdherencePercentageReadModel()
			           {
				           TimeInAdherence = TimeSpan.FromMinutes(74),
						   TimeOutOfAdherence = TimeSpan.FromMinutes(74),
				           LastTimestamp = now.UtcDateTime(),
			           };
			var repo = new PersisterForTest(data);
			var target = new CalculateAdherence(repo, now);

			var result = target.ForToday(Guid.Empty);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		[Test]
		public void HistoricalAdherence_WhenOnlyInAdherence_ShouldBeHundredPercent()
		{
			var now = new ThisIsNow(DateTime.Now);
			var data = new AdherencePercentageReadModel()
			           {
				           TimeInAdherence = TimeSpan.FromMinutes(12),
				           TimeOutOfAdherence = TimeSpan.FromMinutes(0),
				           LastTimestamp = now.UtcDateTime(),
			           };
			var repo = new PersisterForTest(data);

			var target = new CalculateAdherence(repo, now);
			var result = target.ForToday(Guid.Empty);

			result.AdherencePercent.Should().Be.EqualTo(100);
		}

		[Test]
		public void HistoricalAdherence_WhenOnlyOutOfAdherence_ShouldBeZeroPercent()
		{
			var data = new AdherencePercentageReadModel()
			           {
				           TimeInAdherence = TimeSpan.FromMinutes(0),
				           TimeOutOfAdherence = TimeSpan.FromMinutes(54)
			           };

			var repo = new PersisterForTest(data);

			var target = new CalculateAdherence(repo, new Now());
			var result = target.ForToday(Guid.Empty);
			result.AdherencePercent.Should().Be.EqualTo(0);
		}

		[Test]
		public void HistoricalAdherence_WhenThereIsNoReadmodel_ShouldBeFalsy()
		{
			var repo = new PersisterForTest(null);
			var target = new CalculateAdherence(repo, new Now());

			var result = target.ForToday(Guid.Empty);

			result.IsValid.Should().Be.False();
		}

		[Test]
		public void HistoricalAdherence_WhenAgentIsInAdherence_ShouldCalculateThatTimeAsInAdherenceBasedOnTheCurrentTime()
		{
			var timeThatStateChangedToInAdherence = new DateTime(2001, 1, 12, 10, 0, 0, 0);
			var now = new ThisIsNow(timeThatStateChangedToInAdherence.AddMinutes(60));
			var data = new AdherencePercentageReadModel()
			           {
				           TimeInAdherence = TimeSpan.FromMinutes(0),
				           TimeOutOfAdherence = TimeSpan.FromMinutes(60),
				           LastTimestamp = timeThatStateChangedToInAdherence,
				           IsLastTimeInAdherence = true,
				           ShiftEnd = now.UtcDateTime().AddHours(2)
			           };

			var repo = new PersisterForTest(data);
			var target = new CalculateAdherence(repo, now);

			var result = target.ForToday(Guid.Empty);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		[Test]
		public void HistoricalAdherence_WhenAgentIsOutOfAdherence_ShouldCalculateThatTimeAsInAdherenceBasedOnTheCurrentTime()
		{

			var timeThatStateChangedToOutOfAdherence = new DateTime(2010, 12, 24, 8, 30, 0, 0);
			var now = new ThisIsNow(timeThatStateChangedToOutOfAdherence.AddMinutes(60));
			var data = new AdherencePercentageReadModel()
			           {
				           TimeInAdherence = TimeSpan.FromMinutes(60),
				           TimeOutOfAdherence = TimeSpan.FromMinutes(0),
				           LastTimestamp = timeThatStateChangedToOutOfAdherence,
				           IsLastTimeInAdherence = false,
			           };
			var repo = new PersisterForTest(data);
			var target = new CalculateAdherence(repo, now);

			var result = target.ForToday(Guid.Empty);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		[Test]
		public void HistoricalAdherence_WhenTimeIsAfterTheShiftHasEnded_ShouldNotBeIncludedInTheCalculation()
		{

			var timeThatStateChangedToOutOfAdherence = new DateTime(2010, 12, 24, 8, 30, 0, 0);
			var now = new ThisIsNow(timeThatStateChangedToOutOfAdherence.AddMinutes(60));
			var fiveMinutesAfterStateChanged = timeThatStateChangedToOutOfAdherence.AddMinutes(5);
			var data = new AdherencePercentageReadModel()
			           {
				           TimeInAdherence = TimeSpan.FromMinutes(5),
				           TimeOutOfAdherence = TimeSpan.FromMinutes(0),
				           LastTimestamp = timeThatStateChangedToOutOfAdherence,
				           IsLastTimeInAdherence = false,
				           ShiftEnd = fiveMinutesAfterStateChanged
			           };
			var repo = new PersisterForTest(data);
			var target = new CalculateAdherence(repo, now);

			var result = target.ForToday(Guid.Empty);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		[Test]
		public void HistoricalAdherence_WhenBothInAdherenceAndOutOfAdherenceIsZero_ShouldNotCalculateAdherenceAtAll()
		{
			var now = new ThisIsNow(DateTime.Now);
			var data = new AdherencePercentageReadModel()
			{
				TimeInAdherence = TimeSpan.FromMinutes(0),
				TimeOutOfAdherence = TimeSpan.FromMinutes(0),
				LastTimestamp = now.UtcDateTime(),
			};
			var repo = new PersisterForTest(data);
			var target = new CalculateAdherence(repo, now);

			var result = target.ForToday(Guid.Empty);

			result.IsValid.Should().Be.False();
		}

		[Test]
		public void HistoricalAdherence_WhenShiftEndsIsNotSet_ShouldBeComputedAsIfShiftIsOngoing()
		{
			var timeThatStateChangedToInAdherence = new DateTime(2001, 1, 12, 10, 0, 0, 0);
			var now = new ThisIsNow(timeThatStateChangedToInAdherence.AddMinutes(60));
			var data = new AdherencePercentageReadModel()
			{
				TimeInAdherence = TimeSpan.FromMinutes(0),
				TimeOutOfAdherence = TimeSpan.FromMinutes(60),
				LastTimestamp = timeThatStateChangedToInAdherence,
				IsLastTimeInAdherence = true,
				ShiftEnd = null
			};

			var repo = new PersisterForTest(data);
			var target = new CalculateAdherence(repo, now);

			var result = target.ForToday(Guid.Empty);

			result.AdherencePercent.Should().Be.EqualTo(50);
		}

		#region helpers
		public class PersisterForTest : IAdherencePercentageReadModelPersister
		{
			private readonly AdherencePercentageReadModel _model;

			public PersisterForTest(AdherencePercentageReadModel model)
			{
				_model = model;
			}

			public void Persist(AdherencePercentageReadModel model)
			{
				throw new NotImplementedException();
			}

			public AdherencePercentageReadModel Get(DateOnly date, Guid personId)
			{
				return _model;
			}
		}
		private static bool isFalsy(dynamic resultFromController)
		{
			return resultFromController.GetType().GetProperty("AdherencePercent") == null;
		}
		#endregion
	}
}
