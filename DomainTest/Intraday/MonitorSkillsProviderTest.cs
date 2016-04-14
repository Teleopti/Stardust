using System;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class MonitorSkillsProviderTest
	{
		public MonitorSkillsProvider Target;
		public FakeIntradayMonitorDataLoader IntradayMonitorDataLoader;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;

		private IncomingIntervalModel _firstInterval;
		private IncomingIntervalModel _secondInterval;
		private const int minutesPerInterval = 15;

		[SetUp]
		public void Setup()
		{
			_firstInterval = new IncomingIntervalModel()
			{
				IntervalId = 32,
				ForecastedCalls = 10,
				ForecastedHandleTime = 120,
				OfferedCalls = 12,
				HandleTime = 200
			};
			_secondInterval = new IncomingIntervalModel()
			{
				IntervalId = 33,
				ForecastedCalls = 15,
				ForecastedHandleTime = 150,
				OfferedCalls = 16,
				HandleTime = 180
			};
		}

		[Test]
		public void ShouldSummarise()
		{
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.Summary.ForecastedCalls.Should().Be.EqualTo(25);
			viewModel.Summary.ForecastedHandleTime.Should().Be.EqualTo(270);
			viewModel.Summary.ForecastedAverageHandleTime.Should().Be.EqualTo(270d / 25d);
			viewModel.Summary.OfferedCalls.Should().Be.EqualTo(28);
			viewModel.Summary.HandleTime.Should().Be.EqualTo(380);
			viewModel.Summary.AverageHandleTime.Should().Be.EqualTo(380d / 28d);

		}
        
        [Test, SetCulture("sv-SE")]
        public void ShouldReturnTimeSeries()
		{
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.DataSeries.Time.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.Time.First().Should().Be.EqualTo(DateTime.MinValue.AddMinutes(_firstInterval.IntervalId * minutesPerInterval));
			viewModel.DataSeries.Time.Second().Should().Be.EqualTo(DateTime.MinValue.AddMinutes(_secondInterval.IntervalId * minutesPerInterval));
		}

		[Test]
		public void ShouldReturnForecastedCallsSeries()
		{
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.DataSeries.ForecastedCalls.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.ForecastedCalls.First().Should().Be.EqualTo(_firstInterval.ForecastedCalls);
			viewModel.DataSeries.ForecastedCalls.Second().Should().Be.EqualTo(_secondInterval.ForecastedCalls);
		}

		[Test]
		public void ShouldReturnForecastedAverageHandleTimeSeries()
		{
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.DataSeries.ForecastedAverageHandleTime.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.ForecastedAverageHandleTime.First().Should().Be.EqualTo(_firstInterval.ForecastedAverageHandleTime);
			viewModel.DataSeries.ForecastedAverageHandleTime.Second().Should().Be.EqualTo(_secondInterval.ForecastedAverageHandleTime);
		}

		[Test]
		public void ShouldReturnOfferedCallsSeries()
		{
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.DataSeries.OfferedCalls.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.OfferedCalls.First().Should().Be.EqualTo(_firstInterval.OfferedCalls);
			viewModel.DataSeries.OfferedCalls.Second().Should().Be.EqualTo(_secondInterval.OfferedCalls);
		}

		[Test]
		public void ShouldReturnAverageHandleTimeSeries()
		{
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.DataSeries.AverageHandleTime.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.AverageHandleTime.First().Should().Be.EqualTo(_firstInterval.AverageHandleTime);
			viewModel.DataSeries.AverageHandleTime.Second().Should().Be.EqualTo(_secondInterval.AverageHandleTime);
		}

		[Test]
		public void ShouldFillWithNullValueIfNoOfferedCalls()
		{
			var thirdInterval = new IncomingIntervalModel()
			{
				IntervalId = 34,
				ForecastedCalls = 12,
				ForecastedHandleTime = 140,
				OfferedCalls = 12,
				HandleTime = 200
			};
			_secondInterval.OfferedCalls = null;
			_secondInterval.HandleTime = null;
			_secondInterval.AverageHandleTime = null;
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntradayMonitorDataLoader.AddInterval(thirdInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.DataSeries.OfferedCalls.Length.Should().Be.EqualTo(3);
			viewModel.DataSeries.OfferedCalls[1].Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldFillWithNullValueIfNoAverageHandleTime()
		{
			var thirdInterval = new IncomingIntervalModel()
			{
				IntervalId = 34,
				ForecastedCalls = 12,
				ForecastedHandleTime = 140,
				OfferedCalls = 12,
				HandleTime = 200
			};
			_secondInterval.OfferedCalls = null;
			_secondInterval.HandleTime = null;
			_secondInterval.AverageHandleTime = 0;
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntradayMonitorDataLoader.AddInterval(thirdInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.DataSeries.AverageHandleTime.Length.Should().Be.EqualTo(3);
			viewModel.DataSeries.AverageHandleTime[1].Should().Be.EqualTo(null);
		}


		[Test]
		public void ShouldReturnLatestStatsTime()
		{
			_secondInterval.OfferedCalls = null;
			_secondInterval.HandleTime = null;
			_secondInterval.AverageHandleTime = null;
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.DataSeries.AverageHandleTime.Length.Should().Be.EqualTo(2);
			viewModel.LatestStatsTime.Should().Be.EqualTo(DateTime.MinValue.AddMinutes((_firstInterval.IntervalId + 1) * minutesPerInterval));
		}

		[Test]
		public void ShouldReturnLatestStatsTimeWhenNoData()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.DataSeries.AverageHandleTime.Length.Should().Be.EqualTo(0);
			viewModel.LatestStatsTime.Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnDifferenceBetweenForecastedAndActualCalls()
		{
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });
			double allForecastedCalls = _firstInterval.ForecastedCalls + _secondInterval.ForecastedCalls;
			double allOfferedCalls = _firstInterval.OfferedCalls.Value + _secondInterval.OfferedCalls.Value;
			var expectedDiff = Math.Abs(allForecastedCalls - allOfferedCalls) * 100 / allForecastedCalls;

			viewModel.Summary.ForecastedActualCallsDiff.Should().Be.EqualTo(expectedDiff);
		}

		[Test]
		public void ShouldReturnPredefinedDifferenceBetweenForecastedAndActualCallsWhenNoData()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.Summary.ForecastedActualCallsDiff.Should().Be.EqualTo(-99);
		}

		[Test]
		public void ShouldReturnDifferenceBetweenForecastedAndActualHandleTime()
		{
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			double allForecastedHandleTime = _firstInterval.ForecastedHandleTime + _secondInterval.ForecastedHandleTime;
			double allHandleTime = _firstInterval.HandleTime.Value + _secondInterval.HandleTime.Value;
			var expectedDiff = Math.Abs(allForecastedHandleTime - allHandleTime) * 100 / allForecastedHandleTime;

			viewModel.Summary.ForecastedActualHandleTimeDiff.Should().Be.EqualTo(expectedDiff);
		}

		[Test]
		public void ShouldReturnPredefinedDifferenceBetweenForecastedAndActualHandleTimeWhenNoData()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.Summary.ForecastedActualHandleTimeDiff.Should().Be.EqualTo(-99);
		}
	}
}