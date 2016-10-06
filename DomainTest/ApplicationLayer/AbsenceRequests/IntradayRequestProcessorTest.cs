using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	public class IntradayRequestProcessorTest : ISetup
	{
		public IntradayRequestProcessor Target;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeCurrentScenario CurrentScenario;
		public FakeConfigReader ConfigReader;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public FakeSkillRepository SkillRepository;
	    private DateTime _now;
		private ISkill _primarySkill1;
		private ISkill _primarySkill2;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<IntradayRequestProcessor>().For<IntradayRequestProcessor>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<ApprovalServiceForTest>().For<IRequestApprovalService>();
       
            _now = new DateTime(2016,03,14,0,5,0);
        }

		[Test]
		public void ShouldDenyIfUnderstaffedOnAtLeastOnePrimarySkill()
		{
			var request = createNewRequest();

			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId =  _primarySkill1.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
                    ForecastWithShrinkage = 2,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList,DateTime.Now);

			staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId =  _primarySkill2.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
					ForecastWithShrinkage = 20,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);
            
			Target.Process(request, _now);

			Assert.AreEqual(4, getRequestStatus(PersonRequestRepository.Get(request.Id.GetValueOrDefault())));

		}

		[Test]
		public void ShouldApproveIfAllChecksOk()
		{
			var request = createNewRequest();

			
			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId =  _primarySkill1.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
					Forecast = 2,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);
			staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId =  _primarySkill2.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
					Forecast = 5,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist( staffingList, DateTime.Now);

			Target.Process(request, _now);

			Assert.AreEqual(2, getRequestStatus(PersonRequestRepository.Get(request.Id.GetValueOrDefault())));
		}


		[Test]
		public void ShouldUpdateResourcesWhenApprove()
		{
			var request = createNewRequest();

			var startDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc);

			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId =  _primarySkill1.Id.GetValueOrDefault(),
					StartDateTime = startDateTime,
					EndDateTime = endDateTime,
					Forecast = 2,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);
			staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId =  _primarySkill2.Id.GetValueOrDefault(),
					StartDateTime = startDateTime,
					EndDateTime = endDateTime,
					Forecast = 5,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);

			Target.Process(request, _now);

			var changes = ScheduleForecastSkillReadModelRepository.GetReadModelChanges(new DateTimePeriod(startDateTime, endDateTime)).ToList();

			changes.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldDenyIfUnderstaffedAfterApplyingChanges()
		{
			var request = createNewRequest();
			
			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId = _primarySkill1.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
					ForecastWithShrinkage = 10,
					StaffingLevel = 12
				}
			};
			// adding three different  changes on the same interval on the same skill
			ScheduleForecastSkillReadModelRepository.PersistChange(new StaffingIntervalChange()
			{
				StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
				SkillId = _primarySkill1.Id.GetValueOrDefault(),
				StaffingLevel = -1
			});
			ScheduleForecastSkillReadModelRepository.PersistChange(new StaffingIntervalChange()
			{
				StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
				SkillId = _primarySkill1.Id.GetValueOrDefault(),
				StaffingLevel = -1
			});

			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);
			staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId =  _primarySkill2.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
					ForecastWithShrinkage = 5,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);

			Target.Process(request, _now);

			Assert.AreEqual(4, getRequestStatus(PersonRequestRepository.Get(request.Id.GetValueOrDefault())));
		}

		[Test]
		public void ShouldApproveIfTheReadModelChangeIsOnDifferentInterval()
		{
			var request = createNewRequest();
			
			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{	SkillId = _primarySkill1.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
					ForecastWithShrinkage = 10,
					StaffingLevel = 12
				}
			};
			
			ScheduleForecastSkillReadModelRepository.PersistChange(new StaffingIntervalChange()
			{
				StartDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 14, 13, 30, 0, DateTimeKind.Utc),
				SkillId = _primarySkill1.Id.GetValueOrDefault(),
				StaffingLevel = -1
			});
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);

			Target.Process(request, _now);

			Assert.AreEqual(2, getRequestStatus(PersonRequestRepository.Get(request.Id.GetValueOrDefault())));
		}

		[Test]
		public void DenyIfUnderstaffedOnNonCascadingSkills()
		{
			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-03-14 05:00");

			var period = new DateTimePeriod(new DateTime(2016, 3, 14, 13, 30, 0, DateTimeKind.Utc),
											new DateTime(2016, 3, 14, 13, 45, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(absence);
			var primarySkill = SkillFactory.CreateSkillWithId("PrimarySkill1");
			primarySkill.SetCascadingIndex(1);
			var cascadingSkill = SkillFactory.CreateSkillWithId("cascadingSkill");

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 03, 01), new[] { primarySkill, cascadingSkill });
			person.WorkflowControlSet = workflowControlSet;

			LoggedOnUser.SetFakeLoggedOnUser(person);
			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, person);

			var request = new FakePersonRequest(person, new AbsenceRequest(absence, period));

			request.SetId(Guid.NewGuid());
			request.SetCreated(new DateTime(2016, 3, 14, 0, 5, 0, DateTimeKind.Utc));
			PersonRequestRepository.Add(request);

			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId = primarySkill.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 30, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 45, 0, DateTimeKind.Utc),
					ForecastWithShrinkage = 2,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);

			staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId = cascadingSkill.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 30, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 45, 0, DateTimeKind.Utc),
					ForecastWithShrinkage = 15,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);

			Target.Process(request, _now);

			Assert.AreEqual(4, getRequestStatus(PersonRequestRepository.Get(request.Id.GetValueOrDefault())));
		}

		[Test]
		public void ApproveIfOverstaffedOnNonCascadingSkill()
		{
			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-03-14 05:00");

			var period = new DateTimePeriod(new DateTime(2016, 3, 14, 13, 30, 0, DateTimeKind.Utc),
											new DateTime(2016, 3, 14, 13, 45, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(absence);
			var primarySkill = SkillFactory.CreateSkillWithId("PrimarySkill1");
			primarySkill.SetCascadingIndex(1);
			var cascadingSkill = SkillFactory.CreateSkillWithId("cascadingSkill");

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 03, 01), new[] { primarySkill, cascadingSkill });
			person.WorkflowControlSet = workflowControlSet;

			LoggedOnUser.SetFakeLoggedOnUser(person);
			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, person);

			var request = new FakePersonRequest(person, new AbsenceRequest(absence, period));

			request.SetId(Guid.NewGuid());
			request.SetCreated(new DateTime(2016, 3, 14, 0, 5, 0, DateTimeKind.Utc));
			PersonRequestRepository.Add(request);

			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId = primarySkill.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 30, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 45, 0, DateTimeKind.Utc),
					ForecastWithShrinkage = 2,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);

			staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId = cascadingSkill.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 30, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 45, 0, DateTimeKind.Utc),
					ForecastWithShrinkage = 10,
					StaffingLevel = 15
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);

			Target.Process(request, _now);

			Assert.AreEqual(2, getRequestStatus(PersonRequestRepository.Get(request.Id.GetValueOrDefault())));
		}

		private IPersonRequest createNewRequest(bool useWorkflowControlSet = true)
		{

			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-03-14 05:00");

			var period = new DateTimePeriod(new DateTime(2016, 3, 14, 12, 0, 0, DateTimeKind.Utc),
											new DateTime(2016, 3, 14, 14, 59, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(absence);
			var person = createAndSetupPerson(workflowControlSet, useWorkflowControlSet);

			LoggedOnUser.SetFakeLoggedOnUser(person);
			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, person);

			var request = createAbsenceRequest(person, absence, period);

			return request;
		}

		private IPerson createAndSetupPerson(IWorkflowControlSet workflowControlSet, bool useWorkflowControlSet)
		{
			_primarySkill1 = SkillFactory.CreateSkillWithId("PrimarySkill1");
			_primarySkill1.SetCascadingIndex(1);
			_primarySkill2 = SkillFactory.CreateSkillWithId("PrimarySkill2");
			_primarySkill2.SetCascadingIndex(1);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 03, 01), new[] {_primarySkill1, _primarySkill2});

			if(useWorkflowControlSet)
				person.WorkflowControlSet = workflowControlSet;

			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(IAbsence absence)
		{
			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = false };
			workflowControlSet.SetId(Guid.NewGuid());

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2016, 01, 01), new DateOnly(2016, 12, 31));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;
		}
		

		private PersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			var personRequest = new FakePersonRequest(person, new AbsenceRequest(absence, requestDateTimePeriod));

			personRequest.SetId(Guid.NewGuid());
			personRequest.SetCreated(new DateTime(2016, 3, 14, 0, 5, 0, DateTimeKind.Utc));
			PersonRequestRepository.Add(personRequest);

			return personRequest;
		}


		private int getRequestStatus(IPersonRequest request)
		{
			var requestStatus = 10;

			if (request.IsApproved)
				requestStatus = 2;
			else if (request.IsPending)
				requestStatus = 0;
			else if (request.IsNew)
				requestStatus = 3;
			else if (request.IsCancelled)
				requestStatus = 6;
			else if (request.IsWaitlisted && request.IsDenied)
				requestStatus = 5;
			else if (request.IsAutoDenied)
				requestStatus = 4;
			else if (request.IsDenied)
				requestStatus = 1;


			return requestStatus;
		}

	}
}
