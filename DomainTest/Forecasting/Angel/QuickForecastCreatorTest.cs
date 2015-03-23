using System;
using NUnit.Framework;
using Rhino.Mocks;
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
		public void ShouldForecastForWorkloads()
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
			var historicalPeriod = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-1)), nowDate);

			var target = new QuickForecastCreator(quickForecaster, skillRepository, now);
			target.CreateForecastForWorkloads( futurePeriod,new[] { id1 , id2});
			quickForecaster.AssertWasCalled(x => x.ForecastWorkloadsWithinSkill(skill1, new[] { id1, id2 }, futurePeriod, historicalPeriod));
			quickForecaster.AssertWasCalled(x => x.ForecastWorkloadsWithinSkill(skill2, new[] { id1, id2 }, futurePeriod, historicalPeriod));
		}
	}
}