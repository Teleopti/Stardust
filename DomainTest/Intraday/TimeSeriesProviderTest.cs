using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[TestFixture]
	public class TimeSeriesProviderTest
	{
		[Test]
		public void ShouldRemoveInvalidTimeAtDayLightTimeSavingBegins()
		{
			var forcastedStaffing = new List<StaffingIntervalModel>
			{
				new StaffingIntervalModel{StartTime = new DateTime(2018, 03, 25, 1, 0, 0)}
			};

			var scheduledStaffing = new List<SkillStaffingIntervalLightModel>
			{
				new SkillStaffingIntervalLightModel{StartDateTime = new DateTime(2018, 03, 25, 21, 0, 0)}
			};

			var timeSeries = TimeSeriesProvider.DataSeries(forcastedStaffing, scheduledStaffing, 15, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

			timeSeries.Length.Should().Be(20 * 4 + 1 - 4);
			timeSeries.Count(ts => ts.Hour == 2).Should().Be(0);
		}
		
		[Test]
		public void ShouldHandleAmbiguousTimeAtDayLightTimeSavingEnds()
		{
			var forcastedStaffing = new List<StaffingIntervalModel>
			{
				new StaffingIntervalModel{StartTime = new DateTime(2018, 10, 28, 1, 0, 0)}
			};

			var scheduledStaffing = new List<SkillStaffingIntervalLightModel>
			{
				new SkillStaffingIntervalLightModel{StartDateTime = new DateTime(2018, 10, 28, 21, 0, 0)}
			};

			var timeSeries = TimeSeriesProvider.DataSeries(forcastedStaffing, scheduledStaffing, 15, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

			timeSeries.Length.Should().Be(20 * 4 + 1 + 4);
			timeSeries.Count(ts => ts.Hour == 2 && ts.Minute == 0).Should().Be(2);
			timeSeries[3].Should().Be(new DateTime(2018, 10, 28, 1, 45, 0));
			timeSeries[4].Should().Be(new DateTime(2018, 10, 28, 2, 0, 0));
			timeSeries[5].Should().Be(new DateTime(2018, 10, 28, 2, 15, 0));
			timeSeries[6].Should().Be(new DateTime(2018, 10, 28, 2, 30, 0));
			timeSeries[7].Should().Be(new DateTime(2018, 10, 28, 2, 45, 0));
			timeSeries[8].Should().Be(new DateTime(2018, 10, 28, 2, 0, 0));
			timeSeries[9].Should().Be(new DateTime(2018, 10, 28, 2, 15, 0));
			timeSeries[10].Should().Be(new DateTime(2018, 10, 28, 2, 30, 0));
			timeSeries[11].Should().Be(new DateTime(2018, 10, 28, 2, 45, 0));
			timeSeries[12].Should().Be(new DateTime(2018, 10, 28, 3, 0, 0));
		}
	}
}