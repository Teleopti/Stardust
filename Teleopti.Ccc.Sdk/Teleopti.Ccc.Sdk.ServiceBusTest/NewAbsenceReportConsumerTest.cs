using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.AbsenceReport;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
	[TestFixture]
	public class NewAbsenceReportConsumerTest
	{
		private NewAbsenceReportConsumer _target;
		private ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private IUnitOfWork _unitOfWork;
		private IPerson _person;
		private readonly IScenario _scenario = new Scenario("Test");

		private readonly IAbsence _absence = new Absence
		{
			Description = new Description("Vacation", "VAC"),
			Tracker = Tracker.CreateDayTracker()
		};

		private NewAbsenceReportCreated _message;
		private readonly DateTimePeriod _period = new DateTimePeriod(2010, 3, 30, 2010, 3, 31);
		private IWorkflowControlSet _workflowControlSet;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IScheduleDictionary _scheduleDictionary;
		private IRequestApprovalService _requestApprovalService;
		private IRequestFactory _factory;
		private IScheduleDifferenceSaver _scheduleDictionarySaver;
		private ICurrentScenario _scenarioRepository;
		private IUpdateScheduleProjectionReadModel _updateScheduleProjectionReadModel;
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
			_message = new NewAbsenceReportCreated
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

			_updateScheduleProjectionReadModel = MockRepository.GenerateMock<IUpdateScheduleProjectionReadModel>();

			_target = new NewAbsenceReportConsumer(_unitOfWorkFactory, _scenarioRepository,
				_schedulingResultStateHolder, _factory, _scheduleDictionarySaver, _updateScheduleProjectionReadModel,
				_loaderWithoutResourceCalculation, _personRepository, businessRules);
			prepareUnitOfWork();
		}

		[Test]
		public void ShouldLoadEverythingNeededForResourceCalculationWhenCheckingIntradayStaffing()
		{
			prepareAbsenceReport();
			expectLoadOfSchedules();
			_requestApprovalService.Stub(x => x.ApproveAbsence(_absence, _period, _person))
				.Return(new List<IBusinessRuleResponse>());

			_factory.Stub(x => x.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(_requestApprovalService);

			_target.Consume(_message);


			var expectedPeriod = getExpectedPeriod().ChangeStartTime(TimeSpan.FromDays(-1));

			_loaderWithoutResourceCalculation.AssertWasCalled(
				x => x.Execute(_scenario, expectedPeriod, new List<IPerson> {_person}));
		}

		[Test]
		public void ShouldRecreateScheduleReadModels()
		{
			var dateOnlyPeriod = getExpectedPeriod().ToDateOnlyPeriod(_person.PermissionInformation.DefaultTimeZone());
			_loaderWithoutResourceCalculation.Execute(_scenario, getExpectedPeriod().ChangeStartTime(TimeSpan.FromDays(-1)),
				new List<IPerson> {_person});

			prepareAbsenceReport();
			_requestApprovalService.Stub(x => x.ApproveAbsence(_absence, _period, _person))
				.Return(new List<IBusinessRuleResponse>());

			_factory.Stub(x => x.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(_requestApprovalService);

			expectLoadOfSchedules();
			expectPersistOfDictionary();

			_target.Consume(_message);
			_unitOfWork.AssertWasCalled(x => x.PersistAll());
			_updateScheduleProjectionReadModel.AssertWasCalled(x => x.Execute(_scheduleRange, dateOnlyPeriod));
		}

		[Test]
		public void VerifyAbsenceReportNotAppliedWhenAbsenceNotReportable()
		{
			prepareAbsenceReport();

			var message = new NewAbsenceReportCreated
			{
				AbsenceId = Guid.NewGuid(),
				RequestedDate = _message.RequestedDate,
				PersonId = _message.PersonId
			};
			_target.Consume(message);
			_unitOfWork.AssertWasNotCalled(x => x.PersistAll());
		}

		#region Private methods

		private void prepareAbsenceReport()
		{
			_scenarioRepository.Stub(x => x.Current()).Return(_scenario);
			_personRepository.Stub(x => x.FindPeople(new List<Guid>())).IgnoreArguments().Return(new List<IPerson> {_person});
		}

		private void prepareUnitOfWork()
		{
			var currentUowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			currentUowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_unitOfWorkFactory.Stub(x => x.Current()).Return(currentUowFactory);
		}

		private void createServices()
		{
			_requestApprovalService = MockRepository.GenerateMock<IRequestApprovalService>();
		}

		private void createInfrastructure()
		{
			_unitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			_unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			_factory = MockRepository.GenerateMock<IRequestFactory>();
		}

		private void createPersonAndWorkflowControlSet()
		{
			_workflowControlSet = MockRepository.GenerateMock<IWorkflowControlSet>();
			_workflowControlSet.Stub(x => x.AllowedAbsencesForReport).Return(new List<IAbsence> {_absence});
			_person = new Person {Name = new Name("John", "Doe")};
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
			_scheduleRange = new ScheduleRange(_scheduleDictionary, new ScheduleParameters(_scenario, _person, _period));
			_scheduleDictionary.Stub(x => x[_person]).Return(_scheduleRange);
			_scheduleDictionary.Stub(x => x.Scenario).Return(_scenario);
		}

		private void expectPersistOfDictionary()
		{
			var changes = new DifferenceCollection<IPersistableScheduleData>();
			_scheduleDictionary.Stub(x => x.DifferenceSinceSnapshot()).Return(changes);
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