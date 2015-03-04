using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class QuickForecastForAllSkillsTest
	{
		[Test]
		public void ShouldSumDifferenceForAllSkills()
		{
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill1 = SkillFactory.CreateSkill("skill1");
			var skill2 = SkillFactory.CreateSkill("skill2");
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] {skill1, skill2});
			var quickForecaster = MockRepository.GenerateMock<IQuickForecaster>();
			var futurePeriod = new DateOnlyPeriod();
			var now = new Now();
			var nowDate = now.LocalDateOnly();
			var historicalPeriod = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-1)), nowDate);

			quickForecaster.Stub(x => x.Execute(skill1, futurePeriod, historicalPeriod)).Return(2.3);
			quickForecaster.Stub(x => x.Execute(skill2, futurePeriod, historicalPeriod)).Return(3.4);
			
			var target = new QuickForecastForAllSkills(quickForecaster, skillRepository, now);
			var result = target.CreateForecast(futurePeriod);
			result.ToString().Should().Be.EqualTo((5.7/2).ToString());
		}
	}
	public class ForecastingMeasurerTest
	{
		[Test]
		public void ShouldCalculateSumOfDifferenceBetweenForecastingAndHistorical()
		{
			var date1 = new DateOnly(2015, 1, 2);
			var date2 = new DateOnly(2015, 1, 3);
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.Tasks = 8d;
			var workloadDay2 = new WorkloadDay();
			workloadDay2.Create(date2, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay2.MakeOpen24Hours();
			workloadDay2.Tasks = 8d;
			var result = new ForecastingMeasurer().Measure(new List<IForecastingTarget>
			{
				new ForecastingTarget(date1, new OpenForWork(true, true))
				{
					Tasks = 10d
				},
				new ForecastingTarget(date2, new OpenForWork(true, true))
				{
					Tasks = 8d
				}
			},
				new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>
				{
					workloadDay1,
					workloadDay2
				}, TaskOwnerPeriodType.Other).TaskOwnerDayCollection);

			result.Should().Be.EqualTo(0.125);
		}

		[Test]
		public void ShouldCalculateDifferenceBetweenForecastingAndHistorical()
		{
			var workloadDay1 = new WorkloadDay();
			var date1 = new DateOnly(2015, 1, 2);
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.Tasks = 4d;
			var result = new ForecastingMeasurer().Measure(new List<IForecastingTarget>
			{
				new ForecastingTarget(date1, new OpenForWork(true, true))
				{
					Tasks = 4d
				}
			},
				new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>
				{
					workloadDay1
				}, TaskOwnerPeriodType.Other).TaskOwnerDayCollection);

			result.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSkipCalculationWhenHistoricalIsZero()
		{
			var date1 = new DateOnly(2015, 1, 2);
			var date2 = new DateOnly(2015, 1, 3);
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.Tasks = 8d;
			var workloadDay2 = new WorkloadDay();
			workloadDay2.Create(date2, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay2.MakeOpen24Hours();
			workloadDay2.Tasks = 0d;
			var result = new ForecastingMeasurer().Measure(new List<IForecastingTarget>
			{
				new ForecastingTarget(date1, new OpenForWork(true, true))
				{
					Tasks = 10d
				},
				new ForecastingTarget(date2, new OpenForWork(true, true))
				{
					Tasks = 8d
				}
			},
				new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>
				{
					workloadDay1,
					workloadDay2
				}, TaskOwnerPeriodType.Other).TaskOwnerDayCollection);

			result.Should().Be.EqualTo(0.25);
		}

		[Test]
		public void ShouldCalculateWhenBothHistoricalAndForecastingAreZero()
		{
			var date1 = new DateOnly(2015, 1, 2);
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.Tasks = 0d;

			var result = new ForecastingMeasurer().Measure(new List<IForecastingTarget>
			{
				new ForecastingTarget(date1, new OpenForWork(true, true))
				{
					Tasks = 0d
				}
			},
				new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>
				{
					workloadDay1
				}, TaskOwnerPeriodType.Other).TaskOwnerDayCollection);

			result.Should().Be.EqualTo(0);
		}
	}
}