using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class QuickForecastCreatorTest
	{
		[Test]
		public void ShouldGetAverageAccuracyForAllSkills()
		{
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill1 = SkillFactory.CreateSkill("skill1");
			var skill2 = SkillFactory.CreateSkill("skill2");
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] {skill1, skill2});
			var quickForecaster = MockRepository.GenerateMock<IQuickForecaster>();
			var futurePeriod = new DateOnlyPeriod();
			var now = new Now();
			var nowDate = now.LocalDateOnly();
			var historicalPeriod = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-2)), nowDate);

			quickForecaster.Stub(x => x.ForecastForSkill(skill1, futurePeriod, historicalPeriod)).Return(2.3);
			quickForecaster.Stub(x => x.ForecastForSkill(skill2, futurePeriod, historicalPeriod)).Return(3.4);
			
			var target = new QuickForecastCreator(quickForecaster, skillRepository, now);
			var result = target.CreateForecastForAllSkills(futurePeriod);
			result.Accuracy.Should().Be.EqualTo((Math.Round(5.7/2, 1)));
		}

		[Test]
		public void ShouldMeasureForecastForAllSkills()
		{
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill1 = SkillFactory.CreateSkill("skill1");
			var skill2 = SkillFactory.CreateSkill("skill2");
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] { skill1, skill2 });
			var quickForecaster = MockRepository.GenerateMock<IQuickForecaster>();
			var futurePeriod = new DateOnlyPeriod();
			var now = new Now();
			var nowDate = now.LocalDateOnly();
			var historicalPeriod = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-2)), nowDate);

			quickForecaster.Stub(x => x.MeasureForecastForSkill(skill1, futurePeriod, historicalPeriod)).Return(new[] { new ForecastingAccuracy { Accuracy = 2.3, Id = Guid.NewGuid() }, new ForecastingAccuracy { Accuracy = 2.4, Id = Guid.NewGuid() } });
			quickForecaster.Stub(x => x.MeasureForecastForSkill(skill2, futurePeriod, historicalPeriod)).Return(new[] { new ForecastingAccuracy { Accuracy = 3.4, Id = Guid.NewGuid() } });

			var target = new QuickForecastCreator(quickForecaster, skillRepository, now);
			var result = target.MeasureForecastForAllSkills(futurePeriod);
			result[0].Accuracy.Should().Be.EqualTo(2.3);
			result[1].Accuracy.Should().Be.EqualTo(2.4);
			result[2].Accuracy.Should().Be.EqualTo(3.4);
		}

		[Test]
		public void ShouldGetAccuracyForWorkloads()
		{
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill1 = SkillFactory.CreateSkill("skill1");
			var skill2 = SkillFactory.CreateSkill("skill2");
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] { skill1, skill2 });
			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();
			var workload1 = WorkloadFactory.CreateWorkload("workload1", skill1);
			workload1.SetId(id1);
			var workload2 = WorkloadFactory.CreateWorkload("workload2", skill2);
			workload2.SetId(id2);
			var quickForecaster = MockRepository.GenerateMock<IQuickForecaster>();
			var futurePeriod = new DateOnlyPeriod();
			var now = new Now();
			var nowDate = now.LocalDateOnly();
			var historicalPeriod = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-2)), nowDate);

			quickForecaster.Stub(x => x.ForecastForWorkload(workload1, futurePeriod, historicalPeriod)).Return(2.3);
			quickForecaster.Stub(x => x.ForecastForWorkload(workload2, futurePeriod, historicalPeriod)).Return(3.4);

			var target = new QuickForecastCreator(quickForecaster, skillRepository, now);
			var result = target.CreateForecastForWorkloads( futurePeriod,new[] { id1 , id2});
			result[0].Accuracy.Should().Be.EqualTo(2.3);
			result[1].Accuracy.Should().Be.EqualTo(3.4);
		}
	}
}