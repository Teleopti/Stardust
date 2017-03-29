using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Test
{
	public class SetUpCascadingShifts
	{
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public ICurrentBusinessUnit CurrentBusinessUnit;
		public IUpdateStaffingLevelReadModel UpdateStaffingLevelReadModel;

		public string CreateDenyMessage30Min(int understaffedHour, CultureInfo culture, CultureInfo uiCulture, TimeZoneInfo timeZone, DateTime dateTime)
		{
			IEnumerable<TimePeriod> understaffedTimeperiods = new List<TimePeriod>
			{
				new TimePeriod(understaffedHour, 0, understaffedHour, 30),
				new TimePeriod(understaffedHour, 30, understaffedHour + 1, 0)
			};

			var errorMessageBuilder = new StringBuilder();
			var understaffingHoursValidationError = string.Format(uiCulture,
																  Resources.ResourceManager.GetString("InsufficientStaffingHours", uiCulture),
																  dateTime.ToString("d", culture));
			var insufficientHours = string.Join(", ", understaffedTimeperiods.Select(t => t.ToShortTimeString(culture)).Take(4));
			errorMessageBuilder.AppendLine($"{understaffingHoursValidationError}{insufficientHours}{Environment.NewLine}");
			return errorMessageBuilder.ToString();
		}


		public void SetUpMixedSkillDays(double defaultDemand, Tuple<int, double> hourDemands)
		{
			var skillDayGoldToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "GoldSkill",
				DefaultDemand = defaultDemand,
				Shrinkage = 0.2,
				HourDemand = hourDemands
			};
			var skillDaySilverToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "SilverSkill",
				DefaultDemand = defaultDemand,
				Shrinkage = 0.2,
				HourDemand = hourDemands
			};
			var skillDayBronzeToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "BronzeSkill",
				DefaultDemand = defaultDemand,
				Shrinkage = 0.2,
				HourDemand = hourDemands
			};

			Data.Apply(skillDayGoldToday);
			Data.Apply(skillDaySilverToday);
			Data.Apply(skillDayBronzeToday);

			UpdateStaffingLevelReadModel.Update(new DateTimePeriod(DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date.AddDays(2)));
		}

		public void SetUpHighDemandSkillDays()
		{
			var skillDayGoldToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "GoldSkill",
				DefaultDemand = 5,
				Shrinkage = 0.2
			};
			var skillDaySilverToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "SilverSkill",
				DefaultDemand = 5,
				Shrinkage = 0.2
			};
			var skillDayBronzeToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "BronzeSkill",
				DefaultDemand = 5,
				Shrinkage = 0.2
			};

			Data.Apply(skillDayGoldToday);
			Data.Apply(skillDaySilverToday);
			Data.Apply(skillDayBronzeToday);

			UpdateStaffingLevelReadModel.Update(new DateTimePeriod(DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date.AddDays(2)));
		}

		public void SetUpLowDemandSkillDays()
		{
			var skillDayGoldToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "GoldSkill",
				DefaultDemand = 0.1,
				Shrinkage = 0.2
			};
			var skillDaySilverToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "SilverSkill",
				DefaultDemand = 0.1,
				Shrinkage = 0.2
			};
			var skillDayBronzeToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "BronzeSkill",
				DefaultDemand = 0.1,
				Shrinkage = 0.2
			};

			Data.Apply(skillDayGoldToday);
			Data.Apply(skillDaySilverToday);
			Data.Apply(skillDayBronzeToday);

			UpdateStaffingLevelReadModel.Update(new DateTimePeriod(DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date.AddDays(2)));
		}

		public virtual void SetUpRelevantStuffWithCascading()
		{
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			TestState.UnitOfWork = uow;
			TestState.TestDataFactory = new TestDataFactory(new ThisUnitOfWork(uow), CurrentTenantSession, TenantUnitOfWork);

			var site = new SiteConfigurable { BusinessUnit = TestState.BusinessUnit.Name, Name = "Västerhaninge" };
			var team = new TeamConfigurable { Name = "Yellow", Site = "Västerhaninge" };
			var contract = new ContractConfigurable { Name = "Kontrakt" };
			var contractSchedule = new ContractScheduleConfigurable { Name = "Kontraktsschema" };
			var partTimePercentage = new PartTimePercentageConfigurable { Name = "ppp" };

			var scenario = new ScenarioConfigurable
			{
				EnableReporting = true,
				Name = "Scenario",
				BusinessUnit = TestState.BusinessUnit.Name
			};

			var shiftCategory = new ShiftCategoryConfigurable
			{
				Name = "katastrof"
			};

			Data.Apply(shiftCategory);
			Data.Apply(site);
			Data.Apply(team);
			Data.Apply(contract);
			Data.Apply(contractSchedule);
			Data.Apply(partTimePercentage);
			Data.Apply(scenario);


			var activity = new ActivityConfigurable
			{
				Name = "PhoneActivity"
			};
			var activityWrong = new ActivityConfigurable
			{
				Name = "WrongActivity"
			};

			var bronzeSkill = new SkillConfigurable
			{
				Activity = activity.Name,
				Name = "BronzeSkill",
				Resolution = 30,
				CascadingIndex = 3,
				SeriousUnderstaffingThreshold = -1
			};
			var silverSkill = new SkillConfigurable
			{
				Activity = activity.Name,
				Name = "SilverSkill",
				Resolution = 30,
				CascadingIndex = 2,
				SeriousUnderstaffingThreshold = -1
			};
			var goldSkill = new SkillConfigurable
			{
				Activity = activity.Name,
				Name = "GoldSkill",
				Resolution = 30,
				CascadingIndex = 1,
				SeriousUnderstaffingThreshold = -1
			};


			Data.Apply(activity);
			Data.Apply(activityWrong);
			Data.Apply(bronzeSkill);
			Data.Apply(silverSkill);
			Data.Apply(goldSkill);

			var workloadGold = new WorkloadConfigurable
			{
				WorkloadName = "WorkLoadGold",
				Open24Hours = true,
				SkillName = goldSkill.Name
			};
			var workloadSilver = new WorkloadConfigurable
			{
				WorkloadName = "WorkLoadSilver",
				Open24Hours = true,
				SkillName = silverSkill.Name
			};
			var workloadBronze = new WorkloadConfigurable
			{
				WorkloadName = "WorkLoadBronze",
				Open24Hours = true,
				SkillName = bronzeSkill.Name
			};

			Data.Apply(workloadGold);
			Data.Apply(workloadSilver);
			Data.Apply(workloadBronze);

			var absence = new AbsenceConfigurable
			{
				Name = "Holiday",
				Requestable = true
			};

			var wfcs = new WorkflowControlSetConfigurable
			{
				Name = "wfcs",
				StaffingCheck = "intraday with shrinkage",
				AutoGrant = "yes",
				AvailableAbsence = absence.Name
			};

			Data.Apply(absence);
			Data.Apply(wfcs);


			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSetConfigurable
			{
				Name = "MultiplicatorDefinitionSet"
			};

			Data.Apply(multiplicatorDefinitionSet);

			var shiftStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0);

			var personPeriodAllSkills = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personAllSkills1 = new PersonConfigurable
			{
				Name = "PersonAllSkills1"
			};
			Data.Person(personAllSkills1.Name).Apply(personPeriodAllSkills);
			Data.Person(personAllSkills1.Name).Person.AddSkill(new PersonSkill(goldSkill.Skill, new Percent(1)), Data.Person(personAllSkills1.Name).Person.Period(new DateOnly(2017, 03, 27)));
			Data.Person(personAllSkills1.Name).Person.AddSkill(new PersonSkill(silverSkill.Skill, new Percent(1)), Data.Person(personAllSkills1.Name).Person.Period(new DateOnly(2017, 03, 27)));
			AddShift(personAllSkills1.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);
			AddShift(personAllSkills1.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);

			var personPeriodAllSkills2 = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personAllSkills2 = new PersonConfigurable
			{
				Name = "PersonAllSkills2"
			};
			Data.Person(personAllSkills2.Name).Apply(personPeriodAllSkills2);
			Data.Person(personAllSkills2.Name).Person.AddSkill(new PersonSkill(goldSkill.Skill, new Percent(1)), Data.Person(personAllSkills2.Name).Person.Period(new DateOnly(2017, 03, 27)));
			Data.Person(personAllSkills2.Name).Person.AddSkill(new PersonSkill(silverSkill.Skill, new Percent(1)), Data.Person(personAllSkills2.Name).Person.Period(new DateOnly(2017, 03, 27)));
			AddShift(personAllSkills2.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);
			AddShift(personAllSkills2.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);

			var personPeriodGold = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = goldSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personGold = new PersonConfigurable
			{
				Name = "PersonGold"
			};
			Data.Person(personGold.Name).Apply(personPeriodGold);
			AddShift(personGold.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);
			AddShift(personGold.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);

			var personPeriodSilverBronze1 = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personSilverBronze1 = new PersonConfigurable
			{
				Name = "PersonSilverBronze1"
			};
			Data.Person(personSilverBronze1.Name).Apply(personPeriodSilverBronze1);
			Data.Person(personSilverBronze1.Name).Person.AddSkill(new PersonSkill(silverSkill.Skill, new Percent(1)), Data.Person(personSilverBronze1.Name).Person.Period(new DateOnly(2017, 03, 27)));
			AddShift(personSilverBronze1.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);
			AddShift(personSilverBronze1.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);

			var personPeriodSilverBronze2 = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personSilverBronze2 = new PersonConfigurable
			{
				Name = "PersonSilverBronze2"
			};
			Data.Person(personSilverBronze2.Name).Apply(personPeriodSilverBronze2);
			Data.Person(personSilverBronze2.Name).Person.AddSkill(new PersonSkill(silverSkill.Skill, new Percent(1)), Data.Person(personSilverBronze2.Name).Person.Period(new DateOnly(2017, 03, 27)));
			AddShift(personSilverBronze2.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);
			AddShift(personSilverBronze2.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);

			var personPeriodBronze1 = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personBronze1 = new PersonConfigurable
			{
				Name = "PersonBronze1"
			};
			Data.Person(personBronze1.Name).Apply(personPeriodBronze1);
			AddShift(personBronze1.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);
			AddShift(personBronze1.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activity.Activity,  scenario.Scenario);

			var personPeriodBronze2 = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personBronze2 = new PersonConfigurable
			{
				Name = "PersonBronze2"
			};
			Data.Person(personBronze2.Name).Apply(personPeriodBronze2);
			AddShift(personBronze2.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);
			AddShift(personBronze2.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activity.Activity,  scenario.Scenario);


			var personPeriodAllSkillsNoShift = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personAllSkillsNoShift = new PersonConfigurable
			{
				Name = "PersonAllSkillsNoShift"
			};
			Data.Person(personAllSkillsNoShift.Name).Apply(personPeriodAllSkillsNoShift);
			Data.Person(personAllSkillsNoShift.Name).Person.AddSkill(new PersonSkill(goldSkill.Skill, new Percent(1)), Data.Person(personAllSkillsNoShift.Name).Person.Period(new DateOnly(2017, 03, 27)));
			Data.Person(personAllSkillsNoShift.Name).Person.AddSkill(new PersonSkill(silverSkill.Skill, new Percent(1)), Data.Person(personAllSkillsNoShift.Name).Person.Period(new DateOnly(2017, 03, 27)));

			var personPeriodBronze3 = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personBronzeNoShift = new PersonConfigurable
			{
				Name = "PersonBronzeNoShift"
			};
			Data.Person(personBronzeNoShift.Name).Apply(personPeriodBronze3);

			var personPeriodGoldNoShift = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = goldSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personGoldNoShift = new PersonConfigurable
			{
				Name = "PersonGoldNoShift"
			};
			Data.Person(personGoldNoShift.Name).Apply(personPeriodGoldNoShift);


			var personPeriodAllSkillsWrongActivity = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personAllSkillsWrongActivity = new PersonConfigurable
			{
				Name = "PersonAllSkillsWrongAct"
			};
			Data.Person(personAllSkillsWrongActivity.Name).Apply(personPeriodAllSkillsWrongActivity);
			Data.Person(personAllSkillsWrongActivity.Name).Person.AddSkill(new PersonSkill(goldSkill.Skill, new Percent(1)), Data.Person(personAllSkillsWrongActivity.Name).Person.Period(new DateOnly(2017, 03, 27)));
			Data.Person(personAllSkillsWrongActivity.Name).Person.AddSkill(new PersonSkill(silverSkill.Skill, new Percent(1)), Data.Person(personAllSkillsWrongActivity.Name).Person.Period(new DateOnly(2017, 03, 27)));
			AddShift(personAllSkillsWrongActivity.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activityWrong.Activity, scenario.Scenario);
			AddShift(personAllSkillsWrongActivity.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activityWrong.Activity, scenario.Scenario);

			var personPeriodBronzeWrongActivity = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personBronzeWrongActivity = new PersonConfigurable
			{
				Name = "PersonBronzeWrongActivity"
			};
			Data.Person(personBronzeWrongActivity.Name).Apply(personPeriodBronzeWrongActivity);
			AddShift(personBronzeWrongActivity.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activityWrong.Activity,  scenario.Scenario);
			AddShift(personBronzeWrongActivity.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activityWrong.Activity, scenario.Scenario);

			var personPeriodGoldWrongActivity = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = goldSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personGoldWrongActivity = new PersonConfigurable
			{
				Name = "PersonGoldWrongActivity"
			};
			Data.Person(personGoldWrongActivity.Name).Apply(personPeriodGoldWrongActivity);
			AddShift(personGoldWrongActivity.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activityWrong.Activity, scenario.Scenario);
			AddShift(personGoldWrongActivity.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activityWrong.Activity, scenario.Scenario);

			var personPeriodBronzeOvertimeActivity = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personBronzeOvertime = new PersonConfigurable
			{
				Name = "PersonBronzeOvertime"
			};
			Data.Person(personBronzeOvertime.Name).Apply(personPeriodBronzeOvertimeActivity);
			AddShift(personBronzeOvertime.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario, multiplicatorDefinitionSet.MultiplicatorDefinitionSet, true);
			AddShift(personBronzeOvertime.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario, multiplicatorDefinitionSet.MultiplicatorDefinitionSet, true);

			var personPeriodGoldOvertimeActivity = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = goldSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personGoldOvertime = new PersonConfigurable
			{
				Name = "PersonGoldOvertime"
			};
			Data.Person(personGoldOvertime.Name).Apply(personPeriodGoldOvertimeActivity);
			AddShift(personGoldOvertime.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario, multiplicatorDefinitionSet.MultiplicatorDefinitionSet, true);
			AddShift(personGoldOvertime.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario, multiplicatorDefinitionSet.MultiplicatorDefinitionSet, true);

			var personPeriodAllSkillsOvertimeActivity = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = goldSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personAllSkillsOvertime = new PersonConfigurable
			{
				Name = "PersonAllSkillsOvertime"
			};
			Data.Person(personAllSkillsOvertime.Name).Apply(personPeriodAllSkillsOvertimeActivity);
			Data.Person(personAllSkillsOvertime.Name).Person.AddSkill(new PersonSkill(goldSkill.Skill, new Percent(1)), Data.Person(personAllSkillsOvertime.Name).Person.Period(new DateOnly(2017, 03, 27)));
			Data.Person(personAllSkillsOvertime.Name).Person.AddSkill(new PersonSkill(silverSkill.Skill, new Percent(1)), Data.Person(personAllSkillsOvertime.Name).Person.Period(new DateOnly(2017, 03, 27)));
			AddShift(personAllSkillsOvertime.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario, multiplicatorDefinitionSet.MultiplicatorDefinitionSet, true);
			AddShift(personAllSkillsOvertime.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario, multiplicatorDefinitionSet.MultiplicatorDefinitionSet, true);
		}

		public static void AddShift(string onPerson,
							DateTime dayLocal,
							int startHour,
							int lenghtHour,
							IShiftCategory shiftCategory,
							IActivity activityPhone,
							IScenario scenario,
							IMultiplicatorDefinitionSet multiplicatorDefinitionSet = null,
							bool isOverTime = false)
		{
			var shift = isOverTime ? new ShiftForDate(dayLocal, TimeSpan.FromHours(startHour), TimeSpan.FromHours(startHour + lenghtHour), scenario, shiftCategory, activityPhone, multiplicatorDefinitionSet, true) 
				: new ShiftForDate(dayLocal, TimeSpan.FromHours(startHour), TimeSpan.FromHours(startHour + lenghtHour), false, scenario, shiftCategory, activityPhone, null, null);

			Data.Person(onPerson).Apply(shift);
		}
	}
}
