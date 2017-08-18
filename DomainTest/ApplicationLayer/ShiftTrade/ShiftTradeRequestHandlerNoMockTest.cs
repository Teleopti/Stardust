using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	[TestWithStaticDependenciesAvoidUse]
	public class ShiftTradeRequestHandlerNoMockTest
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
			_personRepository = new FakePersonRepositoryLegacy2();
			_scheduleStorage = new FakeScheduleStorage();
			_businessRuleProvider = new FakeBusinessRuleProvider();
			_businessRuleCollection = new FakeNewBusinessRuleCollection();
			_shiftTradeTestHelper = new ShiftTradeTestHelper(_schedulingResultStateHolder, _scheduleStorage, _personRepository,
				_businessRuleProvider, _currentScenario, new FakeScheduleProjectionReadOnlyActivityProvider());
		}

		[Test]
		public void ShouldTradeShiftsOnAutoApproval()
		{
			var workflowControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(true);
			var result = doBasicShiftTrade(workflowControlSet);

			Assert.IsTrue(result.PersonRequest.IsApproved);
			Assert.IsTrue(result.PersonToSchedule.PersonAssignment().ShiftLayers.Single().Payload.Id == result.ActivityFrom.Id);
			Assert.IsTrue(result.PersonFromSchedule.PersonAssignment().ShiftLayers.Single().Payload.Id == result.ActivityTo.Id);
		}

		[Test]
		public void ShouldSetMinWeeklyWorkTimeBrokenRuleWhenUseMinWeekWorkTimeIsOn()
		{
			var businessRuleProvider = new BusinessRuleProvider();
			var personRequest = doShiftTradeWithBrokenRules(businessRuleProvider, useMinWeekWorkTime: true);
			Assert.IsTrue(personRequest.BrokenBusinessRules.Value.HasFlag(BusinessRuleFlags.MinWeekWorkTimeRule));
		}

		[Test]
		public void ShouldSetSiteOpenHoursBrokenRule()
		{
			var businessRuleProvider = new BusinessRuleProvider();
			var personRequest = doShiftTradeWithBrokenRules(businessRuleProvider);
			Assert.IsTrue(personRequest.BrokenBusinessRules.Value.HasFlag(BusinessRuleFlags.SiteOpenHoursRule));
		}

		[Test]
		public void ShouldReturnDenyReasonForTheFirstDeniableBrokenRule()
		{
			var businessRuleProvider = getConfigurableBusinessRuleProvider(
				new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = typeof(MinWeekWorkTimeRule).FullName,
					Enabled = true,
					HandleOptionOnFailed = RequestHandleOption.Pending
				},
				new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = typeof(SiteOpenHoursRule).FullName,
					Enabled = true,
					HandleOptionOnFailed = RequestHandleOption.AutoDeny
				}
			);
			var personRequest = doShiftTradeWithBrokenRules(businessRuleProvider, useMinWeekWorkTime: true);
			Assert.IsTrue(personRequest.IsDenied);
			Assert.IsTrue(personRequest.DenyReason.Contains("No open hours for"), personRequest.DenyReason);
			Assert.IsTrue(personRequest.GetMessage(new NoFormatting()).Contains("The week contains too little work time"));
		}

		[Test]
		public void ShouldDenyWhenDeniableRuleIsBroken()
		{
			var businessRuleProvider = getConfigurableBusinessRuleProvider(
				new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = typeof(MinWeekWorkTimeRule).FullName,
					Enabled = true,
					HandleOptionOnFailed = RequestHandleOption.AutoDeny
				}
			);
			var personRequest = doShiftTradeWithBrokenRules(businessRuleProvider, useMinWeekWorkTime: true);
			Assert.IsTrue(personRequest.IsDenied);
		}

		[Test]
		public void ShouldNotValidateDisabledRule()
		{
			var businessRuleProvider = getConfigurableBusinessRuleProvider(
				new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = typeof(SiteOpenHoursRule).FullName,
					Enabled = false,
					HandleOptionOnFailed = RequestHandleOption.AutoDeny
				}
			);
			var personRequest = doShiftTradeWithBrokenRules(businessRuleProvider);
			Assert.IsTrue(personRequest.IsApproved);
		}

		[Test]
		public void ShouldNotDenyWithAutoGrantOff()
		{
			var businessRuleProvider = getConfigurableBusinessRuleProvider(
				new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = typeof(SiteOpenHoursRule).FullName,
					Enabled = true,
					HandleOptionOnFailed = RequestHandleOption.AutoDeny
				}
				);
			var personRequest = doShiftTradeWithBrokenRules(businessRuleProvider, autoGrantShiftTrade: false);
			Assert.IsTrue(personRequest.IsPending);
			Assert.IsTrue(personRequest.BrokenBusinessRules.Value.HasFlag(BusinessRuleFlags.SiteOpenHoursRule));
		}

		private static IBusinessRuleProvider getConfigurableBusinessRuleProvider(params ShiftTradeBusinessRuleConfig[] businessRuleConfigs)
		{
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			var shiftTradeSettings = new ShiftTradeSettings
			{
				BusinessRuleConfigs = businessRuleConfigs
			};
			globalSettingDataRepository.PersistSettingValue(ShiftTradeSettings.SettingsKey, shiftTradeSettings);
			var businessRuleProvider = new ConfigurableBusinessRuleProvider(globalSettingDataRepository);
			return businessRuleProvider;
		}

		private basicShiftTradeTestResult doBasicShiftTrade(IWorkflowControlSet workflowControlSet, bool addBrokenBusinessRules = false)
		{
			var personTo = PersonFactory.CreatePerson("To").WithId();
			var personFrom = PersonFactory.CreatePerson("With").WithId();

			var activityPersonTo = new Activity("Shift_PersonTo").WithId();
			var activityPersonFrom = new Activity("Shift_PersonFrom").WithId();

			var dateTimePeriod = new DateTimePeriod(DateTime.Today.AddHours(8).ToUniversalTime(), DateTime.Today.AddHours(8).ToUniversalTime());

			_shiftTradeTestHelper.AddPersonAssignment(personTo, dateTimePeriod, activityPersonTo);
			_shiftTradeTestHelper.AddPersonAssignment(personFrom, dateTimePeriod, activityPersonFrom);

			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom }, null, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), _currentScenario.Current());

			personTo.WorkflowControlSet = workflowControlSet;
			personFrom.WorkflowControlSet = workflowControlSet;

			if (addBrokenBusinessRules)
			{
				var ruleResponse1 = new BusinessRuleResponse(typeof(MinWeeklyRestRule), "no go", true, false, new DateTimePeriod(),
					personTo, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), "tjillevippen");
				var ruleResponse2 = new BusinessRuleResponse(typeof(NewMaxWeekWorkTimeRule), "no go", true, false,
					new DateTimePeriod(), personTo, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), "tjillevippen");
				prepareBusinessRuleProvider(ruleResponse1, ruleResponse2);
			}
			else
			{
				prepareBusinessRuleProvider();
			}

			_personRepository.Add(personTo);

			var personRequest = _shiftTradeTestHelper.PrepareAndGetPersonRequest(personFrom, personTo, DateOnly.Today);

			_shiftTradeTestHelper.SetScheduleDictionary(scheduleDictionary);

			_shiftTradeTestHelper.HandleRequest(_shiftTradeTestHelper.GetAcceptShiftTradeEvent(personTo, personRequest.Id.Value));

			return new basicShiftTradeTestResult
			{
				ActivityTo = activityPersonTo,
				ActivityFrom = activityPersonFrom,
				PersonToSchedule = scheduleDictionary[personTo].ScheduledDay(DateOnly.Today),
				PersonFromSchedule = scheduleDictionary[personFrom].ScheduledDay(DateOnly.Today),
				PersonRequest = personRequest
			};
		}

		private IPersonRequest doShiftTradeWithBrokenRules(IBusinessRuleProvider businessRuleProvider, bool useMinWeekWorkTime = false, bool autoGrantShiftTrade = true)
		{
			var scheduleDate = new DateTime(2016, 7, 25);
			var scheduleDateOnly = new DateOnly(scheduleDate);

			var personTo = createPerson(autoGrantShiftTrade);
			_shiftTradeTestHelper.SetSiteOpenHours(personTo, 8, 17);
			setMinTimePerWeek(personTo, scheduleDateOnly);
			var activityPersonTo = new Activity("Shift_PersonTo").WithId();

			var personFrom = createPerson(autoGrantShiftTrade);
			_shiftTradeTestHelper.SetSiteOpenHours(personFrom, 8, 17);
			setMinTimePerWeek(personFrom, scheduleDateOnly);
			var activityPersonFrom = new Activity("Shift_PersonFrom").WithId();

			for (var i = 0; i < 7; i++)
			{
				var dateTimePeriod = new DateTimePeriod(scheduleDate.AddDays(i).AddHours(8).ToUniversalTime(),
					scheduleDate.AddDays(i).AddHours(10).ToUniversalTime());
				_shiftTradeTestHelper.AddPersonAssignment(personTo, dateTimePeriod, activityPersonTo);
				_shiftTradeTestHelper.AddPersonAssignment(personFrom, dateTimePeriod, activityPersonFrom);
			}

			var personRequest = _shiftTradeTestHelper.PrepareAndGetPersonRequest(personFrom, personTo, scheduleDateOnly);

			_shiftTradeTestHelper.SetPersonAccounts(personTo, personFrom, scheduleDateOnly);

			var @event = _shiftTradeTestHelper.GetAcceptShiftTradeEvent(personTo, personRequest.Id.GetValueOrDefault());
			@event.UseSiteOpenHoursRule = true;
			@event.UseMinWeekWorkTime = useMinWeekWorkTime;
			_schedulingResultStateHolder.UseMinWeekWorkTime = useMinWeekWorkTime;

			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom }, null,
				new DateOnlyPeriod(new DateOnly(scheduleDate), new DateOnly(scheduleDate.AddDays(7))), _currentScenario.Current());
			_shiftTradeTestHelper.SetScheduleDictionary(scheduleDictionary);

			_shiftTradeTestHelper.HandleRequest(@event, businessRuleProvider);
			return personRequest;
		}

		private void prepareBusinessRuleProvider(params IBusinessRuleResponse[] ruleResponses)
		{
			((FakeNewBusinessRuleCollection)_businessRuleCollection).SetRuleResponse(ruleResponses);
			((FakeBusinessRuleProvider)_businessRuleProvider).SetBusinessRules(_businessRuleCollection);
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

		private void setMinTimePerWeek(IPerson person, DateOnly scheduleDateOnly)
		{
			var minTimePerWeek = TimeSpan.FromHours(40);
			person.Period(scheduleDateOnly).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(minTimePerWeek,
				TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);
		}

		private class basicShiftTradeTestResult
		{
			public IScheduleDay PersonFromSchedule { get; set; }
			public IScheduleDay PersonToSchedule { get; set; }
			public IPersonRequest PersonRequest { get; set; }
			public IActivity ActivityTo { get; set; }
			public IActivity ActivityFrom { get; set; }
		}
	}
}