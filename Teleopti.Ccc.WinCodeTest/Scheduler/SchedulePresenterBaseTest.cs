using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;

using Is = Rhino.Mocks.Constraints.Is;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class SchedulePresenterBaseTest
    {
        private SchedulePresenterBase _target;
        private IScheduleViewBase _viewBase;
        private IScenario _scenario;
        private SchedulerStateHolder _schedulerState;
        private GridlockManager _gridlockManager;
        private ClipHandler<IScheduleDay> _clipHandlerSchedulePart;
        private MockRepository _mocks;
        private readonly DateOnly _date = new DateOnly(2008, 11, 04);
        private TimeZoneInfo _timeZoneInfo;
        private DateOnlyPeriod _period;
        private GridControl _grid;
        private IAddLayerViewModel<IAbsence> _dialog;
        private IAbsence _selectedItem;
        private IPerson _person;
        private IScheduleDictionary _scheduleDictionary;
        private IList<IBusinessRuleResponse> _businessRuleResponses;
        private IList<IScheduleDay> _selectedSchedules;
        private IScenario _scen;
        private IScheduleDay _day1;
        private IScheduleDay _day2;
        private IPersonAssignment _ass;
        private IList<IMultiplicatorDefinitionSet> _multiplicatorDefinitionSets;
        private IAddOvertimeViewModel _overtimeDialog;
        private IActivity _selectedActivity;
        private IMultiplicatorDefinitionSet _definitionSet;
        private IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
        private IScheduleDayChangeCallback _scheduleDayChangeCallback;
        private IScheduleTag _scheduleTag;
        private PersonNameComparer _personNameComparer;
        private readonly IList<IPersonAssignment> _personAssignmentsList = new List<IPersonAssignment>();
        private IPersonAssignment _personAssignment;
        private ReadOnlyCollection<IPersonAssignment> readOnlyCollection;
        private PersonAssignment _personAssignment2;

		[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _gridlockManager = new GridlockManager();
			_timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
            
            _clipHandlerSchedulePart = new ClipHandler<IScheduleDay>();
            _period = new DateOnlyPeriod(2009, 2, 2, 2009, 3, 1);
			_schedulerState = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_period, _timeZoneInfo), new List<IPerson>(), _mocks.DynamicMock<IDisableDeletedFilter>(), new SchedulingResultStateHolder());
            _overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();

            createMockObjects();

            _target = new SchedulePresenterBase(_viewBase, _schedulerState, _gridlockManager, _clipHandlerSchedulePart, SchedulePartFilter.None, _overriddenBusinessRulesHolder, _scheduleDayChangeCallback, NullScheduleTag.Instance, new UndoRedoContainer());
			_grid = new GridControl();
            _person = PersonFactory.CreatePerson("person");

            _businessRuleResponses = new List<IBusinessRuleResponse>();
            _selectedSchedules = new List<IScheduleDay>();
            _scen = ScenarioFactory.CreateScenarioAggregate();
            _day2 = null;
            _day1 = null;
            _multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();
            _definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
            _scheduleTag = _mocks.StrictMock<IScheduleTag>();
            _personNameComparer = new PersonNameComparer(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture);
        }

        private void createMockObjects()
        {
            _viewBase = _mocks.StrictMock<IScheduleViewBase>();
            _scenario = _mocks.StrictMock<IScenario>();
            _scheduleDayChangeCallback = _mocks.DynamicMock<IScheduleDayChangeCallback>();
            _dialog = _mocks.StrictMock<IAddLayerViewModel<IAbsence>>();
            _selectedItem = _mocks.StrictMock<IAbsence>();
            _scheduleDictionary = _mocks.DynamicMock<IScheduleDictionary>();
            _ass = _mocks.StrictMock<IPersonAssignment>();
            _overtimeDialog = _mocks.StrictMock<IAddOvertimeViewModel>();
            _selectedActivity = _mocks.StrictMock<IActivity>();
        }

        [TearDown]
        public void Teardown()
        {
            _grid.Dispose();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_schedulerState, _target.SchedulerState);
            Assert.AreEqual(_gridlockManager, _target.LockManager);
            Assert.AreEqual(_clipHandlerSchedulePart, _target.ClipHandlerSchedule);
            Assert.AreEqual(33, _target.ColCount);
            Assert.AreEqual(1, _target.RowCount);
            Assert.AreEqual(20, SchedulePresenterBase.ProjectionHeight);
            Assert.AreEqual(SchedulePartFilter.None, _target.SchedulePartFilter);
            Assert.AreEqual(_period, _target.SelectedPeriod.DateOnlyPeriod);
            Assert.AreEqual(4, _target.VisibleWeeks);
            Assert.AreEqual(0, _target.ColWeekMap.Count);
            Assert.AreEqual(0, _target.TimelineSpan.Count);
            Assert.IsNull(_target.LastUnsavedSchedulePart);
            Assert.AreEqual(1, _target.CurrentSortColumn);
            Assert.IsTrue(_target.IsAscendingSort);
            _target.SortColumn(1);
            Assert.IsFalse(_target.IsAscendingSort);


            var lastUnsavedSchedulePart = _mocks.StrictMock<IScheduleDay>();

            var period = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(_date,_date), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            _target.SelectedPeriod = period;
            _target.VisibleWeeks = 6;
            _target.LastUnsavedSchedulePart = lastUnsavedSchedulePart;
            _target.DefaultScheduleTag = _scheduleTag;

            Assert.AreEqual(6, _target.ColCount);
            Assert.AreEqual(1, _target.RowCount);
            Assert.AreEqual(period, _target.SelectedPeriod);
            Assert.AreEqual(6, _target.VisibleWeeks);
            Assert.AreEqual(lastUnsavedSchedulePart, _target.LastUnsavedSchedulePart);
            Assert.AreEqual(_scheduleTag, _target.DefaultScheduleTag);
        }

        [Test]
        public void ShouldNotSetStyleToNaOnDayOffCellWhenSchedulePeriodAndOpenPeriodMatch()
        {
			_period = new DateOnlyPeriod(2009, 2, 2, 2009, 3, 1);
			var person = PersonFactory.CreatePerson();
			var personWithSchedulePeriod = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(person, new DateOnly(2009, 2, 15));
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
			var scheduleRange = _mocks.StrictMock<IScheduleRange>();

			using (_mocks.Record())
			{
				Expect.Call(_viewBase.ViewGrid).Return(_grid).Repeat.AtLeastOnce();
				Expect.Call(_viewBase.IsOverviewColumnsHidden).Return(false);
				Expect.Call(scheduleDictionary[person]).Return(scheduleRange).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				using (var gridStyleInfo = new GridStyleInfo())
				{

					_target.QueryOverviewStyleInfo(gridStyleInfo, personWithSchedulePeriod, ColumnType.TargetDayOffColumn);
					Assert.AreEqual(UserTexts.Resources.NA, gridStyleInfo.CellValue);
				}
			}
        }

        [Test]
        public void ShouldSetStyleToNaOnContractCellWhenSchedulePeriodAndOpenPeriodDoNotMatch()
        {
			_period = new DateOnlyPeriod(2009, 2, 2, 2009, 3, 1);
	        var person = PersonFactory.CreatePerson();
	        var personWithSchedulePeriod = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(person, new DateOnly(2009, 2, 15));
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            var scheduleRange = _mocks.StrictMock<IScheduleRange>();

            using (_mocks.Record())
            {
                Expect.Call(_viewBase.ViewGrid).Return(_grid).Repeat.AtLeastOnce();
                Expect.Call(_viewBase.IsOverviewColumnsHidden).Return(false);
                Expect.Call(scheduleDictionary[person]).Return(scheduleRange).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                using (var gridStyleInfo = new GridStyleInfo())
                {

					_target.QueryOverviewStyleInfo(gridStyleInfo, personWithSchedulePeriod, ColumnType.TargetContractTimeColumn);
                    Assert.AreEqual(UserTexts.Resources.NA, gridStyleInfo.CellValue);
                }
            }
        }

        [Test]
        public void ShouldNotSetStyleToNaOnContractCellWhenSchedulePeriodAndOpenPeriodMatch()
        {
			_period = new DateOnlyPeriod(2015, 11, 23, 2015, 11, 29);
	        var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
	        var schedulePeriod = new SchedulePeriod(new DateOnly(2015, 11, 23), SchedulePeriodType.Week, 1);
			person.AddSchedulePeriod(schedulePeriod);
			
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
			_schedulerState.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(_period, TimeZoneInfo.Utc);
			var scheduleRange = _mocks.StrictMock<IScheduleRange>();

			using (_mocks.Record())
			{
				Expect.Call(_viewBase.ViewGrid).Return(_grid).Repeat.AtLeastOnce();
				Expect.Call(_viewBase.IsOverviewColumnsHidden).Return(false);
				Expect.Call(scheduleDictionary[person]).Return(scheduleRange).Repeat.AtLeastOnce();
				Expect.Call(scheduleRange.CalculatedTargetTimeHolder(_period)).Return(TimeSpan.Zero);
			}

			using (_mocks.Playback())
			{
				using (var gridStyleInfo = new GridStyleInfo())
				{

					_target.QueryOverviewStyleInfo(gridStyleInfo, person, ColumnType.TargetContractTimeColumn);
					Assert.AreNotEqual(UserTexts.Resources.NA, gridStyleInfo.CellValue);
				}
			}
        }

		[Test]
		[SetCulture("sv-SE")]
		public void CanReturnWeekHeaderStringForMoreThanOneYear()
		{
			var startDate= new DateOnly(2017,4,1);
			var endDate= new DateOnly(2018,3,31);
			Expect.Call(_viewBase.RowHeaders).Return(1).Repeat.AtLeastOnce();
			Expect.Call(_viewBase.ViewGrid).Return(_grid).Repeat.AtLeastOnce();

			_mocks.ReplayAll();

			const int weekNumber = 13;
			const int targetColIndex = 365;
			_target.ColWeekMap.Add((int)ColumnType.StartScheduleColumns, weekNumber);
			_target.ColWeekMap.Add(targetColIndex, weekNumber);
			_target.SelectedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(startDate, endDate), _timeZoneInfo);
            
			GridQueryCellInfoEventArgs eventArgs = new GridQueryCellInfoEventArgs(0, targetColIndex, new GridStyleInfo());
			_target.QueryCellInfo(null, eventArgs);
			Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, Resources.WeekAbbreviationDot, weekNumber,new DateOnly(2018,3,26).ToShortDateString()), eventArgs.Style.Text);
		}

		[Test]
        public void VerifyQueryCellInfoHeaders()
        {
            IScheduleDictionary scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            IPerson person1 = _mocks.StrictMock<IPerson>();
            Expect.Call(person1.Name).Return(new Name("First1", "Last1")).Repeat.AtLeastOnce();
            Expect.Call(person1.EmploymentNumber).Return("120").Repeat.AtLeastOnce();
            Expect.Call(person1.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(_viewBase.RowHeaders).Return(1).Repeat.AtLeastOnce();
            Expect.Call(_viewBase.ColHeaders).Return(1).Repeat.AtLeastOnce();
            _viewBase.SetCellBackTextAndBackColor(null, _date, false, false, null);
            LastCall.IgnoreArguments().Repeat.AtLeastOnce();
            Expect.Call(_viewBase.DayHeaderTooltipText(null, DateOnly.MinValue)).IgnoreArguments().Return("test").Repeat.Once();
            Expect.Call(_viewBase.ViewGrid).Return(_grid).Repeat.AtLeastOnce();

            _mocks.ReplayAll();

            _target.ColWeekMap.Add((int)ColumnType.StartScheduleColumns, 45);
            _target.SelectedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(_date,_date), _timeZoneInfo);
            
			_schedulerState.FilteredAgentsDictionary.Add(person1.Id.Value, person1);
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;

            GridQueryCellInfoEventArgs eventArgs = new GridQueryCellInfoEventArgs(0, 0, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.IsTrue(string.IsNullOrEmpty((string)eventArgs.Style.CellValue));

            eventArgs = new GridQueryCellInfoEventArgs(0, 1, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.IsTrue(string.IsNullOrEmpty((string)eventArgs.Style.CellValue));

            eventArgs = new GridQueryCellInfoEventArgs(0, (int)ColumnType.StartScheduleColumns, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.AreEqual(_target.SelectedPeriod.DateOnlyPeriod.StartDate, eventArgs.Style.Tag);
            Assert.AreEqual(GridMergeCellDirection.ColumnsInRow, eventArgs.Style.MergeCell);

            eventArgs = new GridQueryCellInfoEventArgs(1, 1, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.IsTrue(string.IsNullOrEmpty((string)eventArgs.Style.CellValue));

            eventArgs = new GridQueryCellInfoEventArgs(1, (int)ColumnType.StartScheduleColumns, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.AreEqual(_target.SelectedPeriod.DateOnlyPeriod.StartDate, eventArgs.Style.Tag);
            Assert.AreEqual("test", eventArgs.Style.CellTipText);
            Assert.AreEqual(GridMergeCellDirection.None, eventArgs.Style.MergeCell);

            eventArgs = new GridQueryCellInfoEventArgs(2, 1, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.AreEqual(string.Concat(person1.Name.FirstName, " ", person1.Name.LastName), eventArgs.Style.CellValue);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyQueryCellInfoScheduleCell()
        {
            var range = _mocks.StrictMock<IScheduleRange>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var person1 = _mocks.StrictMock<IPerson>();
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
            
            Expect.Call(person1.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(scheduleDictionary[person1]).IgnoreArguments().Return(range).Repeat.AtLeastOnce();
            Expect.Call(range.ScheduledDay(_date)).IgnoreArguments().Return(schedulePart).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.FullAccess).Return(true).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.Person).Return(person1).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2008, 11, 4),
                                                                                           _timeZoneInfo)).Repeat.Times(2);

            schedulePart.Clear<IPersonAbsence>();
            LastCall.Repeat.Once();
            schedulePart.Clear<IPersonAssignment>();
            LastCall.Repeat.Once();

            Expect.Call(schedulePart.PersonAssignment()).Return(null).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.PersonAbsenceCollection()).Return(
                new IPersonAbsence[0]).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.PersonMeetingCollection()).Return(
                new IPersonMeeting[0]).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.BusinessRuleResponseCollection).Return(
                new List<IBusinessRuleResponse>()).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.ProjectionService()).Return(new VisualLayerProjectionService()).Repeat.Any();
            Expect.Call(_viewBase.RowHeaders).Return(1).Repeat.AtLeastOnce();
            Expect.Call(_viewBase.ColHeaders).Return(1).Repeat.AtLeastOnce();
            _viewBase.SetCellBackTextAndBackColor(null, _date, false, false, null);
            LastCall.IgnoreArguments().Repeat.AtLeastOnce();
            Expect.Call(_viewBase.ViewGrid).Return(_grid).Repeat.AtLeastOnce();
            Expect.Call(person1.IsAgent(new DateOnly(2008, 11, 04))).Return(true).Repeat.Times(2);
			_mocks.ReplayAll();

            _target.SelectedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(_date,_date), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            if (person1.Id != null) _schedulerState.FilteredAgentsDictionary.Add(person1.Id.Value, person1);
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;

            var eventArgs = new GridQueryCellInfoEventArgs(2, (int)ColumnType.StartScheduleColumns, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.AreEqual(schedulePart, eventArgs.Style.CellValue);
            Assert.AreEqual(GridMergeCellDirection.None, eventArgs.Style.MergeCell);

            _target = new SchedulePresenterBase(_viewBase, _schedulerState, _gridlockManager, _clipHandlerSchedulePart, SchedulePartFilter.Meetings, _overriddenBusinessRulesHolder,
                _scheduleDayChangeCallback, NullScheduleTag.Instance, new UndoRedoContainer());
            _target.SelectedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(_date,_date), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);

            eventArgs = new GridQueryCellInfoEventArgs(2, (int)ColumnType.StartScheduleColumns, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.AreEqual(schedulePart, eventArgs.Style.CellValue);
            Assert.AreEqual(GridMergeCellDirection.None, eventArgs.Style.MergeCell);

            _mocks.VerifyAll();
        }

        [Test]
        public void QueryCellInfoUpdateNow()
        {
            _mocks.ReplayAll();

            Assert.GreaterOrEqual(DateTime.UtcNow, _target.Now);
            _target.Now = WorkShift.BaseDate;
            Assert.AreEqual(WorkShift.BaseDate, _target.Now);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyQueryCellInfoOverviewHeaders()
        {
            IScheduleDictionary scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            IPerson person1 = _mocks.StrictMock<IPerson>();
            Expect.Call(person1.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(_viewBase.RowHeaders).Return(1).Repeat.AtLeastOnce();
            Expect.Call(_viewBase.ColHeaders).Return(1).Repeat.AtLeastOnce();
            Expect.Call(_viewBase.ViewGrid).Return(_grid).Repeat.AtLeastOnce();

            _mocks.ReplayAll();

            _target.ColWeekMap.Add((int)ColumnType.StartScheduleColumns, 45);
            _target.SelectedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(_date,_date), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            //_schedulerState.FilteredCombinedAgentsDictionary.Add(person1.Id.Value, person1);
			_schedulerState.FilteredAgentsDictionary.Add(person1.Id.Value, person1);
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;

            GridQueryCellInfoEventArgs eventArgs = new GridQueryCellInfoEventArgs(0, 2, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.AreEqual(UserTexts.Resources.Current, eventArgs.Style.CellValue);
            Assert.AreEqual(GridMergeCellDirection.ColumnsInRow, eventArgs.Style.MergeCell);

            eventArgs = new GridQueryCellInfoEventArgs(0, 3, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.AreEqual(UserTexts.Resources.Current, eventArgs.Style.CellValue);
            Assert.AreEqual(GridMergeCellDirection.ColumnsInRow, eventArgs.Style.MergeCell);

            eventArgs = new GridQueryCellInfoEventArgs(0, 4, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.AreEqual(UserTexts.Resources.Target, eventArgs.Style.CellValue);
            Assert.AreEqual(GridMergeCellDirection.ColumnsInRow, eventArgs.Style.MergeCell);

            eventArgs = new GridQueryCellInfoEventArgs(0, 5, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.AreEqual(UserTexts.Resources.Target, eventArgs.Style.CellValue);
            Assert.AreEqual(GridMergeCellDirection.ColumnsInRow, eventArgs.Style.MergeCell);

            eventArgs = new GridQueryCellInfoEventArgs(1, 2, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.AreEqual(UserTexts.Resources.ScheduledTime, eventArgs.Style.CellValue);

            eventArgs = new GridQueryCellInfoEventArgs(1, 3, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.AreEqual(UserTexts.Resources.ScheduledDaysOff, eventArgs.Style.CellValue);

            eventArgs = new GridQueryCellInfoEventArgs(1, 4, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.AreEqual(UserTexts.Resources.TargetTime, eventArgs.Style.CellValue);

            eventArgs = new GridQueryCellInfoEventArgs(1, 5, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.AreEqual(UserTexts.Resources.TargetDaysOff, eventArgs.Style.CellValue);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyQueryCellInfoBadRow()
        {
            GridQueryCellInfoEventArgs eventArgs = new GridQueryCellInfoEventArgs(120, 0, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
        }

        [Test]
        public void VerifyTryModifyWithMandatoryRuleBroken()
        {
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var businessRuleResponse = _mocks.StrictMock<IBusinessRuleResponse>();
            IList<IBusinessRuleResponse> businessRuleResponses = new List<IBusinessRuleResponse> { businessRuleResponse };
            Expect.Call(businessRuleResponse.Mandatory).Return(true);
            Expect.Call(businessRuleResponse.Message).Return("testar");
            Expect.Call(scheduleDictionary.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay>(), null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(
                businessRuleResponses);
            Expect.Call(() => _viewBase.ShowErrorMessage("", "")).IgnoreArguments();
            Expect.Call(_viewBase.HandleBusinessRuleResponse).Return(null);
            Expect.Call(_viewBase.ViewGrid).Return(_grid);
            Expect.Call(businessRuleResponse.Overridden).Return(false).Repeat.AtLeastOnce();

            _mocks.ReplayAll();
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            Assert.IsFalse(_target.TryModify(new List<IScheduleDay>()));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyTryModifyWithCancel()
        {
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var businessRuleResponse = _mocks.StrictMock<IBusinessRuleResponse>();
            var handleBusinessRuleResponse = _mocks.StrictMock<IHandleBusinessRuleResponse>();
            IList<IBusinessRuleResponse> businessRuleResponses = new List<IBusinessRuleResponse> { businessRuleResponse };
            Expect.Call(businessRuleResponse.Mandatory).Return(false);
            Expect.Call(scheduleDictionary.Modify(ScheduleModifier.Scheduler, (IEnumerable<IScheduleDay>)null, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(
                businessRuleResponses);
            Expect.Call(_viewBase.HandleBusinessRuleResponse).Return(handleBusinessRuleResponse).Repeat.AtLeastOnce();
            handleBusinessRuleResponse.SetResponse(businessRuleResponses);
            LastCall.Repeat.Once();
            Expect.Call(handleBusinessRuleResponse.DialogResult).Return(DialogResult.Cancel);
            Expect.Call(_viewBase.ViewGrid).Return(_grid);
            Expect.Call(businessRuleResponse.Overridden).Return(false).Repeat.AtLeastOnce();

            _mocks.ReplayAll();
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            Assert.IsFalse(_target.TryModify(new List<IScheduleDay>()));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyTryModifyWithMandatoryRuleBrokenWhenTryingToOverride()
        {
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var businessRuleResponse = _mocks.StrictMock<IBusinessRuleResponse>();
            var handleBusinessRuleResponse = _mocks.StrictMock<IHandleBusinessRuleResponse>();
            IList<IBusinessRuleResponse> businessRuleResponses = new List<IBusinessRuleResponse> { businessRuleResponse };

            Expect.Call(scheduleDictionary.Modify(ScheduleModifier.Scheduler, (IEnumerable<IScheduleDay>)null, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(
                businessRuleResponses);
            Expect.Call(businessRuleResponse.Mandatory).Return(false);
            Expect.Call(_viewBase.HandleBusinessRuleResponse).Return(handleBusinessRuleResponse);
            Expect.Call(() => handleBusinessRuleResponse.SetResponse(businessRuleResponses));
            Expect.Call(handleBusinessRuleResponse.DialogResult).Return(DialogResult.OK);
            Expect.Call(businessRuleResponse.Overridden = true);
            Expect.Call(handleBusinessRuleResponse.ApplyToAll).Return(false);
            Expect.Call(scheduleDictionary.Modify(ScheduleModifier.Scheduler, (IEnumerable<IScheduleDay>)null, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(
                businessRuleResponses);
            Expect.Call(businessRuleResponse.Message).Return("testar");
            Expect.Call(() => _viewBase.ShowErrorMessage("", "")).IgnoreArguments();
            Expect.Call(businessRuleResponse.TypeOfRule).Return(typeof(int)).Repeat.AtLeastOnce();
            Expect.Call(businessRuleResponse.Overridden).Return(false).Repeat.AtLeastOnce();
            Expect.Call(_viewBase.ViewGrid).Return(_grid);
            _mocks.ReplayAll();

            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            Assert.IsFalse(_target.TryModify(new List<IScheduleDay>()));

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyTryModifyWithOverride()
        {
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var businessRuleResponse = _mocks.StrictMock<IBusinessRuleResponse>();
            var handleBusinessRuleResponse = _mocks.StrictMock<IHandleBusinessRuleResponse>();
            IList<IBusinessRuleResponse> businessRuleResponses = new List<IBusinessRuleResponse> { businessRuleResponse };

            Expect.Call(scheduleDictionary.Modify(ScheduleModifier.Scheduler, (IEnumerable<IScheduleDay>)null, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(
                businessRuleResponses);
            Expect.Call(businessRuleResponse.Mandatory).Return(false);
            Expect.Call(businessRuleResponse.TypeOfRule).Return(typeof(int)).Repeat.AtLeastOnce();
            Expect.Call(() => handleBusinessRuleResponse.SetResponse(businessRuleResponses));
            Expect.Call(_viewBase.HandleBusinessRuleResponse).Return(handleBusinessRuleResponse);
            Expect.Call(handleBusinessRuleResponse.DialogResult).Return(DialogResult.OK);
            Expect.Call(handleBusinessRuleResponse.ApplyToAll).Return(false);
            Expect.Call(businessRuleResponse.Overridden = true);
            Expect.Call(scheduleDictionary.Modify(ScheduleModifier.Scheduler, (IEnumerable<IScheduleDay>)null, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(
                new List<IBusinessRuleResponse>());
            Expect.Call(_viewBase.ViewGrid).Return(_grid);
            Expect.Call(businessRuleResponse.Overridden).Return(false).Repeat.AtLeastOnce();
            _mocks.ReplayAll();

            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            Assert.IsTrue(_target.TryModify(new List<IScheduleDay>()));

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyTryModifyWithOverrideDirect()
        {
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var businessRuleResponse = _mocks.StrictMock<IBusinessRuleResponse>();
            var allRules = NewBusinessRuleCollection.All(_schedulerState.SchedulingResultState);
            allRules.DoNotHaltModify(typeof(NightlyRestRule));

            Expect.Call(businessRuleResponse.Overridden = true);
            Expect.Call(businessRuleResponse.TypeOfRule).Return(typeof(NightlyRestRule)).Repeat.AtLeastOnce();
            Expect.Call(scheduleDictionary.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay>(), allRules, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).Return(
                new List<IBusinessRuleResponse>()).IgnoreArguments();
            Expect.Call(_viewBase.ViewGrid).Return(_grid);

            _mocks.ReplayAll();
            _overriddenBusinessRulesHolder.AddOverriddenRule(businessRuleResponse);
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            Assert.IsTrue(_target.TryModify(new List<IScheduleDay>()));
            _mocks.VerifyAll();
        }
        [Test]
        public void VerifyModifySchedulePart()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IPerson person = _mocks.StrictMock<IPerson>();
            IScheduleDictionary scheduleDictionary = CreateExpectationForModifySchedulePart(schedulePart, person);
            Expect.Call(_viewBase.ViewGrid).Return(_grid);

            _mocks.ReplayAll();
            _gridlockManager.AddLock(person, _date, LockType.Normal);
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            Assert.IsTrue(_target.ModifySchedulePart(new List<IScheduleDay> { schedulePart }));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyUpdateFromEditorWhenValidationFails()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IPersonAssignment personAssignment = _mocks.StrictMock<IPersonAssignment>();
            Assert.IsNull(_target.LastUnsavedSchedulePart);
            _target.UpdateFromEditor();

            Expect.Call(schedulePart.PersonAssignment()).Return(personAssignment);
            personAssignment.CheckRestrictions();
            LastCall.Throw(new ValidationException());
            _viewBase.ShowErrorMessage(string.Empty, string.Empty);
            LastCall.IgnoreArguments();

            _mocks.ReplayAll();
            _target.LastUnsavedSchedulePart = schedulePart;
            _target.UpdateFromEditor();
            _mocks.VerifyAll();
            Assert.IsNull(_target.LastUnsavedSchedulePart);
        }

        [Test]
        public void VerifyUpdateFromEditorWhenModifySchedulePartFails()
        {
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
            var personAssignment = _mocks.StrictMock<IPersonAssignment>();
            var businessRuleResponse = _mocks.StrictMock<IBusinessRuleResponse>();
            _businessRuleResponses = new List<IBusinessRuleResponse> { businessRuleResponse };

            ExpectCallsViewBaseOnVerifyUpdateFromEditorWhenModifySchedulePartFails(schedulePart, businessRuleResponse, personAssignment);


            _mocks.ReplayAll();
            _target.SchedulerState.SchedulingResultState.Schedules = _scheduleDictionary;
            _target.LastUnsavedSchedulePart = schedulePart;
            _target.UpdateFromEditor();
            _mocks.VerifyAll();
            Assert.IsNull(_target.LastUnsavedSchedulePart);
        }

        private void ExpectCallsViewBaseOnVerifyUpdateFromEditorWhenModifySchedulePartFails(IScheduleDay schedulePart, IBusinessRuleResponse businessRuleResponse, IPersonAssignment personAssignment)
        {
            var handleBusinessRuleResponse = _mocks.StrictMock<IHandleBusinessRuleResponse>();

            Expect.Call(() => _viewBase.ShowErrorMessage(string.Empty, string.Empty)).IgnoreArguments();
            Expect.Call(_viewBase.HandleBusinessRuleResponse).Return(handleBusinessRuleResponse);
            Expect.Call(_viewBase.ViewGrid).Return(_grid);

            Expect.Call(businessRuleResponse.Mandatory).Return(true);
            Expect.Call(businessRuleResponse.Message).Return("testar");
            Expect.Call(_scheduleDictionary.Modify(ScheduleModifier.Scheduler, (IEnumerable<IScheduleDay>)null, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(_businessRuleResponses);
            Expect.Call(schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_date, TimeZoneInfo.Utc));
            Expect.Call(schedulePart.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.PersonAssignment()).Return(personAssignment).Repeat.AtLeastOnce();
            Expect.Call(personAssignment.CheckRestrictions).Repeat.AtLeastOnce();
            Expect.Call(businessRuleResponse.Overridden).Return(false).Repeat.AtLeastOnce();
        }

        [Test]
        public void VerifyUpdateFromEditor()
        {
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
            var person = _mocks.StrictMock<IPerson>();
            var personAssignment = _mocks.StrictMock<IPersonAssignment>();
            IScheduleDictionary scheduleDictionary = CreateExpectationForModifySchedulePart(schedulePart, person);
            Expect.Call(schedulePart.PersonAssignment()).Return(personAssignment).Repeat.AtLeastOnce();
            Expect.Call(personAssignment.CheckRestrictions).Repeat.AtLeastOnce();
            Expect.Call(_viewBase.OnPasteCompleted);
            Expect.Call(_viewBase.ViewGrid).Return(_grid);

            _mocks.ReplayAll();
            _target.SchedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            _target.LastUnsavedSchedulePart = schedulePart;
            _target.UpdateFromEditor();
            _mocks.VerifyAll();
            Assert.IsNull(_target.LastUnsavedSchedulePart);
        }

        [Test]
        public void VerifyUpdateNoteFromEditor()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IList<IBusinessRuleResponse> businessRuleResponses = new List<IBusinessRuleResponse>();
            IScheduleDictionary scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            Expect.Call(scheduleDictionary.Modify(ScheduleModifier.Scheduler, (IEnumerable<IScheduleDay>)null, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(
                businessRuleResponses);

            _mocks.ReplayAll();
            _target.SchedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            _target.LastUnsavedSchedulePart = schedulePart;
            _target.UpdateNoteFromEditor();
            _mocks.VerifyAll();
            Assert.IsNull(_target.LastUnsavedSchedulePart);
        }

        [Test]
        public void VerifyUpdatePublicNoteFromEditor()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IList<IBusinessRuleResponse> businessRuleResponses = new List<IBusinessRuleResponse>();
            IScheduleDictionary scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            Expect.Call(scheduleDictionary.Modify(ScheduleModifier.Scheduler, (IEnumerable<IScheduleDay>)null, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(
                businessRuleResponses);

            _mocks.ReplayAll();
            _target.SchedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            _target.LastUnsavedSchedulePart = schedulePart;
            _target.UpdatePublicNoteFromEditor();
            _mocks.VerifyAll();
            Assert.IsNull(_target.LastUnsavedSchedulePart);
        }

		[Test]
		public void ShouldSkipUpdatesIfNoUnsavedPart()
		{
			//no calls to scheduleDictionary.Modify
			_target.LastUnsavedSchedulePart = null;
			_target.UpdateFromEditor();
			_target.UpdateNoteFromEditor();
			_target.UpdatePublicNoteFromEditor();
		}

		[Test]
		public void ShouldThrowExceptionIfNullSchedulePartsOnTryModify()
		{
			Assert.Throws<ArgumentNullException>(() => _target.TryModify(null));
		}

        [Test]
        public void VerifyAddAbsence()
        {
            _day1 = _mocks.StrictMock<IScheduleDay>();
            _day2 = _mocks.StrictMock<IScheduleDay>();
			var period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDate(_date, _date.AddDays(1), _timeZoneInfo);

            _personAssignment = new PersonAssignment(_person, _scenario, _date);

            ExpectCallsDialogOnVerifyAddAbsence(period);
            ExpectCallsViewBaseOnVerifyAddAbsence(period);
            Expect.Call(_day2.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_day1.Period).Return(new DateTimePeriod(2008, 11, 04, 2008, 11, 05)).Repeat.Any();
			Expect.Call(_day2.Period).Return(new DateTimePeriod(2008, 11, 04, 2008, 11, 05)).Repeat.Any();
			Expect.Call(() => _day1.CreateAndAddAbsence(null)).IgnoreArguments().Repeat.AtLeastOnce();
			Expect.Call(_day1.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
			Expect.Call(_day2.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
            var scheduleDictionary = CreateExpectationForModifySchedulePart(_day1, _person);
            Expect.Call(_day1.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());

            var range1 = _mocks.StrictMock<IScheduleRange>();
            Expect.Call(scheduleDictionary[_person]).Return(range1).Repeat.AtLeastOnce();
            Expect.Call(range1.ScheduledDay(_date))
                .Return(_day1);
			Expect.Call(range1.ScheduledDay(_date.AddDays(1)))
				.Return(_day1);
			Expect.Call(_day1.HasDayOff()).Return(false).Repeat.AtLeastOnce();
			Expect.Call(_day2.HasDayOff()).Return(false).Repeat.AtLeastOnce();
			Expect.Call(_viewBase.TimeZoneGuard).Return(new FakeTimeZoneGuard());

			_mocks.ReplayAll();
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            _target.AddAbsence();
            _mocks.VerifyAll();
        }

        private void ExpectCallsDialogOnVerifyAddAbsence(DateTimePeriod period)
        {
            Expect.Call(_dialog.Result).Return(true);
            Expect.Call(_dialog.SelectedItem).Return(_selectedItem);
            Expect.Call(_dialog.SelectedPeriod).Return(period);
        }

        private void ExpectCallsViewBaseOnVerifyAddAbsence(DateTimePeriod period)
        {
            Expect.Call(_viewBase.CurrentColumnSelectedSchedules()).Return(new List<IScheduleDay> { _day1 });
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { _day1, _day2 });
            Expect.Call(_viewBase.CreateAddAbsenceViewModel(null, null, TimeZoneInfo.Local))
                  .IgnoreArguments().Return(_dialog)
                  .Repeat.Once();
            Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(_person, period)).IgnoreArguments().Repeat.AtLeastOnce();
            Expect.Call(_viewBase.ViewGrid).Return(_grid).Repeat.AtLeastOnce();
        }

        [Test]
        public void VerifyAddAbsenceMultipleDays()
        {
            var periodPart1 = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDate(_date, _date.AddDays(1),_timeZoneInfo);
			var periodPart2 = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDate(_date.AddDays(1), _date.AddDays(2),_timeZoneInfo);
            var periodTotal = new DateTimePeriod(periodPart1.StartDateTime, periodPart2.EndDateTime);

            _personAssignment = new PersonAssignment(_person, _scenario, _date);
            _personAssignment2 = new PersonAssignment(_person, _scenario, _date.AddDays(1));
            _personAssignmentsList.Add(_personAssignment);
            _personAssignmentsList.Add(_personAssignment2);
            readOnlyCollection = new ReadOnlyCollection<IPersonAssignment>(_personAssignmentsList);

            ExpectCallsDialogOnVerifyAddAbsenceMultipleDays(periodTotal);
            ExpectCallsViewBaseOnVerifyAddAbsenceMultipleDays(periodTotal);
            ExpectCallsSchedulePartsOnVerifyAddAbsenceMultipleDays(periodPart1, periodPart2);
            
            var scheduleDictionary = CreateExpectationForModifySchedulePart(_day1, _person);

            // <expect that we call for tags for each days
            var range1 = _mocks.StrictMock<IScheduleRange>();
            Expect.Call(scheduleDictionary[_person]).Return(range1).Repeat.AtLeastOnce();
            Expect.Call(range1.ScheduledDay(_date))
                .Return(_day1);
            Expect.Call(range1.ScheduledDay(_date.AddDays(1)))
                .Return(_day1);
			Expect.Call(range1.ScheduledDay(_date.AddDays(2)))
				.Return(_day1);
			Expect.Call(_day2.HasDayOff()).Return(false);
			Expect.Call(_day1.HasDayOff()).Return(false);
			Expect.Call(_viewBase.TimeZoneGuard).Return(new FakeTimeZoneGuard());

			_mocks.ReplayAll();
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            _target.AddAbsence(_selectedSchedules, null);
            _mocks.VerifyAll();
        }

        private void ExpectCallsSchedulePartsOnVerifyAddAbsenceMultipleDays(DateTimePeriod periodPart1, DateTimePeriod periodPart2)
        {
            _day1 = _mocks.StrictMock<IScheduleDay>();
            _day2 = _mocks.StrictMock<IScheduleDay>();

            _selectedSchedules = new List<IScheduleDay> { _day1, _day2 };

            Expect.Call(_day1.Period).Return(periodPart1).Repeat.Any();
            Expect.Call(_day2.Period).Return(periodPart2).Repeat.Any();
            Expect.Call(_day2.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(() => _day1.CreateAndAddAbsence(null)).IgnoreArguments().Repeat.AtLeastOnce();
            Expect.Call(_day1.PersonAssignment()).Return(readOnlyCollection[0]).Repeat.AtLeastOnce();
            Expect.Call(_day2.PersonAssignment()).Return(readOnlyCollection[1]).Repeat.AtLeastOnce();
            Expect.Call(_day1.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());
        }

        private void ExpectCallsDialogOnVerifyAddAbsenceMultipleDays(DateTimePeriod period)
        {
            Expect.Call(_dialog.Result).Return(true);
            Expect.Call(_dialog.SelectedItem).Return(_selectedItem);
            Expect.Call(_dialog.SelectedPeriod).Return(period);
        }

        private void ExpectCallsViewBaseOnVerifyAddAbsenceMultipleDays(DateTimePeriod period)
        {
            Expect.Call(_viewBase.SelectedSchedules()).Return(_selectedSchedules);
            Expect.Call(_viewBase.CreateAddAbsenceViewModel(null, null, TimeZoneInfo.Local))
                  .IgnoreArguments()
                  .Return(_dialog);
			Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(_person, period)).IgnoreArguments().Repeat.Once();
        	Expect.Call(_viewBase.ViewGrid).Return(_grid).Repeat.AtLeastOnce();

        }

        [Test]
        public void ShouldNotAddAbsenceOnLockedDay()
        {
            var rangePeriod = new DateTimePeriod(2011, 1, 1, 2011, 1, 3);
            IScheduleRange range1 = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay day1 = _mocks.StrictMock<IScheduleDay>();
			IScheduleDay day2 = _mocks.StrictMock<IScheduleDay>();

            IDateOnlyAsDateTimePeriod periodPart1 = new DateOnlyAsDateTimePeriod(new DateOnly(2011, 1, 1), _timeZoneInfo);
			IDateOnlyAsDateTimePeriod periodPart2 = new DateOnlyAsDateTimePeriod(new DateOnly(2011, 1, 2), _timeZoneInfo);

			var absence = new Absence();
			var absenceLayer = new AbsenceLayer(absence, rangePeriod);

            using (_mocks.Record())
            {
                ExpectCallsDialogOnShouldNotAddAbsenceOnLockedDay(rangePeriod);
                ExpectCallsViewBaseOnShouldNotAddAbsenceOnLockedDay(rangePeriod);

                // expect that we call for tags for each days
                Expect.Call(_scheduleDictionary[_person]).Return(range1).Repeat.AtLeastOnce();
                Expect.Call(range1.ScheduledDay(new DateOnly(2011, 1, 1)))
                    .Return(day1);
				Expect.Call(range1.ScheduledDay(new DateOnly(2011, 1, 2)))
					.Return(day2);
                Expect.Call(day1.Person)
                    .Return(_person).Repeat.AtLeastOnce();
				Expect.Call(day2.Person)
					.Return(_person).Repeat.AtLeastOnce();
                Expect.Call(day1.DateOnlyAsPeriod)
                    .Return(periodPart1).Repeat.AtLeastOnce();
				Expect.Call(day2.DateOnlyAsPeriod)
					.Return(periodPart2).Repeat.AtLeastOnce();
				
				Expect.Call(day2.Period).Return(new DateTimePeriod(2011, 01, 02, 2011, 01, 03)).Repeat.AtLeastOnce();
				// the absence is addes to the last day
				Expect.Call(day2.CreateAndAddAbsence(null))
					.IgnoreArguments()
					.Return(new PersonAbsence(_person, _scenario, absenceLayer))
					.Repeat.Once();
				Expect.Call(day2.PersonAssignment()).Return(_ass).Repeat.AtLeastOnce();
            	Expect.Call(_ass.CheckRestrictions);

				Expect.Call(_scheduleDictionary.Scenario).Return(_scen).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDictionary.Modify(ScheduleModifier.Scheduler, (IEnumerable<IScheduleDay>)_day2, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(_businessRuleResponses);
 

            }

            using (_mocks.Playback())
            {
                _schedulerState.SchedulingResultState.Schedules = _scheduleDictionary;
                _day1 = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(2011, 1, 1), new FullPermission());
                _day2 = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(2011, 1, 2), new FullPermission());
                _selectedSchedules = new List<IScheduleDay> { _day1, _day2 };
                _gridlockManager.AddLock(new List<IScheduleDay> { _day1 }, LockType.Normal);
                _target.AddAbsence(_selectedSchedules, null);
            }
        }

        private void ExpectCallsViewBaseOnShouldNotAddAbsenceOnLockedDay(DateTimePeriod period)
        {
            Expect.Call(_viewBase.SelectedSchedules()).Return(_selectedSchedules);
			Expect.Call(_viewBase.CreateAddAbsenceViewModel(null, null, TimeZoneInfo.Local)).IgnoreArguments().Return(_dialog).Repeat.Once();
            Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(_person, period)).IgnoreArguments().Repeat.AtLeastOnce();
            Expect.Call(_viewBase.ViewGrid).Return(_grid);
			Expect.Call(_viewBase.TimeZoneGuard).Return(new FakeTimeZoneGuard());
		}

        private void ExpectCallsDialogOnShouldNotAddAbsenceOnLockedDay(DateTimePeriod period)
        {
            Expect.Call(_dialog.Result).Return(true);
            Expect.Call(_dialog.SelectedItem).Return(_selectedItem);
            Expect.Call(_dialog.SelectedPeriod).Return(period);
        }

		[Test]
		public void ShouldAddAbsenceWithDefaultPeriod()
		{
			var schedulePart = _mocks.StrictMock<IScheduleDay>();
			var scheduleRange = _mocks.StrictMock<IScheduleRange>();
			var date = new DateOnly(2009, 02, 02);

            _personAssignment = new PersonAssignment(_person, _scenario, date);

			_selectedSchedules = new List<IScheduleDay> { schedulePart };
			var startDateTime = _schedulerState.RequestedPeriod.Period().StartDateTime;
			var period = new DateTimePeriod(startDateTime.AddHours(3), startDateTime.AddHours(3.5));

			var absence = new Absence();
			var absenceLayer = new AbsenceLayer(absence, period);

			Expect.Call(schedulePart.Period).Return(DateTimeFactory.CreateDateTimePeriod(DateTime.SpecifyKind(date.Date, DateTimeKind.Utc), 0)).Repeat.Any();
			ExpectCallsDialogOnShouldAddAbsenceWithDefaultPeriod(period);
			ExpectCallsViewBaseOnShouldAddAbsenceWithDefaultPeriod(period);

			Expect.Call(schedulePart.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
			var scheduleDictionary = CreateExpectationForModifySchedulePart(schedulePart, _person);
			Expect.Call(schedulePart.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());

            Expect.Call(scheduleDictionary[_person])
				.Return(scheduleRange);
			Expect.Call(scheduleRange.ScheduledDay(date))
				.Return(schedulePart);
			//Expect.Call(schedulePart.HasDayOff()).Return(false).Repeat.AtLeastOnce();

			Expect.Call(schedulePart
				.CreateAndAddAbsence(null))
				.IgnoreArguments()
				.Return(new PersonAbsence(_person, _scenario, absenceLayer))
				.Repeat.Once();
		    

			_mocks.ReplayAll();
			_schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
			_target.AddAbsence(_selectedSchedules, period);
            
			_mocks.VerifyAll();
		}

        private void ExpectCallsViewBaseOnShouldAddAbsenceWithDefaultPeriod(DateTimePeriod period)
        {
            Expect.Call(_viewBase.SelectedSchedules()).Return(_selectedSchedules);
            Expect.Call(_viewBase.CreateAddAbsenceViewModel(null, null, TimeZoneInfo.Local))
                  .IgnoreArguments().Return(_dialog)
                  .Repeat.Once();
            Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(_person, period)).IgnoreArguments().Repeat.Once();
            Expect.Call(_viewBase.ViewGrid).Return(_grid);
			Expect.Call(_viewBase.TimeZoneGuard).Return(new FakeTimeZoneGuard());
		}

        private void ExpectCallsDialogOnShouldAddAbsenceWithDefaultPeriod(DateTimePeriod period)
        {
            Expect.Call(_dialog.Result).Return(true);
            Expect.Call(_dialog.SelectedItem).Return(_selectedItem);
            Expect.Call(_dialog.SelectedPeriod).Return(period);
        }

        [Test]
        public void VerifyAddAbsenceAccordingToDialogPeriodEvenIfOnlyOneDaySelected()
        {
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
			var scheduleRange = _mocks.StrictMock<IScheduleRange>();
            var selectedItem = _mocks.StrictMock<IAbsence>();
            var dialog = _mocks.StrictMock<IAddLayerViewModel<IAbsence>>();
			var period = new DateTimePeriod(2009, 2, 1, 2009, 2, 2);

			var absence = new Absence();
			var absenceLayer = new AbsenceLayer(absence, period);

            _personAssignment = new PersonAssignment(_person, _scenario, new DateOnly(new DateTime(2009,2,1)));
            _personAssignmentsList.Add(_personAssignment);
            readOnlyCollection = new ReadOnlyCollection<IPersonAssignment>(_personAssignmentsList);

            Expect.Call(schedulePart.Period).Return(period).Repeat.Any();
            Expect.Call(_viewBase.CurrentColumnSelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
			Expect.Call(_viewBase.CreateAddAbsenceViewModel(null, null, TimeZoneInfo.Local)).IgnoreArguments().Return(dialog);
            Expect.Call(dialog.Result).Return(true);
            Expect.Call(dialog.SelectedItem).Return(selectedItem);
            Expect.Call(dialog.SelectedPeriod).Return(period);
            Expect.Call(schedulePart.PersonAssignment()).Return(readOnlyCollection[0]).Repeat.AtLeastOnce();
            IScheduleDictionary scheduleDictionary = CreateExpectationForModifySchedulePart(schedulePart, _person);
            
            Expect.Call(_viewBase.ViewGrid).Return(_grid);
            Expect.Call(schedulePart.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());

			Expect.Call(scheduleDictionary[_person])
				.Return(scheduleRange).Repeat.AtLeastOnce();
			Expect.Call(scheduleRange.ScheduledDay(new DateOnly())).IgnoreArguments()
				.Return(schedulePart).Repeat.AtLeastOnce();
			Expect.Call(schedulePart.CreateAndAddAbsence(null))
					.IgnoreArguments()
					.Return(new PersonAbsence(_person, _scenario, absenceLayer))
					.Repeat.Once();
			Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(_person, period));
			Expect.Call(schedulePart.HasDayOff()).Return(false).Repeat.AtLeastOnce();
			Expect.Call(_viewBase.TimeZoneGuard).Return(new FakeTimeZoneGuard());
			_mocks.ReplayAll();
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            _target.AddAbsence();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyAddAbsenceWhenUserCancelsDialog()
        {
            DateTimePeriod period = new DateTimePeriod(2001, 1, 1, 2001, 1, 2);
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IAddLayerViewModel<IAbsence> dialog = _mocks.StrictMock<IAddLayerViewModel<IAbsence>>();

            _personAssignment = new PersonAssignment(_person, _scenario, new DateOnly(new DateTime(2001,1,1)));

            
            readOnlyCollection = new ReadOnlyCollection<IPersonAssignment>(_personAssignmentsList);

            Expect.Call(schedulePart.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_viewBase.CurrentColumnSelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(schedulePart.Period).Return(period).Repeat.Any();
            Expect.Call(schedulePart.PersonAssignment()).Return(readOnlyCollection[0]).Repeat.AtLeastOnce();
	        Expect.Call(schedulePart.HasDayOff()).Return(false);
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
			Expect.Call(_viewBase.CreateAddAbsenceViewModel(null, null, TimeZoneInfo.Local)).IgnoreArguments().Return(dialog);
            Expect.Call(dialog.Result).Return(false);
            LastCall.Repeat.Once();
            Expect.Call(schedulePart.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			Expect.Call(_viewBase.TimeZoneGuard).Return(new FakeTimeZoneGuard());

			_mocks.ReplayAll();
            _target.AddAbsence();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCatchValidationExceptionWhenAssignmentTooLong()
        {
            var pa = _mocks.StrictMock<IPersonAssignment>();
            var scheduleDay = _mocks.DynamicMock<IScheduleDay>();
            IList<IScheduleDay> list = new List<IScheduleDay> { scheduleDay };
            var dialog = _mocks.StrictMock<IAddActivityViewModel>();
            var selectedItem = _mocks.StrictMock<IActivity>();
            var shiftCategory = _mocks.StrictMock<IShiftCategory>();

            Expect.Call(_viewBase.CreateAddActivityViewModel(
                    null,
                    new List<IShiftCategory> { ShiftCategoryFactory.CreateShiftCategory("Test") },
                    new DateTimePeriod(2001, 1, 1, 2001, 1, 2),
                    null, null)).IgnoreArguments().Return(dialog);

            Expect.Call(() => scheduleDay.CreateAndAddActivity(null, new DateTimePeriod(),  shiftCategory)).IgnoreArguments().Repeat.Once();
            Expect.Call(scheduleDay.PersonAssignment()).Return(pa);
            Expect.Call(scheduleDay.Period).Return(new DateTimePeriod()).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
            Expect.Call(pa.Period).Return(new DateTimePeriod()).Repeat.AtLeastOnce();
            Expect.Call(dialog.Result).Return(true);
            Expect.Call(dialog.SelectedItem).Return(selectedItem);
            Expect.Call(dialog.SelectedShiftCategory).Return(shiftCategory);
            Expect.Call(dialog.SelectedPeriod).Return(new DateTimePeriod());
            Expect.Call(() => pa.CheckRestrictions()).Throw(new ValidationException());
            Expect.Call(() => _viewBase.ShowErrorMessage(string.Empty, string.Empty)).IgnoreArguments();
			Expect.Call(_viewBase.TimeZoneGuard).Return(new FakeTimeZoneGuard());

			_mocks.ReplayAll();
            _target.AddActivity(list, null, null);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyGetDefaultPeriodFromPart()
        {
            DateTime startTime = new DateTime(2001, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            DateTime endTime = new DateTime(2001, 1, 1, 15, 0, 0, DateTimeKind.Utc);
            DateTimePeriod assPeriod = new DateTimePeriod(startTime, endTime);
            ScheduleParameters parameters =
                new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), PersonFactory.CreatePerson(), new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
            IDictionary<IPerson, IScheduleRange> underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
            DateTimePeriod rangePeriod = new DateTimePeriod(2000, 1, 1, 2001, 6, 1);
            ScheduleDictionaryForTest dic = new ScheduleDictionaryForTest(parameters.Scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
            IScheduleDay part = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2001, 1, 1), new FullPermission());

            DateTimePeriod period = AddActivityCommand.GetDefaultPeriodFromPart(part);

            Assert.AreEqual(TimeZoneHelper.ConvertToUtc(new DateTime(2000, 1, 1, 8, 0, 0), part.TimeZone).Hour, period.StartDateTime.Hour);
            Assert.AreEqual(TimeZoneHelper.ConvertToUtc(new DateTime(2000, 1, 1, 17, 0, 0), part.TimeZone).Hour, period.EndDateTime.Hour);

            IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Person, parameters.Scenario, assPeriod);

            part.Add(ass);

            period = AddActivityCommand.GetDefaultPeriodFromPart(part);
            Assert.AreEqual(startTime.Hour, period.StartDateTime.Hour);
            Assert.AreEqual(endTime.Hour, period.EndDateTime.Hour);
        }

        [Test]
        public void VerifyAddActivityWhenUserCancelsDialog()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IPersonAssignment pa = _mocks.StrictMock<IPersonAssignment>();
            var person = _mocks.StrictMock<IPerson>();
            IAddActivityViewModel dialog = _mocks.StrictMock<IAddActivityViewModel>();
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(schedulePart.Person).Return(person).Repeat.AtLeastOnce();
            Expect.Call(_viewBase.CreateAddActivityViewModel(null, new List<IShiftCategory> { ShiftCategoryFactory.CreateShiftCategory("Test") }, new DateTimePeriod(2001, 1, 1, 2001, 1, 2), null, null)).IgnoreArguments().Return(dialog);
            Expect.Call(dialog.Result).Return(false);

            LastCall.Repeat.Once();
            Expect.Call(schedulePart.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.PersonAssignment()).Return(pa).Repeat.AtLeastOnce();
            Expect.Call(pa.Period).Return(new DateTimePeriod()).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.Period).Return(new DateTimePeriod()).Repeat.AtLeastOnce();
			Expect.Call(_viewBase.TimeZoneGuard).Return(new FakeTimeZoneGuard());

			_mocks.ReplayAll();
            _target.AddActivity();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyAddActivityWithSchedulePartNull()
        {
            IScheduleDay schedulePart = null;
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            _viewBase.ShowInformationMessage(UserTexts.Resources.NoExistingShiftSelected, UserTexts.Resources.Information);

            _mocks.ReplayAll();
            _target.AddActivity();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyAddPersonalActivityWithSchedulePartNull()
        {
            IScheduleDay schedulePart = null;
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            _viewBase.ShowInformationMessage(UserTexts.Resources.NoExistingShiftSelected, UserTexts.Resources.Information);

            _mocks.ReplayAll();
            _target.AddPersonalShift();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyAddOvertimeWithSchedulePartNull()
        {
            IScheduleDay schedulePart = null;
            _viewBase.ShowInformationMessage(UserTexts.Resources.NoExistingShiftSelected, UserTexts.Resources.Information);

            _mocks.ReplayAll();
            _target.AddOvertime(new List<IScheduleDay> { schedulePart }, null, new List<IMultiplicatorDefinitionSet>());
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyAddAbsenceWithSchedulePartNull()
        {
            IScheduleDay schedulePart = null;
            Expect.Call(_viewBase.CurrentColumnSelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            _viewBase.ShowInformationMessage(UserTexts.Resources.NoExistingShiftSelected, UserTexts.Resources.Information);

            _mocks.ReplayAll();
            _target.AddAbsence();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyAddActivityWithPeriodAndSchedulePartNull()
        {
            DateTimePeriod period = new DateTimePeriod();
            IScheduleDay schedulePart = null;
            _viewBase.ShowInformationMessage(UserTexts.Resources.NoExistingShiftSelected, UserTexts.Resources.Information);

            _mocks.ReplayAll();
            _target.AddActivity(new List<IScheduleDay> { schedulePart }, period, null);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyAddOvertimeTheNewWay()
        {
            _multiplicatorDefinitionSets.Add(_definitionSet);
            _day1 = _mocks.StrictMock<IScheduleDay>();

            var period1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2008, 1, 1));
            period1.PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime));
            _person.AddPersonPeriod(period1);
            var period = _schedulerState.RequestedPeriod.Period();

            ExpectCallsViewBaseOnVerifyAddOvertimeTheNewWay(period);
            ExpectCallsDialogOnVerifyAddOvertimeTheNewWay(period);
            ExpectCallsScheduleDayOnVerifyAddOvertimeTheNewWay();
            Expect.Call(_day1.Period).Return(new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
            Expect.Call(_ass.CheckRestrictions);
	        Expect.Call(_ass.MainActivities()).Return(new List<MainShiftLayer>());
	        Expect.Call(_ass.OvertimeActivities()).Return(new List<OvertimeShiftLayer>());
			Expect.Call(_viewBase.TimeZoneGuard).Return(new FakeTimeZoneGuard());

			var scheduleDictionary = CreateExpectationForModifySchedulePart(_day1, _person);

            _mocks.ReplayAll();
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            _target.AddOvertime(_multiplicatorDefinitionSets);
            _mocks.VerifyAll();
        }

        private void ExpectCallsScheduleDayOnVerifyAddOvertimeTheNewWay()
        {
            Expect.Call(_day1.PersonAssignment()).Return(_ass).Repeat.AtLeastOnce();
            Expect.Call(() => _day1.CreateAndAddOvertime(null, new DateTimePeriod(), null)).IgnoreArguments();
        }

        private void ExpectCallsDialogOnVerifyAddOvertimeTheNewWay(DateTimePeriod period)
        {
            Expect.Call(_overtimeDialog.Result).Return(true);
            Expect.Call(_overtimeDialog.SelectedItem).Return(_selectedActivity);
            Expect.Call(_overtimeDialog.SelectedPeriod).Return(period);
            Expect.Call(_overtimeDialog.SelectedMultiplicatorDefinitionSet).Return(_definitionSet);
        }

        private void ExpectCallsViewBaseOnVerifyAddOvertimeTheNewWay(DateTimePeriod period)
        {
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { _day1 });
			Expect.Call(_viewBase.CreateAddOvertimeViewModel( null, _multiplicatorDefinitionSets, null, new DateTimePeriod(2001, 1, 1, 2001, 1, 2), TimeZoneInfo.Local)).IgnoreArguments().Return(_overtimeDialog);

            Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(_person, period));
            Expect.Call(_viewBase.ViewGrid).Return(_grid);
        }

        [Test]
        public void CanReturnCorrectDefinitionSetsAccordingToContract()
        {
            IMultiplicatorDefinitionSet overtimeSet1 = new MultiplicatorDefinitionSet("Overtimeset1",
                                                                                      MultiplicatorType.Overtime);
            IMultiplicatorDefinitionSet overtimeSet2 = new MultiplicatorDefinitionSet("Overtimeset2",
                                                                                      MultiplicatorType.Overtime);
            IMultiplicatorDefinitionSet obTimeSet1 = new MultiplicatorDefinitionSet("Overtimeset1",
                                                                                      MultiplicatorType.OBTime);
            IList<IMultiplicatorDefinitionSet> definitionSets = new List<IMultiplicatorDefinitionSet> { overtimeSet1, overtimeSet2, obTimeSet1 };
            IScheduleDay part1 = _mocks.StrictMock<IScheduleDay>();
            IScheduleDay part2 = _mocks.StrictMock<IScheduleDay>();
            IList<IScheduleDay> scheduleParts = new List<IScheduleDay> { part1, part2 };

            IPerson person1 = PersonFactory.CreatePerson("Person1");
            IPerson person2 = PersonFactory.CreatePerson("Person1");
            IPersonPeriod period1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2009, 1, 1));
            IPersonPeriod period2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2009, 1, 1));
            var schedulePartPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 1, 1), (TimeZoneInfo.Utc));
            period1.PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(overtimeSet1);
            period2.PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(overtimeSet2);
            period2.PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(overtimeSet1);
            period2.PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(obTimeSet1);

            person1.AddPersonPeriod(period1);
            person2.AddPersonPeriod(period2);

            Expect.Call(part1.Person).Return(person1).Repeat.Any();
            Expect.Call(part2.Person).Return(person1).Repeat.Any();
            Expect.Call(part1.DateOnlyAsPeriod).Return(schedulePartPeriod).Repeat.Any();
            Expect.Call(part2.DateOnlyAsPeriod).Return(schedulePartPeriod).Repeat.Any();

            _mocks.ReplayAll();
            IList<IMultiplicatorDefinitionSet> returnSets = AddOvertimeCommand.DefinitionSetsAccordingToSchedule(scheduleParts, definitionSets);
            Assert.AreEqual(overtimeSet1, returnSets[0]);
            Assert.AreEqual(1, returnSets.Count);
            _mocks.VerifyAll();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyAddOvertimeNewWhenUserCancelsDialog()
        {
            var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2001, 1, 1), (TimeZoneInfo.Local));
            var person = _mocks.StrictMock<IPerson>();
            var ass = _mocks.StrictMock<IPersonAssignment>();
            IList<IMultiplicatorDefinitionSet> multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
            var dialog = _mocks.StrictMock<IAddOvertimeViewModel>();
            Expect.Call(schedulePart.Period).Return(new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
			Expect.Call(_viewBase.CreateAddOvertimeViewModel( null, multiplicatorDefinitionSets, null, new DateTimePeriod(2001, 1, 1, 2001, 1, 2), TimeZoneInfo.Local)).IgnoreArguments().Return(dialog);
            Expect.Call(schedulePart.PersonAssignment()).Return(ass);
			Expect.Call(ass.MainActivities()).Return(new List<MainShiftLayer>());
            Expect.Call(dialog.Result).Return(false);
            LastCall.Repeat.Once();
            Expect.Call(schedulePart.Person).Return(person).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod);
            Expect.Call(person.IsAgent(new DateOnly(2001,1,1))).Return(true);
			Expect.Call(ass.OvertimeActivities()).Return(new List<OvertimeShiftLayer>());
			Expect.Call(_viewBase.TimeZoneGuard).Return(new FakeTimeZoneGuard());

			_mocks.ReplayAll();
            _target.AddOvertime(multiplicatorDefinitionSets);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldNotAddOvertimeWhenNoPeriod()
        {
            var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2001, 1, 1), (TimeZoneInfo.Local));
            var person = _mocks.StrictMock<IPerson>();
            var ass = _mocks.StrictMock<IPersonAssignment>();
            IList<IMultiplicatorDefinitionSet> multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(schedulePart.PersonAssignment()).Return(ass);
			Expect.Call(ass.MainActivities()).Return(new List<MainShiftLayer>());
            Expect.Call(schedulePart.Period).Return(new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
            Expect.Call(schedulePart.Person).Return(person).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod);
            Expect.Call(person.IsAgent(new DateOnly(2001, 1, 1))).Return(false);
            Expect.Call(() => _viewBase.ShowInformationMessage(UserTexts.Resources.CouldNotAddOverTimeNoPersonPeriods, UserTexts.Resources.Information));
	        Expect.Call(ass.OvertimeActivities()).Return(new List<OvertimeShiftLayer>());

            _mocks.ReplayAll();
            _target.AddOvertime(multiplicatorDefinitionSets);
            _mocks.VerifyAll();
        }

       
        [Test]
        public void ShouldAddActivityToOneSchedulePartPerAgent()
        {
            IScheduleDictionary scheduleDictionary;
            var schedulePartDayOne = _mocks.StrictMock<IScheduleDay>();
            var schedulePartDayTwo = _mocks.StrictMock<IScheduleDay>();

            var dialog = _mocks.StrictMock<IAddActivityViewModel>();
            var person = _mocks.StrictMock<IPerson>();
        	var startDateTime = _schedulerState.RequestedPeriod.Period().StartDateTime;
            var defaultPeriod = new DateTimePeriod(startDateTime.AddHours(8), startDateTime.AddHours(17));
            var personAssignment = _mocks.StrictMock<IPersonAssignment>();

            using (_mocks.Record())
            {
                scheduleDictionary = CreateExpectationForModifySchedulePart(schedulePartDayOne, person);
                Expect.Call(schedulePartDayTwo.Person).Return((person));
                Expect.Call(schedulePartDayOne.Period).Return(defaultPeriod);
                Expect.Call(schedulePartDayOne.SignificantPart()).Return(SchedulePartView.None);
                AddActivityDialogExpectations(dialog, defaultPeriod);
                Expect.Call(() => schedulePartDayOne.CreateAndAddActivity(null, new DateTimePeriod(), null)).IgnoreArguments();
                Expect.Call(schedulePartDayOne.PersonAssignment()).Return(personAssignment).Repeat.Twice();
                Expect.Call(personAssignment.CheckRestrictions).Repeat.Twice();
				Expect.Call(_viewBase.TimeZoneGuard).Return(new FakeTimeZoneGuard());
				AddActivityViewBaseExpectations(schedulePartDayOne, schedulePartDayTwo, defaultPeriod, dialog, person);
            }

            using (_mocks.Playback())
            {
                _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
                _target.AddActivity();
            }
        }

        private void AddActivityViewBaseExpectations(IScheduleDay schedulePartDayOne, IScheduleDay schedulePartDayTwo, DateTimePeriod defaultPeriod, IAddActivityViewModel dialog, IPerson person)
        {
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePartDayOne, schedulePartDayTwo });
            Expect.Call(_viewBase.CreateAddActivityViewModel(null, null, defaultPeriod, (TimeZoneInfo.Local), null)).IgnoreArguments().Return(dialog);
            Expect.Call(_viewBase.ViewGrid).Return(_grid);
            Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(person, defaultPeriod));
        }

        private void AddActivityDialogExpectations(IAddActivityViewModel dialog, DateTimePeriod defaultPeriod)
        {
            var selectedItem = _mocks.StrictMock<IActivity>();
            var selectedShiftCategory = _mocks.StrictMock<IShiftCategory>();

            Expect.Call(dialog.Result).Return(true);
            Expect.Call(dialog.SelectedItem).Return(selectedItem);
            Expect.Call(dialog.SelectedPeriod).Return(defaultPeriod);
            Expect.Call(dialog.SelectedShiftCategory).Return(selectedShiftCategory);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldUseMainShiftPeriodAsDefaultWhenAddingPersonalShift()
        {
            var ass = _mocks.StrictMock<IPersonAssignment>();
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
            var selectedItem = _mocks.StrictMock<IActivity>();
            var dialog = _mocks.StrictMock<IAddLayerViewModel<IActivity>>();
            var person = _mocks.StrictMock<IPerson>();

        	var startDateTime = _schedulerState.RequestedPeriod.Period().StartDateTime;
            var period = new DateTimePeriod(startDateTime.AddHours(3), startDateTime.AddHours(3.5));
	        var shiftLayer = new MainShiftLayer(new Activity("_"), new DateTimePeriod(2017, 1, 1, 8, 2017, 1, 1, 9));

            Expect.Call(schedulePart.Period).Return(new DateTimePeriod(2001, 1, 1, 2001, 1, 2)).Repeat.Twice();
            Expect.Call(ass.Period).Return(period);
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(_viewBase.CreateAddPersonalActivityViewModel(null, new DateTimePeriod(2001, 1, 1, 2001, 1, 2),
                                                                    (TimeZoneInfo.Local))).Constraints(Is.Anything(),
                                                                                                        Is.Equal(period), Is.Anything()).Return(dialog);
            Expect.Call(dialog.Result).Return(true);
            Expect.Call(dialog.SelectedItem).Return(selectedItem);
            Expect.Call(dialog.SelectedPeriod).Return(period);
            Expect.Call(() => schedulePart.CreateAndAddPersonalActivity(null, new DateTimePeriod())).IgnoreArguments();
            Expect.Call(schedulePart.PersonAssignment()).Return(ass).Repeat.AtLeastOnce();
            Expect.Call(ass.CheckRestrictions);
            IScheduleDictionary scheduleDictionary = CreateExpectationForModifySchedulePart(schedulePart, person);
            Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(person, period));
            Expect.Call(_viewBase.ViewGrid).Return(_grid);
            Expect.Call(schedulePart.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());
	        Expect.Call(ass.ShiftLayers).Return(new List<ShiftLayer> {shiftLayer});
			Expect.Call(_viewBase.TimeZoneGuard).Return(new FakeTimeZoneGuard());
			_mocks.ReplayAll();
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            _target.AddPersonalShift();
            _mocks.VerifyAll();
        }

        private IScheduleDictionary CreateExpectationForModifySchedulePart(IScheduleDay schedulePart, IPerson person)
        {
            IList<IBusinessRuleResponse> businessRuleResponses = new List<IBusinessRuleResponse>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            Expect.Call(scheduleDictionary.Modify(ScheduleModifier.Scheduler, (IEnumerable<IScheduleDay>)null, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(
                businessRuleResponses).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_date, TimeZoneInfo.Utc)).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.Person).Return(person).Repeat.AtLeastOnce();
            return scheduleDictionary;
        }

        [Test]
        public void VerifyAddPersonalShiftWhenUserCancelsDialog()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IAddLayerViewModel<IActivity> dialog = _mocks.StrictMock<IAddLayerViewModel<IActivity>>();
            var person = _mocks.StrictMock<IPerson>();
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(schedulePart.Person).Return(person).Repeat.Twice();
            Expect.Call(schedulePart.Period).Return(new DateTimePeriod(2001, 1, 1, 2001, 1, 2)).Repeat.Twice();
            Expect.Call(schedulePart.PersonAssignment()).Return(null);
            Expect.Call(_viewBase.CreateAddPersonalActivityViewModel(null, new DateTimePeriod(2001, 1, 1, 2001, 1, 2), (TimeZoneInfo.Local))).IgnoreArguments().Return(dialog);
            Expect.Call(dialog.Result).Return(false);
            Expect.Call(schedulePart.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			Expect.Call(_viewBase.TimeZoneGuard).Return(new FakeTimeZoneGuard());

			_mocks.ReplayAll();
            _target.AddPersonalShift();
            _mocks.VerifyAll();
        }

        [Test]
        public void MergeHeaders()
        {
            _target.MergeHeaders();
            Assert.AreEqual(28, _target.ColWeekMap.Count);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "Teleopti.Ccc.WinCode.Scheduling.PersonNameComparer.#ctor"), Test]
        public void ShouldHandleNullValuesInPersonNameComparer()
        {
			//with specified culture
            Assert.AreEqual(0, _personNameComparer.Compare(null, null));
            Assert.AreEqual(-1, _personNameComparer.Compare(null, "a"));
            Assert.AreEqual(1, _personNameComparer.Compare("a", null));
            Assert.AreEqual(-1, _personNameComparer.Compare("a", "b"));

			//and with current culture
			_personNameComparer = new PersonNameComparer();
			Assert.AreEqual(0, _personNameComparer.Compare(null, null));
			Assert.AreEqual(-1, _personNameComparer.Compare(null, "a"));
			Assert.AreEqual(1, _personNameComparer.Compare("a", null));
			Assert.AreEqual(-1, _personNameComparer.Compare("a", "b"));
        }

		[Test]
		public void ShouldSortOnTime()
		{
			var dateOnly = new DateOnly();
			var sortCommand = _mocks.StrictMock<IScheduleSortCommand>();

			using (_mocks.Record())
			{
				Expect.Call(() => sortCommand.Execute(dateOnly));
			}

			using (_mocks.Playback())
			{
				_target.SortCommand = sortCommand;
				_target.SortOnTime(dateOnly);	
			}	
		}

		[Test]
		public void ShouldCompareContractTime()
		{
			var comparer = new ContractTimeComparer(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture);
			Assert.AreEqual(0, comparer.Compare(null, null));
			Assert.AreEqual(-1, comparer.Compare(null, "2"));
			Assert.AreEqual(1, comparer.Compare("1", null));
			
			Assert.AreEqual(1, comparer.Compare(1, TimeSpan.FromHours(2)));
			Assert.AreEqual(-1, comparer.Compare(TimeSpan.FromHours(1), TimeSpan.FromHours(2)));
			Assert.AreEqual(1, comparer.Compare(TimeSpan.FromHours(2), TimeSpan.FromHours(1)));
			Assert.AreEqual(0, comparer.Compare(TimeSpan.FromHours(1), TimeSpan.FromHours(1)));
		}

		[Test]
		public void ShouldCompareDayOffCount()
		{
			var comparer = new ContractTimeComparer(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture);
			Assert.AreEqual(0, comparer.Compare(null, null));
			Assert.AreEqual(-1, comparer.Compare(null, "2"));
			Assert.AreEqual(1, comparer.Compare("1", null));

			Assert.AreEqual(-1, comparer.Compare("1", 2));
			Assert.AreEqual(-1, comparer.Compare(1, 2));
			Assert.AreEqual(1, comparer.Compare(2, 1));
			Assert.AreEqual(0, comparer.Compare(1, 1));
		}
    }
}
