﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
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
	[DatabaseTest]
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_xx)]
	public class SingleSkilledBronzeAgentActivityCasesBulkTest : SetUpCascadingShifts
	{
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public IBusinessUnitRepository BusinessUnitRepository;
		public IDataSourceScope DataSourceScope;
		public ImpersonateSystem ImpersonateSystem;
		public WithUnitOfWork WithUnitOfWork;


		public IAbsenceRepository AbsenceRepository;
		public IPersonRepository PersonRepository;
		public IHandleEvent<NewMultiAbsenceRequestsCreatedEvent> UpdateRequestHandler;
		public IPersonRequestRepository PersonRequestRepository;
		public MutableNow Now;


		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.BeginTest();
		}


		[Test]
		[Ignore("Amanda told me to")]
		public void ShouldBeDeniedIfUnderstaffedDuringLunch()
		{
			Now.Is(DateTime.UtcNow);
			var hourNow = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0);
			var requestStart = hourNow.AddHours(2);
			IPersonRequest personRequest;
			IPerson person;
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpMixedSkillDays(1, Tuple.Create(requestStart.AddHours(1).Hour, (double)20));
				
				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronzeWithLunch");

				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(3).Utc()));
				personRequest = new PersonRequest(person, absenceRequest);
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				uow.PersistAll();
			}
			
			UpdateRequestHandler.Handle(new NewMultiAbsenceRequestsCreatedEvent
			{
				PersonRequestIds = new List<Guid> { personRequest.Id.GetValueOrDefault()},
				InitiatorId = Guid.Empty,
				JobName = "JobName",
				LogOnBusinessUnitId = TestState.BusinessUnit.Id.GetValueOrDefault(),
				LogOnDatasource = "TestData",
				Sent = DateTime.UtcNow
			});
			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
				req.IsApproved.Should().Be.False();
				req.DenyReason.Should().Be.EqualTo(CreateDenyMessage30Min(requestStart.AddHours(1).Hour, person.PermissionInformation.Culture(), person.PermissionInformation.Culture(), TimeZoneInfo.Utc, requestStart.Date));
			}
		}

		[Test]
		[Ignore("Amanda told me to")]
		public void ShouldBeDeniedIfUnderstaffedDuringLunchShortRequest()
		{
			Now.Is(DateTime.UtcNow);
			var hourNow = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0);
			var requestStart = hourNow.AddHours(3);
			IPersonRequest personRequest;
			IPerson person;
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpMixedSkillDays(1, Tuple.Create(requestStart.Hour, (double)20));

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronzeWithLunch");

				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(1).Utc()));
				personRequest = new PersonRequest(person, absenceRequest);
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				uow.PersistAll();
			}

			UpdateRequestHandler.Handle(new NewMultiAbsenceRequestsCreatedEvent
			{
				PersonRequestIds = new List<Guid> { personRequest.Id.GetValueOrDefault() },
				InitiatorId = Guid.Empty,
				JobName = "JobName",
				LogOnBusinessUnitId = TestState.BusinessUnit.Id.GetValueOrDefault(),
				LogOnDatasource = "TestData",
				Sent = DateTime.UtcNow
			});
			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
				req.IsApproved.Should().Be.False();
				req.DenyReason.Should().Be.EqualTo(CreateDenyMessage30Min(requestStart.Hour, person.PermissionInformation.Culture(), person.PermissionInformation.Culture(), TimeZoneInfo.Utc, requestStart.Date));
			}
		}

		[Test]
		public void ShouldBeDeniedIfUnderstaffedDuringLunchAndLunchInBeginningOfRequest()
		{
			Now.Is(DateTime.UtcNow);
			var hourNow = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0).Utc();
			var requestStart = hourNow.AddHours(3);
			IPersonRequest personRequest;
			IPerson person;
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpMixedSkillDays(1, Tuple.Create(requestStart.Hour, (double)20));

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronzeWithLunch");

				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(3).Utc()));
				personRequest = new PersonRequest(person, absenceRequest);
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				uow.PersistAll();
			}

			UpdateRequestHandler.Handle(new NewMultiAbsenceRequestsCreatedEvent
			{
				PersonRequestIds = new List<Guid> { personRequest.Id.GetValueOrDefault() },
				InitiatorId = Guid.Empty,
				JobName = "JobName",
				LogOnBusinessUnitId = TestState.BusinessUnit.Id.GetValueOrDefault(),
				LogOnDatasource = "TestData",
				Sent = DateTime.UtcNow
			});
			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
				req.IsApproved.Should().Be.False();
				req.DenyReason.Should().Be.EqualTo(CreateDenyMessage30Min(requestStart.Hour, person.PermissionInformation.Culture(), person.PermissionInformation.Culture(), TimeZoneInfo.Utc, requestStart.Date));
			}
		}

		[Test]
		public void ShouldBeDeniedIfUnderstaffedDuringLunchAndLunchInEndOfRequest()
		{
			Now.Is(DateTime.UtcNow);
			var hourNow = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0).Utc();
			var requestStart = hourNow.AddHours(1);
			IPersonRequest personRequest;
			IPerson person;
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpMixedSkillDays(1, Tuple.Create(requestStart.Hour + 2, (double)20));

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronzeWithLunch");

				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddHours(3).Utc()));
				personRequest = new PersonRequest(person, absenceRequest);
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				uow.PersistAll();
			}

			UpdateRequestHandler.Handle(new NewMultiAbsenceRequestsCreatedEvent
			{
				PersonRequestIds = new List<Guid> { personRequest.Id.GetValueOrDefault() },
				InitiatorId = Guid.Empty,
				JobName = "JobName",
				LogOnBusinessUnitId = TestState.BusinessUnit.Id.GetValueOrDefault(),
				LogOnDatasource = "TestData",
				Sent = DateTime.UtcNow
			});
			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
				req.IsApproved.Should().Be.False();
				req.DenyReason.Should().Be.EqualTo(CreateDenyMessage30Min(requestStart.Hour + 2, person.PermissionInformation.Culture(), person.PermissionInformation.Culture(), TimeZoneInfo.Utc, requestStart.Date));
			}
		}

	}
}