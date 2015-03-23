using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy
{
	public class QuickForecastEvaluatorTest
	{
		[Test]
		public void ShouldMeasureForecastForAllSkills()
		{
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill1 = SkillFactory.CreateSkillWithWorkloadAndSources();
			var skill2 = SkillFactory.CreateSkillWithWorkloadAndSources();
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] { skill1, skill2 });

			var quickForecastSkillEvaluator = MockRepository.GenerateMock<IQuickForecastSkillEvaluator>();

			var now = new Now();
			var nowDate = now.LocalDateOnly();
			var historicalPeriod = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-2)), nowDate);

			var workload1 = Guid.NewGuid();
			var workload2 = Guid.NewGuid();
			var workload3 = Guid.NewGuid();
			quickForecastSkillEvaluator.Stub(x => x.Measure(skill1, historicalPeriod)).Return(new[] { new ForecastingAccuracy { Accuracy = 2.3, WorkloadId = workload1 }, new ForecastingAccuracy { Accuracy = 2.4, WorkloadId = workload2 } });
			quickForecastSkillEvaluator.Stub(x => x.Measure(skill2, historicalPeriod)).Return(new[] { new ForecastingAccuracy { Accuracy = 3.4, WorkloadId = workload3 } });


			var target = new QuickForecastEvaluator(quickForecastSkillEvaluator, skillRepository, now);
			var result = target.MeasureForecastForAllSkills();
			result[0].Accuracy.Should().Be.EqualTo(2.3);
			result[0].WorkloadId.Should().Be.EqualTo(workload1);
			result[1].Accuracy.Should().Be.EqualTo(2.4);
			result[1].WorkloadId.Should().Be.EqualTo(workload2);
			result[2].Accuracy.Should().Be.EqualTo(3.4);
			result[2].WorkloadId.Should().Be.EqualTo(workload3);
		}
	}
}