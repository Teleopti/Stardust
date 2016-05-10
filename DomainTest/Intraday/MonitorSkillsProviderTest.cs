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
				ForecastedCalls = 12,
				ForecastedHandleTime = 120,
				OfferedCalls = 12,
				HandleTime = 80
			};
			_secondInterval = new IncomingIntervalModel()
			{
				IntervalId = 33,
				ForecastedCalls = 16,
				ForecastedHandleTime = 150,
				OfferedCalls = 13,
				HandleTime = 180
			};
		}

		[Test]
		public void ShouldSummariseUpUntilLatestUpdate()
		{
            var thirdInterval = new IncomingIntervalModel()
            {
                IntervalId = 34,
                ForecastedCalls = 15,
                ForecastedHandleTime = 140,
                OfferedCalls = null,
                HandleTime = null
            };

            IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntradayMonitorDataLoader.AddInterval(thirdInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

		    var expectedForecastedCallsSum = _firstInterval.ForecastedCalls + _secondInterval.ForecastedCalls;
            var expectedForecastedHandleTimeSum = _firstInterval.ForecastedHandleTime + _secondInterval.ForecastedHandleTime;
            var expectedActualCallsSum = _firstInterval.OfferedCalls+ _secondInterval.OfferedCalls;
            var expectedActualHandleTimeSum = _firstInterval.HandleTime + _secondInterval.HandleTime;

            viewModel.Summary.ForecastedCalls.Should().Be.EqualTo(expectedForecastedCallsSum);
		    viewModel.Summary.ForecastedHandleTime.Should().Be.EqualTo(expectedForecastedHandleTimeSum);
			viewModel.Summary.ForecastedAverageHandleTime.Should().Be.EqualTo(expectedForecastedHandleTimeSum / expectedForecastedCallsSum);
			viewModel.Summary.OfferedCalls.Should().Be.EqualTo(expectedActualCallsSum);
			viewModel.Summary.HandleTime.Should().Be.EqualTo(expectedActualHandleTimeSum);
			viewModel.Summary.AverageHandleTime.Should().Be.EqualTo(expectedActualHandleTimeSum / expectedActualCallsSum);
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
			viewModel.LatestActualInterval.Should().Be.EqualTo(DateTime.MinValue.AddMinutes((_firstInterval.IntervalId + 1) * minutesPerInterval));
		}

		[Test]
		public void ShouldReturnLatestStatsTimeWhenNoData()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.DataSeries.AverageHandleTime.Length.Should().Be.EqualTo(0);
			viewModel.LatestActualInterval.Should().Be.EqualTo(null);
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
			var expectedDiff =(allOfferedCalls - allForecastedCalls) * 100 / allForecastedCalls;

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
		public void ShouldReturnDifferenceBetweenForecastedAndActualAverageHandleTime()
		{
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

            double allForecastedCalls = _firstInterval.ForecastedCalls + _secondInterval.ForecastedCalls;
            double allOfferedCalls = _firstInterval.OfferedCalls.Value + _secondInterval.OfferedCalls.Value;
            double allForecastedHandleTime = _firstInterval.ForecastedHandleTime + _secondInterval.ForecastedHandleTime;
            double allHandleTime = _firstInterval.HandleTime.Value + _secondInterval.HandleTime.Value;
            double forecastedAverageHandleTime = allForecastedHandleTime / allForecastedCalls;
            double actualAverageHandleTime = allHandleTime / allOfferedCalls;
            
            var expectedDiff = (actualAverageHandleTime - forecastedAverageHandleTime) * 100 / forecastedAverageHandleTime;

            viewModel.Summary.ForecastedActualHandleTimeDiff.Should().Be.EqualTo(expectedDiff);
		}

		[Test]
		public void ShouldReturnPredefinedDifferenceBetweenForecastedAndActualHandleTimeWhenNoData()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.Summary.ForecastedActualHandleTimeDiff.Should().Be.EqualTo(-99);
		}

	    [Test]
	    public void ShouldSetForecastedAverageHandleTimeToZeroWhenForecastedCallsAreZero()
	    {
	        _firstInterval.ForecastedCalls = 0;
	        _secondInterval.ForecastedCalls = 0;

	        var viewModel = Target.Load(new[] {Guid.NewGuid()});

	        viewModel.Summary.ForecastedAverageHandleTime.Should().Be.EqualTo(0);
	    }

	    [Test]
	    public void ShouldSetAverageHandleTimeToZeroWhenOfferedCallsAreZero()
	    {
	        _firstInterval.OfferedCalls = 0;
	        _secondInterval.OfferedCalls = 0;

	        var viewModel = Target.Load(new[] {Guid.NewGuid()});

	        viewModel.Summary.AverageHandleTime.Should().Be.EqualTo(0);
	    }
	}
}