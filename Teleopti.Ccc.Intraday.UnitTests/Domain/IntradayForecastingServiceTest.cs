using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Intraday.UnitTests.Domain
{
	public class IntradayForecastingServiceTest
	{
		public IStaffingCalculatorServiceFacade _staffingCalculatorService;

		public IntradayForecastingServiceTest()
		{
			_staffingCalculatorService = new StaffingCalculatorServiceFacade();
		}

		[Test]
		public void CalculateEstimatedServiceLevels_ForMultisiteSkillWithTwoChilds_ReturnsEsl()
		{
			var skillDays = new List<ISkillDay>();
			var startOfPeriodUtc = new DateTime(2018, 8, 27, 8, 0, 0, DateTimeKind.Utc);
			var endOfPeriodUtc = new DateTime(2018, 8, 27, 9, 0, 0, DateTimeKind.Utc);

			var intervalFetcherMock = MockRepository.GenerateMock<IIntervalLengthFetcher>();
			intervalFetcherMock.Stub(f => f.IntervalLength).Return(15);

			var issMock = MockRepository.GenerateMock<IIntradayStaffingService>();

			var date = new DateOnly(2018, 8, 27);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			var skillTypePhone = new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony);

			var openHours = new TimePeriod(8, 0, 9, 0);
			var parentSkill = new MultisiteSkill("multi", "Multi site skill", Color.Empty, 15, skillTypePhone)
			{
				Activity = new Activity("activity_multi").WithId(),
				TimeZone = TimeZoneInfo.Utc

			}.WithId();
			this.CreateWorkload(parentSkill, new List<TimePeriod> { openHours }, false);
			var parentSkillDay = this.CreateSkillDay(parentSkill, scenario, date, openHours);
			skillDays.Add(parentSkillDay);

			var childSkill1 = new ChildSkill("Child1", "First child skill", Color.FromArgb(123), parentSkill).WithId();
			this.CreateWorkload(childSkill1, new List<TimePeriod> { openHours }, false);
			var childSkillDay1 = this.CreateSkillDay(childSkill1, scenario, date, openHours);
			skillDays.Add(childSkillDay1);

			var childSkill2 = new ChildSkill("Child2", "second child skill", Color.FromArgb(123), parentSkill).WithId();
			this.CreateWorkload(childSkill2, new List<TimePeriod> { openHours }, false);
			var childSkillDay2 = this.CreateSkillDay(childSkill2, scenario, date, openHours);
			skillDays.Add(childSkillDay2);

			var ssiProviderMock = MockRepository.GenerateMock<ISkillStaffingIntervalProvider>();
			ssiProviderMock
				.Stub(f => f.StaffingForSkills(Arg<Guid[]>.Is.Anything, Arg<DateTimePeriod>.Is.Anything,
					Arg<TimeSpan>.Is.Anything, Arg<bool>.Is.Anything))
				.Return(
					new List<SkillStaffingIntervalLightModel>
					{
						new SkillStaffingIntervalLightModel
						{
							Id = parentSkill.Id.GetValueOrDefault(),
							StartDateTime = new DateTime(2018, 8, 27, 8, 0, 0, DateTimeKind.Utc),
							EndDateTime = new DateTime(2018, 8, 27, 8, 15, 0, DateTimeKind.Utc),
							StaffingLevel = 0
						},
						new SkillStaffingIntervalLightModel
						{
							Id = childSkill1.Id.GetValueOrDefault(),
							StartDateTime = new DateTime(2018, 8, 27, 8, 0, 0, DateTimeKind.Utc),
							EndDateTime = new DateTime(2018, 8, 27, 8, 15, 0, DateTimeKind.Utc),
							StaffingLevel = 6
						},
						new SkillStaffingIntervalLightModel
						{
							Id = childSkill2.Id.GetValueOrDefault(),
							StartDateTime = new DateTime(2018, 8, 27, 8, 0, 0, DateTimeKind.Utc),
							EndDateTime = new DateTime(2018, 8, 27, 8, 15, 0, DateTimeKind.Utc),
							StaffingLevel = 6
						},
						new SkillStaffingIntervalLightModel
						{
							Id = parentSkill.Id.GetValueOrDefault(),
							StartDateTime = new DateTime(2018, 8, 27, 8, 15, 0, DateTimeKind.Utc),
							EndDateTime = new DateTime(2018, 8, 27, 8, 30, 0, DateTimeKind.Utc),
							StaffingLevel = 0
						},
						new SkillStaffingIntervalLightModel
						{
							Id = childSkill1.Id.GetValueOrDefault(),
							StartDateTime = new DateTime(2018, 8, 27, 8, 15, 0, DateTimeKind.Utc),
							EndDateTime = new DateTime(2018, 8, 27, 8, 30, 0, DateTimeKind.Utc),
							StaffingLevel = 4
						},
						new SkillStaffingIntervalLightModel
						{
							Id = childSkill2.Id.GetValueOrDefault(),
							StartDateTime = new DateTime(2018, 8, 27, 8, 15, 0, DateTimeKind.Utc),
							EndDateTime = new DateTime(2018, 8, 27, 8, 30, 0, DateTimeKind.Utc),
							StaffingLevel = 4
						},
						new SkillStaffingIntervalLightModel
						{
							Id = parentSkill.Id.GetValueOrDefault(),
							StartDateTime = new DateTime(2018, 8, 27, 8, 30, 0, DateTimeKind.Utc),
							EndDateTime = new DateTime(2018, 8, 27, 8, 45, 0, DateTimeKind.Utc),
							StaffingLevel = 0
						},
						new SkillStaffingIntervalLightModel
						{
							Id = childSkill1.Id.GetValueOrDefault(),
							StartDateTime = new DateTime(2018, 8, 27, 8, 30, 0, DateTimeKind.Utc),
							EndDateTime = new DateTime(2018, 8, 27, 8, 45, 0, DateTimeKind.Utc),
							StaffingLevel = 5
						},
						new SkillStaffingIntervalLightModel
						{
							Id = childSkill2.Id.GetValueOrDefault(),
							StartDateTime = new DateTime(2018, 8, 27, 8, 30, 0, DateTimeKind.Utc),
							EndDateTime = new DateTime(2018, 8, 27, 8, 45, 0, DateTimeKind.Utc),
							StaffingLevel = 5
						},
						new SkillStaffingIntervalLightModel
						{
							Id = parentSkill.Id.GetValueOrDefault(),
							StartDateTime = new DateTime(2018, 8, 27, 8, 45, 0, DateTimeKind.Utc),
							EndDateTime = new DateTime(2018, 8, 27, 9, 0, 0, DateTimeKind.Utc),
							StaffingLevel = 0
						},
						new SkillStaffingIntervalLightModel
						{
							Id = childSkill1.Id.GetValueOrDefault(),
							StartDateTime = new DateTime(2018, 8, 27, 8, 45, 0, DateTimeKind.Utc),
							EndDateTime = new DateTime(2018, 8, 27, 9, 0, 0, DateTimeKind.Utc),
							StaffingLevel = 7.5
						},
						new SkillStaffingIntervalLightModel
						{
							Id = childSkill2.Id.GetValueOrDefault(),
							StartDateTime = new DateTime(2018, 8, 27, 8, 45, 0, DateTimeKind.Utc),
							EndDateTime = new DateTime(2018, 8, 27, 9, 0, 0, DateTimeKind.Utc),
							StaffingLevel = 7.5
						}
					});

			var target = new IntradayForecastingService(intervalFetcherMock, _staffingCalculatorService, ssiProviderMock, issMock);

			var eslResults = target
				.CalculateEstimatedServiceLevels(skillDays, startOfPeriodUtc, endOfPeriodUtc)
				.Select(x => x.Esl)
				.ToList();

			var scheduledAgents = new List<double> { 12, 8, 10, 15 };
			var taskPeriods = parentSkillDay.WorkloadDayCollection.First().TaskPeriodList;
			var skillData = parentSkillDay.SkillDataPeriodCollection.First();

			var eslTargets = new List<double>();
			for (int i = 0; i < taskPeriods.Count(); i++)
			{
				eslTargets.Add(_staffingCalculatorService.ServiceLevelAchievedOcc(
					scheduledAgents[i],
					skillData.ServiceAgreement.ServiceLevel.Seconds,
					taskPeriods[i].Tasks,
					taskPeriods[i].TotalAverageTaskTime.TotalSeconds + taskPeriods[i].TotalAverageAfterTaskTime.TotalSeconds,
					TimeSpan.FromMinutes(15),
					skillData.ServiceAgreement.ServiceLevel.Percent.Value,
					parentSkillDay.SkillStaffPeriodCollection[i].FStaff,
					1,
					parentSkill.AbandonRate.Value));
			}

			eslResults.Count().Should().Be.EqualTo(eslTargets.Count());
			Math.Round(eslResults[0].Value, 7).Should().Be.EqualTo(Math.Round(eslTargets[0], 7));
			Math.Round(eslResults[1].Value, 7).Should().Be.EqualTo(Math.Round(eslTargets[1], 7));
			Math.Round(eslResults[2].Value, 7).Should().Be.EqualTo(Math.Round(eslTargets[2], 7));
			Math.Round(eslResults[3].Value, 7).Should().Be.EqualTo(Math.Round(eslTargets[3], 7));
		}

		private void CreateWorkload(ISkill skill, IList<TimePeriod> openHours, bool isClosedOnWeekends)
		{
			IWorkload workload = new Workload(skill);
			workload.Description = "desc from factory";
			workload.Name = "name from factory";
			if (isClosedOnWeekends)
			{
				for (var i = 0; i < workload.TemplateWeekCollection.Count; i++)
				{
					if (i == 0 || i == 6)
					{
						workload.TemplateWeekCollection[i].Close();
					}
					else
					{
						workload.TemplateWeekCollection[i].ChangeOpenHours(openHours);
					}
				}
			}
			else
			{
				workload.TemplateWeekCollection.ForEach(x => x.Value.ChangeOpenHours(openHours));
			}
		}

		private ISkillDay CreateSkillDay(ISkill skill, IScenario scenario, DateOnly date, TimePeriod openHours)
		{
			var serviceAgreement = ServiceAgreement.DefaultValues();
			var random = new Random();
			int upperLimit = 10;
			int lowerLimit = 3;

			var intervals = Enumerable
				.Range(0, (int)Math.Ceiling((decimal)(date.Date.AddDays(1) - date.Date).TotalMinutes / skill.DefaultResolution))
				.Select(offset =>
				{
					var startTime = new DateTime(date.Date.AddMinutes(offset * skill.DefaultResolution).Ticks, DateTimeKind.Utc);
					return new DateTimePeriod(startTime, startTime.AddMinutes(skill.DefaultResolution));
				});

			var opensAt = date.Date.Add(openHours.StartTime);
			var closesAt = date.Date.Add(openHours.EndTime);
			var skillData = intervals
				.Select(x => new SkillDataPeriod(serviceAgreement, new SkillPersonData(), x)
				{
					ManualAgents = ((x.StartDateTime >= opensAt && x.EndDateTime <= closesAt) ? random.Next(lowerLimit, upperLimit) : 0)
				});

			var workloadDays = new List<IWorkloadDay>();
			var workloadDay = new WorkloadDay();
			var workload = skill.WorkloadCollection.First();
			workloadDay.CreateFromTemplate(date, workload, (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, date.DayOfWeek));
			workloadDays.Add(workloadDay);
			var skillDay = new SkillDay(date, skill, scenario, workloadDays, skillData).WithId();
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new List<ISkillDay> { skillDay }, date.ToDateOnlyPeriod());

			var index = 0;
			if (!(skill is IChildSkill))
			{
				//				var workloadDay = skillDay.WorkloadDayCollection.First();
				workloadDay.Lock();
				for (TimeSpan intervalStart = openHours.StartTime;
					intervalStart < openHours.EndTime;
					intervalStart = intervalStart.Add(TimeSpan.FromMinutes(skill.DefaultResolution)))
				{
					workloadDay.TaskPeriodList[index].Tasks = random.Next(5, 50);
					workloadDay.TaskPeriodList[index].AverageTaskTime = TimeSpan.FromSeconds(120);
					workloadDay.TaskPeriodList[index].AverageAfterTaskTime = TimeSpan.FromSeconds(200);
					index++;
				}

				workloadDay.Release();
			}

			return skillDay;
		}

	}
}