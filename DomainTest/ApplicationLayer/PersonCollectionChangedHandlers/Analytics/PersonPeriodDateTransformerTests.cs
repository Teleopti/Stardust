using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics.Transformer;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	public class PersonPeriodDateTransformerTests
	{
		[Test]
		public void PersonPeriodStartDateGreaterEternity_Transform_Eternity()
		{
			var validFromDate = PersonPeriodTransformer.ValidFromDate(
				new DateTime(2067, 03, 06),
				TimeZoneInfo.Utc);

			Assert.AreEqual(new DateTime(2059, 12, 31), validFromDate);
		}

		[Test]
		public void PersonPeriodStartDateEternity_Transform_Eternity()
		{
			var validFromDate = PersonPeriodTransformer.ValidFromDate(
				new DateTime(2059, 12, 31),
				TimeZoneInfo.Utc);

			Assert.AreEqual(new DateTime(2059, 12, 31), validFromDate);
		}

		[Test]
		public void PersonPeriodStartDate_Transform_SameStart()
		{
			var validFromDate = PersonPeriodTransformer.ValidFromDate(
				new DateTime(2020, 01, 01),
				TimeZoneInfo.Utc);

			Assert.AreEqual(new DateTime(2020, 01, 01), validFromDate);
		}

		[Test]
		public void PersonPeriodEndDate_Transform_Eternity()
		{
			var validToDate = PersonPeriodTransformer.ValidToDate(
				new DateTime(2021, 03, 06),
				TimeZoneInfo.Utc,
				new DateTime(2020, 01, 01));

			Assert.AreEqual(new DateTime(2059, 12, 31), validToDate);
		}

		[Test]
		public void PersonPeriodEndDate_Transform_NewEndDate()
		{
			var validToDate = PersonPeriodTransformer.ValidToDate(
				new DateTime(2011, 03, 06),
				TimeZoneInfo.Utc,
				new DateTime(2020, 01, 01));

			Assert.AreEqual(new DateTime(2011, 03, 07), validToDate);
		}

		[Test]
		public void PersonPeriodEndDateEternity_Transform_Eternity()
		{
			var validToDate = PersonPeriodTransformer.ValidToDate(
				new DateTime(2059, 12, 31),
				TimeZoneInfo.Utc,
				new DateTime(2020, 01, 01));

			Assert.AreEqual(new DateTime(2059, 12, 31), validToDate);
		}

		[Test]
		public void PersonPeriodValidFromDate_Transform_CorrectIntervalId()
		{
			var validFrom = new DateTime(2016, 03, 03, 12, 15, 00);
			var res = PersonPeriodTransformer.ValidFromIntervalId(validFrom, 96);
			Assert.AreEqual(49, res);
		}

		[Test]
		public void PersonPeriodValidFromDate_Transform_FirstIntervalId()
		{
			var validFrom = new DateTime(2016, 03, 03);
			var res = PersonPeriodTransformer.ValidFromIntervalId(validFrom, 96);
			Assert.AreEqual(0, res);
		}

		[Test]
		public void PersonPeriodValidFromDate_Transform_LastIntervalId()
		{
			var validFrom = new DateTime(2016, 03, 03, 23, 45, 00);
			var res = PersonPeriodTransformer.ValidFromIntervalId(validFrom, 96);
			Assert.AreEqual(95, res);
		}



		[Test]
		public void PersonPeriodValidToDate_Transform_IntervalId()
		{
			var validTo = new DateTime(2016, 05, 01, 12, 0, 0);
			var res = PersonPeriodTransformer.ValidToIntervalId(validTo, 96);
			Assert.AreEqual(47, res);
		}

		[Test]
		public void PersonPeriodValidToDate_Transform_LastIntervalId()
		{
			var validTo = new DateTime(2016, 05, 01);
			var res = PersonPeriodTransformer.ValidToIntervalId(validTo, 96);
			Assert.AreEqual(95, res);
		}

		[Test]
		public void PersonPeriodValidToDate_Transform_ADateId()
		{
			var validTo = new DateTime(2016, 05, 01);
			var maxDate = new AnalyticsDate
			{
				DateId = 1000,
				DateDate = new DateTime(2020, 12, 31)
			};
			var res = PersonPeriodTransformer.GetValidToDateIdMaxDate(validTo, maxDate, 10);
			Assert.AreEqual(10, res);
		}

		[Test]
		public void PersonPeriodValidToDateEternity_Transform_SecondToMaxDateId()
		{
			var validTo = new DateTime(2059, 12, 31);
			var maxDate = new AnalyticsDate
			{
				DateId = 1000,
				DateDate = new DateTime(2020, 12, 31)
			};
			var res = PersonPeriodTransformer.GetValidToDateIdMaxDate(validTo, maxDate, 10);
			Assert.AreEqual(999, res);
		}
	}
}