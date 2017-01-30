﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[TestFixture]
	public class NewAbsenceReportHandlerTest
	{
		private NewAbsenceReport _target;
		private IUnitOfWork _unitOfWork;
		private IPerson _person;
		private readonly IScenario _scenario = new Scenario("Test");

		private readonly IAbsence _absence = new Absence
		{
			Description = new Description("Vacation", "VAC"),
			Tracker = Tracker.CreateDayTracker()
		};

		private NewAbsenceReportCreatedEvent _message;
		private readonly DateTimePeriod _period = new DateTimePeriod(2010, 3, 30, 2010, 3, 31);
		private IWorkflowControlSet _workflowControlSet;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IScheduleDictionary _scheduleDictionary;
		private IRequestApprovalService _requestApprovalService;
		private IRequestFactory _factory;
		private IScheduleDifferenceSaver _scheduleDictionarySaver;
		private ICurrentScenario _scenarioRepository;
		private IScheduleRange _scheduleRange;
		private ILoadSchedulesForRequestWithoutResourceCalculation _loaderWithoutResourceCalculation;
		private IPersonRepository _personRepository;

		[SetUp]
		public void Setup()
		{
			createInfrastructure();
			createServices();
			createSchedulingResultStateHolder();
			createPersonAndWorkflowControlSet();
			createRepositories();

			_absence.SetId(Guid.NewGuid());
			_message = new NewAbsenceReportCreatedEvent
			{
				AbsenceId = (Guid) _absence.Id,
				RequestedDate = new DateTime(2010, 3, 30),
				PersonId = (Guid) _person.Id
			};
			_scenarioRepository = MockRepository.GenerateMock<ICurrentScenario>();

			_loaderWithoutResourceCalculation = MockRepository.GenerateMock<ILoadSchedulesForRequestWithoutResourceCalculation>();
			MockRepository.GenerateMock<IBudgetGroupAllowanceSpecification>();
			MockRepository.GenerateMock<IBudgetGroupHeadCountSpecification>();
			var businessRules = MockRepository.GenerateMock<IBusinessRulesForPersonalAccountUpdate>();

			_target = new NewAbsenceReport( _scenarioRepository,
				new FakeSchedulingResultStateHolderProvider(_schedulingResultStateHolder), _factory, _scheduleDictionarySaver,
				_loaderWithoutResourceCalculation, _personRepository, businessRules);
		}

		[Test]
		public void ShouldLoadEverythingNeededForResourceCalculationWhenCheckingIntradayStaffing()
		{
			prepareAbsenceReport();
			expectLoadOfSchedules();
			_requestApprovalService.Stub(x => x.ApproveAbsence(_absence, _period, _person))
				.Return(new List<IBusinessRuleResponse>());

			_factory.Stub(x => x.GetRequestApprovalService(null, _scenario, _schedulingResultStateHolder)).IgnoreArguments().Return(_requestApprovalService);

			_target.Handle(_message);


			var expectedPeriod = getExpectedPeriod().ChangeStartTime(TimeSpan.FromDays(-1));

			_loaderWithoutResourceCalculation.AssertWasCalled(
				x => x.Execute(_scenario, expectedPeriod, new List<IPerson> {_person}, _schedulingResultStateHolder));
		}
		
		[Test]
		public void VerifyAbsenceReportNotAppliedWhenAbsenceNotReportable()
		{
			prepareAbsenceReport();

			var message = new NewAbsenceReportCreatedEvent
			{
				AbsenceId = Guid.NewGuid(),
				RequestedDate = _message.RequestedDate,
				PersonId = _message.PersonId
			};
			_target.Handle(message);
			_unitOfWork.AssertWasNotCalled(x => x.PersistAll());
		}

		#region Private methods

		private void prepareAbsenceReport()
		{
			_scenarioRepository.Stub(x => x.Current()).Return(_scenario);
			_personRepository.Stub(x => x.FindPeople(new List<Guid>())).IgnoreArguments().Return(new List<IPerson> {_person});
		}

		private void createServices()
		{
			_requestApprovalService = MockRepository.GenerateMock<IRequestApprovalService>();
		}

		private void createInfrastructure()
		{
			_unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			_factory = MockRepository.GenerateMock<IRequestFactory>();
		}

		private void createPersonAndWorkflowControlSet()
		{
			_workflowControlSet = MockRepository.GenerateMock<IWorkflowControlSet>();
			_workflowControlSet.Stub(x => x.AllowedAbsencesForReport).Return(new List<IAbsence> {_absence});
			_person = new Person().WithName(new Name("John", "Doe"));
			_person.SetId(Guid.NewGuid());
			_person.WorkflowControlSet = _workflowControlSet;
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
		}

		private void createSchedulingResultStateHolder()
		{
			_schedulingResultStateHolder = new SchedulingResultStateHolder();
			_scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			_scheduleDictionarySaver = MockRepository.GenerateMock<IScheduleDifferenceSaver>();
			_schedulingResultStateHolder.Schedules = _scheduleDictionary;
		}

		private void createRepositories()
		{
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
		}

		private void expectLoadOfSchedules()
		{
			_scheduleRange = new ScheduleRange(_scheduleDictionary, new ScheduleParameters(_scenario, _person, _period), new PersistableScheduleDataPermissionChecker());
			_scheduleDictionary.Stub(x => x[_person]).Return(_scheduleRange);
			_scheduleDictionary.Stub(x => x.Scenario).Return(_scenario);
		}

		private DateTimePeriod getExpectedPeriod()
		{
			var fullDayTimeSpanStart = new TimeSpan(0, 0, 0);
			var fullDayTimeSpanEnd = new TimeSpan(23, 59, 0);
			var startDateTime = new DateTime(_message.RequestedDate.Year, _message.RequestedDate.Month,
				_message.RequestedDate.Day, fullDayTimeSpanStart.Hours, fullDayTimeSpanStart.Minutes,
				fullDayTimeSpanStart.Seconds);
			var endDateTime = new DateTime(_message.RequestedDate.Year, _message.RequestedDate.Month,
				_message.RequestedDate.Day, fullDayTimeSpanEnd.Hours, fullDayTimeSpanEnd.Minutes,
				fullDayTimeSpanEnd.Seconds);
			var timezone = _person.PermissionInformation.DefaultTimeZone();
			var expectedPeriod = new DateTimePeriod(
				DateTime.SpecifyKind(
					TimeZoneHelper.ConvertToUtc(startDateTime, timezone),
					DateTimeKind.Utc),
				DateTime.SpecifyKind(
					TimeZoneHelper.ConvertToUtc(endDateTime, timezone),
					DateTimeKind.Utc));
			return expectedPeriod;
		}

		#endregion
	}
}