﻿using System;
using System.Collections.Generic;
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
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest;
using Teleopti.Ccc.TestCommon.IoC;
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
			SetUpOverStaffedSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");

			var requestStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour + 2, 0, 0);
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddMinutes(30).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestIntradayFilter.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldBeApprovedIfOverstaffedMultipleIntervals()
		{
			Now.Is(DateTime.UtcNow);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();
			SetUpOverStaffedSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");

			var requestStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour + 2, 0, 0);
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(3).Utc()));
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
			SetUpUnderStaffedSkillDays();

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
			req.DenyReason.Should().StartWith(Resources.ResourceManager.GetString("InsufficientStaffingHours", person.PermissionInformation.Culture()).Substring(0, 10));
		}

		[Test]
		public void ShouldBeDeniedIfUnderstaffedMultipleIntervals()
		{
			Now.Is(DateTime.UtcNow);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();
			SetUpUnderStaffedSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");

			var requestStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour + 2, 0, 0);
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(3).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestIntradayFilter.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.False();
			req.DenyReason.Should().StartWith(Resources.ResourceManager.GetString("InsufficientStaffingHours", person.PermissionInformation.Culture()).Substring(0, 10));
		}

		[Test]
		public void ShouldBeDeniedIfUnderstaffedOnFirstHourAndOverstaffedOnSecond()
		{
			Now.Is(DateTime.UtcNow);
			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			SetUpRelevantStuffWithCascading();
			var requestStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour + 2, 0, 0);
			SetUpUnderStaffedAndOverStaffedSkillDays(1, new Tuple<int, double>(requestStart.Hour, 2));

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(2).Utc()));
			var personRequest = new PersonRequest(person, absenceRequest);
			PersonRequestRepository.Add(personRequest);
			uow.PersistAll();
			AbsenceRequestIntradayFilter.Process(personRequest);
			var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			req.IsApproved.Should().Be.False();
			req.DenyReason.Should().Be.EqualTo(CreateDenyMessage30Min(requestStart.Hour, person.PermissionInformation.Culture(), person.PermissionInformation.Culture(), TimeZoneInfo.Utc, requestStart.Date));
		}
	}
}