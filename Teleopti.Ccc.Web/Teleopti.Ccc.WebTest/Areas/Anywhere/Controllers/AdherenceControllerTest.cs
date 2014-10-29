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
		public void ForToday_ShouldGetTheReadModelForSuppliedAgentWithTodaysDate()
		{
			var thisIsNow = new DateTime(2014, 12, 24, 15, 0, 0);
			const int expectedMinutesInAdherence = 12;
			const int expectedminutesOutOfAdherence = 36;
			var expectedLastTimeStamp = thisIsNow.AddMinutes(-5);
			var personId = Guid.NewGuid();
			var now = new ThisIsNow(thisIsNow);
			var readModelRepo = MockRepository.GenerateStub<IAdherencePercentageReadModelPersister>();

			readModelRepo.Stub(r => r.Get(new DateOnly(now.UtcDateTime()), personId)).Return(new AdherencePercentageReadModel()
			                                                                                 {
				                                                                                 MinutesInAdherence =
					                                                                                 expectedMinutesInAdherence,
				                                                                                 MinutesOutOfAdherence =
					                                                                                 expectedminutesOutOfAdherence,
				                                                                                 LastTimestamp =
					                                                                                 expectedLastTimeStamp
			                                                                                 });

			var target = new AdherenceController(readModelRepo, now);
			dynamic result = target.ForToday(personId).Data;

			((object) result.MinutesInAdherence).Should().Be.EqualTo(expectedMinutesInAdherence);
			((object) result.MinutesOutOfAdherence).Should().Be.EqualTo(expectedminutesOutOfAdherence);
			((object) result.LastTimestamp).Should().Be.EqualTo(expectedLastTimeStamp);
		}

		[Test]
		public void HistoricalAdherence_WhenInAdherenceAndOutOfAdherenceAreSame_ShouldBeFiftyPercent()
		{
			var now = new ThisIsNow(DateTime.Now);
			var data = new AdherencePercentageReadModel()
			           {
				           MinutesInAdherence = 74,
				           MinutesOutOfAdherence = 74,
				           LastTimestamp = now.UtcDateTime(),
			           };
			var repo = new PersisterForTest(data);
			var target = new AdherenceController(repo, now);

			dynamic result = target.ForToday(Guid.Empty).Data;

			((object) result.AdherencePercent).Should().Be.EqualTo(fiftyPercent);
		}

		[Test]
		public void HistoricalAdherence_WhenOnlyInAdherence_ShouldBeHundredPercent()
		{
			var now = new ThisIsNow(DateTime.Now);
			var data = new AdherencePercentageReadModel()
			           {
				           MinutesInAdherence = 12,
				           MinutesOutOfAdherence = 0,
				           LastTimestamp = now.UtcDateTime(),
			           };
			var repo = new PersisterForTest(data);

			var target = new AdherenceController(repo, now);
			dynamic result = target.ForToday(Guid.Empty).Data;
			((object) result.AdherencePercent).Should().Be.EqualTo(hundredPercent);
		}

		[Test]
		public void HistoricalAdherence_WhenOnlyOutOfAdherence_ShouldBeZeroPercent()
		{
			var data = new AdherencePercentageReadModel()
			           {
				           MinutesInAdherence = 0,
				           MinutesOutOfAdherence = 54
			           };

			var repo = new PersisterForTest(data);

			var target = new AdherenceController(repo, new Now());
			dynamic result = target.ForToday(Guid.Empty).Data;
			((object) result.AdherencePercent).Should().Be.EqualTo(zeroPercent);
		}

		[Test]
		public void HistoricalAdherence_WhenThereIsNoReadmodel_ShouldBeFalsy()
		{
			var repo = new PersisterForTest(null);
			var target = new AdherenceController(repo, new Now());

			var result = target.ForToday(Guid.Empty);

			Assert.That(isFalsy(result));
		}

		[Test]
		public void HistoricalAdherence_WhenAgentIsInAdherence_ShouldCalculateThatTimeAsInAdherenceBasedOnTheCurrentTime()
		{
			var timeThatStateChangedToInAdherence = new DateTime(2001, 1, 12, 10, 0, 0, 0);
			var now = new ThisIsNow(timeThatStateChangedToInAdherence.AddMinutes(60));
			var data = new AdherencePercentageReadModel()
			           {
				           MinutesInAdherence = 0,
				           MinutesOutOfAdherence = 60,
				           LastTimestamp = timeThatStateChangedToInAdherence,
				           IsLastTimeInAdherence = true,
				           ShiftEnd = now.UtcDateTime().AddHours(2)
			           };

			var repo = new PersisterForTest(data);
			var target = new AdherenceController(repo, now);

			dynamic result = target.ForToday(Guid.Empty).Data;

			((object) result.AdherencePercent).Should().Be.EqualTo(fiftyPercent);
		}

		[Test]
		public void HistoricalAdherence_WhenAgentIsOutOfAdherence_ShouldCalculateThatTimeAsInAdherenceBasedOnTheCurrentTime()
		{

			var timeThatStateChangedToOutOfAdherence = new DateTime(2010, 12, 24, 8, 30, 0, 0);
			var now = new ThisIsNow(timeThatStateChangedToOutOfAdherence.AddMinutes(60));
			var data = new AdherencePercentageReadModel()
			           {
				           MinutesInAdherence = 60,
				           MinutesOutOfAdherence = 0,
				           LastTimestamp = timeThatStateChangedToOutOfAdherence,
				           IsLastTimeInAdherence = false,
			           };
			var repo = new PersisterForTest(data);
			var target = new AdherenceController(repo, now);

			dynamic result = target.ForToday(Guid.Empty).Data;

			((object) result.AdherencePercent).Should().Be.EqualTo(fiftyPercent);
		}

		[Test]
		public void HistoricalAdherence_WhenTimeIsAfterTheShiftHasEnded_ShouldNotBeIncludedInTheCalculation()
		{

			var timeThatStateChangedToOutOfAdherence = new DateTime(2010, 12, 24, 8, 30, 0, 0);
			var now = new ThisIsNow(timeThatStateChangedToOutOfAdherence.AddMinutes(60));
			var fiveMinutesAfterStateChanged = timeThatStateChangedToOutOfAdherence.AddMinutes(5);
			var data = new AdherencePercentageReadModel()
			           {
				           MinutesInAdherence = 5,
				           MinutesOutOfAdherence = 0,
				           LastTimestamp = timeThatStateChangedToOutOfAdherence,
				           IsLastTimeInAdherence = false,
				           ShiftEnd = fiveMinutesAfterStateChanged
			           };
			var repo = new PersisterForTest(data);
			var target = new AdherenceController(repo, now);

			dynamic result = target.ForToday(Guid.Empty).Data;

			((object) result.AdherencePercent).Should().Be.EqualTo(fiftyPercent);
		}

		[Test]
		public void HistoricalAdherence_WhenBothInAdherenceAndOutOfAdherenceIsZero_ShouldNotCalculateAdherenceAtAll()
		{
			var now = new ThisIsNow(DateTime.Now);
			var data = new AdherencePercentageReadModel()
			{
				MinutesInAdherence = 0,
				MinutesOutOfAdherence = 0,
				LastTimestamp = now.UtcDateTime(),
			};
			var repo = new PersisterForTest(data);
			var target = new AdherenceController(repo, now);

			dynamic result = target.ForToday(Guid.Empty).Data;

			Assert.That(isFalsy(result));
		}


		[Test]
		public void HistoricalAdherence_WhenShiftEndsIsNotSet_ShouldBeComputedAsIfShiftIsOngoing()
		{
			var timeThatStateChangedToInAdherence = new DateTime(2001, 1, 12, 10, 0, 0, 0);
			var now = new ThisIsNow(timeThatStateChangedToInAdherence.AddMinutes(60));
			var data = new AdherencePercentageReadModel()
			{
				MinutesInAdherence = 0,
				MinutesOutOfAdherence = 60,
				LastTimestamp = timeThatStateChangedToInAdherence,
				IsLastTimeInAdherence = true,
				ShiftEnd = null
			};

			var repo = new PersisterForTest(data);
			var target = new AdherenceController(repo, now);

			dynamic result = target.ForToday(Guid.Empty).Data;

			((object)result.AdherencePercent).Should().Be.EqualTo(fiftyPercent);
		}

		#region helpers

		private static int fiftyPercent
		{
			get { return 50; }
		}

		private static int hundredPercent
		{
			get { return 100; }
		}

		private static int zeroPercent
		{
			get { return 0; }
		}

		private static bool isFalsy(dynamic resultFromController)
		{
			return resultFromController.GetType().GetProperty("AdherencePercent") == null;
		}

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
		#endregion
	}
}
