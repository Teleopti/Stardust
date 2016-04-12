using System;
using System.Collections.ObjectModel;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class MonitorSkillAreaProviderTest
	{
		public MonitorSkillAreaProvider Target;
		public FakeSkillAreaRepository SkillAreaRepository;
		public FakeIntradayMonitorDataLoader IntradayMonitorDataLoader;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;

		private IncomingIntervalModel firstInterval;
		private IncomingIntervalModel secondInterval;
		private SkillArea _existingSkillArea;
		

		[SetUp]
		public void Setup()
		{
			var skills = new Collection<SkillInIntraday>
				{
					new SkillInIntraday
					{
						Id = Guid.NewGuid()
					}
				};
			
			_existingSkillArea = new SkillArea
			{
				Skills = skills
			};
			_existingSkillArea.WithId();

			firstInterval = new IncomingIntervalModel()
			{
				IntervalId = 32,
				ForecastedCalls = 10,
				ForecastedHandleTime = 120,
				OfferedCalls = 12,
				HandleTime = 200
			};
			secondInterval = new IncomingIntervalModel()
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
			IntradayMonitorDataLoader.AddInterval(firstInterval);
			IntradayMonitorDataLoader.AddInterval(secondInterval);
			SkillAreaRepository.Has(_existingSkillArea);
			IntervalLengthFetcher.Has(15);

			var viewModel = Target.Load(_existingSkillArea.Id.Value);

			viewModel.Summary.ForecastedCalls.Should().Be.EqualTo(25);
			viewModel.Summary.ForecastedHandleTime.Should().Be.EqualTo(270);
			viewModel.Summary.ForecastedAverageHandleTime.Should().Be.EqualTo(270d / 25d);
			viewModel.Summary.OfferedCalls.Should().Be.EqualTo(28);
			viewModel.Summary.HandleTime.Should().Be.EqualTo(380);
			viewModel.Summary.AverageHandleTime.Should().Be.EqualTo(380d / 28d);

		}

		[Test]
		public void ShouldReturnTimeSeries()
		{
			IntradayMonitorDataLoader.AddInterval(firstInterval);
			IntradayMonitorDataLoader.AddInterval(secondInterval);
			SkillAreaRepository.Has(_existingSkillArea);
			IntervalLengthFetcher.Has(15);

			var viewModel = Target.Load(_existingSkillArea.Id.Value);

			viewModel.DataSeries.Time.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.Time.First().Should().Be.EqualTo("08:00");
			viewModel.DataSeries.Time.Second().Should().Be.EqualTo("08:15");
		}

		[Test]
		public void ShouldReturnForecastedCallsSeries()
		{
			IntradayMonitorDataLoader.AddInterval(firstInterval);
			IntradayMonitorDataLoader.AddInterval(secondInterval);
			SkillAreaRepository.Has(_existingSkillArea);
			IntervalLengthFetcher.Has(15);

			var viewModel = Target.Load(_existingSkillArea.Id.Value);

			viewModel.DataSeries.ForecastedCalls.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.ForecastedCalls.First().Should().Be.EqualTo(firstInterval.ForecastedCalls);
			viewModel.DataSeries.ForecastedCalls.Second().Should().Be.EqualTo(secondInterval.ForecastedCalls);
		}

		[Test]
		public void ShouldReturnForecastedAverageHandleTimeSeries()
		{
			IntradayMonitorDataLoader.AddInterval(firstInterval);
			IntradayMonitorDataLoader.AddInterval(secondInterval);
			SkillAreaRepository.Has(_existingSkillArea);
			IntervalLengthFetcher.Has(15);

			var viewModel = Target.Load(_existingSkillArea.Id.Value);

			viewModel.DataSeries.ForecastedAverageHandleTime.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.ForecastedAverageHandleTime.First().Should().Be.EqualTo(firstInterval.ForecastedAverageHandleTime);
			viewModel.DataSeries.ForecastedAverageHandleTime.Second().Should().Be.EqualTo(secondInterval.ForecastedAverageHandleTime);
		}

		[Test]
		public void ShouldReturnOfferedCallsSeries()
		{
			IntradayMonitorDataLoader.AddInterval(firstInterval);
			IntradayMonitorDataLoader.AddInterval(secondInterval);
			SkillAreaRepository.Has(_existingSkillArea);
			IntervalLengthFetcher.Has(15);

			var viewModel = Target.Load(_existingSkillArea.Id.Value);

			viewModel.DataSeries.OfferedCalls.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.OfferedCalls.First().Should().Be.EqualTo(firstInterval.OfferedCalls);
			viewModel.DataSeries.OfferedCalls.Second().Should().Be.EqualTo(secondInterval.OfferedCalls);
		}

		[Test]
		public void ShouldReturnAverageHandleTimeSeries()
		{
			IntradayMonitorDataLoader.AddInterval(firstInterval);
			IntradayMonitorDataLoader.AddInterval(secondInterval);
			SkillAreaRepository.Has(_existingSkillArea);
			IntervalLengthFetcher.Has(15);

			var viewModel = Target.Load(_existingSkillArea.Id.Value);

			viewModel.DataSeries.AverageHandleTime.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.AverageHandleTime.First().Should().Be.EqualTo(firstInterval.AverageHandleTime);
			viewModel.DataSeries.AverageHandleTime.Second().Should().Be.EqualTo(secondInterval.AverageHandleTime);
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
            secondInterval.OfferedCalls = null;
            secondInterval.HandleTime = null;
            secondInterval.AverageHandleTime = null;
            IntradayMonitorDataLoader.AddInterval(firstInterval);
            IntradayMonitorDataLoader.AddInterval(secondInterval);
            IntradayMonitorDataLoader.AddInterval(thirdInterval);
            SkillAreaRepository.Has(_existingSkillArea);
            IntervalLengthFetcher.Has(15);

            var viewModel = Target.Load(_existingSkillArea.Id.Value);

            viewModel.DataSeries.AverageHandleTime.Length.Should().Be.EqualTo(3);
            viewModel.DataSeries.AverageHandleTime[1].Should().Be.EqualTo(null);
        }

        [Test]
		public void ShouldReturnLatestStatsTime()
		{	
			secondInterval.OfferedCalls = null;
			secondInterval.HandleTime = null;
			secondInterval.AverageHandleTime = null;
            IntradayMonitorDataLoader.AddInterval(firstInterval);
            IntradayMonitorDataLoader.AddInterval(secondInterval);
			SkillAreaRepository.Has(_existingSkillArea);
			IntervalLengthFetcher.Has(15);

			var viewModel = Target.Load(_existingSkillArea.Id.Value);

			viewModel.DataSeries.AverageHandleTime.Length.Should().Be.EqualTo(2);
			viewModel.LatestStatsTime.Should().Be.EqualTo("08:15");
		}
	}
}