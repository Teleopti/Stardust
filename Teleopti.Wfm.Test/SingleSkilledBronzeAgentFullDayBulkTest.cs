using System;
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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Test
{
	[DatabaseTest]
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_xx)]
	public class SingleSkilledBronzeAgentFullDayBulkTest : SetUpCascadingShifts
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
		public void ShouldBeApprovedIfOverstaffedFullDay()
		{
			Now.Is(DateTime.UtcNow);
			IPersonRequest personRequest;
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpLowDemandSkillDays();

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");

				var requestStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0);
				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddDays(1).AddMinutes(-1).Utc()));
				personRequest = new PersonRequest(person, absenceRequest);
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				uow.PersistAll();
			}

			UpdateRequestHandler.Handle(new NewMultiAbsenceRequestsCreatedEvent
			{
				PersonRequestIds = new List<Guid> {personRequest.Id.GetValueOrDefault()},
				InitiatorId = Guid.Empty,
				JobName = "JobName",
				LogOnBusinessUnitId = TestState.BusinessUnit.Id.GetValueOrDefault(),
				LogOnDatasource = "TestData",
				Sent = DateTime.UtcNow
			});
			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var req = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
				req.IsApproved.Should().Be.True();
			}
		}

		[Test]
		public void ShouldBeDeniedIfUnderStaffedFullDay()
		{
			Now.Is(DateTime.UtcNow);
			IPersonRequest personRequest;
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpHighDemandSkillDays();

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");

				var requestStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0);
				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart.Utc(), requestStart.AddDays(1).AddMinutes(-1).Utc()));
				personRequest = new PersonRequest(person, absenceRequest);
				PersonRequestRepository.Add(personRequest);
				personRequest.Pending();
				uow.PersistAll();
			}
			UpdateRequestHandler.Handle(new NewMultiAbsenceRequestsCreatedEvent
			{
				PersonRequestIds = new List<Guid> {personRequest.Id.GetValueOrDefault()},
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
			}
		}
	}
}