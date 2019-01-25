using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
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
	[SpecialDatabaseTest]
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
		public UpdateStaffingLevelReadModelStartDate UpdateStaffingLevelReadModelStartDate;


		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.BeginTest();
		}


		[Test]
		public void ShouldBeApprovedIfOverstaffedFullDay()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			IPersonRequest personRequest;
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpLowDemandSkillDays();

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");
				person.WorkflowControlSet.AbsenceRequestExpiredThreshold = null;

				var requestStart = now.Date;
				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddDays(1).AddMinutes(-1)));
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
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			IPersonRequest personRequest;
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpHighDemandSkillDays();

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonBronze1");

				var requestStart = now.Date;
				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddDays(1).AddMinutes(-1)));
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