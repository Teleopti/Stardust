using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsPersonPeriodDateFixerTests
	{
		private IAnalyticsPersonPeriodDateFixer _analyticsPersonPeriodDateFixer;
		private FakeAnalyticsDateRepository analyticsDateRepository;
		FakeAnalyticsIntervalRepository analyticsIntervalRepository;

		[SetUp]
		public void Setup()
		{
			analyticsDateRepository = new FakeAnalyticsDateRepository(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			analyticsIntervalRepository = new FakeAnalyticsIntervalRepository();
			_analyticsPersonPeriodDateFixer = new AnalyticsPersonPeriodDateFixer(analyticsDateRepository, analyticsIntervalRepository);
		}

		[Test]
		public void PersonPeriodStartDateGreaterEternity_Transform_Eternity()
		{
			var validFromDate = _analyticsPersonPeriodDateFixer.ValidFromDate(
				new DateTime(2067, 03, 06),
				TimeZoneInfo.Utc);

			Assert.AreEqual(AnalyticsDate.Eternity.DateDate, validFromDate);
		}

		[Test]
		public void PersonPeriodStartDateEternity_Transform_Eternity()
		{
			var validFromDate = _analyticsPersonPeriodDateFixer.ValidFromDate(
				AnalyticsDate.Eternity.DateDate,
				TimeZoneInfo.Utc);

			Assert.AreEqual(AnalyticsDate.Eternity.DateDate, validFromDate);
		}

        [Test]
        public void PersonPeriodStartDateBeforeMinDate_Transform_MinDate()
        {
            var validFromDate = _analyticsPersonPeriodDateFixer.ValidFromDate(
                new DateTime(1999, 12, 31),
                TimeZoneInfo.Utc);

            Assert.AreEqual(new DateTime(2000, 1, 1), validFromDate);
        }

        [Test]
		public void PersonPeriodStartDate_Transform_SameStart()
		{
			var validFromDate = _analyticsPersonPeriodDateFixer.ValidFromDate(
				new DateTime(2020, 01, 01),
				TimeZoneInfo.Utc);

			Assert.AreEqual(new DateTime(2020, 01, 01), validFromDate);
		}

		[Test]
		public void PersonPeriodEndDate_Transform_EternityOld()
		{
			var oldDateGetting = new AnalyticsPersonPeriodDateFixerWithoutDateCreation(analyticsDateRepository, analyticsIntervalRepository);
			var validToDate = oldDateGetting.ValidToDate(
				new DateTime(2021, 03, 06),
				TimeZoneInfo.Utc);

			Assert.AreEqual(AnalyticsDate.Eternity.DateDate, validToDate);
		}

		[Test]
		public void PersonPeriodEndDate_Transform_NotEternity()
		{
			var validToDate = _analyticsPersonPeriodDateFixer.ValidToDate(
				new DateTime(2021, 03, 06),
				TimeZoneInfo.Utc);

			validToDate.Should().Not.Be.EqualTo(AnalyticsDate.Eternity.DateDate);
		}

		[Test]
		public void PersonPeriodEndDate_Transform_NewEndDate()
		{
			var validToDate = _analyticsPersonPeriodDateFixer.ValidToDate(
				new DateTime(2011, 03, 06),
				TimeZoneInfo.Utc);

			Assert.AreEqual(new DateTime(2011, 03, 07), validToDate);
		}

		[Test]
		public void PersonPeriodEndDateEternity_Transform_Eternity()
		{
			var validToDate = _analyticsPersonPeriodDateFixer.ValidToDate(
				AnalyticsDate.Eternity.DateDate,
				TimeZoneInfo.Utc);

			Assert.AreEqual(AnalyticsDate.Eternity.DateDate, validToDate);
		}

		[Test]
		public void PersonPeriodValidFromDate_Transform_CorrectIntervalId()
		{
			var validFrom = new DateTime(2016, 03, 03, 12, 15, 00);
			var res = _analyticsPersonPeriodDateFixer.ValidFromIntervalId(validFrom, 96);
			Assert.AreEqual(49, res);
		}

		[Test]
		public void PersonPeriodValidFromDate_Transform_FirstIntervalId()
		{
			var validFrom = new DateTime(2016, 03, 03);
			var res = _analyticsPersonPeriodDateFixer.ValidFromIntervalId(validFrom, 96);
			Assert.AreEqual(0, res);
		}

		[Test]
		public void PersonPeriodValidFromDate_Transform_LastIntervalId()
		{
			var validFrom = new DateTime(2016, 03, 03, 23, 45, 00);
			var res = _analyticsPersonPeriodDateFixer.ValidFromIntervalId(validFrom, 96);
			Assert.AreEqual(95, res);
		}

		[Test]
		public void PersonPeriodValidToDate_Transform_IntervalId()
		{
			var validTo = new DateTime(2016, 05, 01, 12, 0, 0);
			var res = _analyticsPersonPeriodDateFixer.ValidToIntervalId(validTo, 96);
			Assert.AreEqual(47, res);
		}

		[Test]
		public void PersonPeriodValidToDate_Transform_LastIntervalId()
		{
			var validTo = new DateTime(2016, 05, 01);
			var res = _analyticsPersonPeriodDateFixer.ValidToIntervalId(validTo, 96);
			Assert.AreEqual(95, res);
		}

		[Test]
		public void PersonPeriodValidToDate_Transform_ADateId()
		{
			var validTo = new DateTime(2016, 05, 01);
			var res = _analyticsPersonPeriodDateFixer.GetValidToDateIdMaxDate(validTo, 10);
			Assert.AreEqual(10, res);
		}

		[Test]
		public void PersonPeriodValidToDateEternity_Transform_SecondToMaxDateId()
		{
			var validTo = AnalyticsDate.Eternity.DateDate;
			var res = _analyticsPersonPeriodDateFixer.GetValidToDateIdMaxDate(validTo, 10);
			Assert.AreEqual(analyticsDateRepository.MaxDate().DateId-1, res);
		}

		[Test]
		public void PersonPeriodValidToDateEternity_Transform_ShouldNotReturnMinus3AsDateId()
		{
			var validTo = AnalyticsDate.Eternity.DateDate;
			var res = _analyticsPersonPeriodDateFixer.GetValidToDateIdMaxDate(validTo, 10);
			res.Should().Not.Be.EqualTo(-3);
		}
		
		[Test]
		public void NormalDate_MapDateToId_Date()
		{
			var date = new DateTime(2015, 01, 01);
			var dateId = _analyticsPersonPeriodDateFixer.MapDateId(date);
			dateId.Should().Be.GreaterThanOrEqualTo(0);
		}

		[Test]
		public void BigBangDate_MapDateToId_NotDefinedDate()
		{
			var date = AnalyticsDate.NotDefined.DateDate;
			var dateId = _analyticsPersonPeriodDateFixer.MapDateId(date);
			Assert.AreEqual(AnalyticsDate.NotDefined.DateId, dateId);
		}

		[Test]
		public void Eternity_MapDateToId_EternityDate()
		{
			var date = AnalyticsDate.Eternity.DateDate;
			var dateId = _analyticsPersonPeriodDateFixer.MapDateId(date);
			Assert.AreEqual(AnalyticsDate.Eternity.DateId, dateId);
		}

		[Test]
		public void LessThanMinDate_MapDateToId_NotDefinedDate()
		{
			var date = new DateTime(1995, 12, 31);
			var dateId = _analyticsPersonPeriodDateFixer.MapDateId(date);
			Assert.AreEqual(-1, dateId);
		}
	}
}