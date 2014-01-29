using System;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping;
using Teleopti.Interfaces.Domain;

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

			viewModel.AverageAfterCallWork.Should().Be.EqualTo(new TimeSpan(0,0,40).TotalSeconds.ToString(CultureInfo.InvariantCulture));
		}

		[Test]
		public void ShouldMapTalkTimeAverage()
		{
			var target = new DailyMetricsMapper();
			var dataModel = new DailyMetricsForDayResult { TalkTimeAverage = new TimeSpan(0, 0, 110) };
			var viewModel = target.Map(dataModel);

			viewModel.AverageTalkTime.Should().Be.EqualTo(new TimeSpan(0, 0, 110).TotalSeconds.ToString(CultureInfo.InvariantCulture));
		}

		[Test]
		public void ShouldMapHandlingTimeAverage()
		{
			var target = new DailyMetricsMapper();
			var dataModel = new DailyMetricsForDayResult { HandlingTimeAverage = new TimeSpan(0, 0, 120) };
			var viewModel = target.Map(dataModel);

			viewModel.AverageHandlingTime.Should().Be.EqualTo(new TimeSpan(0, 0, 120).TotalSeconds.ToString(CultureInfo.InvariantCulture));
		}

		[Test]
		public void ShouldMapReadyTimePerScheduledReadyTime()
		{
			var target = new DailyMetricsMapper();
			var dataModel = new DailyMetricsForDayResult { ReadyTimePerScheduledReadyTime = new Percent(0.75) };
			var viewModel = target.Map(dataModel);

			viewModel.ReadyTimePerScheduledReadyTime.Should().Be.EqualTo(75.ToString(CultureInfo.InvariantCulture));
		}

		[Test]
		public void ShouldMapAdherence()
		{
			var target = new DailyMetricsMapper();
			var dataModel = new DailyMetricsForDayResult { Adherence = new Percent(0.80) };
			var viewModel = target.Map(dataModel);

			viewModel.Adherence.Should().Be.EqualTo(80.ToString(CultureInfo.InvariantCulture));
		}

		[Test]
		public void ShouldSetDataAvailableToFalseWhenDataIsUnavailable()
		{
			var target = new DailyMetricsMapper();
			var viewModel = target.Map(null);

			viewModel.DataAvailable.Should().Be.False();
		}

		[Test]
		public void ShouldSetDataAvailableToTrueWhenDataIsAvailable()
		{
			var target = new DailyMetricsMapper();
			var viewModel = target.Map(new DailyMetricsForDayResult());

			viewModel.DataAvailable.Should().Be.True();
		}
	}
}
