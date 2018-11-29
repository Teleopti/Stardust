using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class PeriodExctractorFromSchedulePartsTest
	{
		private PeriodExtractorFromScheduleParts _target;

		[SetUp]
		public void Setup()
		{
			_target = new PeriodExtractorFromScheduleParts();
		}

		[Test]
		public void ShouldExtractPeriod()
		{		
			var p1 = ScheduleDayFactory.Create(new DateOnly(2016, 1, 12));
			var p2 = ScheduleDayFactory.Create(new DateOnly(2016, 1, 22));
			var parts = new List<IScheduleDay>{p1, p2};
			var result = _target.ExtractPeriod(parts).Value;
			result.StartDate.Should().Be.EqualTo(new DateOnly(2016, 1, 12));
			result.EndDate.Should().Be.EqualTo(new DateOnly(2016, 1, 22));
		}

		[Test]
		public void ShouldReturnNullableWithNoValueWhenYouHaveNoScheduleParts()
		{
			var result = _target.ExtractPeriod(new List<IScheduleDay>());
			result.HasValue.Should().Be.False();
		}
	}
}