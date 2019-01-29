using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	[DomainTest]
	public class ShiftTradeTargetTimeSpecificationTests : IIsolateSystem
	{
		public FakeScenarioRepository CurrentScenario;
		public ISchedulingResultStateHolder SchedulingResultStateHolder;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonRequestRepository PersonRequestRepository;
		public IScheduleStorage ScheduleStorage;
		public IBusinessRuleProvider BusinessRuleProvider;
		public FakeGlobalSettingDataRepository GlobalSettingDataRepository;
		public ITimeZoneGuard TimeZoneGuard;
		public ShiftTradeTestHelper ShiftTradeTestHelper;

		[Test]
		public void ShouldDenyWhenSpecificationIsNotConfigured()
		{
			CurrentScenario.Has("Default");

			var personRequest = createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(new[] { new ShiftTradeBusinessRuleConfig()});
			Assert.IsTrue(personRequest.IsDenied);
			Assert.IsTrue(personRequest.DenyReason == "ShiftTradeTargetTimeDenyReason");
		}

		[Test]
		public void ShouldNotValidateWhenSpecificationIsConfiguredAsDisabled()
		{
			CurrentScenario.Has("Default");
			var shiftTradeBusinessRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(ShiftTradeTargetTimeSpecification).FullName,
				Enabled = false,
				HandleOptionOnFailed = RequestHandleOption.AutoDeny
			};
			var personRequest = createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(new[] { shiftTradeBusinessRuleConfig});
			Assert.IsTrue(personRequest.IsPending);

			acceptShiftTradeWithShiftTradeTargetTimeSpecificationBroken(personRequest);
			Assert.IsTrue(personRequest.IsApproved);
		}

		[Test]
		public void ShouldDenyWhenSpecificationIsConfiguredAsDeny()
		{
			CurrentScenario.Has("Default");
			var shiftTradeBusinessRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(ShiftTradeTargetTimeSpecification).FullName,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.AutoDeny
			};
			var personRequest = createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(new[] { shiftTradeBusinessRuleConfig});
			Assert.IsTrue(personRequest.IsDenied);
			Assert.IsTrue(personRequest.DenyReason == "ShiftTradeTargetTimeDenyReason");
		}

		[Test, SetCulture("en-US")]
		public void ShouldPendingWhenSpecificationConfiguredAsPending()
		{
			CurrentScenario.Has("Default");
			var shiftTradeBusinessRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(ShiftTradeTargetTimeSpecification).FullName,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.Pending
			};
			var personRequest = createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(new[] {shiftTradeBusinessRuleConfig});
			Assert.IsTrue(personRequest.IsPending);

			acceptShiftTradeWithShiftTradeTargetTimeSpecificationBroken(personRequest);
			Assert.IsTrue(personRequest.IsPending);

			var denyReason = Resources.ResourceManager.GetString("ShiftTradeTargetTimePendingReason");
			Assert.IsTrue(personRequest.GetMessage(new NoFormatting()).Contains(denyReason));
		}

		[Test, SetCulture("en-US")]
		public void ShouldSetBrokenRulesWhenSpecificationConfiguredAsPending()
		{
			CurrentScenario.Has("Default");
			var shiftTradeBusinessRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(ShiftTradeTargetTimeSpecification).FullName,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.Pending
			};
			var personRequest = createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(new[] { shiftTradeBusinessRuleConfig });
			Assert.IsTrue(personRequest.IsPending);

			acceptShiftTradeWithShiftTradeTargetTimeSpecificationBroken(personRequest);
			Assert.IsTrue(personRequest.IsPending);

			var denyReason = Resources.ResourceManager.GetString("ShiftTradeTargetTimePendingReason");
			Assert.IsTrue(personRequest.GetMessage(new NoFormatting()).Contains(denyReason));
			Assert.IsTrue(personRequest.BrokenBusinessRules.Value.HasFlag(BusinessRuleFlags.ShiftTradeTargetTimeRule));
		}

		[Test, SetCulture("en-US")]
		public void ShouldPendingWhenAutoGrantIsOff()
		{
			CurrentScenario.Has("Default");
			var shiftTradeBusinessRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(ShiftTradeTargetTimeSpecification).FullName,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.Pending
			};
			var personRequest = createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(
				new[] {shiftTradeBusinessRuleConfig});
			Assert.IsTrue(personRequest.IsPending);

			acceptShiftTradeWithShiftTradeTargetTimeSpecificationBroken(personRequest);
			Assert.IsTrue(personRequest.IsPending);

			var denyReason = Resources.ResourceManager.GetString("ShiftTradeTargetTimePendingReason");
			Assert.IsTrue(personRequest.GetMessage(new NoFormatting()).Contains(denyReason));
		}

		[Test, SetCulture("en-US")]
		public void ShouldDenyWhenSpecificationIsPendingAndThereIsOtherDeniableResponse()
		{
			CurrentScenario.Has("Default");
			var shiftTradeTargetTimeSpecificationRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(ShiftTradeTargetTimeSpecification).FullName,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.Pending
			};
			var siteOpenHourRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(SiteOpenHoursRule).FullName,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.AutoDeny
			};
			var personRequest = createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(new[]
			{
				shiftTradeTargetTimeSpecificationRuleConfig,
				siteOpenHourRuleConfig
			});
			Assert.IsTrue(personRequest.IsPending);

			acceptShiftTradeWithShiftTradeTargetTimeSpecificationBroken(personRequest, true);
			Assert.IsTrue(personRequest.IsDenied);
			Assert.IsTrue(personRequest.DenyReason.Contains("No open hours for"));

			var denyReason = Resources.ResourceManager.GetString("ShiftTradeTargetTimePendingReason");
			Assert.IsTrue(personRequest.GetMessage(new NoFormatting()).Contains(denyReason));
		}

		[Test, SetCulture("sv")]
		public void ShouldUsePermissionInformationUICulture()
		{
			CurrentScenario.Has("Default");

			var shiftTradeTargetTimeSpecificationRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(ShiftTradeTargetTimeSpecification).FullName,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.Pending
			};
			var siteOpenHourRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(SiteOpenHoursRule).FullName,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.AutoDeny
			};
			var personRequest = createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(new[]
			{
				shiftTradeTargetTimeSpecificationRuleConfig,
				siteOpenHourRuleConfig
			});
			Assert.IsTrue(personRequest.IsPending);

			acceptShiftTradeWithShiftTradeTargetTimeSpecificationBroken(personRequest, true);
			Assert.IsTrue(personRequest.IsDenied);
			Assert.IsTrue(personRequest.DenyReason.Contains("No open hours for"));

			var denyReasonInSV = "Toleransen för kontraktstiden har överskridits";
			var denyReasonInEN = "Schedule contract time tolerance was exceeded";
			var message = personRequest.GetMessage(new NoFormatting());
			Assert.IsFalse(message.Contains(denyReasonInSV));
			Assert.IsTrue(message.Contains(denyReasonInEN));
		}

		private IPersonRequest createShiftTradeWithShiftTradeTargetTimeSpecificationBroken(
			ShiftTradeBusinessRuleConfig[] shiftTradeBusinessRuleConfigs
			, bool autoGrantShiftTrade = true)
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
			
			ShiftTradeTestHelper.AddPersonAssignment(personTo, dateTimePeriod, activityPersonTo);
			ShiftTradeTestHelper.AddPersonAssignment(personFrom, dateTimePeriod, activityPersonFrom);

			var personRequest = ShiftTradeTestHelper.PrepareAndGetPersonRequest(personFrom, personTo, scheduleDateOnly);

			var @event = new NewShiftTradeRequestCreatedEvent
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault()
			};

			var shiftTradeSpecifications = ShiftTradeTestHelper.GetDefaultShiftTradeSpecifications();
			shiftTradeSpecifications.Add(createShiftTradeTargetTimeSpecification());

			GlobalSettingDataRepository.PersistSettingValue(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings
			{
				BusinessRuleConfigs = shiftTradeBusinessRuleConfigs
			});

			ShiftTradeTestHelper.UseSpecificationCheckerWithConfig(shiftTradeSpecifications, GlobalSettingDataRepository);

			ShiftTradeTestHelper.HandleRequest(@event);
			return personRequest;
		}

		private ShiftTradeTargetTimeSpecification createShiftTradeTargetTimeSpecification()
		{
			var gridLockManager = new GridlockManager();
			var matrixUserLocker = new MatrixUserLockLocker(() => gridLockManager, CurrentAuthorization.Make());
			var notPermittedLocker = new MatrixNotPermittedLocker(new ThisAuthorization(new FullPermission()));
			var personListExtraxtor = new PersonListExtractorFromScheduleParts();
			var periodExtractor = new PeriodExtractorFromScheduleParts();
			var matrixListFactory = new MatrixListFactory(matrixUserLocker, notPermittedLocker, personListExtraxtor, periodExtractor);
			return new ShiftTradeTargetTimeSpecification(
				() => new SchedulerStateHolder(SchedulingResultStateHolder, null, TimeZoneGuard)
				, matrixListFactory, new SchedulePeriodTargetTimeCalculator());
		}

		private void acceptShiftTradeWithShiftTradeTargetTimeSpecificationBroken(IPersonRequest personRequest, bool enableSiteOpenHoursRule = false)
		{
			var shiftTradeRequest = (IShiftTradeRequest) personRequest.Request;
			var personTo = shiftTradeRequest.PersonTo;
			var personFrom = shiftTradeRequest.PersonFrom;
			var scheduleDate = shiftTradeRequest.Period.StartDateTime;
			
			var @event = ShiftTradeTestHelper.GetAcceptShiftTradeEvent(personTo, personRequest.Id.GetValueOrDefault());
			@event.UseSiteOpenHoursRule = enableSiteOpenHoursRule;

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom }, new ScheduleDictionaryLoadOptions(false,false), 
				new DateOnlyPeriod(new DateOnly(scheduleDate), new DateOnly(scheduleDate.AddDays(7))), CurrentScenario.LoadDefaultScenario());
			var businessRuleProvider = new ConfigurableBusinessRuleProvider(GlobalSettingDataRepository);
			((ReadOnlyScheduleDictionary)scheduleDictionary).MakeEditable();
			ShiftTradeTestHelper.SetScheduleDictionary(scheduleDictionary);

			ShiftTradeTestHelper.HandleRequest(@event, businessRuleProvider);
		}

		private IPerson createPerson(bool autoGrantShiftTrade = true)
		{
			var workControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(autoGrantShiftTrade);
			var startDate = new DateOnly(2016, 1, 1);
			var team = TeamFactory.CreateTeam("team", "site");

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, team);
			person.WorkflowControlSet = workControlSet;
			PersonRepository.Add(person);
			
			ShiftTradeTestHelper.SetSiteOpenHours(person, 8, 17);

			return person;
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ShiftTradeTestHelper>().For<ShiftTradeTestHelper>();
		}
	}
}
