using System;
using System.Collections.Generic;
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


namespace Teleopti.Wfm.Test
{
	[SpecialUnitOfWork]
	public class SingleSkilledBronzeAgentSameDayShiftOpenHourCasesTest : SetUpCascadingShifts
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
		public void ShouldApproveAbsenceIfAppliedBeforeShiftStarts()
		{
			var now = new DateTime(2017, 04, 06, 0, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var hourNow = now.Date.AddHours(now.Hour);
			var requestStart = hourNow.AddHours(3);
			SetUpRelevantStuffWithCascading();
			SetUpMixedSkillDays(1, Tuple.Create(requestStart.Hour, (double)20));

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PrsnBronzeOutsideShift");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(1).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveAbsenceIfAppliedAfterShiftEnds()
		{
			var now = new DateTime(2017, 04, 06, 0, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var hourNow = now.Date.AddHours(now.Hour);
			var requestStart = hourNow.AddHours(12);
			SetUpRelevantStuffWithCascading();
			SetUpMixedSkillDays(1, Tuple.Create(requestStart.Hour, (double)20));

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PrsnBronzeOutsideShift");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(2).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveAbsenceIfAppliedBetweenShifts()
		{
			var now = new DateTime(2017, 04, 06, 0, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var requestStart = new DateTime(2017, 04, 06, 6, 0, 0, DateTimeKind.Utc);
			SetUpRelevantStuffWithCascading();
			SetUpMixedSkillDays(1, Tuple.Create(requestStart.Hour, (double)20));

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PrsnBronzeBtwnShift");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(1).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyAbsenceIfAppliedBeforeShiftAndEndsWitinShiftIsUnderstaffed()
		{
			var now = new DateTime(2017, 04, 06, 0, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var hourNow = now.Date.AddHours(now.Hour);
			var requestStart = hourNow.AddHours(4);
			SetUpRelevantStuffWithCascading();
			SetUpHighDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PAInMiddleOfShift");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(2).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			uow.Clear();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.False();
			req.DenyReason.Should().Be.EqualTo(CreateDenyMessage30Min(requestStart.AddHours(1).Hour, person.PermissionInformation.Culture(), person.PermissionInformation.Culture(), TimeZoneInfo.Utc, requestStart.Date));
		}

		[Test]
		public void ShouldApproveAbsenceIfAppliedBeforeShiftAndEndsWitinShiftIsNotUnderstaffed()
		{
			var now = new DateTime(2017, 04, 06, 0, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var hourNow = now.Date.AddHours(now.Hour);
			var requestStart = hourNow.AddHours(4);
			SetUpRelevantStuffWithCascading();
			SetUpLowDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PAInMiddleOfShift");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(2).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveIfAppliedWithinShiftAndEndsAfterShiftIfOverstaffed()
		{
			var now = new DateTime(2017, 04, 06, 0, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var hourNow = now.Date.AddHours(now.Hour);
			var requestStart = hourNow.AddHours(6);
			SetUpRelevantStuffWithCascading();
			SetUpLowDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PAWSEASO");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(4).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyIfAppliedWithinShiftAndEndsAfterShiftIfUnderstaffed()
		{
			//shift 13 to 15
			var now = new DateTime(2017, 04, 06, 0, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var hourNow = now.Date.AddHours(now.Hour);
			var requestStart = hourNow.AddHours(14);
			SetUpRelevantStuffWithCascading();
			SetUpHighDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PAWSEASU");

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
		public void ShouldApproveIfRequestSpawnsOverTwoShifts()
		{
			var now = new DateTime(2017, 04, 06, 13, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var requestStart = now.AddHours(1);
			SetUpRelevantStuffWithCascading();
			SetUpLowDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PATwoShiftSpawn");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(20).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveIfRequestSpawnsOverMidnightShiftsWhenOverstaffed()
		{
			var now = new DateTime(2017, 04, 06, 9, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var requestStart = now.AddHours(1);
			SetUpRelevantStuffWithCascading();
			SetUpLowDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "TwoMidnightO");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(8).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveIfRequestSpawnsOverMidnightShiftsWhenUnderstaffed()
		{
			var now = new DateTime(2017, 04, 06, 9, 0, 0, DateTimeKind.Utc);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var requestStart = now.AddHours(1);
			SetUpRelevantStuffWithCascading();
			SetUpSkillDaysWithDemandListWhichWontWorkWithOpenHours(20,
				new List<Tuple<int, double>>()
				{
					Tuple.Create(10, (double) 0.1),
					Tuple.Create(17, (double) 0.1)
				});

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "TwoMidnightU");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(8).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}
	}
}