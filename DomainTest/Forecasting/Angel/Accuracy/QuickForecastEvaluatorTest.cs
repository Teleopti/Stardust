using System;
using System.Linq;
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
			skill1.SetId(Guid.NewGuid());
			var skill2 = SkillFactory.CreateSkillWithWorkloadAndSources();
			skill2.SetId(Guid.NewGuid());
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] { skill1, skill2 });

			var quickForecastSkillEvaluator = MockRepository.GenerateMock<IQuickForecastSkillEvaluator>();

			var now = new Now();
			var nowDate = now.LocalDateOnly();
			var historicalPeriod = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-2)), nowDate);

			var workload1 = Guid.NewGuid();
			var workload2 = Guid.NewGuid();
			var workload3 = Guid.NewGuid();
			quickForecastSkillEvaluator.Stub(x => x.Measure(skill1, historicalPeriod)).Return(new SkillAccuracy()
			{
				Id = skill1.Id.Value,
				Name = skill1.Name,
				Workloads = new []{new WorkloadAccuracy
				{
					Id = workload1,
					Name = "work1",
					Accuracies = new []{new MethodAccuracy { Number = 2.3 }}
				}, new WorkloadAccuracy
				{
					Id = workload2,
					Name = "work2",
					Accuracies = new []{new MethodAccuracy { Number = 2.4 }}
				}}
			});
			quickForecastSkillEvaluator.Stub(x => x.Measure(skill2, historicalPeriod)).Return(new SkillAccuracy()
			{
				Id = skill2.Id.Value,
				Name = skill2.Name,
				Workloads = new[]{new WorkloadAccuracy
				{
					Id = workload3,
					Name = "work3",
					Accuracies = new []{new MethodAccuracy { Number = 3.4 }}
				}}
			});
			

			var target = new QuickForecastEvaluator(quickForecastSkillEvaluator, skillRepository, now);
			var result = target.MeasureForecastForAllSkills();
			result.First().Id.Should().Be.EqualTo(skill1.Id.Value);
			result.First().Name.Should().Be.EqualTo(skill1.Name);
			result.First().Workloads.First().Id.Should().Be.EqualTo(workload1);
			result.First().Workloads.First().Name.Should().Be.EqualTo("work1");
			result.First().Workloads.First().Accuracies.Single().Number.Should().Be.EqualTo(2.3);
			result.First().Workloads.Last().Id.Should().Be.EqualTo(workload2);
			result.First().Workloads.Last().Name.Should().Be.EqualTo("work2");
			result.First().Workloads.Last().Accuracies.Single().Number.Should().Be.EqualTo(2.4);
			result.Last().Workloads.First().Id.Should().Be.EqualTo(workload3);
			result.Last().Workloads.First().Name.Should().Be.EqualTo("work3");
			result.Last().Workloads.First().Accuracies.Single().Number.Should().Be.EqualTo(3.4);
		}
	}
}