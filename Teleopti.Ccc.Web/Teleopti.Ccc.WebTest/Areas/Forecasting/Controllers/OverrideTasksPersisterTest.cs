using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	public class OverrideTasksPersisterTest
	{
		[Test]
		public void ShouldPersist()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill1"));
			var scenario = new Scenario("scenario1");
			var dateOnly = new DateOnly();
			var workloadDay = WorkloadDayFactory.CreateWorkloadDayFromWorkloadTemplate(workload, dateOnly);
			workloadDay.MakeOpen24Hours();
			workloadDay.Tasks = 100d;
			var skillDays = new[] { new SkillDay() };
			var futurePeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			skillDayRepository.Stub(
				x => x.FindRange(futurePeriod, workload.Skill, scenario))
				.Return(skillDays);
			var futureData = MockRepository.GenerateMock<IFutureData>();
			futureData.Stub(x => x.Fetch(workload, skillDays, futurePeriod))
				.Return(new[] { workloadDay });

			var target = new OverrideTasksPersister(skillDayRepository, futureData);
			workloadDay.OverrideTasks.Should().Be.EqualTo(null);
			target.Persist(scenario, workload, new[]
			{
				new ModifiedDay()
				{
					Date = dateOnly.Date
				}
			}, 300);
			Math.Round(workloadDay.OverrideTasks.Value, 2).Should().Be.EqualTo(300d);
		}
	}
}