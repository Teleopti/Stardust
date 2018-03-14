using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	[TestWithStaticDependenciesDONOTUSE]
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
			_currentScenario = new FakeCurrentScenario_DoNotUse();
			_schedulingResultStateHolder = new SchedulingResultStateHolder();
			_personRepository = new FakePersonRepositoryLegacy2();
			_scheduleStorage = new FakeScheduleStorage_DoNotUse();
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
			var personRequest = doShiftTradeWithBrokenRules(businessRuleProvider, true);
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

		[Test]
		public void ShouldDenyOtherShiftTradeRequestsWhenShiftExchangeOfferIsCompleted()
		{
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
		public void ShouldApprovePendingRequestAfterNightlyRuleIsSatisfied()
		{
			var workflowControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(true);

			setPermissions();

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
				null, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), _currentScenario.Current());

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

		private IScheduleDay createScheduleDay(DateOnly date, DateTimePeriod period, IPerson agent, IActivity activity)
		{
			var scheduleDay = ScheduleDayFactory.Create(date, agent, _currentScenario.Current());
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
			setPermissions();

			var personTo = PersonFactory.CreatePerson("To").WithId();
			var personFrom = PersonFactory.CreatePerson("With").WithId();

			var activityPersonTo = new Activity("Shift_PersonTo").WithId();
			var activityPersonFrom = new Activity("Shift_PersonFrom").WithId();

			var dateTimePeriod = new DateTimePeriod(DateTime.Today.AddHours(8).ToUniversalTime(), DateTime.Today.AddHours(9).ToUniversalTime());

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

		private static void setPermissions()
		{
			var principal = Thread.CurrentPrincipal as ITeleoptiPrincipal;
			var claims = new List<Claim>
			{
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.ViewSchedules)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(
					string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
						"/AvailableData"), new AuthorizeEveryone(), Rights.PossessProperty)
			};
			principal.AddClaimSet(new DefaultClaimSet(ClaimSet.System, claims));
		}

		private IPersonRequest doShiftTradeWithBrokenRules(IBusinessRuleProvider businessRuleProvider
			, bool useMinWeekWorkTime = false, bool autoGrantShiftTrade = true,
			IShiftExchangeOffer offer = null)
		{
			setPermissions();

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