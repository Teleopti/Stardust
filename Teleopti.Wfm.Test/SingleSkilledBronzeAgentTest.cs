using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
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
using Teleopti.Ccc.InfrastructureTest;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Test
{
	[UnitOfWorkTest]
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_xx)]
	public class SingleSkilledBronzeAgentTest : SetUpCascadingShifts
	{
		public ICurrentUnitOfWork CurrentUnitOfWork;
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


		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.BeginTest();
		}

		[Test]
		public void ShouldBeApprovedIfOverstaffedSingleInterval()
		{  
			Now.Is(DateTime.UtcNow);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();

			var skillDayGoldToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "GoldSkill",
				Demand = 0.1,
				Shrinkage = 0.2
			};
			var skillDaySilverToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "SilverSkill",
				Demand = 0.1,
				Shrinkage = 0.2
			};
			var skillDayBronzeToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "BronzeSkill",
				Demand = 0.1,
				Shrinkage = 0.2
			};

			Data.Apply(skillDayGoldToday);
			Data.Apply(skillDaySilverToday);
			Data.Apply(skillDayBronzeToday);

			UpdateStaffingLevelReadModel.Update(new DateTimePeriod(DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date.AddDays(2)));

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");

			var requestStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour + 2, 0, 0);
			var absenceRequest = new AbsenceRequest(absence,new DateTimePeriod(requestStart.Utc(), requestStart.AddMinutes(30).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestIntradayFilter.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldBeDeniedIfUnderstaffedSingleInterval()
		{
			Now.Is(DateTime.UtcNow);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();

			var skillDayGoldToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "GoldSkill",
				Demand = 2,
				Shrinkage = 0.2
			};
			var skillDaySilverToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "SilverSkill",
				Demand = 2,
				Shrinkage = 0.2
			};
			var skillDayBronzeToday = new SkillDayConfigurable
			{
				DateOnly = new DateOnly(DateTime.UtcNow.Date),
				Scenario = "Scenario",
				Skill = "BronzeSkill",
				Demand = 2,
				Shrinkage = 0.2
			};

			Data.Apply(skillDayGoldToday);
			Data.Apply(skillDaySilverToday);
			Data.Apply(skillDayBronzeToday);

			UpdateStaffingLevelReadModel.Update(new DateTimePeriod(DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date.AddDays(2)));

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");

			var requestStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour + 2, 0, 0);
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddMinutes(30).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestIntradayFilter.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.False();
			req.DenyReason.Should().StartWith(Resources.ResourceManager.GetString("InsufficientStaffingHours", person.PermissionInformation.Culture()).Substring(0,10));
		}

		
	}
}