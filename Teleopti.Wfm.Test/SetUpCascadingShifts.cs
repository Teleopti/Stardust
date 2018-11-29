using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;


namespace Teleopti.Wfm.Test
{
	public class SetUpCascadingShifts : IIsolateSystem
	{
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public ICurrentBusinessUnit CurrentBusinessUnit;
		public IUpdateStaffingLevelReadModel UpdateStaffingLevelReadModel;
		public MutableNow Now;

		public string CreateDenyMessage30Min(int understaffedHour, CultureInfo culture, CultureInfo uiCulture, TimeZoneInfo timeZone, DateTime dateTime)
		{
			var detail = new UnderstaffingDetails();
			var val = new StaffingThresholdWithShrinkageValidator();
			detail.AddUnderstaffingPeriod(new DateTimePeriod(dateTime.AddHours(understaffedHour), dateTime.AddHours(understaffedHour).AddMinutes(30)));
			detail.AddUnderstaffingPeriod(new DateTimePeriod(dateTime.AddHours(understaffedHour).AddMinutes(30), dateTime.AddHours(understaffedHour).AddMinutes(60)));

			return val.GetUnderStaffingPeriodsString(detail, culture, uiCulture, timeZone);
		}

		public void SetUpMixedSkillDays(double defaultDemand, Tuple<int, double> hourDemands)
		{
			var skillDayGoldToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime().Date),
				Scenario = "Scenario",
				Skill = "GoldSkill",
				DefaultDemand = defaultDemand,
				Shrinkage = 0.2,
				HourDemand = hourDemands
			};
			var skillDaySilverToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime().Date),
				Scenario = "Scenario",
				Skill = "SilverSkill",
				DefaultDemand = defaultDemand,
				Shrinkage = 0.2,
				HourDemand = hourDemands
			};
			var skillDayBronzeToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime().Date),
				Scenario = "Scenario",
				Skill = "BronzeSkill",
				DefaultDemand = defaultDemand,
				Shrinkage = 0.2,
				HourDemand = hourDemands
			};

			Data.Apply(skillDayGoldToday);
			Data.Apply(skillDaySilverToday);
			Data.Apply(skillDayBronzeToday);

			UpdateStaffingLevelReadModel.Update(new DateTimePeriod(Now.UtcDateTime().Date.AddDays(-1), Now.UtcDateTime().Date.AddDays(2)));
		}

		//we know its not an idea name so dont complain
		public void SetUpSkillDaysWithDemandListWhichWontWorkWithOpenHours(double defaultDemand, List<Tuple<int, double>> hourDemands)
		{
			var skillDayGoldToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime().Date),
				Scenario = "Scenario",
				Skill = "GoldSkill",
				DefaultDemand = defaultDemand,
				Shrinkage = 0.2,
			};
			var skillDaySilverToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime().Date),
				Scenario = "Scenario",
				Skill = "SilverSkill",
				DefaultDemand = defaultDemand,
				Shrinkage = 0.2,
			};
			var skillDayBronzeToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime().Date),
				Scenario = "Scenario",
				Skill = "BronzeSkill",
				DefaultDemand = defaultDemand,
				Shrinkage = 0.2,
				HourDemandList = hourDemands
			};
			Data.Apply(skillDayGoldToday);
			Data.Apply(skillDaySilverToday);
			Data.Apply(skillDayBronzeToday);

			UpdateStaffingLevelReadModel.Update(new DateTimePeriod(Now.UtcDateTime().Date.AddDays(-1), Now.UtcDateTime().Date.AddDays(2)));
		}

		public void SetUpHighDemandSkillDays()
		{
			var skillDayGoldToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime().Date),
				Scenario = "Scenario",
				Skill = "GoldSkill",
				DefaultDemand = 10,
				Shrinkage = 0.2
			};
			var skillDaySilverToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime().Date),
				Scenario = "Scenario",
				Skill = "SilverSkill",
				DefaultDemand = 10,
				Shrinkage = 0.2
			};
			var skillDayBronzeToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime().Date),
				Scenario = "Scenario",
				Skill = "BronzeSkill",
				DefaultDemand = 10,
				Shrinkage = 0.2
			};

			var skillDayPrivateCustomerToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime()),
				Scenario = "Scenario",
				Skill = "privateCustomerSkill",
				DefaultDemand = 10,
				Shrinkage = 0.2,
				OpenHours = new TimePeriod(9,17)
			};

			var skillDayPrivateCustomerTomrrow = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime()).AddDays(1),
				Scenario = "Scenario",
				Skill = "privateCustomerSkill",
				DefaultDemand = 10,
				Shrinkage = 0.2,
				OpenHours = new TimePeriod(9, 17)
			};

			var skillDayBusinessCustomerToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime()),
				Scenario = "Scenario",
				Skill = "businessCustomerSkill",
				DefaultDemand = 10,
				Shrinkage = 0.2,
				OpenHours = new TimePeriod(9, 12)
			};

			Data.Apply(skillDayGoldToday);
			Data.Apply(skillDaySilverToday);
			Data.Apply(skillDayBronzeToday);
			Data.Apply(skillDayPrivateCustomerToday);
			Data.Apply(skillDayPrivateCustomerTomrrow);
			Data.Apply(skillDayBusinessCustomerToday);

			UpdateStaffingLevelReadModel.Update(new DateTimePeriod(Now.UtcDateTime().Date.AddDays(-1), Now.UtcDateTime().Date.AddDays(2)));
		}

		public void SetUpLowDemandSkillDays()
		{
			var skillDayGoldToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime().Date),
				Scenario = "Scenario",
				Skill = "GoldSkill",
				DefaultDemand = 0.1,
				Shrinkage = 0.2
			};
			var skillDaySilverToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime().Date),
				Scenario = "Scenario",
				Skill = "SilverSkill",
				DefaultDemand = 0.1,
				Shrinkage = 0.2
			};
			var skillDayBronzeToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime().Date),
				Scenario = "Scenario",
				Skill = "BronzeSkill",
				DefaultDemand = 0.1,
				Shrinkage = 0.2
			};
			var skillDayPrivateCustomerToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime().Date),
				Scenario = "Scenario",
				Skill = "privateCustomerSkill",
				DefaultDemand = 0.1,
				Shrinkage = 0.2,
				OpenHours = new TimePeriod(9, 17)
			};
			var skillDayPrivateCustomerTomrrow = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime()).AddDays(1),
				Scenario = "Scenario",
				Skill = "privateCustomerSkill",
				DefaultDemand = 10,
				Shrinkage = 0.2,
				OpenHours = new TimePeriod(9, 17)
			};
			var skillDayBusinessCustomerToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(Now.UtcDateTime().Date),
				Scenario = "Scenario",
				Skill = "businessCustomerSkill",
				DefaultDemand = 0.1,
				Shrinkage = 0.2,
				OpenHours = new TimePeriod(9, 12)
			};

			Data.Apply(skillDayGoldToday);
			Data.Apply(skillDaySilverToday);
			Data.Apply(skillDayBronzeToday);
			Data.Apply(skillDayPrivateCustomerToday);
			Data.Apply(skillDayPrivateCustomerTomrrow);
			Data.Apply(skillDayBusinessCustomerToday);

			UpdateStaffingLevelReadModel.Update(new DateTimePeriod(Now.UtcDateTime().Date.AddDays(-1), Now.UtcDateTime().Date.AddDays(2)));
		}

		public virtual void SetUpRelevantStuffWithCascading()
		{
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			TestState.UnitOfWork = uow;
			TestState.TestDataFactory = TestDataFactory.Make(uow, CurrentTenantSession, TenantUnitOfWork);
			
			var site = new SiteConfigurable { BusinessUnit = TestState.BusinessUnit.Name, Name = "Västerhaninge" };
			var team = new TeamConfigurable { Name = "Yellow", Site = "Västerhaninge" };
			var contract = new ContractConfigurable { Name = "Kontrakt" };
			var contractSchedule = new ContractScheduleConfigurable { Name = "Kontraktsschema" };
			var partTimePercentage = new PartTimePercentageConfigurable { Name = "ppp" };
			
			var shiftStart = Now.UtcDateTime().Date.AddHours(Now.UtcDateTime().Hour);

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


			var activity = new ActivitySpec
			{
				Name = "PhoneActivity"
			};
			var activityWrong = new ActivitySpec
			{
				Name = "WrongActivity"
			};
			var activityLunch = new ActivitySpec
			{
				Name = "LunchActivity"
			};
			var activityAdministration = new ActivitySpec
			{
				Name = "AdministrationActivity"
			};
			var nonSkillActivity = new ActivitySpec
			{
				Name = "NonSkillActivity",
				RequiresSkill = false
			};
			
			var bronzeSkill = new SkillConfigurable
			{
				Activity = activity.Name,
				Name = "BronzeSkill",
				Resolution = 30,
				CascadingIndex = 3,
				SeriousUnderstaffingThreshold = -1
			};
			var privateCustomerSkill = new SkillConfigurable
			{
				Activity = activity.Name,
				Name = "privateCustomerSkill",
				Resolution = 30,
				CascadingIndex = 3,
				SeriousUnderstaffingThreshold = -1
			};
			var businessCustomerSkill = new SkillConfigurable
			{
				Activity = activity.Name,
				Name = "businessCustomerSkill",
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
			Data.Apply(activityLunch);
			Data.Apply(activityAdministration);
			Data.Apply(nonSkillActivity);
			Data.Apply(bronzeSkill);
			Data.Apply(privateCustomerSkill);
			Data.Apply(businessCustomerSkill);
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
			var workloadPrivateCustomerSkill = new WorkloadConfigurable
			{
				WorkloadName = "WorkLoadBronzePrivateCustomerSkill",
				Open24Hours = false,
				SkillName = privateCustomerSkill.Name,
				OpenHourPeriod = new TimePeriod(9,17)
			};

			var workloadBusinessCustomerSkill = new WorkloadConfigurable
			{
				WorkloadName = "WorkLoadBronzePrivateCustomerSkill",
				Open24Hours = false,
				SkillName = businessCustomerSkill.Name,
				OpenHourPeriod = new TimePeriod(9,12)
			};

			Data.Apply(workloadGold);
			Data.Apply(workloadSilver);
			Data.Apply(workloadBronze);
			Data.Apply(workloadPrivateCustomerSkill);
			Data.Apply(workloadBusinessCustomerSkill);

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

			var personPeriodBronzeWithLunch = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personBronzeWithLunch = new PersonConfigurable
			{
				Name = "PersonBronzeWithLunch"
			};
			Data.Person(personBronzeWithLunch.Name).Apply(personPeriodBronzeWithLunch);
			AddShiftWithLunch(personBronzeWithLunch.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, activityLunch.Activity, scenario.Scenario);
			AddShiftWithLunch(personBronzeWithLunch.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activity.Activity, activityLunch.Activity ,scenario.Scenario);

			var personPeriodBronzeWithMeeting = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personBronzeWithMeeting = new PersonConfigurable
			{
				Name = "PersonBronzeWithMeeting"
			};
			var meeting = new MeetingConfigurable
			{
				Description = "Meeting",
				StartTime = shiftStart.AddHours(3),
				EndTime = shiftStart.AddHours(4),
				Subject = "Meeting Subject",
				Scenario = scenario.Scenario,
				Location = "Location"
			};
			Data.Person(personBronzeWithMeeting.Name).Apply(personPeriodBronzeWithMeeting);
			Data.Person(personBronzeWithMeeting.Name).Apply(meeting);
			AddShift(personBronzeWithMeeting.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);
			AddShift(personBronzeWithMeeting.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);


			var personPeriodBronzeWithAdministration = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personBronzeWithAdministration = new PersonConfigurable
			{
				Name = "PersonBronzeWithAdmin"
			};
			Data.Person(personBronzeWithAdministration.Name).Apply(personPeriodBronzeWithAdministration);
			//need to rename the method
			AddShiftWithLunch(personBronzeWithAdministration.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, activityAdministration.Activity, scenario.Scenario);

			personSetupForShiftHours(contract, contractSchedule, partTimePercentage, team, bronzeSkill, wfcs, shiftStart, shiftCategory, activity, scenario);

			personSetupForSkillOpenhours(contract, contractSchedule, partTimePercentage, team, privateCustomerSkill, wfcs, shiftStart, shiftCategory, activity, scenario, businessCustomerSkill);

			personSetupForOvertime(contract, contractSchedule, partTimePercentage, team, bronzeSkill, wfcs, shiftStart, shiftCategory, activity, scenario);

			var personPeriodBronzeWithNonSkill = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};
			var personBronzeWithNonSkill = new PersonConfigurable
			{
				Name = "PersonBronzeWithNS"
			};
			Data.Person(personBronzeWithNonSkill.Name).Apply(personPeriodBronzeWithNonSkill);
			AddShiftWithLunch(personBronzeWithNonSkill.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, nonSkillActivity.Activity, scenario.Scenario);


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

	    private void personSetupForOvertime(ContractConfigurable contract, ContractScheduleConfigurable contractSchedule, PartTimePercentageConfigurable partTimePercentage, TeamConfigurable team, SkillConfigurable bronzeSkill, WorkflowControlSetConfigurable wfcs, DateTime shiftStart, ShiftCategoryConfigurable shiftCategory, ActivitySpec activity, ScenarioConfigurable scenario)
	    {
			var personPeriodOvertime = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = bronzeSkill.Name,
				WorkflowControlSet = wfcs.Name
			};

	        var multiplicatorDefinitionSetConfigurable = new MultiplicatorDefinitionSetConfigurable()
	        {
				Name = "overtimeAgentMS"
	        };
			Data.Apply(multiplicatorDefinitionSetConfigurable);

			var personOvertime = new PersonConfigurable
			{
				Name = "overtimeAgent"
			};
			Data.Person(personOvertime.Name).Apply(personPeriodOvertime);
			AddShift(personOvertime.Name, shiftStart.Date, 9, 1, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario, multiplicatorDefinitionSetConfigurable.MultiplicatorDefinitionSet, true);
		}

	    private static void personSetupForShiftHours(ContractConfigurable contract,ContractScheduleConfigurable contractSchedule, PartTimePercentageConfigurable partTimePercentage,
	        TeamConfigurable team, SkillConfigurable bronzeSkill, WorkflowControlSetConfigurable wfcs, DateTime shiftStart,ShiftCategoryConfigurable shiftCategory, ActivitySpec activity, ScenarioConfigurable scenario)
	    {
	        var personPeriodBronzeOpenHours = new PersonPeriodConfigurable
	        {
	            Contract = contract.Name,
	            ContractSchedule = contractSchedule.Name,
	            PartTimePercentage = partTimePercentage.Name,
	            StartDate = new DateTime(1980, 01, 01),
	            Team = team.Name,
	            Skill = bronzeSkill.Name,
	            WorkflowControlSet = wfcs.Name
	        };
	        var personBronzeOpenHoursOutsideShift = new PersonConfigurable
	        {
	            Name = "PrsnBronzeOutsideShift"
	        };
	        Data.Person(personBronzeOpenHoursOutsideShift.Name).Apply(personPeriodBronzeOpenHours);
	        AddShift(personBronzeOpenHoursOutsideShift.Name, shiftStart.Date, 6, 4, shiftCategory.ShiftCategory,
	            activity.Activity, scenario.Scenario);

			var personBronzeBetweenShifts = new PersonConfigurable
			{
				Name = "PrsnBronzeBtwnShift"
			};
			Data.Person(personBronzeBetweenShifts.Name).Apply(personPeriodBronzeOpenHours);
			AddShift(personBronzeBetweenShifts.Name, shiftStart.Date.AddDays(-1), 20, 9, shiftCategory.ShiftCategory,
				activity.Activity, scenario.Scenario);
			AddShift(personBronzeBetweenShifts.Name, shiftStart.Date, 20, 22, shiftCategory.ShiftCategory, activity.Activity,
				scenario.Scenario);

			var personAbsenceInMiddleOfShift = new PersonConfigurable
			{
				Name = "PAInMiddleOfShift"
			};
			Data.Person(personAbsenceInMiddleOfShift.Name).Apply(personPeriodBronzeOpenHours);
			AddShift(personAbsenceInMiddleOfShift.Name, shiftStart.Date, 5, 9, shiftCategory.ShiftCategory, activity.Activity,
				scenario.Scenario);

			var personAbsenceWithinShiftEndsAfterShiftOverstaffed = new PersonConfigurable
			{
				Name = "PAWSEASO"
			};
			Data.Person(personAbsenceWithinShiftEndsAfterShiftOverstaffed.Name).Apply(personPeriodBronzeOpenHours);
			AddShift(personAbsenceWithinShiftEndsAfterShiftOverstaffed.Name, shiftStart.Date, 5, 4, shiftCategory.ShiftCategory, activity.Activity,
				scenario.Scenario);

			var personAbsenceWithinShiftEndsAfterShiftUndertsaffed = new PersonConfigurable
			{
				Name = "PAWSEASU"
			};
			Data.Person(personAbsenceWithinShiftEndsAfterShiftUndertsaffed.Name).Apply(personPeriodBronzeOpenHours);
			AddShift(personAbsenceWithinShiftEndsAfterShiftUndertsaffed.Name, shiftStart.Date, 13, 2, shiftCategory.ShiftCategory, activity.Activity,
				scenario.Scenario);

			var personAbsenceAcrossTwoShifts = new PersonConfigurable
			{
				Name = "PATwoShiftSpawn"
			};
			Data.Person(personAbsenceAcrossTwoShifts.Name).Apply(personPeriodBronzeOpenHours);
			AddShift(personAbsenceAcrossTwoShifts.Name, shiftStart.Date, 15, 1, shiftCategory.ShiftCategory, activity.Activity,scenario.Scenario);
			AddShift(personAbsenceAcrossTwoShifts.Name, shiftStart.AddDays(1).Date, 9, 1, shiftCategory.ShiftCategory, activity.Activity,scenario.Scenario);

			var personTwoMidnightO = new PersonConfigurable
			{
				Name = "TwoMidnightO"
			};
			Data.Person(personTwoMidnightO.Name).Apply(personPeriodBronzeOpenHours);
			AddShift(personTwoMidnightO.Name, shiftStart.Date.AddDays(-1), 23, 12, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);
			AddShift(personTwoMidnightO.Name, shiftStart.Date, 17, 20, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);

			var personTwoMidnightU = new PersonConfigurable
			{
				Name = "TwoMidnightU"
			};
			Data.Person(personTwoMidnightU.Name).Apply(personPeriodBronzeOpenHours);
			AddShift(personTwoMidnightU.Name, shiftStart.AddDays(-1).Date, 23, 12, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);
			AddShift(personTwoMidnightU.Name, shiftStart.Date, 17, 20, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);
		}

		private static void personSetupForSkillOpenhours(ContractConfigurable contract, ContractScheduleConfigurable contractSchedule, PartTimePercentageConfigurable partTimePercentage,
		  TeamConfigurable team, SkillConfigurable privateCustomerSkill, WorkflowControlSetConfigurable wfcs, DateTime shiftStart, ShiftCategoryConfigurable shiftCategory, ActivitySpec activity, ScenarioConfigurable scenario, SkillConfigurable businessCustomerSkill)
		{
			var personPeriodBronzeSkillOpenHours = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980, 01, 01),
				Team = team.Name,
				Skill = privateCustomerSkill.Name,
				WorkflowControlSet = wfcs.Name
			};

			var personBronzeOpenHoursOutsideShift = new PersonConfigurable
			{
				Name = "OutsideOfOpenHours"
			};
			Data.Person(personBronzeOpenHoursOutsideShift.Name).Apply(personPeriodBronzeSkillOpenHours);
			AddShift(personBronzeOpenHoursOutsideShift.Name, shiftStart.Date, 6, 1, shiftCategory.ShiftCategory,activity.Activity, scenario.Scenario);

			var personBronzeOpenHoursBeforeShift = new PersonConfigurable
			{
				Name = "BeforeOpenHoursO"
			};
			Data.Person(personBronzeOpenHoursBeforeShift.Name).Apply(personPeriodBronzeSkillOpenHours);
			AddShift(personBronzeOpenHoursBeforeShift.Name, shiftStart.Date, 8, 11, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);

			var personBronzeOpenHoursBeforeShiftU = new PersonConfigurable
			{
				Name = "BeforeOpenHoursU"
			};
			Data.Person(personBronzeOpenHoursBeforeShiftU.Name).Apply(personPeriodBronzeSkillOpenHours);
			AddShift(personBronzeOpenHoursBeforeShiftU.Name, shiftStart.Date, 8, 11, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);

			var personBronzeAfterOpenHoursO = new PersonConfigurable
			{
				Name = "AfterOpenHoursO"
			};
			Data.Person(personBronzeAfterOpenHoursO.Name).Apply(personPeriodBronzeSkillOpenHours);
			AddShift(personBronzeAfterOpenHoursO.Name, shiftStart.Date, 16, 2, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);

			var personBronzeAfterOpenHoursU = new PersonConfigurable
			{
				Name = "AfterOpenHoursU"
			};
			Data.Person(personBronzeAfterOpenHoursU.Name).Apply(personPeriodBronzeSkillOpenHours);
			AddShift(personBronzeAfterOpenHoursU.Name, shiftStart.Date, 16, 2, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);

			var multiSkillPerson = new PersonConfigurable
			{
				Name = "multiSkillPerson"
			};
			Data.Person(multiSkillPerson.Name).Apply(personPeriodBronzeSkillOpenHours);
			Data.Person(multiSkillPerson.Name).Person.AddSkill(new PersonSkill(businessCustomerSkill.Skill, new Percent(1)), Data.Person(multiSkillPerson.Name).Person.Period(new DateOnly(2016, 03, 27)));
			AddShift(multiSkillPerson.Name, shiftStart.Date, 13, 1, shiftCategory.ShiftCategory, activity.Activity, scenario.Scenario);


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

		public static void AddShiftWithLunch(string onPerson,
					DateTime dayLocal,
					int startHour,
					int lenghtHour,
					IShiftCategory shiftCategory,
					IActivity activityPhone,
					IActivity activityLunch,
					IScenario scenario,
					IMultiplicatorDefinitionSet multiplicatorDefinitionSet = null,
					bool isOverTime = false)
		{
			var shift = new ShiftForDate(dayLocal, TimeSpan.FromHours(startHour), TimeSpan.FromHours(startHour + lenghtHour), true, scenario, shiftCategory, activityPhone, activityLunch, null);

			Data.Person(onPerson).Apply(shift);
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
		}
	}
}
