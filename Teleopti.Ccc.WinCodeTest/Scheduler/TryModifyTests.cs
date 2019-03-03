using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[DomainTest, UseIocForFatClient]
	public class TryModifyTests
	{
		public ISchedulerStateHolder StateHolder;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldReturnTrueForModificationWithAlreadyOverridenRuleBroken()
		{
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
			var scenario = new Scenario("Default").WithId();
			var shiftCategory = new ShiftCategory("DY").WithId();
			var activity = new Activity("Phone")
			{
				InContractTime = true,
				InWorkTime = true,
				InPaidTime = true,
			};
			var lunch = new Activity("Lunch")
			{
				AllowOverwrite = false
			};

			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson().WithId(),
				new DateOnly(2014, 3, 1));
			var personPeriod = person.Period(new DateOnly(2014, 3, 1));
			personPeriod.Team.Site = SiteFactory.CreateSimpleSite();
			personPeriod.PersonContract.Contract.NegativePeriodWorkTimeTolerance = TimeSpan.FromHours(24);
			personPeriod.PersonContract.Contract.PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(24);
			
			var dictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2014, 3, 22, 2014, 4, 4));
			dictionary.UsePermissions(false);
			dictionary.SetUndoRedoContainer(new UndoRedoContainer());
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2014, 3, 25, 8, 2014, 3, 25, 22), shiftCategory);
			assignment.AddActivity(lunch,new DateTimePeriod(2014,3,25,13,2014,3,25,14));
			dictionary.AddPersonAssignment(assignment);

			StateHolder.SetRequestedScenario(scenario);
			StateHolder.SchedulingResultState.Schedules = dictionary;
			StateHolder.SchedulingResultState.UseValidation = true;
			
			var overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();
			overriddenBusinessRulesHolder.AddOverriddenRule(new BusinessRuleResponse(typeof(NotOverwriteLayerRule),"",false,false,new DateTimePeriod(), person,new DateOnlyPeriod(), ""));
			
			var fakeResponseHandler = new FakeBusinessRulesResponseHandler();
			var view = new FakeScheduleView().WithBusinessRuleResponse(fakeResponseHandler);

			var target = new SchedulePresenterBase(view, StateHolder, new GridlockManager(), new ClipHandler<IScheduleDay>(), SchedulePartFilter.None, overriddenBusinessRulesHolder, new DoNothingScheduleDayChangeCallBack(), NullScheduleTag.Instance, new UndoRedoContainer(), new FakeTimeZoneGuard());

			var dayToChange = StateHolder.Schedules[person].ScheduledDay(new DateOnly(2014, 3, 25));
			dayToChange.PersonAssignment().AddActivity(activity,new DateTimePeriod(2014, 3, 25, 13, 2014, 3, 25, 14));
			Assert.IsTrue(target.TryModify(new List<IScheduleDay> {dayToChange}));
		}

		[Test]
		public void ShouldReturnTrueForModificationWithRuleBroken()
		{
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
			var scenario = new Scenario("Default").WithId();
			var shiftCategory = new ShiftCategory("DY").WithId();
			var activity = new Activity("Phone")
			{
				InContractTime = true,
				InWorkTime = true,
				InPaidTime = true
			};
			var lunch = new Activity("Lunch")
			{
				AllowOverwrite = false
			};

			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson().WithId(),
				new DateOnly(2014, 3, 1));
			var personPeriod = person.Period(new DateOnly(2014, 3, 1));
			personPeriod.Team.Site = SiteFactory.CreateSimpleSite();
			personPeriod.PersonContract.Contract.NegativePeriodWorkTimeTolerance = TimeSpan.FromHours(24);
			personPeriod.PersonContract.Contract.PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(24);

			var dictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2014, 3, 22, 2014, 4, 4));
			dictionary.UsePermissions(false);
			dictionary.SetUndoRedoContainer(new UndoRedoContainer());
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2014, 3, 25, 8, 2014, 3, 25, 22), shiftCategory);
			assignment.AddActivity(lunch, new DateTimePeriod(2014, 3, 25, 13, 2014, 3, 25, 14));
			dictionary.AddPersonAssignment(assignment);

			StateHolder.SetRequestedScenario(scenario);
			StateHolder.SchedulingResultState.Schedules = dictionary;
			StateHolder.SchedulingResultState.UseValidation = true;

			var overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();

			var fakeResponseHandler = new FakeBusinessRulesResponseHandler();
			var view = new FakeScheduleView().WithBusinessRuleResponse(fakeResponseHandler);

			var target = new SchedulePresenterBase(view, StateHolder, new GridlockManager(), new ClipHandler<IScheduleDay>(),
				SchedulePartFilter.None, overriddenBusinessRulesHolder, new DoNothingScheduleDayChangeCallBack(),
				NullScheduleTag.Instance, new UndoRedoContainer(), new FakeTimeZoneGuard());

			var dayToChange = StateHolder.Schedules[person].ScheduledDay(new DateOnly(2014, 3, 25));
			dayToChange.PersonAssignment().AddActivity(activity, new DateTimePeriod(2014, 3, 25, 13, 2014, 3, 25, 14));
			Assert.IsTrue(target.TryModify(new List<IScheduleDay> {dayToChange}));
		}
	}
}