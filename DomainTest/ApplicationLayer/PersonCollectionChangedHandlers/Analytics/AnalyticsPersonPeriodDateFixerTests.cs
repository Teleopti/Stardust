using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[ToggleOff(Toggles.ETL_EventbasedDate_39562)]
	[TestFixture]
	[DomainTest]
	public class AnalyticsPersonPeriodDateFixerWithoutDateCreationTests
	{
		public IAnalyticsPersonPeriodDateFixer Target;
		public FakeAnalyticsDateRepository AnalyticsDateRepository;

		[Test]
		public void PersonPeriodEndDate_Transform_EternityOld()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validToDate = Target.ValidToDate(
				new DateTime(2021, 03, 06),
				TimeZoneInfo.Utc);

			Assert.AreEqual(AnalyticsDate.Eternity.DateDate, validToDate);
		}
	}

	[Toggle(Toggles.ETL_EventbasedDate_39562)]
	[TestFixture]
	[DomainTest]
	public class AnalyticsPersonPeriodDateFixerTests
	{
		public IAnalyticsPersonPeriodDateFixer Target;
		public FakeAnalyticsDateRepository AnalyticsDateRepository;
		public FakeAnalyticsIntervalRepository AnalyticsIntervalRepository;

		[Test]
		public void PersonPeriodStartDateGreaterEternity_Transform_Eternity()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validFromDate = Target.ValidFromDate(
				new DateTime(2067, 03, 06),
				TimeZoneInfo.Utc);

			Assert.AreEqual(AnalyticsDate.Eternity.DateDate, validFromDate);
		}

		[Test]
		public void PersonPeriodStartDateEternity_Transform_Eternity()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validFromDate = Target.ValidFromDate(
				AnalyticsDate.Eternity.DateDate,
				TimeZoneInfo.Utc);

			Assert.AreEqual(AnalyticsDate.Eternity.DateDate, validFromDate);
		}

        [Test]
        public void PersonPeriodStartDateBeforeMinDate_Transform_MinDate()
        {
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validFromDate = Target.ValidFromDate(
                new DateTime(1999, 12, 31),
                TimeZoneInfo.Utc);

            Assert.AreEqual(new DateTime(2000, 1, 1), validFromDate);
        }

        [Test]
		public void PersonPeriodStartDate_Transform_SameStart()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validFromDate = Target.ValidFromDate(
				new DateTime(2020, 01, 01),
				TimeZoneInfo.Utc);

			Assert.AreEqual(new DateTime(2020, 01, 01), validFromDate);
		}


		[Test]
		public void PersonPeriodEndDate_Transform_NotEternity()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validToDate = Target.ValidToDate(
				new DateTime(2021, 03, 06),
				TimeZoneInfo.Utc);

			validToDate.Should().Not.Be.EqualTo(AnalyticsDate.Eternity.DateDate);
		}

		[Test]
		public void PersonPeriodEndDate_Transform_NewEndDate()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validToDate = Target.ValidToDate(
				new DateTime(2011, 03, 06),
				TimeZoneInfo.Utc);

			Assert.AreEqual(new DateTime(2011, 03, 07), validToDate);
		}

		[Test]
		public void PersonPeriodEndDateEternity_Transform_Eternity()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validToDate = Target.ValidToDate(
				AnalyticsDate.Eternity.DateDate,
				TimeZoneInfo.Utc);

			Assert.AreEqual(AnalyticsDate.Eternity.DateDate, validToDate);
		}

		[Test]
		public void PersonPeriodValidFromDate_Transform_CorrectIntervalId()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validFrom = new DateTime(2016, 03, 03, 12, 15, 00);
			var res = Target.ValidFromIntervalId(validFrom, 96);
			Assert.AreEqual(49, res);
		}

		[Test]
		public void PersonPeriodValidFromDate_Transform_FirstIntervalId()
		{
			var validFrom = new DateTime(2016, 03, 03);
			var res = Target.ValidFromIntervalId(validFrom, 96);
			Assert.AreEqual(0, res);
		}

		[Test]
		public void PersonPeriodValidFromDate_Transform_LastIntervalId()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validFrom = new DateTime(2016, 03, 03, 23, 45, 00);
			var res = Target.ValidFromIntervalId(validFrom, 96);
			Assert.AreEqual(95, res);
		}

		[Test]
		public void PersonPeriodValidToDate_Transform_IntervalId()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validTo = new DateTime(2016, 05, 01, 12, 0, 0);
			var res = Target.ValidToIntervalId(validTo, 96);
			Assert.AreEqual(47, res);
		}

		[Test]
		public void PersonPeriodValidToDate_Transform_LastIntervalId()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validTo = new DateTime(2016, 05, 01);
			var res = Target.ValidToIntervalId(validTo, 96);
			Assert.AreEqual(95, res);
		}

		[Test]
		public void PersonPeriodValidToDate_Transform_ADateId()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validTo = new DateTime(2016, 05, 01);
			var res = Target.GetValidToDateIdMaxDate(validTo, 10);
			Assert.AreEqual(10, res);
		}

		[Test]
		public void PersonPeriodValidToDateEternity_Transform_SecondToMaxDateId()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validTo = AnalyticsDate.Eternity.DateDate;
			var res = Target.GetValidToDateIdMaxDate(validTo, 10);
			Assert.AreEqual(AnalyticsDateRepository.MaxDate().DateId-1, res);
		}

		[Test]
		public void PersonPeriodValidToDateEternity_Transform_ShouldNotReturnMinus3AsDateId()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var validTo = AnalyticsDate.Eternity.DateDate;
			var res = Target.GetValidToDateIdMaxDate(validTo, 10);
			res.Should().Not.Be.EqualTo(-3);
		}
		
		[Test]
		public void NormalDate_MapDateToId_Date()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var date = new DateTime(2015, 01, 01);
			var dateId = Target.MapDateId(date);
			dateId.Should().Be.GreaterThanOrEqualTo(0);
		}

		[Test]
		public void BigBangDate_MapDateToId_NotDefinedDate()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var date = AnalyticsDate.NotDefined.DateDate;
			var dateId = Target.MapDateId(date);
			Assert.AreEqual(AnalyticsDate.NotDefined.DateId, dateId);
		}

		[Test]
		public void Eternity_MapDateToId_EternityDate()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var date = AnalyticsDate.Eternity.DateDate;
			var dateId = Target.MapDateId(date);
			Assert.AreEqual(AnalyticsDate.Eternity.DateId, dateId);
		}

		[Test]
		public void LessThanMinDate_MapDateToId_NotDefinedDate()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2000, 1, 1), new DateTime(2020, 01, 01));
			var date = new DateTime(1995, 12, 31);
			var dateId = Target.MapDateId(date);
			Assert.AreEqual(-1, dateId);
		}
	}
}