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
using Teleopti.Ccc.InfrastructureTest;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Wfm.Test
{
	[DatabaseTest]
	public class MultiSkilledGoldSilverBronzeAgentBulkTest : SetUpCascadingShifts
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
		public void ShouldBeApprovedIfOverstaffedSingleInterval()
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
				var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonAllSkills1");
				
				var requestStart = now.AddHours(2);
				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddMinutes(30)));
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
				req.IsApproved.Should().Be.True();
			}
		}

		[Test]
		public void ShouldBeApprovedIfOverstaffedMultipleIntervals()
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
				var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonAllSkills1");
				
				var requestStart = now.AddHours(2);
				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(3)));
				personRequest = new PersonRequest(person, absenceRequest);
				PersonRequestRepository.Add(personRequest);
				personRequest.Pending();
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
				req.IsApproved.Should().Be.True();
			}
		}

		[Test]
		public void ShouldBeDeniedIfUnderstaffedSingleInterval()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			IPersonRequest personRequest;
			IPerson person;
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpHighDemandSkillDays();

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonAllSkills1");
				
				var requestStart = now.AddHours(2);
				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddMinutes(30)));
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
				req.DenyReason.Should().StartWith(Resources.ResourceManager.GetString("InsufficientStaffingHours", person.PermissionInformation.Culture()).Substring(0, 10));
			}
		}

		[Test]
		public void ShouldBeDeniedIfUnderstaffedMultipleIntervals()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			IPersonRequest personRequest;
			IPerson person;
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpHighDemandSkillDays();

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonAllSkills1");
				
				var requestStart = now.AddHours(2);
				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(3)));
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
				req.IsApproved.Should().Be.False();
				req.DenyReason.Should().StartWith(Resources.ResourceManager.GetString("InsufficientStaffingHours", person.PermissionInformation.Culture()).Substring(0, 10));
			}
		}

		[Test]
		public void ShouldBeDeniedIfUnderstaffedOnFirstHourAndOverstaffedOnSecond()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			IPersonRequest personRequest;
			IPerson person;
			
			var requestStart = now.AddHours(2);
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpMixedSkillDays(1, new Tuple<int, double>(requestStart.Hour, 10));

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonAllSkills1");

				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(2)));
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
				req.IsApproved.Should().Be.False();
				req.DenyReason.Should().Be.EqualTo(CreateDenyMessage30Min(requestStart.Hour, person.PermissionInformation.Culture(), person.PermissionInformation.Culture(), TimeZoneInfo.Utc, requestStart.Date));
			}
		}

		[Test]
		public void ShouldBeApprovedIfNoShiftInRequestPeriodAndOverStaffed()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			IPersonRequest personRequest;
			var requestStart = now.AddHours(2);
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpLowDemandSkillDays();

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonAllSkillsNoShift");

				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(2)));
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
		public void ShouldBeApprovedIfNoShiftInRequestPeriodAndUnderstaffed()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			IPersonRequest personRequest;
			var requestStart = now.AddHours(2);
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
			SetUpHighDemandSkillDays();

			var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
			var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonAllSkillsNoShift");

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(2)));
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
				req.IsApproved.Should().Be.True();
			}
		}

		[Test]
		public void ShouldBeApprovedWhenActivityIsNotConnectedToPersonSkillAndOverStaffed()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			IPersonRequest personRequest;
			var requestStart = now.AddHours(2);
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();

				SetUpLowDemandSkillDays();

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonAllSkillsWrongAct");

				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(2)));
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
		public void ShouldBeApprovedWhenActivityIsNotConnectedToPersonSkillAndUnderStaffed()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			IPersonRequest personRequest;
			IPerson person;
			var requestStart = now.AddHours(2);
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpHighDemandSkillDays();

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				 person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonAllSkillsWrongAct");

				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(2)));
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
				req.IsApproved.Should().Be.False();
				req.DenyReason.Should().StartWith(Resources.ResourceManager.GetString("InsufficientStaffingHours", person.PermissionInformation.Culture()).Substring(0, 10));
			}
		}

		[Test]
		public void ShouldBeApprovedIfOverstaffedAndActivityIsOvertime()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			IPersonRequest personRequest;
			var requestStart = now.AddHours(2);
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpLowDemandSkillDays();

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonAllSkillsOvertime");

				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(3)));
				personRequest = new PersonRequest(person, absenceRequest);
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				uow.PersistAll();
			}

			UpdateRequestHandler.Handle(new NewMultiAbsenceRequestsCreatedEvent
			{
				PersonRequestIds = new List<Guid>
				{
					personRequest.Id.GetValueOrDefault()
				},
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
		public void ShouldBeDeniedIfUnderstaffedAndActivityIsOvertime()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
			Now.Is(now);
			IPersonRequest personRequest;
			var requestStart = now.AddHours(2);
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SetUpRelevantStuffWithCascading();
				SetUpHighDemandSkillDays();

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonAllSkillsOvertime");

				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddHours(3)));
				personRequest = new PersonRequest(person, absenceRequest);
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				uow.PersistAll();
			}

			UpdateRequestHandler.Handle(new NewMultiAbsenceRequestsCreatedEvent
			{
				PersonRequestIds = new List<Guid>
				{
					personRequest.Id.GetValueOrDefault()
				},
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