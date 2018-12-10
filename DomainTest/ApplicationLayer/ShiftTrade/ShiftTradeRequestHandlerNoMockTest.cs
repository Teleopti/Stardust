using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	[DomainTest]
	public class ShiftTradeRequestHandlerNoMockTest : IIsolateSystem
	{
		public FakeScenarioRepository _currentScenario;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public ISchedulingResultStateHolder _schedulingResultStateHolder;
		public IPersonRepository _personRepository;
		public IScheduleStorage _scheduleStorage;
		public IBusinessRuleProvider _businessRuleProvider;
		public ShiftTradeTestHelper _shiftTradeTestHelper;
		
		[Test]
		public void ShouldTradeShiftsOnAutoApproval()
		{
			_currentScenario.Has("Default");
			
			var workflowControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(true);
			var result = doBasicShiftTrade(workflowControlSet);

			Assert.IsTrue(result.PersonRequest.IsApproved);
			Assert.IsTrue(result.PersonToSchedule.PersonAssignment().ShiftLayers.Single().Payload.Id == result.ActivityFrom.Id);
			Assert.IsTrue(result.PersonFromSchedule.PersonAssignment().ShiftLayers.Single().Payload.Id == result.ActivityTo.Id);
		}

		[Test]
		public void ShouldSetMinWeeklyWorkTimeBrokenRuleWhenUseMinWeekWorkTimeIsOn()
		{
			_currentScenario.Has("Default");

			var businessRuleProvider = new BusinessRuleProvider();
			var personRequest = doShiftTradeWithBrokenRules(businessRuleProvider);
			Assert.IsTrue(personRequest.BrokenBusinessRules.Value.HasFlag(BusinessRuleFlags.MinWeekWorkTimeRule));
		}

		[Test]
		public void ShouldSetSiteOpenHoursBrokenRule()
		{
			_currentScenario.Has("Default");
			
			var businessRuleProvider = new BusinessRuleProvider();
			var personRequest = doShiftTradeWithBrokenRules(businessRuleProvider);
			Assert.IsTrue(personRequest.BrokenBusinessRules.Value.HasFlag(BusinessRuleFlags.SiteOpenHoursRule));
		}

		[Test]
		public void ShouldReturnDenyReasonForTheFirstDeniableBrokenRule()
		{
			_currentScenario.Has("Default");
			
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
			var personRequest = doShiftTradeWithBrokenRules(businessRuleProvider);
			Assert.IsTrue(personRequest.IsDenied);
			Assert.IsTrue(personRequest.DenyReason.Contains("No open hours for"), personRequest.DenyReason);
			Assert.IsTrue(personRequest.GetMessage(new NoFormatting()).Contains("The week contains too little work time"));
		}

		[Test]
		public void ShouldDenyWhenDeniableRuleIsBroken()
		{
			_currentScenario.Has("Default");
			
			var businessRuleProvider = getConfigurableBusinessRuleProvider(
				new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = typeof(MinWeekWorkTimeRule).FullName,
					Enabled = true,
					HandleOptionOnFailed = RequestHandleOption.AutoDeny
				}
			);
			var personRequest = doShiftTradeWithBrokenRules(businessRuleProvider);
			Assert.IsTrue(personRequest.IsDenied);
		}

		[Test]
		public void ShouldNotValidateDisabledRule()
		{
			_currentScenario.Has("Default");

			var businessRuleProvider = getConfigurableBusinessRuleProvider(
				new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = typeof(SiteOpenHoursRule).FullName,
					Enabled = false,
					HandleOptionOnFailed = RequestHandleOption.AutoDeny
				},				
				new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = typeof(MinWeekWorkTimeRule).FullName,
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
			_currentScenario.Has("Default");
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

		[Test]
		public void ShouldDenyOtherShiftTradeRequestsWhenShiftExchangeOfferIsCompleted()
		{
			_currentScenario.Has("Default");
			var shiftDate = new DateOnly(2007, 1, 1);
			var agent1 = PersonFactory.CreatePersonWithId();
			_personRepository.AddRange(new[]
			{
				agent1
			});

			var period = new DateTimePeriod(new DateTime(2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime(2007, 01, 01, 15, 0, 0, DateTimeKind.Utc));
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var agent1Shift = createScheduleDay(shiftDate, period.ChangeEndTime(TimeSpan.FromHours(3)), agent1, activity);
			var shiftTradeOffer =
				new ShiftExchangeOffer(agent1Shift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending)
				{
					Criteria = new ShiftExchangeCriteria(DateOnly.Today.AddDays(1),
						new ScheduleDayFilterCriteria
						{
							DayType = ShiftExchangeLookingForDay.DayOffOrEmptyDay
						}),
					ShiftExchangeOfferId = Guid.NewGuid().ToString()
				}.WithId();

			var workflowControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(false);

			var result1 = doBasicShiftTrade(workflowControlSet, offer: shiftTradeOffer);

			workflowControlSet.AutoGrantShiftTradeRequest = true;
			var result2 = doBasicShiftTrade(workflowControlSet, offer: shiftTradeOffer);

			Assert.IsTrue(result2.PersonRequest.IsApproved);
			Assert.IsTrue(result1.PersonRequest.IsDenied);
			result1.PersonRequest.DenyReason.Should().Be(nameof(Resources.ShiftTradeRequestForExchangeOfferHasBeenCompleted));
		}

		[Test]
		public void ShouldNotDenyOtherShiftTradeRequestsWhenShiftExchangeOfferIsCompletedWithAutoGrantOff()
		{
			_currentScenario.Has("Default");
			var shiftDate = new DateOnly(2007, 1, 1);
			var agent1 = PersonFactory.CreatePersonWithId();
			_personRepository.AddRange(new[]
			{
				agent1
			});

			var period = new DateTimePeriod(new DateTime(2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime(2007, 01, 01, 15, 0, 0, DateTimeKind.Utc));
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var agent1Shift = createScheduleDay(shiftDate, period.ChangeEndTime(TimeSpan.FromHours(3)), agent1, activity);
			var shiftTradeOffer =
				new ShiftExchangeOffer(agent1Shift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending)
				{
					Criteria = new ShiftExchangeCriteria(DateOnly.Today.AddDays(1),
						new ScheduleDayFilterCriteria
						{
							DayType = ShiftExchangeLookingForDay.DayOffOrEmptyDay
						}),
					ShiftExchangeOfferId = Guid.NewGuid().ToString()
				}.WithId();

			var workflowControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(false);

			var result1 = doBasicShiftTrade(workflowControlSet, offer: shiftTradeOffer);
			var result2 = doBasicShiftTrade(workflowControlSet, offer: shiftTradeOffer);

			Assert.IsTrue(result2.PersonRequest.IsPending);
			Assert.IsTrue(result1.PersonRequest.IsPending);
		}

		[Test]
		public void ShouldNotDenyOtherShiftTradeRequestsWhenShiftTradeRequestIsInvalid()
		{
			_currentScenario.Has("Default");
			var shiftDate = new DateOnly(2007, 1, 1);
			var agent1 = PersonFactory.CreatePersonWithId();
			_personRepository.AddRange(new[]
			{
				agent1
			});

			var period = new DateTimePeriod(new DateTime(2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime(2007, 01, 01, 15, 0, 0, DateTimeKind.Utc));
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var agent1Shift = createScheduleDay(shiftDate, period.ChangeEndTime(TimeSpan.FromHours(3)), agent1, activity);
			var shiftTradeOffer =
				new ShiftExchangeOffer(agent1Shift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending)
				{
					Criteria = new ShiftExchangeCriteria(DateOnly.Today.AddDays(1),
						new ScheduleDayFilterCriteria
						{
							DayType = ShiftExchangeLookingForDay.DayOffOrEmptyDay
						}),
					ShiftExchangeOfferId = Guid.NewGuid().ToString()
				}.WithId();

			var workflowControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(false);
			var result1 = doBasicShiftTrade(workflowControlSet, offer: shiftTradeOffer);

			var invalidPersonRequest = doShiftTradeWithBrokenRules(new BusinessRuleProvider(), offer: shiftTradeOffer);

			Assert.IsTrue(invalidPersonRequest.IsPending);
			Assert.IsTrue(result1.PersonRequest.IsPending);
		}

		[Test]
		public void ShouldNotDenyOtherShiftTradeRequestsWhenShiftTradeRequestIsDenied()
		{
			_currentScenario.Has("Default");
			var shiftDate = new DateOnly(2007, 1, 1);
			var agent1 = PersonFactory.CreatePersonWithId();
			_personRepository.AddRange(new[]
			{
				agent1
			});

			var period = new DateTimePeriod(new DateTime(2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime(2007, 01, 01, 15, 0, 0, DateTimeKind.Utc));
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var agent1Shift = createScheduleDay(shiftDate, period.ChangeEndTime(TimeSpan.FromHours(3)), agent1, activity);
			var shiftTradeOffer =
				new ShiftExchangeOffer(agent1Shift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending)
				{
					Criteria = new ShiftExchangeCriteria(DateOnly.Today.AddDays(1),
						new ScheduleDayFilterCriteria
						{
							DayType = ShiftExchangeLookingForDay.DayOffOrEmptyDay
						}),
					ShiftExchangeOfferId = Guid.NewGuid().ToString()
				}.WithId();

			var workflowControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(true);
			var shiftTradeRequest1 = doShiftTradeWithBrokenRules(new BusinessRuleProvider(), true, shiftTradeOffer);
			shiftTradeRequest1.Deny("test", new PersonRequestAuthorizationCheckerConfigurable());

			var shiftTradeRequest2 = doBasicShiftTrade(workflowControlSet, offer: shiftTradeOffer).PersonRequest;

			Assert.IsTrue(shiftTradeRequest1.IsDenied);
			Assert.IsTrue(shiftTradeRequest2.IsApproved);
		}

		[Test]
		public void ShouldApprovePendingRequestAfterNightlyRuleIsSatisfied()
		{
			_currentScenario.Has("Default");
			var workflowControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(true);
			
			var personTo = PersonFactory.CreatePerson("To").WithId();
			var personFrom = PersonFactory.CreatePerson("With").WithId();
			_personRepository.Add(personTo);
			_personRepository.Add(personFrom);

			var dateTimePeriod = new DateTimePeriod(DateTime.Today.AddHours(8).ToUniversalTime(),
				DateTime.Today.AddHours(9).ToUniversalTime());

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(dateTimePeriod.StartDateTime.Date));
			personTo.AddPersonPeriod(personPeriod);
			personFrom.AddPersonPeriod(personPeriod);

			var contract = ContractFactory.CreateContract("test contract");
			personTo.PersonPeriodCollection.First().PersonContract.Contract = contract;
			personFrom.PersonPeriodCollection.First().PersonContract.Contract = contract;

			var activityPersonTo = new Activity("Shift_PersonTo").WithId();
			var activityPersonFrom = new Activity("Shift_PersonFrom").WithId();

			_shiftTradeTestHelper.AddPersonAssignment(personTo, dateTimePeriod, activityPersonTo);
			_shiftTradeTestHelper.AddPersonAssignment(personFrom, dateTimePeriod, activityPersonFrom);

			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] {personTo, personFrom},
				new ScheduleDictionaryLoadOptions(false,false), new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), _currentScenario.LoadDefaultScenario());
			((ReadOnlyScheduleDictionary)scheduleDictionary).MakeEditable();

			personTo.WorkflowControlSet = workflowControlSet;
			personFrom.WorkflowControlSet = workflowControlSet;

			var businessRuleProvider = getConfigurableBusinessRuleProvider(
				new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = typeof(NightlyRestRule).FullName,
					Enabled = true,
					HandleOptionOnFailed = RequestHandleOption.Pending
				}
			);

			var personRequest = _shiftTradeTestHelper.PrepareAndGetPersonRequest(personFrom, personTo, DateOnly.Today);
			personRequest.ForcePending();
			(personRequest.Request as IShiftTradeRequest)?.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts,
				new PersonRequestCheckAuthorization());

			_shiftTradeTestHelper.SetScheduleDictionary(scheduleDictionary);

			_shiftTradeTestHelper.HandleRequest(
				_shiftTradeTestHelper.GetAcceptShiftTradeEvent(personTo, personRequest.Id.GetValueOrDefault()),
				businessRuleProvider);

			Assert.IsTrue(personRequest.IsApproved);
			Assert.IsTrue(scheduleDictionary[personTo].ScheduledDay(DateOnly.Today).PersonAssignment().ShiftLayers.Single()
							  .Payload.Id == activityPersonFrom.Id);
			Assert.IsTrue(scheduleDictionary[personFrom].ScheduledDay(DateOnly.Today).PersonAssignment().ShiftLayers.Single()
							  .Payload.Id == activityPersonTo.Id);
		}

		[Test]
		public void ShouldNotRemoveCrossNightAbsenceWhenShifTradeTargetPersonHasFulldayAbsence()
		{
			_currentScenario.Has("Default");
			
			var personFrom = PersonFactory.CreatePerson("From").WithId();
			var personTo = PersonFactory.CreatePerson("To").WithId();

			var workflowControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(true);
			personTo.WorkflowControlSet = workflowControlSet;
			personFrom.WorkflowControlSet = workflowControlSet;

			personFrom.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personTo.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			_personRepository.Add(personFrom);
			_personRepository.Add(personTo);

			var todayOvernightPeriod = new DateTimePeriod(new DateTime(2018, 07, 19, 21, 0, 0, DateTimeKind.Utc), new DateTime(2018, 07, 20, 06, 0, 0, DateTimeKind.Utc));

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(todayOvernightPeriod.StartDateTime.Date.AddDays(-1)));
			personTo.AddPersonPeriod(personPeriod);
			personFrom.AddPersonPeriod(personPeriod);

			var contract = ContractFactory.CreateContract("test contract");
			personTo.PersonPeriodCollection.First().PersonContract.Contract = contract;
			personFrom.PersonPeriodCollection.First().PersonContract.Contract = contract;

			var scenario = _currentScenario.LoadDefaultScenario();
			var assPersonFrom = PersonAssignmentFactory
				.CreateAssignmentWithMainShift(personFrom, scenario, new Activity("Shift_PersonFrom"), todayOvernightPeriod, new ShiftCategory("LT")).WithId();
			PersonAssignmentRepository.Add(assPersonFrom);

			var yesterdayOvernightPeriod = new DateTimePeriod(new DateTime(2018, 07, 18, 21, 0, 0, DateTimeKind.Utc), new DateTime(2018, 07, 19, 6, 0, 0, DateTimeKind.Utc));
			var ass2PersonFrom = PersonAssignmentFactory
				.CreateAssignmentWithMainShift(personFrom, scenario, new Activity("Shift_PersonFrom_Yesterday"), yesterdayOvernightPeriod, new ShiftCategory("LT")).WithId();
			PersonAssignmentRepository.Add(ass2PersonFrom);

			var fullDayAbsence = PersonAbsenceFactory.CreatePersonAbsence(personFrom, scenario,
				yesterdayOvernightPeriod, AbsenceFactory.CreateAbsence("fromPersonFullDayAbsence")).WithId();
			PersonAbsenceRepository.Add(fullDayAbsence);

			var assPersonTo = PersonAssignmentFactory.CreateAssignmentWithDayOff(personTo, scenario, new DateOnly(2018, 07, 19),
				new DayOffTemplate(new Description("DayOff_PersonTo")));
			PersonAssignmentRepository.Add(assPersonTo);

			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom },
				new ScheduleDictionaryLoadOptions(false,false), new DateOnlyPeriod(new DateOnly(2018, 07, 18), new DateOnly(2018, 07, 20)), scenario);
			((ReadOnlyScheduleDictionary)scheduleDictionary).MakeEditable();

			var shiftTradeRequest = _shiftTradeTestHelper.PrepareAndGetPersonRequest(personFrom, personTo, new DateOnly(2018, 07, 19));
			shiftTradeRequest.ForcePending();
			(shiftTradeRequest.Request as IShiftTradeRequest)?.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts,
				new PersonRequestCheckAuthorization());

			_shiftTradeTestHelper.SetScheduleDictionary(scheduleDictionary);

			_shiftTradeTestHelper.HandleRequest(
				_shiftTradeTestHelper.GetAcceptShiftTradeEvent(personTo, shiftTradeRequest.Id.GetValueOrDefault()),
				getConfigurableBusinessRuleProvider());

			Assert.IsTrue(shiftTradeRequest.IsApproved);

			var overnightAbsence = scheduleDictionary[personFrom].ScheduledDay(new DateOnly(2018, 07, 18)).PersonAbsenceCollection()
				.First();

			Assert.IsTrue(overnightAbsence.Id == fullDayAbsence.Id);
			Assert.IsTrue(overnightAbsence.Period.StartDateTime == yesterdayOvernightPeriod.StartDateTime);
			Assert.IsTrue(overnightAbsence.Period.EndDateTime == yesterdayOvernightPeriod.EndDateTime);
		}

		[Test]
		public void ShouldNotRemoveCrossNightAbsenceWhenShifTradeTargetPersonHasFulldayAbsenceInNewYorkTimeZone()
		{
			_currentScenario.Has("Default");
			
			var personFrom = PersonFactory.CreatePerson("From").WithId();
			var personTo = PersonFactory.CreatePerson("To").WithId();

			var workflowControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(true);
			personTo.WorkflowControlSet = workflowControlSet;
			personFrom.WorkflowControlSet = workflowControlSet;

			var timeZone = TimeZoneInfoFactory.NewYorkTimeZoneInfo();
			personFrom.PermissionInformation.SetDefaultTimeZone(timeZone);
			personTo.PermissionInformation.SetDefaultTimeZone(timeZone);
			_personRepository.Add(personFrom);
			_personRepository.Add(personTo);

			var todayOvernightPeriod = new DateTimePeriod(new DateTime(2018, 07, 20, 1, 0, 0, DateTimeKind.Utc), new DateTime(2018, 07, 20, 10, 0, 0, DateTimeKind.Utc));

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(todayOvernightPeriod.StartDateTime.Date.AddDays(-1)));
			personTo.AddPersonPeriod(personPeriod);
			personFrom.AddPersonPeriod(personPeriod);

			var contract = ContractFactory.CreateContract("test contract");
			personTo.PersonPeriodCollection.First().PersonContract.Contract = contract;
			personFrom.PersonPeriodCollection.First().PersonContract.Contract = contract;

			var scenario = _currentScenario.LoadDefaultScenario();
			var assPersonFrom = PersonAssignmentFactory
				.CreateAssignmentWithMainShift(personFrom, scenario, new Activity("Shift_PersonFrom"), todayOvernightPeriod, new ShiftCategory("LT")).WithId();
			PersonAssignmentRepository.Add(assPersonFrom);

			var yesterdayOvernightPeriod = new DateTimePeriod(new DateTime(2018, 07, 19, 1, 0, 0, DateTimeKind.Utc), new DateTime(2018, 07, 19, 10, 0, 0, DateTimeKind.Utc));
			var ass2PersonFrom = PersonAssignmentFactory
				.CreateAssignmentWithMainShift(personFrom, scenario, new Activity("Shift_PersonFrom_Yesterday"), yesterdayOvernightPeriod, new ShiftCategory("LT")).WithId();
			PersonAssignmentRepository.Add(ass2PersonFrom);

			var fullDayAbsence = PersonAbsenceFactory.CreatePersonAbsence(personFrom, scenario,
				yesterdayOvernightPeriod, AbsenceFactory.CreateAbsence("fromPersonFullDayAbsence")).WithId();
			PersonAbsenceRepository.Add(fullDayAbsence);

			var assPersonTo = PersonAssignmentFactory.CreateAssignmentWithDayOff(personTo, scenario, new DateOnly(2018, 07, 19),
				new DayOffTemplate(new Description("DayOff_PersonTo")));
			PersonAssignmentRepository.Add(assPersonTo);

			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom },
				new ScheduleDictionaryLoadOptions(false,false), new DateOnlyPeriod(new DateOnly(2018, 07, 18), new DateOnly(2018, 07, 20)), scenario);
			((ReadOnlyScheduleDictionary)scheduleDictionary).MakeEditable();

			var shiftTradeRequest = _shiftTradeTestHelper.PrepareAndGetPersonRequest(personFrom, personTo, new DateOnly(2018, 07, 19));
			shiftTradeRequest.ForcePending();
			(shiftTradeRequest.Request as IShiftTradeRequest)?.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts,
				new PersonRequestCheckAuthorization());

			_shiftTradeTestHelper.SetScheduleDictionary(scheduleDictionary);

			_shiftTradeTestHelper.HandleRequest(
				_shiftTradeTestHelper.GetAcceptShiftTradeEvent(personTo, shiftTradeRequest.Id.GetValueOrDefault()),
				getConfigurableBusinessRuleProvider());

			Assert.IsTrue(shiftTradeRequest.IsApproved);

			var overnightAbsence = scheduleDictionary[personFrom].ScheduledDay(new DateOnly(2018, 07, 18)).PersonAbsenceCollection()
				.First();

			Assert.IsTrue(overnightAbsence.Id == fullDayAbsence.Id);
			Assert.IsTrue(overnightAbsence.Period.StartDateTime == yesterdayOvernightPeriod.StartDateTime);
			Assert.IsTrue(overnightAbsence.Period.EndDateTime == yesterdayOvernightPeriod.EndDateTime);
		}

		private IScheduleDay createScheduleDay(DateOnly date, DateTimePeriod period, IPerson agent, IActivity activity)
		{
			var scheduleDay = ScheduleDayFactory.Create(date, agent, _currentScenario.LoadDefaultScenario());
			scheduleDay.AddMainShift(EditableShiftFactory.CreateEditorShift(activity,
				period,
				ShiftCategoryFactory.CreateShiftCategory("Early")));
			return scheduleDay;
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

		private basicShiftTradeTestResult doBasicShiftTrade(IWorkflowControlSet workflowControlSet, bool addBrokenBusinessRules = false, IShiftExchangeOffer offer = null)
		{
			var personTo = PersonFactory.CreatePerson("To").WithId();
			var personFrom = PersonFactory.CreatePerson("With").WithId();

			var activityPersonTo = new Activity("Shift_PersonTo").WithId();
			var activityPersonFrom = new Activity("Shift_PersonFrom").WithId();

			var dateTimePeriod = new DateTimePeriod(DateTime.Today.AddHours(8).ToUniversalTime(), DateTime.Today.AddHours(9).ToUniversalTime());

			_shiftTradeTestHelper.AddPersonAssignment(personTo, dateTimePeriod, activityPersonTo);
			_shiftTradeTestHelper.AddPersonAssignment(personFrom, dateTimePeriod, activityPersonFrom);

			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom }, new ScheduleDictionaryLoadOptions(false,false), DateOnly.Today.ToDateOnlyPeriod(), _currentScenario.LoadDefaultScenario());
			((ReadOnlyScheduleDictionary)scheduleDictionary).MakeEditable();
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
			((ShiftTradeRequest)personRequest.Request).Offer = offer;

			_shiftTradeTestHelper.SetScheduleDictionary(scheduleDictionary);

			_shiftTradeTestHelper.HandleRequest(_shiftTradeTestHelper.GetAcceptShiftTradeEvent(personTo, personRequest.Id.GetValueOrDefault()));

			return new basicShiftTradeTestResult
			{
				ActivityTo = activityPersonTo,
				ActivityFrom = activityPersonFrom,
				PersonToSchedule = scheduleDictionary[personTo].ScheduledDay(DateOnly.Today),
				PersonFromSchedule = scheduleDictionary[personFrom].ScheduledDay(DateOnly.Today),
				PersonRequest = personRequest
			};
		}
		
		private IPersonRequest doShiftTradeWithBrokenRules(IBusinessRuleProvider businessRuleProvider, bool autoGrantShiftTrade = true, 
			IShiftExchangeOffer offer = null, bool useMaximumWorkday = false)
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
			((ShiftTradeRequest) personRequest.Request).Offer = offer;

			_shiftTradeTestHelper.SetPersonAccounts(personTo, personFrom, scheduleDateOnly);

			var @event = _shiftTradeTestHelper.GetAcceptShiftTradeEvent(personTo, personRequest.Id.GetValueOrDefault());
			@event.UseSiteOpenHoursRule = true;
			@event.UseMaximumWorkday = useMaximumWorkday;
			_schedulingResultStateHolder.UseMaximumWorkday = useMaximumWorkday;

			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom }, new ScheduleDictionaryLoadOptions(false,false), 
				new DateOnlyPeriod(new DateOnly(scheduleDate), new DateOnly(scheduleDate.AddDays(7))), _currentScenario.LoadDefaultScenario());
			((ReadOnlyScheduleDictionary)scheduleDictionary).MakeEditable();
			_shiftTradeTestHelper.SetScheduleDictionary(scheduleDictionary);

			_shiftTradeTestHelper.HandleRequest(@event, businessRuleProvider);
			return personRequest;
		}

		private void prepareBusinessRuleProvider(params IBusinessRuleResponse[] ruleResponses)
		{
			var businessRuleCollection = new FakeNewBusinessRuleCollection();
			businessRuleCollection.SetRuleResponse(ruleResponses);
			((FakeBusinessRuleProvider)_businessRuleProvider).SetBusinessRules(businessRuleCollection);
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

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ShiftTradeTestHelper>().For<ShiftTradeTestHelper>();
			isolate.UseTestDouble<FakeBusinessRuleProvider>().For<IBusinessRuleProvider>();
			isolate.UseTestDouble<FakeScheduleProjectionReadOnlyActivityProvider>()
				.For<IScheduleProjectionReadOnlyActivityProvider>();
		}
	}
}