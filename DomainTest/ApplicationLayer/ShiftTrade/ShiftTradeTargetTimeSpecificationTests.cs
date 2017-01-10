using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class ShiftTradeTargetTimeSpecificationTests
	{
		private ICurrentScenario _currentScenario;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IPersonRepository _personRepository;
		private IScheduleStorage _scheduleStorage;
		private IBusinessRuleProvider _businessRuleProvider;
		private INewBusinessRuleCollection _businessRuleCollection;
		private ShiftTradeTestHelper _shiftTradeTestHelper;

		[SetUp]
		public void Setup()
		{
			_currentScenario = new FakeCurrentScenario();
			_schedulingResultStateHolder = new SchedulingResultStateHolder();
			_personRepository = new FakePersonRepository();
			_scheduleStorage = new FakeScheduleStorage();
			_businessRuleProvider = new FakeBusinessRuleProvider();
			_businessRuleCollection = new FakeNewBusinessRuleCollection();
			_shiftTradeTestHelper = new ShiftTradeTestHelper(_schedulingResultStateHolder, _scheduleStorage, _personRepository,
				_businessRuleProvider, _businessRuleCollection, _currentScenario, new FakeScheduleProjectionReadOnlyActivityProvider());
		}

		[Test]
		public void ShouldDenyWhenUsingSpecificationChecker()
		{
			var personRequest = createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(new ShiftTradeBusinessRuleConfig(),
				useSpecificationCheckerWithConfig: false);
			Assert.IsTrue(personRequest.IsDenied);
			Assert.IsTrue(personRequest.DenyReason == "ShiftTradeTargetTimeDenyReason");
		}

		[Test]
		public void ShouldNotValidateWhenSpecificationIsConfiguredAsDisabled()
		{
			var shiftTradeBusinessRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(ShiftTradeTargetTimeSpecification).FullName,
				Enabled = false,
				HandleOptionOnFailed = RequestHandleOption.AutoDeny
			};
			var personRequest = createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(shiftTradeBusinessRuleConfig);
			Assert.IsTrue(personRequest.IsPending);

			acceptShiftTradeWithShiftTradeTargetTimeSpecificationBroken(personRequest);
			Assert.IsTrue(personRequest.IsApproved);
		}

		[Test]
		public void ShouldDenyWhenSpecificationIsConfiguredAsDeny()
		{
			var shiftTradeBusinessRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(ShiftTradeTargetTimeSpecification).FullName,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.AutoDeny
			};
			var personRequest = createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(shiftTradeBusinessRuleConfig);
			Assert.IsTrue(personRequest.IsDenied);
			Assert.IsTrue(personRequest.DenyReason == "ShiftTradeTargetTimeDenyReason");
		}

		[Test]
		public void ShouldPendingWhenSpecificationConfiguredAsPending()
		{
			var shiftTradeBusinessRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(ShiftTradeTargetTimeSpecification).FullName,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.Pending
			};
			var personRequest = createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(shiftTradeBusinessRuleConfig);
			Assert.IsTrue(personRequest.IsPending);

			acceptShiftTradeWithShiftTradeTargetTimeSpecificationBroken(personRequest);
			Assert.IsTrue(personRequest.IsPending);
		}

		[Test]
		public void ShouldPendingWhenAutoGrantIsOff()
		{
			var shiftTradeBusinessRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(ShiftTradeTargetTimeSpecification).FullName,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.Pending
			};
			var personRequest = createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(shiftTradeBusinessRuleConfig, false);
			Assert.IsTrue(personRequest.IsPending);

			acceptShiftTradeWithShiftTradeTargetTimeSpecificationBroken(personRequest);
			Assert.IsTrue(personRequest.IsPending);
		}

		private IPersonRequest createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(
			ShiftTradeBusinessRuleConfig shiftTradeBusinessRuleConfig
			, bool autoGrantShiftTrade = true, bool useSpecificationCheckerWithConfig = true)
		{
			var scheduleDate = new DateTime(2016, 7, 25);
			var scheduleDateOnly = new DateOnly(scheduleDate);

			var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(scheduleDate));
			var personTo = createPerson(autoGrantShiftTrade);
			personTo.AddSchedulePeriod(schedulePeriod);
			var activityPersonTo = new Activity("Shift_PersonTo").WithId();

			var personFrom = createPerson(autoGrantShiftTrade);
			personFrom.AddSchedulePeriod(schedulePeriod);
			var activityPersonFrom = new Activity("Shift_PersonFrom").WithId();

			var dateTimePeriod = new DateTimePeriod(scheduleDate.AddHours(8).ToUniversalTime(),
				scheduleDate.AddHours(10).ToUniversalTime());
			_shiftTradeTestHelper.AddPersonAssignment(personTo, dateTimePeriod, activityPersonTo);
			_shiftTradeTestHelper.AddPersonAssignment(personFrom, dateTimePeriod, activityPersonFrom);

			var personRequest = _shiftTradeTestHelper.PrepareAndGetPersonRequest(personFrom, personTo, scheduleDateOnly);

			var @event = new NewShiftTradeRequestCreatedEvent
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault()
			};

			var shiftTradeSpecifications = _shiftTradeTestHelper.GetDefaultShiftTradeSpecifications();
			shiftTradeSpecifications.Add(createShiftTradeTargetTimeSpecification());

			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			globalSettingDataRepository.PersistSettingValue(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings
			{
				BusinessRuleConfigs = new[] {shiftTradeBusinessRuleConfig}
			});

			if (useSpecificationCheckerWithConfig)
			{
				_shiftTradeTestHelper.UseSpecificationCheckerWithConfig(shiftTradeSpecifications, globalSettingDataRepository);
			}
			else
			{
				_shiftTradeTestHelper.UseSpecificationChecker(shiftTradeSpecifications);
			}

			_shiftTradeTestHelper.HandleRequest(@event);
			return personRequest;
		}

		private ShiftTradeTargetTimeSpecification createShiftTradeTargetTimeSpecification()
		{
			var gridLockManager = new GridlockManager();
			var matrixUserLocker = new MatrixUserLockLocker(() => gridLockManager);
			var notPermittedLocker = new MatrixNotPermittedLocker(new ThisAuthorization(new FullPermission()));
			var personListExtraxtor = new PersonListExtractorFromScheduleParts();
			var periodExtractor = new PeriodExtractorFromScheduleParts();
			var matrixListFactory = new MatrixListFactory(matrixUserLocker, notPermittedLocker, personListExtraxtor, periodExtractor);
			return new ShiftTradeTargetTimeSpecification(
				() => new SchedulerStateHolder(_schedulingResultStateHolder, new CommonStateHolder(null), new TimeZoneGuard())
				, matrixListFactory, new SchedulePeriodTargetTimeCalculator());
		}

		private void acceptShiftTradeWithShiftTradeTargetTimeSpecificationBroken(IPersonRequest personRequest)
		{
			var shiftTradeRequest = (IShiftTradeRequest) personRequest.Request;
			var personTo = shiftTradeRequest.PersonTo;
			var personFrom = shiftTradeRequest.PersonFrom;
			var scheduleDate = shiftTradeRequest.Period.StartDateTime;
			var @event = _shiftTradeTestHelper.GetAcceptShiftTradeEvent(personTo, personRequest.Id.GetValueOrDefault());

			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom }, null,
				new DateOnlyPeriod(new DateOnly(scheduleDate), new DateOnly(scheduleDate.AddDays(7))), _currentScenario.Current());
			var businessRuleProvider = new BusinessRuleProvider();
			var businessRules = businessRuleProvider.GetBusinessRulesForShiftTradeRequest(_schedulingResultStateHolder, false);
			_shiftTradeTestHelper.SetApprovalService(scheduleDictionary, businessRules);

			_shiftTradeTestHelper.HandleRequest(@event, false, businessRuleProvider);
		}

		private IPerson createPerson(bool autoGrantShiftTrade = true)
		{
			var workControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(autoGrantShiftTrade);
			var startDate = new DateOnly(2016, 1, 1);
			var team = TeamFactory.CreateTeam("team", "site");

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, team);
			person.WorkflowControlSet = workControlSet;
			_personRepository.Add(person);

			return person;
		}
	}
}
