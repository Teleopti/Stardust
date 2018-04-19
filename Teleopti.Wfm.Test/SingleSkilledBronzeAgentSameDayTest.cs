﻿using System;
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
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Test
{
	[UnitOfWorkTest]
	public class SingleSkilledBronzeAgentSameDayTest : SetUpCascadingShifts
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


		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.BeginTest();
		}


		[Test]
		public void ShouldBeApprovedIfOverstaffedSingleInterval()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();
			SetUpLowDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");
			
			var requestStart = now.AddHours(2);
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddMinutes(30)));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldBeApprovedIfOverstaffedMultipleIntervals()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();
			SetUpLowDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");
			
			var requestStart = now.AddHours(2);
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(3)));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test, Ignore("to investigate")]
		public void ShouldBeDeniedIfUnderstaffedSingleInterval()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();
			SetUpHighDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");
			
			var requestStart = now.AddHours(2);
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddMinutes(30)));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.False();
			req.DenyReason.Should().StartWith(Resources.ResourceManager.GetString("InsufficientStaffingHours", person.PermissionInformation.Culture()).Substring(0, 10));
		}

		[Test]
		public void ShouldBeDeniedIfUnderstaffedMultipleIntervals()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();
			SetUpHighDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");
			
			var requestStart = now.AddHours(2);
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(3)));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.False();
			req.DenyReason.Should().StartWith(Resources.ResourceManager.GetString("InsufficientStaffingHours", person.PermissionInformation.Culture()).Substring(0, 10));
		}

		[Test, Ignore("to investigate")]
		public void ShouldBeDeniedIfUnderstaffedOnFirstHourAndOverstaffedOnSecond()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();
			var requestStart = now.AddHours(2);
			SetUpMixedSkillDays(1, new Tuple<int, double>(requestStart.Hour, 10));

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(2)));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.False();
			req.DenyReason.Should().Be.EqualTo(CreateDenyMessage30Min(requestStart.Hour, person.PermissionInformation.Culture(), person.PermissionInformation.Culture(), TimeZoneInfo.Utc, requestStart.Date));
		}

		[Test]
		public void ShouldBeApprovedIfNoShiftInRequestPeriodAndOverStaffed()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();
			var requestStart = now.AddHours(2);
			SetUpLowDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronzeNoShift");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(2)));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldBeApprovedIfNoShiftInRequestPeriodAndUnderstaffed()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();
			var requestStart = now.AddHours(2);
			SetUpHighDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronzeNoShift");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(2)));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldBeApprovedWhenActivityIsNotConnectedToPersonSkillAndOverStaffed()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();
			var requestStart = now.AddHours(2);
			SetUpLowDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronzeWrongActivity");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(2)));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldBeApprovedWhenActivityIsNotConnectedToPersonSkillAndUnderStaffed()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();
			var requestStart = now.AddHours(2);
			SetUpHighDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronzeWrongActivity");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(2)));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.False();
			req.DenyReason.Should().StartWith(Resources.ResourceManager.GetString("InsufficientStaffingHours", person.PermissionInformation.Culture()).Substring(0, 10));
		}

		[Test]
		public void ShouldBeApprovedIfOverstaffedAndActivityIsOvertime()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();
			SetUpLowDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronzeOvertime");
			
			var requestStart = now.AddHours(2);
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(3)));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldBeDeniedIfUnderstaffedAndActivityIsOvertime()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			Now.Is(now);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();
			SetUpHighDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronzeOvertime");
			
			var requestStart = now.AddHours(2);
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(3)));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestProcessor.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.False();
		}
	}
}