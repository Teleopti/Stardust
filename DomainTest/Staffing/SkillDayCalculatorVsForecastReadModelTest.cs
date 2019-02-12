using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest, Ignore("For testing new readmodel")]
	public class SkillDayCalculatorVsForecastReadModelTest
	{
		private SkillDayCalculator target;
		private ISkill skill;
		private IList<ISkillDay> skillDays;
		private DateOnlyPeriod _visiblePeriod;

		public SkillForecastIntervalCalculator SkillForecastIntervalCalculator;
		public ResourceCalculationUsingReadModels ResourceCalculationUsingReadModels;
		public FakeSkillRepository SkillRepository;

		[SetUp]
		public void Setup()
		{
			var startDate = new DateOnly(2019,02,9);
			var startDateUtc = new DateTime(startDate.Date.Ticks, DateTimeKind.Utc);
			skill = SkillFactory.CreateSkill("E-Mail", SkillTypeFactory.CreateSkillTypeEmail(), 60, TimeZoneInfo.Utc, TimeSpan.Zero);
			SkillRepository.Add(skill);
			
			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromHours(24));
			
			var workload = WorkloadFactory.CreateWorkloadWithOpenHoursOnDays(skill, new Dictionary<DayOfWeek, TimePeriod>
			{
				{DayOfWeek.Monday, skillOpenPeriod},
				{DayOfWeek.Tuesday, skillOpenPeriod},
				{DayOfWeek.Wednesday, skillOpenPeriod},
				{DayOfWeek.Thursday, skillOpenPeriod},
				{DayOfWeek.Friday, skillOpenPeriod},
				{DayOfWeek.Saturday, skillOpenPeriod}
			});

//			var agreement = new ServiceAgreement(
//				new ServiceLevel(new Percent(0.8), TimeSpan.FromHours(4).TotalSeconds), new Percent(0.5), new Percent(0.7));
			
			var agreement = new ServiceAgreement(
				new ServiceLevel(new Percent(0.8), TimeSpan.FromHours(4).TotalSeconds), new Percent(0.0), new Percent(0.0));
			
			skillDays = new List<ISkillDay>
			{
				SkillDayFactory.CreateSkillDay(skill, workload, startDate,
					ScenarioFactory.CreateScenario("default", true, true), true,agreement, 
					new DateTimePeriod(startDateUtc, startDateUtc.AddDays(1))),
				
//				SkillDayFactory.CreateSkillDay(skill, workload, startDate.AddDays(1),
//					ScenarioFactory.CreateScenario("default", true, true), false,agreement,
//					new DateTimePeriod(startDateUtc.AddDays(1), startDateUtc.AddDays(2))),
//				
//				SkillDayFactory.CreateSkillDay(skill, workload, startDate.AddDays(2),
//					ScenarioFactory.CreateScenario("default", true, true), true,agreement,
//					new DateTimePeriod(startDateUtc.AddDays(2), startDateUtc.AddDays(3))),
//				
//				SkillDayFactory.CreateSkillDay(skill, workload, startDate.AddDays(3),
//				ScenarioFactory.CreateScenario("default", true, true), true,agreement,
//				new DateTimePeriod(startDateUtc.AddDays(3), startDateUtc.AddDays(4)))
			};
			
			_visiblePeriod = new DateOnlyPeriod(skillDays[0].CurrentDate, skillDays[0].CurrentDate);
			
			target = new SkillDayCalculator(skill, skillDays, _visiblePeriod);
			//skillDays[0].WorkloadDayCollection[0].TaskPeriodList[0].Task.

			foreach (var skillDay in skillDays)
			{
				foreach (var workloadDay in skillDay.WorkloadDayCollection)
				{
					
					foreach (var taskPeriod in workloadDay.TaskPeriodList)
					{
						taskPeriod.AverageTaskTime = TimeSpan.FromMinutes(10);
						taskPeriod.AverageAfterTaskTime = TimeSpan.Zero;
						taskPeriod.SetTasks(20);
					}
				}
			}

			var skills = new[] {skill};
			SkillForecastIntervalCalculator.Calculate(skillDays, skills, _visiblePeriod);

			var intervals = ResourceCalculationUsingReadModels.LoadAndResourceCalculate(
				skills.Select(s => s.Id.GetValueOrDefault()), startDateUtc, startDateUtc.AddDays(1), false,
				new UtcTimeZone());
			//ForecastReadModelHandler.Handle(new TenantHourTickEvent());

//			skillDays.ForEach(sd => target.CalculateTaskPeriods(sd,false));
//			skillDays.ForEach(sd => sd.RecalculateDailyTasks());
		}

		[Test, Ignore("For testing new readmodel")]
		public void SkillDayCalculatorTest()
		{
				//TODO: use Target.
				
		}
		
		[Test, Ignore("For testing new readmodel")]
		public void ForecastReadModelTest()
		{
			
		}
	}
}
