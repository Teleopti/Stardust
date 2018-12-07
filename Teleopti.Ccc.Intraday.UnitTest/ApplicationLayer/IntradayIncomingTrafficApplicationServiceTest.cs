using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.Intraday.UnitTests.ApplicationLayer
{
	[DomainTest]
	public class IntradayIncomingTrafficApplicationServiceTest
	{
		public IntradayIncomingTrafficApplicationService Target;
		public FakeIntradayMonitorDataLoader IntradayMonitorDataLoader;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillRepository SkillRepository;
		public MutableNow Now;
		private IncomingIntervalModel _firstInterval;
		private IncomingIntervalModel _secondInterval;
		private const int minutesPerInterval = 15;

		[SetUp]
		public void Setup()
		{
			_firstInterval = new IncomingIntervalModel()
			{
				IntervalDate = new DateTime(2016, 12, 02),
				IntervalId = 32,
				ForecastedCalls = 12,
				ForecastedHandleTime = 120,
				CalculatedCalls = 12,
				HandleTime = 80,
				AnsweredCallsWithinSL = 10,
				AbandonedCalls = 2,
				AbandonedRate = 2d / 12d,
				ServiceLevel = 10d / 12d,
				SpeedOfAnswer = 20,
				AverageSpeedOfAnswer = 20d / 9d,
				AnsweredCalls = 10
			};
			_secondInterval = new IncomingIntervalModel()
			{
				IntervalDate = new DateTime(2016, 12, 02),
				IntervalId = 33,
				ForecastedCalls = 16,
				ForecastedHandleTime = 150,
				CalculatedCalls = 13,
				HandleTime = 180,
				AnsweredCallsWithinSL = 8,
				AbandonedCalls = 2,
				AbandonedRate = 2d / 12d,
				ServiceLevel = 8d / 13d,
				SpeedOfAnswer = 18,
				AverageSpeedOfAnswer = 18d / 11d,
				AnsweredCalls = 11
			};
		}

		[Test]
		public void ServiceLevelShouldNotExceed100Percent()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));
			var interval = new IncomingIntervalModel()
			{
				IntervalDate = new DateTime(2016, 12, 02),
				IntervalId = 35,
				ForecastedCalls = 16,
				ForecastedHandleTime = 150,
				CalculatedCalls = 13,
				HandleTime = 180,
				AnsweredCallsWithinSL = 15,
				AbandonedCalls = 2,
				AbandonedRate = 2d / 12d,
				ServiceLevel = 15d / 13d,
				SpeedOfAnswer = 18,
				AverageSpeedOfAnswer = 18d / 11d,
				AnsweredCalls = 11
			};

			IntradayMonitorDataLoader.AddInterval(interval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var result = Target.GenerateIncomingTrafficViewModel(new[] {Guid.NewGuid() });

			result.DataSeries.ServiceLevel[0].Should().Not.Be.GreaterThan(100).And.Be.GreaterThan(0);
			result.Summary.ServiceLevel.Should().Not.Be.GreaterThan(1).And.Be.GreaterThan(0);
		}

		[Test]
		public void ShouldSummariseUpUntilLatestUpdate()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			var thirdInterval = new IncomingIntervalModel()
			{
				IntervalDate = new DateTime(2016, 12, 02),
				IntervalId = 34,
				ForecastedCalls = 15,
				ForecastedHandleTime = 140,
				CalculatedCalls = null,
				HandleTime = null
			};

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntradayMonitorDataLoader.AddInterval(thirdInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);
			

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			var expectedForecastedCallsSum = _firstInterval.ForecastedCalls + _secondInterval.ForecastedCalls;
			var expectedForecastedHandleTimeSum = _firstInterval.ForecastedHandleTime + _secondInterval.ForecastedHandleTime;
			var expectedActualCallsSum = _firstInterval.CalculatedCalls + _secondInterval.CalculatedCalls;
			var expectedActualAnsweredCallsSum = _firstInterval.AnsweredCalls + _secondInterval.AnsweredCalls;
			var expectedActualHandleTimeSum = _firstInterval.HandleTime + _secondInterval.HandleTime;

			viewModel.Summary.ForecastedCalls.Should().Be.EqualTo(expectedForecastedCallsSum);
			viewModel.Summary.ForecastedHandleTime.Should().Be.EqualTo(expectedForecastedHandleTimeSum);
			viewModel.Summary.ForecastedAverageHandleTime.Should().Be.EqualTo(expectedForecastedHandleTimeSum / expectedForecastedCallsSum);
			viewModel.Summary.CalculatedCalls.Should().Be.EqualTo(expectedActualCallsSum);
			viewModel.Summary.HandleTime.Should().Be.EqualTo(expectedActualHandleTimeSum);
			viewModel.Summary.AverageHandleTime.Should().Be.EqualTo(expectedActualHandleTimeSum / expectedActualAnsweredCallsSum);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldReturnTimeSeries()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.DataSeries.Time.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.Time.First().Should().Be.EqualTo(_firstInterval.IntervalDate.AddMinutes(_firstInterval.IntervalId * minutesPerInterval));
			viewModel.DataSeries.Time.Second().Should().Be.EqualTo(_firstInterval.IntervalDate.AddMinutes(_secondInterval.IntervalId * minutesPerInterval));
		}

		[Test]
		public void ShouldReturnForecastedCallsSeries()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.DataSeries.ForecastedCalls.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.ForecastedCalls.First().Should().Be.EqualTo(_firstInterval.ForecastedCalls);
			viewModel.DataSeries.ForecastedCalls.Second().Should().Be.EqualTo(_secondInterval.ForecastedCalls);
		}

		[Test]
		public void ShouldReturnForecastedAverageHandleTimeSeries()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.DataSeries.ForecastedAverageHandleTime.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.ForecastedAverageHandleTime.First().Should().Be.EqualTo(_firstInterval.ForecastedAverageHandleTime);
			viewModel.DataSeries.ForecastedAverageHandleTime.Second().Should().Be.EqualTo(_secondInterval.ForecastedAverageHandleTime);
		}

		[Test]
		public void ShouldReturnOfferedCallsSeries()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.DataSeries.CalculatedCalls.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.CalculatedCalls.First().Should().Be.EqualTo(_firstInterval.CalculatedCalls);
			viewModel.DataSeries.CalculatedCalls.Second().Should().Be.EqualTo(_secondInterval.CalculatedCalls);
		}

		[Test]
		public void ShouldReturnAverageHandleTimeSeries()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.DataSeries.AverageHandleTime.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.AverageHandleTime.First().Should().Be.EqualTo(_firstInterval.AverageHandleTime);
			viewModel.DataSeries.AverageHandleTime.Second().Should().Be.EqualTo(_secondInterval.AverageHandleTime);
		}

		[Test]
		public void ShouldFillWithNullValueIfNoOfferedCalls()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			var thirdInterval = new IncomingIntervalModel()
			{
				IntervalDate = new DateTime(2016, 12, 02),
				IntervalId = 34,
				ForecastedCalls = 12,
				ForecastedHandleTime = 140,
				CalculatedCalls = 12,
				HandleTime = 200
			};
			_secondInterval.CalculatedCalls = null;
			_secondInterval.HandleTime = null;
			_secondInterval.AverageHandleTime = null;
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntradayMonitorDataLoader.AddInterval(thirdInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.DataSeries.CalculatedCalls.Length.Should().Be.EqualTo(3);
			viewModel.DataSeries.CalculatedCalls[1].Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldFillWithNullValueIfNoAverageHandleTime()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			var thirdInterval = new IncomingIntervalModel()
			{
				IntervalDate = new DateTime(2016, 12, 02),
				IntervalId = 34,
				ForecastedCalls = 12,
				ForecastedHandleTime = 140,
				CalculatedCalls = 12,
				HandleTime = 200
			};
			_secondInterval.CalculatedCalls = null;
			_secondInterval.HandleTime = null;
			_secondInterval.AverageHandleTime = 0;
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntradayMonitorDataLoader.AddInterval(thirdInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.DataSeries.AverageHandleTime.Length.Should().Be.EqualTo(3);
			viewModel.DataSeries.AverageHandleTime[1].Should().Be.EqualTo(null);
		}


		[Test]
		public void ShouldReturnLatestStatsTime()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			_secondInterval.CalculatedCalls = null;
			_secondInterval.HandleTime = null;
			_secondInterval.AverageHandleTime = null;
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.DataSeries.AverageHandleTime.Length.Should().Be.EqualTo(2);
			viewModel.LatestActualIntervalStart.Should().Be.EqualTo(_firstInterval.IntervalDate.AddMinutes(_firstInterval.IntervalId * minutesPerInterval));
			viewModel.LatestActualIntervalEnd.Should().Be.EqualTo(_firstInterval.IntervalDate.AddMinutes((_firstInterval.IntervalId + 1) * minutesPerInterval));
		}

		[Test]
		public void ShouldReturnLatestStatsTimeWhenNoData()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.DataSeries.AverageHandleTime.Length.Should().Be.EqualTo(0);
			viewModel.LatestActualIntervalStart.Should().Be.EqualTo(null);
			viewModel.LatestActualIntervalEnd.Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnDifferenceBetweenForecastedAndActualCalls()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });
			double allForecastedCalls = _firstInterval.ForecastedCalls + _secondInterval.ForecastedCalls;
			double allOfferedCalls = _firstInterval.CalculatedCalls.Value + _secondInterval.CalculatedCalls.Value;
			var expectedDiff = (allOfferedCalls - allForecastedCalls) * 100 / allForecastedCalls;

			viewModel.Summary.ForecastedActualCallsDiff.Should().Be.EqualTo(expectedDiff);
		}
		
		[Test]
		public void ShouldReturnDifferenceBetweenForecastedAndActualAverageHandleTime()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			double allForecastedCalls = _firstInterval.ForecastedCalls + _secondInterval.ForecastedCalls;
			double allAnsweredCalls = _firstInterval.AnsweredCalls.Value + _secondInterval.AnsweredCalls.Value;
			double allForecastedHandleTime = _firstInterval.ForecastedHandleTime + _secondInterval.ForecastedHandleTime;
			double allHandleTime = _firstInterval.HandleTime.Value + _secondInterval.HandleTime.Value;
			double forecastedAverageHandleTime = allForecastedHandleTime / allForecastedCalls;
			double actualAverageHandleTime = allHandleTime / allAnsweredCalls;

			var expectedDiff = (actualAverageHandleTime - forecastedAverageHandleTime) * 100 / forecastedAverageHandleTime;

			viewModel.Summary.ForecastedActualHandleTimeDiff.Should().Be.EqualTo(expectedDiff);
		}
		
		[Test]
		public void ShouldSetAverageSpeedOfAnswer()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.DataSeries.AverageSpeedOfAnswer.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.AverageSpeedOfAnswer.First().Should().Be.EqualTo(_firstInterval.AverageSpeedOfAnswer);
			viewModel.DataSeries.AverageSpeedOfAnswer.Second().Should().Be.EqualTo(_secondInterval.AverageSpeedOfAnswer);
		}

		[Test]
		public void ShouldSetAbandonedRate()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.DataSeries.AbandonedRate.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.AbandonedRate.First().Should().Be.EqualTo(_firstInterval.AbandonedRate * 100);
			viewModel.DataSeries.AbandonedRate.Second().Should().Be.EqualTo(_secondInterval.AbandonedRate * 100);
		}

		[Test]
		public void ShouldSetServiceLevel()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.DataSeries.ServiceLevel.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.ServiceLevel.First().Should().Be.EqualTo(_firstInterval.ServiceLevel * 100);
			viewModel.DataSeries.ServiceLevel.Second().Should().Be.EqualTo(_secondInterval.ServiceLevel * 100);
		}

		[Test]
		public void ShouldSetSummaryForAverageSpeedOfAnswer()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.Summary.AverageSpeedOfAnswer.Should()
					.Be.EqualTo((_firstInterval.SpeedOfAnswer + _secondInterval.SpeedOfAnswer) / (_firstInterval.AnsweredCalls + _secondInterval.AnsweredCalls));
		}
		
		[Test]
		public void ShouldSetSummaryForServiceLevel()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.Summary.ServiceLevel.Should()
					.Be.EqualTo((_firstInterval.AnsweredCallsWithinSL + _secondInterval.AnsweredCallsWithinSL) / (_firstInterval.CalculatedCalls + _secondInterval.CalculatedCalls));
		}

		[Test]
		public void ShouldReturnPredefinedSummaryValuesWhenNoData()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.Summary.ForecastedAverageHandleTime.Should().Be.EqualTo(0);
			viewModel.Summary.AverageHandleTime.Should().Be.EqualTo(0);
			viewModel.Summary.ServiceLevel.Should().Be.EqualTo(-99);
			viewModel.Summary.AbandonRate.Should().Be.EqualTo(-99);
			viewModel.Summary.ForecastedActualCallsDiff.Should().Be.EqualTo(-99);
			viewModel.Summary.ForecastedActualHandleTimeDiff.Should().Be.EqualTo(-99);
			viewModel.Summary.AverageSpeedOfAnswer.Should().Be.EqualTo(-99);
		}

		[Test]
		public void ShouldShowThatIncomingTrafficHasData()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.IncomingTrafficHasData.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldShowThatIncomingTrafficHasNoData()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.IncomingTrafficHasData.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldSetSummaryForAbandonRate()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.Summary.AbandonRate.Should()
					.Be.EqualTo((_firstInterval.AbandonedCalls + _secondInterval.AbandonedCalls) / (_firstInterval.CalculatedCalls + _secondInterval.CalculatedCalls));
		}

		[Test]
		public void ShouldFillAverageSpeedOfAnswerWithNullValueIfNoSpeedOfAnswer()
		{
			Now.Is(new DateTime(2016, 12, 2, 12, 0, 0, DateTimeKind.Utc));

			var thirdInterval = new IncomingIntervalModel()
			{
				IntervalDate = new DateTime(2016, 12, 02),
				IntervalId = 34,
				ForecastedCalls = 12,
				ForecastedHandleTime = 140,
				CalculatedCalls = 12,
				HandleTime = 200
			};
			_secondInterval.AbandonedCalls = null;
			_secondInterval.AnsweredCalls = null;
			_secondInterval.AnsweredCallsWithinSL = null;
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntradayMonitorDataLoader.AddInterval(thirdInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() });

			viewModel.DataSeries.AverageSpeedOfAnswer.Length.Should().Be.EqualTo(3);
			viewModel.DataSeries.AbandonedRate.Length.Should().Be.EqualTo(3);
			viewModel.DataSeries.ServiceLevel.Length.Should().Be.EqualTo(3);
			viewModel.DataSeries.AverageSpeedOfAnswer[1].Should().Be.EqualTo(null);
			viewModel.DataSeries.AbandonedRate[1].Should().Be.EqualTo(null);
			viewModel.DataSeries.ServiceLevel[1].Should().Be.EqualTo(null);
		}
		[Test]
		public void ShouldOnlyLoadDataForSupportedSkills()
		{
			var phoneSkill = SkillFactory.CreateSkill("phone", new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony), 15).WithId();
			var multisiteSkill = SkillFactory.CreateMultisiteSkill("multisite", new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.MaxSeatSkill), 15).WithId();
			var emailSkill = SkillFactory.CreateSkill("email", new SkillTypeEmail(new Description("Email"), ForecastSource.Email), 60).WithId();
			
			SkillRepository.Has(phoneSkill);
			SkillRepository.Has(multisiteSkill);
			SkillRepository.Has(emailSkill);
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			Target.GenerateIncomingTrafficViewModel(new[] { phoneSkill.Id.Value, multisiteSkill.Id.Value, emailSkill.Id.Value });

			IntradayMonitorDataLoader.Skills.Count.Should().Be.EqualTo(2);
			IntradayMonitorDataLoader.Skills.Should().Contain(phoneSkill.Id.Value);
			IntradayMonitorDataLoader.Skills.Should().Contain(multisiteSkill.Id.Value);
		}

		[Test]
		public void ShouldLoadDataForSkillTypesLikeEmail()
		{
			Now.Is(new DateTime(2016, 12, 2, 8, 20, 0, DateTimeKind.Utc));

			var backOfficeSkill = SkillFactory.CreateSkill("backOffice",
					new SkillTypeEmail(new Description("SkillTypeBackoffice"), ForecastSource.Backoffice), 60)
				.WithId();
			var projectSkill = SkillFactory.CreateSkill("projectSkill",
					new SkillTypeEmail(new Description("SkillTypeProject"), ForecastSource.Email), 60)
				.WithId();
			var faxSkill = SkillFactory.CreateSkill("faxSkill",
					new SkillTypeEmail(new Description("SkillTypeFax"), ForecastSource.Email), 60)
				.WithId();
			var timeSkill = SkillFactory.CreateSkill("timeSkill",
					new SkillTypeEmail(new Description("SkillTypeTime"), ForecastSource.Time), 60)
				.WithId();

			SkillRepository.Has(backOfficeSkill);
			SkillRepository.Has(projectSkill);
			SkillRepository.Has(faxSkill);
			SkillRepository.Has(timeSkill);
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			Target.GenerateIncomingTrafficViewModel(new[] {backOfficeSkill.Id.Value, projectSkill.Id.Value, faxSkill.Id.Value, timeSkill.Id.Value});

			IntradayMonitorDataLoader.Skills.Count.Should().Be.EqualTo(4);
			IntradayMonitorDataLoader.Skills.Should().Contain(backOfficeSkill.Id.Value);
			IntradayMonitorDataLoader.Skills.Should().Contain(projectSkill.Id.Value);
			IntradayMonitorDataLoader.Skills.Should().Contain(faxSkill.Id.Value);
			IntradayMonitorDataLoader.Skills.Should().Contain(timeSkill.Id.Value);
		}

		[Test]
		public void ShouldLoadDataForSpecifiedDate()
		{
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);
			IntradayMonitorDataLoader.ShouldCompareDate = true;

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() }, _firstInterval.IntervalDate);
			viewModel.LatestActualIntervalStart.Should().Not.Be.EqualTo(null);
			viewModel.LatestActualIntervalEnd.Should().Not.Be.EqualTo(null);

			var startDate = new DateOnly(viewModel.LatestActualIntervalStart.Value);
			var endDate = new DateOnly(viewModel.LatestActualIntervalEnd.Value);

			startDate.Should().Be.EqualTo(new DateOnly(_firstInterval.IntervalDate));
			endDate.Should().Be.EqualTo(new DateOnly(_firstInterval.IntervalDate));
		}

		[Test]
		public void ShouldNotLoadDataForInvalidSpecifiedDate()
		{
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);
			IntradayMonitorDataLoader.ShouldCompareDate = true;

			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() }, new DateTime(2017, 1, 1));
			viewModel.LatestActualIntervalStart.Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldLoadDataForSpecifiedOffset()
		{
			IntradayMonitorDataLoader.AddInterval(_firstInterval);
			IntradayMonitorDataLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);
			IntradayMonitorDataLoader.ShouldCompareDate = true;
			Now.Is(_firstInterval.IntervalDate.AddDays(-1));
			
			var viewModel = Target.GenerateIncomingTrafficViewModel(new[] { Guid.NewGuid() }, 1);
			viewModel.LatestActualIntervalStart.Should().Not.Be.EqualTo(null);
			viewModel.LatestActualIntervalEnd.Should().Not.Be.EqualTo(null);

			var startDate = new DateOnly(viewModel.LatestActualIntervalStart.Value);
			var endDate = new DateOnly(viewModel.LatestActualIntervalEnd.Value);

			startDate.Should().Be.EqualTo(new DateOnly(_firstInterval.IntervalDate));
			endDate.Should().Be.EqualTo(new DateOnly(_firstInterval.IntervalDate));
		}
	}
}