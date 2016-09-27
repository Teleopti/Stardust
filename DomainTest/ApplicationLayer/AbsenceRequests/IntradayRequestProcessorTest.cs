﻿using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
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
		public IPersonRepository PersonRepository;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeCurrentScenario CurrentScenario;
		public FakeConfigReader ConfigReader;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		private ISkill _skill1;
		private ISkill _skill2;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<IntradayRequestProcessor>().For<IntradayRequestProcessor>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<ApprovalServiceForTest>().For<IRequestApprovalService>();

		}

		[Test]
		public void ShouldDenyIfUnderstaffedOnAtLeastOnePrimarySkill()
		{
			var request = createNewRequest();

			ScheduleForecastSkillReadModelRepository.FakeStaffingList = new Dictionary<Guid, List<SkillStaffingInterval>>();
			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
					Forecast = 2,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.FakeStaffingList.Add(_skill1.Id.GetValueOrDefault(), staffingList);
			staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
					Forecast = 20,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.FakeStaffingList.Add(_skill2.Id.GetValueOrDefault(), staffingList);


			Target.Process(request);

			Assert.AreEqual(4, getRequestStatus(PersonRequestRepository.Get(request.Id.GetValueOrDefault())));

		}

		[Test]
		public void ShouldApproveIfAllChecksOk()
		{
			var request = createNewRequest();

			ScheduleForecastSkillReadModelRepository.FakeStaffingList = new Dictionary<Guid, List<SkillStaffingInterval>>();
			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
					Forecast = 2,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.FakeStaffingList.Add(_skill1.Id.GetValueOrDefault(), staffingList);
			staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					StartDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc),
					Forecast = 5,
					StaffingLevel = 10
				}
			};
			ScheduleForecastSkillReadModelRepository.FakeStaffingList.Add(_skill2.Id.GetValueOrDefault(), staffingList);

			Target.Process(request);

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
			//var person = PersonFactory.CreatePersonWithId();
			_skill1 = SkillFactory.CreateSkillWithId("GoodSkillName1");
			_skill1.SetCascadingIndex(1);
			_skill2 = SkillFactory.CreateSkillWithId("GoodSkillName2");
			_skill2.SetCascadingIndex(1);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 03, 01), new[] {_skill1, _skill2});
			PersonRepository.Add(person);

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
