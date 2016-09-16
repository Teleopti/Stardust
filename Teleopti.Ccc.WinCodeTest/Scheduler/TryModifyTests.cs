using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[DomainTest]
	public class TryModifyTests : ISetup
	{
		public ISchedulerStateHolder StateHolder;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldReturnTrueForModificationWithAlreadyOverridenRuleBroken()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
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
			dictionary.SetUndoRedoContainer(new UndoRedoContainer(new DoNothingScheduleDayChangeCallBack(), 500));
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person,
				new DateTimePeriod(2014, 3, 25, 8, 2014, 3, 25, 22), shiftCategory, scenario);
			assignment.AddActivity(lunch,new DateTimePeriod(2014,3,25,13,2014,3,25,14));
			dictionary.AddPersonAssignment(assignment);

			StateHolder.SetRequestedScenario(scenario);
			StateHolder.SchedulingResultState.Schedules = dictionary;
			StateHolder.SchedulingResultState.UseValidation = true;
			
			var overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();
			overriddenBusinessRulesHolder.AddOverriddenRule(new BusinessRuleResponse(typeof(NotOverwriteLayerRule),"",false,false,new DateTimePeriod(), person,new DateOnlyPeriod(), ""));
			
			var fakeResponseHandler = new FakeBusinessRulesResponseHandler();
			var view = new FakeView().WithBusinessRuleResponse(fakeResponseHandler);

			var target = new SchedulePresenterBase(view, StateHolder, new GridlockManager(), new ClipHandler<IScheduleDay>(), SchedulePartFilter.None, overriddenBusinessRulesHolder, new DoNothingScheduleDayChangeCallBack(), NullScheduleTag.Instance);

			var dayToChange = StateHolder.Schedules[person].ScheduledDay(new DateOnly(2014, 3, 25));
			dayToChange.PersonAssignment().AddActivity(activity,new DateTimePeriod(2014, 3, 25, 13, 2014, 3, 25, 14));
			Assert.IsTrue(target.TryModify(new List<IScheduleDay> {dayToChange}));
		}

		[Test]
		public void ShouldReturnTrueForModificationWithRuleBroken()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
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
			dictionary.SetUndoRedoContainer(new UndoRedoContainer(new DoNothingScheduleDayChangeCallBack(), 500));
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person,
				new DateTimePeriod(2014, 3, 25, 8, 2014, 3, 25, 22), shiftCategory, scenario);
			assignment.AddActivity(lunch, new DateTimePeriod(2014, 3, 25, 13, 2014, 3, 25, 14));
			dictionary.AddPersonAssignment(assignment);

			StateHolder.SetRequestedScenario(scenario);
			StateHolder.SchedulingResultState.Schedules = dictionary;
			StateHolder.SchedulingResultState.UseValidation = true;

			var overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();

			var fakeResponseHandler = new FakeBusinessRulesResponseHandler();
			var view = new FakeView().WithBusinessRuleResponse(fakeResponseHandler);

			var target = new SchedulePresenterBase(view, StateHolder, new GridlockManager(), new ClipHandler<IScheduleDay>(), SchedulePartFilter.None, overriddenBusinessRulesHolder, new DoNothingScheduleDayChangeCallBack(), NullScheduleTag.Instance);

			var dayToChange = StateHolder.Schedules[person].ScheduledDay(new DateOnly(2014, 3, 25));
			dayToChange.PersonAssignment().AddActivity(activity, new DateTimePeriod(2014, 3, 25, 13, 2014, 3, 25, 14));
			Assert.IsTrue(target.TryModify(new List<IScheduleDay> { dayToChange }));
		}

		private class FakeView : IScheduleViewBase
		{
			private readonly GridControl _grid = new GridControl();
			private IHandleBusinessRuleResponse _handleBusinessRuleResponse;

			public void ShowErrorMessage(string text, string caption)
			{
			}

			public DialogResult ShowConfirmationMessage(string text, string caption)
			{
				throw new NotImplementedException();
			}

			public DialogResult ShowYesNoMessage(string text, string caption)
			{
				throw new NotImplementedException();
			}

			public void ShowInformationMessage(string text, string caption)
			{
			}

			public DialogResult ShowOkCancelMessage(string text, string caption)
			{
				throw new NotImplementedException();
			}

			public DialogResult ShowWarningMessage(string text, string caption)
			{
				throw new NotImplementedException();
			}

			public IAddLayerViewModel<IAbsence> CreateAddAbsenceViewModel(IEnumerable<IAbsence> bindingList, ISetupDateTimePeriod period,
				TimeZoneInfo timeZoneInfo)
			{
				throw new NotImplementedException();
			}

			public IAddActivityViewModel CreateAddActivityViewModel(IEnumerable<IActivity> activities, IList<IShiftCategory> shiftCategories, DateTimePeriod period,
				TimeZoneInfo timeZoneInfo, IActivity defaultActivity)
			{
				throw new NotImplementedException();
			}

			public IAddLayerViewModel<IActivity> CreateAddPersonalActivityViewModel(IEnumerable<IActivity> activities, DateTimePeriod period,
				TimeZoneInfo timeZoneInfo)
			{
				throw new NotImplementedException();
			}

			public IAddOvertimeViewModel CreateAddOvertimeViewModel(IEnumerable<IActivity> activities, IList<IMultiplicatorDefinitionSet> definitionSets, IActivity defaultActivity,
				DateTimePeriod period, TimeZoneInfo timeZoneInfo)
			{
				throw new NotImplementedException();
			}

			public IAddLayerViewModel<IDayOffTemplate> CreateAddDayOffViewModel(IEnumerable<IDayOffTemplate> dayOffTemplates, TimeZoneInfo timeZoneInfo,
				DateTimePeriod period)
			{
				throw new NotImplementedException();
			}

			public int ColHeaders { get; }
			public int RowHeaders { get; }

			public void SetCellBackTextAndBackColor(GridQueryCellInfoEventArgs e, DateOnly dateTime, bool backColor, bool textColor,
				IScheduleDay schedulePart)
			{
			}

			public string DayHeaderTooltipText(GridStyleInfo gridStyle, DateOnly currentDate)
			{
				throw new NotImplementedException();
			}

			public bool IsRightToLeft { get; }
			public bool IsOverviewColumnsHidden { get; }

			public FakeView WithBusinessRuleResponse(IHandleBusinessRuleResponse handleBusinessRuleResponse)
			{
				_handleBusinessRuleResponse = handleBusinessRuleResponse;
				return this;
			}

			public IHandleBusinessRuleResponse HandleBusinessRuleResponse { get { return _handleBusinessRuleResponse; } }
			public void InvalidateSelectedRows(IEnumerable<IScheduleDay> schedules)
			{
			}

			public void OnPasteCompleted()
			{
			}

			public GridControl TheGrid { get { return _grid; } }
			public IList<IScheduleDay> CurrentColumnSelectedSchedules()
			{
				throw new NotImplementedException();
			}

			public IList<IScheduleDay> SelectedSchedules()
			{
				throw new NotImplementedException();
			}

			public void RefreshRangeForAgentPeriod(IEntity person, DateTimePeriod period)
			{
			}

			public void GridClipboardPaste(PasteOptions options, IUndoRedoContainer undoRedo)
			{
			}

			public ICollection<DateOnly> AllSelectedDates()
			{
				throw new NotImplementedException();
			}

			public ICollection<DateOnly> AllSelectedDates(IEnumerable<IScheduleDay> selectedSchedules)
			{
				throw new NotImplementedException();
			}
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DesktopOptimizationContext>().For<IFillSchedulerStateHolder>();
		}
	}

	public class FakeBusinessRulesResponseHandler : IHandleBusinessRuleResponse
	{
		public void SetResponse(IEnumerable<IBusinessRuleResponse> businessRulesResponse)
		{
		}

		public bool ApplyToAll { get; }
		public DialogResult DialogResult { get; }
	}
}