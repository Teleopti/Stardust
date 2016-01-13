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
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
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
			var dateOnly = new DateOnly();
			var campaignDays = new[]
			{
				new ModifiedDay
				{
					Date = dateOnly.Date
				}
			};
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill1"));
			var scenario = new Scenario("scenario1");
			var skillDays = new[] {new SkillDay()};
			var futurePeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			skillDayRepository.Stub(
				x => x.FindRange(futurePeriod, workload.Skill, scenario))
				.Return(skillDays);
			var workloadDay = WorkloadDayFactory.CreateWorkloadDayFromWorkloadTemplate(workload, dateOnly);
			workloadDay.MakeOpen24Hours();
			futureData.Stub(x => x.Fetch(workload, skillDays, futurePeriod))
				.Return(new[] {workloadDay});

			workloadDay.CampaignTasks.ValueAsPercent().Should().Be.EqualTo(0d);
			target.Persist(scenario, workload, campaignDays, 80);
			workloadDay.CampaignTasks.ValueAsPercent().Should().Be.EqualTo(80d);
		}

		[Test]
		public void ShouldNotPersistForClosedDay()
		{
			var skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			var futureData = MockRepository.GenerateMock<IFutureData>();
			var target = new CampaignPersister(skillDayRepository, futureData);
			var dateOnly = new DateOnly();
			var campaignDays = new[]
			{
				new ModifiedDay
				{
					Date = dateOnly.Date
				}
			};
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill1"));
			var scenario = new Scenario("scenario1");
			var skillDays = new[] { new SkillDay() };
			var futurePeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			skillDayRepository.Stub(
				x => x.FindRange(futurePeriod, workload.Skill, scenario))
				.Return(skillDays);
			var workloadDay = WorkloadDayFactory.CreateWorkloadDayFromWorkloadTemplate(workload, dateOnly);
			futureData.Stub(x => x.Fetch(workload, skillDays, futurePeriod))
				.Return(new[] { workloadDay });

			workloadDay.CampaignTasks.ValueAsPercent().Should().Be.EqualTo(0d);
			target.Persist(scenario, workload, campaignDays, 80);
			workloadDay.CampaignTasks.ValueAsPercent().Should().Be.EqualTo(0d);
		}

		[Test]
		public void ShouldPersistForInputDates()
		{
			var skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			var futureData = MockRepository.GenerateMock<IFutureData>();
			var target = new CampaignPersister(skillDayRepository, futureData);
			var dateOnly = new DateOnly();
			var fiveDaysLater = dateOnly.AddDays(5);
			var campaignDays = new[]
			{
				new ModifiedDay
				{
					Date = dateOnly.Date
				},

				new ModifiedDay
				{
					Date = fiveDaysLater.Date
				}
			};
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill1"));
			var scenario = new Scenario("scenario1");
			var skillDays = new[] { new SkillDay(), };
			var futurePeriod = new DateOnlyPeriod(dateOnly, fiveDaysLater);
			skillDayRepository.Stub(
				x => x.FindRange(futurePeriod, workload.Skill, scenario))
				.Return(skillDays);
			var workloadDay = WorkloadDayFactory.CreateWorkloadDayFromWorkloadTemplate(workload, dateOnly);
			var workloadDayOneDayLater = WorkloadDayFactory.CreateWorkloadDayFromWorkloadTemplate(workload, dateOnly.AddDays(1));
			var workloadDayFiveDaysLater = WorkloadDayFactory.CreateWorkloadDayFromWorkloadTemplate(workload, fiveDaysLater);
			workloadDay.MakeOpen24Hours();
			workloadDayFiveDaysLater.MakeOpen24Hours();
			futureData.Stub(x => x.Fetch(workload, skillDays, futurePeriod)).Return(new[] {workloadDay, workloadDayOneDayLater, workloadDayFiveDaysLater});

			workloadDay.CampaignTasks.ValueAsPercent().Should().Be.EqualTo(0d);
			workloadDayOneDayLater.CampaignTasks.ValueAsPercent().Should().Be.EqualTo(0d);
			workloadDayFiveDaysLater.CampaignTasks.ValueAsPercent().Should().Be.EqualTo(0d);
			target.Persist(scenario, workload, campaignDays, 80);
			workloadDay.CampaignTasks.ValueAsPercent().Should().Be.EqualTo(80d);
			workloadDayOneDayLater.CampaignTasks.ValueAsPercent().Should().Be.EqualTo(0d);
			workloadDayFiveDaysLater.CampaignTasks.ValueAsPercent().Should().Be.EqualTo(80d);
		}
	}
}