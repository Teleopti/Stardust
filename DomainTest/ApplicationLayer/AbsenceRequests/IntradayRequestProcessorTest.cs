using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Common;
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
	[LegacyDomainTest]
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
		public FakeIntradayRequestWithinOpenHourValidator IntradayRequestWithinOpenHourValidator;
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
			system.UseTestDouble<FakeIntradayRequestWithinOpenHourValidator>().For<IIntradayRequestWithinOpenHourValidator>();
       
            _now = new DateTime(2016,03,14,0,5,0);
        }

		[Test]
		public void DenyIfUnderstaffedOnAtLeastOnePrimarySkill()
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
		public void ApproveIfAllChecksOk()
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
		public void UpdateResourcesAllocationWhenApproved()
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
		public void DenyIfUnderstaffedAfterApplyingChanges()
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
		public void ApproveIfTheReadModelChangeIsOnDifferentInterval()
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

			staffingList.Add(
				new SkillStaffingInterval()
				{   SkillId = _primarySkill2.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
					ForecastWithShrinkage = 10,
					StaffingLevel = 12
				
			});

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

			IntradayRequestWithinOpenHourValidator.FakeOpenHourStatus.Add(primarySkill.Id.GetValueOrDefault(), OpenHourStatus.WithinOpenHour);
			IntradayRequestWithinOpenHourValidator.FakeOpenHourStatus.Add(cascadingSkill.Id.GetValueOrDefault(), OpenHourStatus.WithinOpenHour);

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

			IntradayRequestWithinOpenHourValidator.FakeOpenHourStatus.Add(primarySkill.Id.GetValueOrDefault(), OpenHourStatus.WithinOpenHour);
			IntradayRequestWithinOpenHourValidator.FakeOpenHourStatus.Add(cascadingSkill.Id.GetValueOrDefault(), OpenHourStatus.WithinOpenHour);

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

		[Test]
		public void DenyWithIntervalsInAgentsTimezone()
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
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);

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

			var personRequest = PersonRequestRepository.Get(request.Id.GetValueOrDefault());
			personRequest.DenyReason.Should().Not.Contain("13:00 - 13:15");
			personRequest.DenyReason.Should().Contain("14:00 - 14:15");
		}

		[Test]
		public void ApproveIfAllSkillAreClosedOrMissingOpenHours()
		{
			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-03-14 05:00");

			var period = new DateTimePeriod(new DateTime(2016, 3, 14, 12, 0, 0, DateTimeKind.Utc),
											new DateTime(2016, 3, 14, 14, 59, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(absence);
			var skill1 = SkillFactory.CreateSkillWithId("PrimarySkill1");
			skill1.SetCascadingIndex(1);
			var skill2 = SkillFactory.CreateSkillWithId("PrimarySkill2");
			skill2.SetCascadingIndex(1);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 03, 01), new[] { skill1, skill2 });
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			person.WorkflowControlSet = workflowControlSet;

			LoggedOnUser.SetFakeLoggedOnUser(person);
			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, person);

			var personRequest = new FakePersonRequest(person, new AbsenceRequest(absence, period));

			personRequest.SetId(Guid.NewGuid());
			personRequest.SetCreated(new DateTime(2016, 3, 14, 0, 5, 0, DateTimeKind.Utc));
			PersonRequestRepository.Add(personRequest);
			IntradayRequestWithinOpenHourValidator.FakeOpenHourStatus.Add(skill1.Id.GetValueOrDefault(), OpenHourStatus.OutsideOpenHour);
			IntradayRequestWithinOpenHourValidator.FakeOpenHourStatus.Add(skill2.Id.GetValueOrDefault(), OpenHourStatus.MissingOpenHour);
			Target.Process(personRequest, _now);

			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());
			Assert.AreEqual(2, getRequestStatus(PersonRequestRepository.Get(request.Id.GetValueOrDefault())));
		}

		[Test]
		public void ApproveOnIntervalsEvenIfSomeSkillsAreClosed()
		{
			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-03-14 05:00");

			var period = new DateTimePeriod(new DateTime(2016, 3, 14, 12, 0, 0, DateTimeKind.Utc),
											new DateTime(2016, 3, 14, 14, 59, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(absence);
			var skill1 = SkillFactory.CreateSkillWithId("PrimarySkill1");
			skill1.SetCascadingIndex(1);
			var skill2 = SkillFactory.CreateSkillWithId("PrimarySkill2");
			skill2.SetCascadingIndex(1);
			var skill3 = SkillFactory.CreateSkillWithId("PrimarySkill2");
			

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 03, 01), new[] { skill1, skill2,skill3 });
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			person.WorkflowControlSet = workflowControlSet;

			LoggedOnUser.SetFakeLoggedOnUser(person);
			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, person);

			var personRequest = new FakePersonRequest(person, new AbsenceRequest(absence, period));

			personRequest.SetId(Guid.NewGuid());
			personRequest.SetCreated(new DateTime(2016, 3, 14, 0, 5, 0, DateTimeKind.Utc));
			PersonRequestRepository.Add(personRequest);
			IntradayRequestWithinOpenHourValidator.FakeOpenHourStatus.Add(skill1.Id.GetValueOrDefault(), OpenHourStatus.OutsideOpenHour);
			IntradayRequestWithinOpenHourValidator.FakeOpenHourStatus.Add(skill2.Id.GetValueOrDefault(), OpenHourStatus.MissingOpenHour);
			IntradayRequestWithinOpenHourValidator.FakeOpenHourStatus.Add(skill3.Id.GetValueOrDefault(), OpenHourStatus.WithinOpenHour);

			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId =  skill3.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
					Forecast = 2,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);

			Target.Process(personRequest, _now);

			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());
			Assert.AreEqual(2, getRequestStatus(PersonRequestRepository.Get(request.Id.GetValueOrDefault())));
		}

		[Test]
		public void DenyIfNoStaffingIntervalsFoundForASkill()
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
			Target.Process(request, _now);

			Assert.AreEqual(4, getRequestStatus(PersonRequestRepository.Get(request.Id.GetValueOrDefault())));
		}

		[Test]
		public void ApproveIfForecastWithShrinkageIsZero()
		{
			var request = createNewRequest();

			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId =  _primarySkill1.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
					Forecast = 10,
					ForecastWithShrinkage = 0,
					StaffingLevel = 0
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
					Forecast = 10,
					ForecastWithShrinkage = 0,
					StaffingLevel = 0
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);

			Target.Process(request, _now);

			Assert.AreEqual(2, getRequestStatus(PersonRequestRepository.Get(request.Id.GetValueOrDefault())));
		}

		[Test]
		public void ApproveIfForecastIsZero()
		{
			var request = createNewRequest(true,false);

			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId =  _primarySkill1.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
					Forecast = 0,
					ForecastWithShrinkage = 10,
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
					Forecast = 0,
					ForecastWithShrinkage = 10,
					StaffingLevel = 0
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);

			Target.Process(request, _now);

			Assert.AreEqual(2, getRequestStatus(PersonRequestRepository.Get(request.Id.GetValueOrDefault())));
		}

		[Test]
		public void DenyIfRequestIsOvernightWithProperDenyReason()
		{

			var period = new DateTimePeriod(new DateTime(2016, 10, 17, 21, 15, 0, DateTimeKind.Utc),
											new DateTime(2016, 10, 17, 22, 15, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(absence);
			var primarySkill = SkillFactory.CreateSkillWithId("PrimarySkill1");
			primarySkill.SetCascadingIndex(1);
			
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 03, 01), new[] { primarySkill });
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			person.WorkflowControlSet = workflowControlSet;

			IntradayRequestWithinOpenHourValidator.FakeOpenHourStatus.Add(primarySkill.Id.GetValueOrDefault(), OpenHourStatus.WithinOpenHour);
			
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, person);

			var request = new FakePersonRequest(person, new AbsenceRequest(absence, period));

			request.SetId(Guid.NewGuid());
			request.SetCreated(new DateTime(2016, 10, 17, 0, 5, 0, DateTimeKind.Utc));
			PersonRequestRepository.Add(request);

			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId = primarySkill.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 10, 17, 21, 30, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 10, 17, 21, 45, 0, DateTimeKind.Utc),
					ForecastWithShrinkage = 10,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);

			staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId = primarySkill.Id.GetValueOrDefault(),
					StartDateTime = new DateTime(2016, 10, 17, 21, 45, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 10, 17, 22, 0, 0, DateTimeKind.Utc),
					ForecastWithShrinkage = 15,
					StaffingLevel = 15
				}
			};
			ScheduleForecastSkillReadModelRepository.Persist(staffingList, DateTime.Now);

			Target.Process(request, _now);

			var result = PersonRequestRepository.Get(request.Id.GetValueOrDefault());

			Assert.AreEqual(4, getRequestStatus(result));
			result.DenyReason.Should().Contain("23:30 - 23:45, 23:45 - 00:00 +1");

		}

		private IPersonRequest createNewRequest(bool useWorkflowControlSet = true, bool useThresholdValidator = true)
		{

			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-03-14 05:00");

			var period = new DateTimePeriod(new DateTime(2016, 3, 14, 12, 0, 0, DateTimeKind.Utc),
											new DateTime(2016, 3, 14, 14, 59, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(absence, useThresholdValidator);
			var person = createAndSetupPerson(workflowControlSet, useWorkflowControlSet);
			
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, person);

			var request = createAbsenceRequest(person, absence, period);

			IntradayRequestWithinOpenHourValidator.FakeOpenHourStatus.Add(_primarySkill1.Id.GetValueOrDefault(), OpenHourStatus.WithinOpenHour);
			IntradayRequestWithinOpenHourValidator.FakeOpenHourStatus.Add(_primarySkill2.Id.GetValueOrDefault(), OpenHourStatus.WithinOpenHour);

			return request;
		}

		private IPerson createAndSetupPerson(IWorkflowControlSet workflowControlSet, bool useWorkflowControlSet)
		{
			_primarySkill1 = SkillFactory.CreateSkillWithId("PrimarySkill1");
			_primarySkill1.SetCascadingIndex(1);
			_primarySkill2 = SkillFactory.CreateSkillWithId("PrimarySkill2");
			_primarySkill2.SetCascadingIndex(1);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 03, 01), new[] {_primarySkill1, _primarySkill2});
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			if(useWorkflowControlSet)
				person.WorkflowControlSet = workflowControlSet;

			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(IAbsence absence, bool useThresholdValidator = true)
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

			if(useThresholdValidator)
				absenceRequestOpenPeriod.StaffingThresholdValidator = new StaffingThresholdWithShrinkageValidator();
			else
				absenceRequestOpenPeriod.StaffingThresholdValidator = new StaffingThresholdValidator();

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

	public class FakeIntradayRequestWithinOpenHourValidator : IIntradayRequestWithinOpenHourValidator
	{
		public Dictionary<Guid, OpenHourStatus> FakeOpenHourStatus = new Dictionary<Guid, OpenHourStatus>();
		public OpenHourStatus Validate(ISkill skill, DateTimePeriod requestPeriod)
		{
			return FakeOpenHourStatus[skill.Id.GetValueOrDefault()];
		}
	}
}
