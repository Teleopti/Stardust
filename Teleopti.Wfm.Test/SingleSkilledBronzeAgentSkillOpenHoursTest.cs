using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Wfm.Test
{
	[UnitOfWorkTest]
	public class SingleSkilledBronzeAgentSkillOpenHoursTest : SetUpCascadingShifts
	{
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public IBusinessUnitRepository BusinessUnitRepository;
		public IDataSourceScope DataSourceScope;
		public ImpersonateSystem ImpersonateSystem;
		public WithUnitOfWork WithUnitOfWork;
		public IAbsenceRepository AbsenceRepository;
		public IPersonRepository PersonRepository;
		public IAbsenceRequestProcessor AbsenceRequestProcessor;
		public IPersonRequestRepository PersonRequestRepository;
		public UpdateStaffingLevelReadModelStartDate UpdateStaffingLevelReadModelStartDate;


		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.BeginTest();
		}

		[Test]
		public void ShouldApproveIfActivityOutsideOfSkillOpenHours()
		{
			var now = new DateTime(2017, 04, 06, 0, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var hourNow = now.Date.AddHours(now.Hour);
			var requestStart = hourNow.AddHours(6);
			SetUpRelevantStuffWithCascading();
			SetUpHighDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "OutsideOfOpenHours");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(1).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveIfActivityStartsBeforeSkillIsOpen()
		{
			
			var now = new DateTime(2017, 04, 06, 0, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var hourNow = now.Date.AddHours(now.Hour);
			var requestStart = hourNow.AddHours(8);
			SetUpRelevantStuffWithCascading();
			SetUpLowDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "BeforeOpenHoursO");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(2).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyIfActivityStartsBeforeSkillIsOpenAndUnderstaffed()
		{
			var now = new DateTime(2017, 04, 06, 0, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var hourNow = now.Date.AddHours(now.Hour);
			var requestStart = hourNow.AddHours(8);
			SetUpRelevantStuffWithCascading();
			SetUpHighDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "BeforeOpenHoursU");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(2).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.False();
			req.DenyReason.Should().Be.EqualTo(CreateDenyMessage30Min(requestStart.AddHours(1).Hour, person.PermissionInformation.Culture(), person.PermissionInformation.Culture(), TimeZoneInfo.Utc, requestStart.Date));
		}

		[Test]
		public void ShouldApproveIfActivityContinuesAfterSkillIsClosed()
		{
			var now = new DateTime(2017, 04, 06, 0, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var hourNow = now.Date.AddHours(now.Hour);
			var requestStart = hourNow.AddHours(16);
			SetUpRelevantStuffWithCascading();
			SetUpLowDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "AfterOpenHoursO");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(2).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyIfActivityContinuesAfterSkillIsClosed()
		{
			var now = new DateTime(2017, 04, 06, 0, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var hourNow = now.Date.AddHours(now.Hour);
			var requestStart = hourNow.AddHours(16);
			SetUpRelevantStuffWithCascading();
			SetUpHighDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "AfterOpenHoursU");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(2).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.False();
			req.DenyReason.Should().Be.EqualTo(CreateDenyMessage30Min(requestStart.Hour, person.PermissionInformation.Culture(), person.PermissionInformation.Culture(), TimeZoneInfo.Utc, requestStart.Date));
		}

		[Test]
		public void ShouldApproveIfOverstaffedOnAnOpenSkill()
		{
			var now = new DateTime(2017, 04, 06, 0, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var hourNow = now.Date.AddHours(now.Hour);
			var requestStart = hourNow.AddHours(13);
			SetUpRelevantStuffWithCascading();
			SetUpLowDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "multiSkillPerson");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(1).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		
	}

}