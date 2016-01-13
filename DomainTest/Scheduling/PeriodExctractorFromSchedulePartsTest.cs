using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class PeriodExctractorFromSchedulePartsTest
	{
		private PeriodExctractorFromScheduleParts _target;

		[SetUp]
		public void Setup()
		{
			_target = new PeriodExctractorFromScheduleParts();
		}

		[Test]
		public void ShouldExtractPeriod()
		{		
			var p1 = ScheduleDayFactory.Create(new DateOnly(2016, 1, 12));
			var p2 = ScheduleDayFactory.Create(new DateOnly(2016, 1, 22));
			var p3 = ScheduleDayFactory.Create(new DateOnly(2016, 1, 20));
			var parts = new List<IScheduleDay>{p1, p2};
			var result = _target.ExtractPeriod(parts);
			result.StartDate.Should().Be.EqualTo(new DateOnly(2016, 1, 12));
			result.EndDate.Should().Be.EqualTo(new DateOnly(2016, 1, 22));
		}
	}
}