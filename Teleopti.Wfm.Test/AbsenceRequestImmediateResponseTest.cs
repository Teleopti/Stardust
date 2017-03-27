using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.InfrastructureTest;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Test
{
	[UnitOfWorkTest]
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_xx)]
	public class AbsenceRequestImmediateResponseTest
	{
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public ICurrentBusinessUnit CurrentBusinessUnit;
		public IBusinessUnitRepository BusinessUnitRepository;
		public IDataSourceScope DataSourceScope;
		public ImpersonateSystem ImpersonateSystem;
		public WithUnitOfWork WithUnitOfWork;
		public IUpdateStaffingLevelReadModel UpdateStaffingLevelReadModel;

		public IAbsenceRepository AbsenceRepository;
		public IPersonRepository PersonRepository;
		public IAbsenceRequestIntradayFilter AbsenceRequestIntradayFilter;
		public IPersonRequestRepository PersonRequestRepository;
		public MutableNow Now;


		[Test]
		public void Gront()
		{  //Add Shrinkage
			Now.Is(DateTime.UtcNow);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			setup();
			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "Person1");

			var requestStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour + 2, 0, 0);
			var absenceRequest = new AbsenceRequest(absence,new DateTimePeriod(requestStart.Utc(), requestStart.AddMinutes(30).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestIntradayFilter.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.False();
		}

		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.BeginTest();
		}

		private void setup()
		{
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			TestState.UnitOfWork = uow;
			TestState.TestDataFactory = new TestDataFactory(new ThisUnitOfWork(uow), CurrentTenantSession, TenantUnitOfWork);

			var site = new SiteConfigurable {BusinessUnit = TestState.BusinessUnit.Name, Name = "Västerhaninge"};
			var team = new TeamConfigurable {Name = "Yellow", Site = "Västerhaninge"};
			var contract = new ContractConfigurable {Name = "Kontrakt"};
			var contractSchedule = new ContractScheduleConfigurable {Name = "Kontraktsschema"};
			var partTimePercentage = new PartTimePercentageConfigurable {Name = "ppp"};

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
		
			var bronzeSkill = new SkillConfigurable
			{
				Activity = activity.Name,
				Name = "BronzeSkill",
				Resolution = 30,
				CascadingIndex = 3
			};
			var silverSkill = new SkillConfigurable
			{
				Activity = activity.Name,
				Name = "SilverSkill",
				Resolution = 30,
				CascadingIndex = 2
			};
			var goldSkill = new SkillConfigurable
			{
				Activity = activity.Name,
				Name = "GoldSkill",
				Resolution = 30,
				CascadingIndex = 1
			};


			Data.Apply(activity);
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
				Requestable = true			};

			var wfcs = new WorkflowControlSetConfigurable
			{
				Name = "wfcs",
				StaffingCheck = "intraday with shrinkage",
				AutoGrant = "yes",
				AvailableAbsence = absence.Name,
				//BusinessUnit = TestState.BusinessUnit.Name,
			};


			Data.Apply(absence);
			Data.Apply(wfcs);


			var personPeriodAllSkills = new PersonPeriodConfigurable
			{
				Contract = contract.Name,
				ContractSchedule = contractSchedule.Name,
				PartTimePercentage = partTimePercentage.Name,
				StartDate = new DateTime(1980,01,01),
				Team = team.Name,
				Skill = goldSkill.Name,
				WorkflowControlSet = wfcs.Name
			};


			var skillDayGoldToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = scenario.Name,
				Skill = goldSkill.Name,
				Demand = 0.5
			};
			var skillDaySilverToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = scenario.Name,
				Skill = silverSkill.Name,
				Demand = 0.5
			};
			var skillDayBronzeToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = scenario.Name,
				Skill = bronzeSkill.Name,
				Demand = 0.5
			};
			var skillDayGold3Days = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date.AddDays(3)),
				Scenario = scenario.Name,
				Skill = goldSkill.Name,
				Demand = 1
			};
			var skillDaySilver3Days = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date.AddDays(3)),
				Scenario = scenario.Name,
				Skill = silverSkill.Name,
				Demand = 1
			};
			var skillDayBronze3Days = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date.AddDays(3)),
				Scenario = scenario.Name,
				Skill = bronzeSkill.Name,
				Demand = 1
			};

			Data.Apply(skillDayGoldToday);
			Data.Apply(skillDaySilverToday);
			Data.Apply(skillDayBronzeToday);
			Data.Apply(skillDayGold3Days);
			Data.Apply(skillDaySilver3Days);
			Data.Apply(skillDayBronze3Days);

			var person1 = new PersonConfigurable
			{
				Name = "Person1"
			};
			Data.Person(person1.Name).Apply(personPeriodAllSkills);
			Data.Person(person1.Name).Person.AddSkill(new PersonSkill(silverSkill.Skill, new Percent(1)), Data.Person(person1.Name).Person.Period(new DateOnly(2017,03,27)));
			Data.Person(person1.Name).Person.AddSkill(new PersonSkill(bronzeSkill.Skill, new Percent(1)), Data.Person(person1.Name).Person.Period(new DateOnly(2017,03,27)));

			var person2 = new PersonConfigurable
			{
				Name = "Person2"
			};
			Data.Person(person2.Name).Apply(personPeriodAllSkills);
			Data.Person(person2.Name).Person.AddSkill(new PersonSkill(silverSkill.Skill, new Percent(1)), Data.Person(person2.Name).Person.Period(new DateOnly(2017, 03, 27)));
			Data.Person(person2.Name).Person.AddSkill(new PersonSkill(bronzeSkill.Skill, new Percent(1)), Data.Person(person2.Name).Person.Period(new DateOnly(2017, 03, 27)));

			var shiftStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0);
			AddShift(person1.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, activity.Activity, scenario.Scenario);
			AddShift(person1.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activity.Activity, activity.Activity, scenario.Scenario);

			AddShift(person2.Name, shiftStart, 0, 9, shiftCategory.ShiftCategory, activity.Activity, activity.Activity, scenario.Scenario);
			AddShift(person2.Name, shiftStart.AddDays(3), 0, 9, shiftCategory.ShiftCategory, activity.Activity, activity.Activity, scenario.Scenario);

			UpdateStaffingLevelReadModel.Update(new DateTimePeriod(DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date.AddDays(2)));
		}

		public static void AddShift(string onPerson,
									DateTime dayLocal,
									int startHour,
									int lenghtHour,
									IShiftCategory shiftCategory,
									IActivity activityLunch,
									IActivity activityPhone,
									IScenario scenario)
		{
			var shift = new ShiftForDate(dayLocal, TimeSpan.FromHours(startHour), TimeSpan.FromHours(startHour + lenghtHour), scenario, shiftCategory, activityPhone, activityLunch);
			Data.Person(onPerson).Apply(shift);
		}
	}
}