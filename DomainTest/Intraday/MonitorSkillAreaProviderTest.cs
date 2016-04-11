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

			viewModel.DataSeries.TimeSeries.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.TimeSeries.First().Should().Be.EqualTo("08:00");
			viewModel.DataSeries.TimeSeries.Second().Should().Be.EqualTo("08:15");
		}
	}
}