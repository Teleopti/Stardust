using System;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.MyReport.Mapping
{
	[TestFixture]
	public class DailyMetricsMapperTest
	{
		[Test]
		public void ShouldMapAnsweredCalls()
		{
			var target = new DailyMetricsMapper();
			var dataModel = new DailyMetricsForDayResult {AnsweredCalls = 55};
			var viewModel = target.Map(dataModel);

			viewModel.AnsweredCalls.Should().Be.EqualTo(dataModel.AnsweredCalls);
		}

		[Test]
		public void ShouldMapAfterCallWorkTimeAverage()
		{
			var target = new DailyMetricsMapper();
			var dataModel = new DailyMetricsForDayResult { AfterCallWorkTimeAverage = new TimeSpan(0,0,40) };
			var viewModel = target.Map(dataModel);

			viewModel.AverageAfterCallWork.Should().Be.EqualTo(dataModel.AfterCallWorkTimeAverage.TotalSeconds.ToString(CultureInfo.InvariantCulture));
		}

		[Test]
		public void ShouldMapTalkTimeAverage()
		{
			var target = new DailyMetricsMapper();
			var dataModel = new DailyMetricsForDayResult { AfterCallWorkTimeAverage = new TimeSpan(0, 0, 110) };
			var viewModel = target.Map(dataModel);

			viewModel.AverageTalkTime.Should().Be.EqualTo(dataModel.TalkTimeAverage.TotalSeconds.ToString(CultureInfo.InvariantCulture));
		}

		[Test]
		public void ShouldMapHandlingTimeAverage()
		{
			var target = new DailyMetricsMapper();
			var dataModel = new DailyMetricsForDayResult { AfterCallWorkTimeAverage = new TimeSpan(0, 0, 120) };
			var viewModel = target.Map(dataModel);

			viewModel.AverageHandlingTime.Should().Be.EqualTo(dataModel.HandlingTimeAverage.TotalSeconds.ToString(CultureInfo.InvariantCulture));
		}

		[Test]
		public void ShouldMapReadyTimePerScheduledReadyTime()
		{
			var target = new DailyMetricsMapper();
			var dataModel = new DailyMetricsForDayResult { AfterCallWorkTimeAverage = new TimeSpan(0, 0, 75) };
			var viewModel = target.Map(dataModel);

			viewModel.ReadyTimePerScheduledReadyTime.Should().Be.EqualTo(dataModel.ReadyTimePerScheduledReadyTime.Value.ToString(CultureInfo.InvariantCulture));
		}

		[Test]
		public void ShouldMapAdherence()
		{
			var target = new DailyMetricsMapper();
			var dataModel = new DailyMetricsForDayResult { AfterCallWorkTimeAverage = new TimeSpan(0, 0, 66) };
			var viewModel = target.Map(dataModel);

			viewModel.Adherence.Should().Be.EqualTo(dataModel.Adherence.Value.ToString(CultureInfo.InvariantCulture));
		}
	}
}
