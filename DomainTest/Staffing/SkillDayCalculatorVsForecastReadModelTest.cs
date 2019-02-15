using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	public class SkillDayCalculatorVsForecastReadModelTest
	{
		private SkillDayCalculator _target;
		private ISkill _skill;
		private IList<ISkillDay> _skillDays;
		private DateOnlyPeriod _visiblePeriod;

		public SkillForecastIntervalCalculator SkillForecastIntervalCalculator;
		public ResourceCalculationUsingReadModels ResourceCalculationUsingReadModels;
		public FakeSkillRepository SkillRepository;

		private DateOnly _startDate;
		private DateTime _startDateUtc;
		[SetUp]
		public void Setup()
		{
			_startDate = new DateOnly(2019,02,9);
			_startDateUtc = new DateTime(_startDate.Date.Ticks, DateTimeKind.Utc);
			_skill = SkillFactory.CreateSkill("E-Mail", SkillTypeFactory.CreateSkillTypeEmail(), 60, TimeZoneInfo.Utc, TimeSpan.Zero);
			SkillRepository.Add(_skill);
			
			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromHours(24));
			
			var workload = WorkloadFactory.CreateWorkloadWithOpenHoursOnDays(_skill, new Dictionary<DayOfWeek, TimePeriod>
			{
				{DayOfWeek.Monday, skillOpenPeriod},
				{DayOfWeek.Tuesday, skillOpenPeriod},
				{DayOfWeek.Wednesday, skillOpenPeriod},
				{DayOfWeek.Thursday, skillOpenPeriod},
				{DayOfWeek.Friday, skillOpenPeriod},
				{DayOfWeek.Saturday, skillOpenPeriod}
			});
			
			var agreement = new ServiceAgreement(
				new ServiceLevel(new Percent(0.8), TimeSpan.FromHours(4).TotalSeconds), new Percent(0.0), new Percent(0.0));
			
			_skillDays = new List<ISkillDay>
			{
				SkillDayFactory.CreateSkillDay(_skill, workload, _startDate,
					ScenarioFactory.CreateScenario("default", true, true), true,agreement, 
					new DateTimePeriod(_startDateUtc, _startDateUtc.AddDays(1))),
				
				SkillDayFactory.CreateSkillDay(_skill, workload, _startDate.AddDays(1),
					ScenarioFactory.CreateScenario("default", true, true), false,agreement,
					new DateTimePeriod(_startDateUtc.AddDays(1), _startDateUtc.AddDays(2))),
				
				SkillDayFactory.CreateSkillDay(_skill, workload, _startDate.AddDays(2),
					ScenarioFactory.CreateScenario("default", true, true), true,agreement,
					new DateTimePeriod(_startDateUtc.AddDays(2), _startDateUtc.AddDays(3))),
				
				SkillDayFactory.CreateSkillDay(_skill, workload, _startDate.AddDays(3),
				ScenarioFactory.CreateScenario("default", true, true), true,agreement,
				new DateTimePeriod(_startDateUtc.AddDays(3), _startDateUtc.AddDays(4)))
			};
			
			_visiblePeriod = new DateOnlyPeriod(_skillDays[0].CurrentDate, _skillDays[3].CurrentDate);
			
			_target = new SkillDayCalculator(_skill, _skillDays, _visiblePeriod);

			foreach (var skillDay in _skillDays)
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

			var skills = new[] {_skill};
			SkillForecastIntervalCalculator.Calculate(_skillDays, skills, _visiblePeriod);
		}

		[Test]
		public void SkillDayCalculatorTest()
		{
			var skills = new[] {_skill};
			var intervals = ResourceCalculationUsingReadModels.LoadAndResourceCalculate(
				skills.Select(s => s.Id.GetValueOrDefault()), _startDateUtc, _startDateUtc.AddDays(1), false,
				new UtcTimeZone());
			
			var firstDayPeriods = _skillDays[0].SkillStaffPeriodCollection;
			intervals[0].Forecast.Should().Be(firstDayPeriods[0].FStaff);
			intervals[4].Forecast.Should().Be(firstDayPeriods[1].FStaff);
			intervals[8].Forecast.Should().Be(firstDayPeriods[2].FStaff);
			intervals[12].Forecast.Should().Be(firstDayPeriods[3].FStaff);
		}
	}
}
