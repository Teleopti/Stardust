using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation.IntraIntervalAnalyze
{
	[TestFixture]
	public class SkillActivityCounterTest
	{
		private SkillActivityCounter _target;

		[SetUp]
		public void SetUp()
		{
			_target = new SkillActivityCounter();	
		}

		[Test]
		public void ShouldCountEvery5Minutes()
		{
			var dateTime1 = new DateTime(2014, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var dateTime2 = new DateTime(2014, 1, 1, 10, 5, 0, DateTimeKind.Utc);

			var dateTime3 = new DateTime(2014, 1, 1, 10, 5, 0, DateTimeKind.Utc);
			var dateTime4 = new DateTime(2014, 1, 1, 10, 10, 0, DateTimeKind.Utc);

			var dateTime5 = new DateTime(2014, 1, 1, 10, 10, 0, DateTimeKind.Utc);
			var dateTime6 = new DateTime(2014, 1, 1, 10, 15, 0, DateTimeKind.Utc);

			var dateTimePeriod1 = new DateTimePeriod(dateTime1, dateTime2);
			var dateTimePeriod2 = new DateTimePeriod(dateTime3, dateTime4);
			var dateTimePeriod3 = new DateTimePeriod(dateTime5, dateTime6);

			var periods = new List<DateTimePeriod> {dateTimePeriod1, dateTimePeriod2, dateTimePeriod3};
			var interval = new DateTimePeriod(dateTime1, dateTime6);

			var result = _target.Count(periods, interval);

			Assert.IsTrue(result.Count == 3);
		}
	}
}
