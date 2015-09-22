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
	public class CampaignPersisterTest
	{
		[Test]
		public void ShouldPersist()
		{
			var skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			var futureData = MockRepository.GenerateMock<IFutureData>();
			var target = new CampaignPersister(skillDayRepository, futureData);
			var dateTime = new DateTime();
			var campaignDays = new[]
			{
				new CampaignDay()
				{
					Date = dateTime
				}
			};
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill1"));
			var scenario = new Scenario("scenario1");
			var skillDays = new[] {new SkillDay(),};
			var futurePeriod = new DateOnlyPeriod(new DateOnly(campaignDays[0].Date), new DateOnly(campaignDays[0].Date));
			skillDayRepository.Stub(
				x => x.FindRange(futurePeriod, workload.Skill, scenario))
				.Return(skillDays);
			var workloadDay = WorkloadDayFactory.CreateWorkloadDayFromWorkloadTemplate(workload, new DateOnly(dateTime));
			workloadDay.MakeOpen24Hours();
			futureData.Stub(x => x.Fetch(workload, skillDays, futurePeriod))
				.Return(new[] {workloadDay});

			workloadDay.CampaignTasks.ValueAsPercent().Should().Be.EqualTo(0d);
			target.Persist(scenario, workload, campaignDays, 80);
			workloadDay.CampaignTasks.ValueAsPercent().Should().Be.EqualTo(80d);
		}
	}
}