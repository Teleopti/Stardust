﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands;
using Teleopti.Interfaces.Domain;
using Is = Rhino.Mocks.Constraints.Is;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
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
        private ICccTimeZoneInfo _timeZoneInfo;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _gridlockManager = new GridlockManager();
            _clipHandlerSchedulePart = new ClipHandler<IScheduleDay>();
            _period = new DateOnlyPeriod(2009, 2, 2, 2009, 3, 1);
            _schedulerState = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_period,TeleoptiPrincipal.Current.Regional.TimeZone), new List<IPerson>());
            _overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();

            createMockObjects();

            _target = new SchedulePresenterBase(_viewBase, _schedulerState, _gridlockManager, _clipHandlerSchedulePart, SchedulePartFilter.None, _overriddenBusinessRulesHolder, _scheduleDayChangeCallback, NullScheduleTag.Instance);
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
            _timeZoneInfo = new CccTimeZoneInfo(zone);
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
            _personNameComparer = new PersonNameComparer(TeleoptiPrincipal.Current.Regional.Culture);
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
            Assert.AreEqual(0, _target.TimelineHourPositions.Count);
            Assert.AreEqual(0, _target.TimelineSpan.Count);
            Assert.IsNull(_target.LastUnsavedSchedulePart);
            Assert.AreEqual(1, _target.CurrentSortColumn);
            Assert.IsTrue(_target.IsAscendingSort);
            _target.SortColumn(1);
            Assert.IsFalse(_target.IsAscendingSort);


            var lastUnsavedSchedulePart = _mocks.StrictMock<IScheduleDay>();

            var period = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(_date,_date), TeleoptiPrincipal.Current.Regional.TimeZone);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyScheduleSort()
        {
            var range1 = _mocks.StrictMock<IScheduleRange>();
            var range2 = _mocks.StrictMock<IScheduleRange>();
            var range3 = _mocks.StrictMock<IScheduleRange>();
            var range4 = _mocks.StrictMock<IScheduleRange>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var person1 = _mocks.StrictMock<IPerson>();
            var person2 = _mocks.StrictMock<IPerson>();
            var person3 = _mocks.StrictMock<IPerson>();
            var person4 = _mocks.StrictMock<IPerson>();
            var permissionInformation = _mocks.StrictMock<IPermissionInformation>();
            ISchedulePeriod schedulePeriod1 = new SchedulePeriod(new DateOnly(2009, 02, 02), SchedulePeriodType.Day, 28);
            ISchedulePeriod schedulePeriod2 = new SchedulePeriod(new DateOnly(2009, 02, 02), SchedulePeriodType.Day, 28);
            ISchedulePeriod schedulePeriod3 = new SchedulePeriod(new DateOnly(2009, 02, 02), SchedulePeriodType.Day, 28);
            ISchedulePeriod schedulePeriod4 = new SchedulePeriod(new DateOnly(2009, 02, 02), SchedulePeriodType.Day, 28);

            var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2009, 1, 1), new DateOnly(2009, 1, 1).AddDays(14));
            schedulePeriod1.SetParent(person1);
            schedulePeriod2.SetParent(person2);
            schedulePeriod3.SetParent(person3);
            schedulePeriod4.SetParent(person4);

            IList<ISchedulePeriod> schedulePeriods1 = new List<ISchedulePeriod> { schedulePeriod1 };
            IList<ISchedulePeriod> schedulePeriods2 = new List<ISchedulePeriod> { schedulePeriod2 };
            IList<ISchedulePeriod> schedulePeriods3 = new List<ISchedulePeriod> { schedulePeriod3 };
            IList<ISchedulePeriod> schedulePeriods4 = new List<ISchedulePeriod> { schedulePeriod4 };


            Expect.Call(person1.Name).Return(new Name("a", "d")).Repeat.AtLeastOnce();
            Expect.Call(person1.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(person2.Name).Return(new Name("b", "c")).Repeat.AtLeastOnce();
            Expect.Call(person2.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(person3.Name).Return(new Name("c", "b")).Repeat.AtLeastOnce();
            Expect.Call(person3.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(person4.Name).Return(new Name("d", "a")).Repeat.AtLeastOnce();
            Expect.Call(person4.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(_viewBase.TheGrid).Return(_grid).Repeat.AtLeastOnce();

            Expect.Call(person1.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();
            Expect.Call(person2.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();
            Expect.Call(person3.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();
            Expect.Call(person4.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();

            Expect.Call(permissionInformation.Culture()).Return(CultureInfo.CurrentCulture).Repeat.AtLeastOnce();

            Expect.Call(person1.PersonSchedulePeriods(dateOnlyPeriod)).IgnoreArguments().Return(schedulePeriods1).Repeat.AtLeastOnce();
            Expect.Call(person2.PersonSchedulePeriods(dateOnlyPeriod)).IgnoreArguments().Return(schedulePeriods2).Repeat.AtLeastOnce();
            Expect.Call(person3.PersonSchedulePeriods(dateOnlyPeriod)).IgnoreArguments().Return(schedulePeriods3).Repeat.AtLeastOnce();
            Expect.Call(person4.PersonSchedulePeriods(dateOnlyPeriod)).IgnoreArguments().Return(schedulePeriods4).Repeat.AtLeastOnce();

            Expect.Call(scheduleDictionary[person1]).Return(range1).Repeat.AtLeastOnce();
            Expect.Call(scheduleDictionary[person2]).Return(range2).Repeat.AtLeastOnce();
            Expect.Call(scheduleDictionary[person3]).Return(range3).Repeat.AtLeastOnce();
            Expect.Call(scheduleDictionary[person4]).Return(range4).Repeat.AtLeastOnce();

            Expect.Call(range1.CalculatedTargetScheduleDaysOff).Return(null).Repeat.Once();
            Expect.Call(range2.CalculatedTargetScheduleDaysOff).Return(null).Repeat.Once();
            Expect.Call(range3.CalculatedTargetScheduleDaysOff).Return(null).Repeat.Once();
            Expect.Call(range4.CalculatedTargetScheduleDaysOff).Return(null).Repeat.Once();

            Expect.Call(() => range1.CalculatedTargetScheduleDaysOff = 4).Repeat.AtLeastOnce();
            Expect.Call(() => range2.CalculatedTargetScheduleDaysOff = 3).Repeat.AtLeastOnce();
            Expect.Call(() => range3.CalculatedTargetScheduleDaysOff = 2).Repeat.AtLeastOnce();
            Expect.Call(() => range4.CalculatedTargetScheduleDaysOff = 1).Repeat.AtLeastOnce();

            Expect.Call(range1.CalculatedTargetScheduleDaysOff).Return(4).Repeat.AtLeastOnce();
            Expect.Call(range2.CalculatedTargetScheduleDaysOff).Return(3).Repeat.AtLeastOnce();
            Expect.Call(range3.CalculatedTargetScheduleDaysOff).Return(2).Repeat.AtLeastOnce();
            Expect.Call(range4.CalculatedTargetScheduleDaysOff).Return(1).Repeat.AtLeastOnce();

            var vPeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            var vPeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            var vPeriod3 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            var vPeriod4 = _mocks.StrictMock<IVirtualSchedulePeriod>();

            Expect.Call(person1.VirtualSchedulePeriod(new DateOnly())).IgnoreArguments().Return(vPeriod1).Repeat.AtLeastOnce();
            Expect.Call(person2.VirtualSchedulePeriod(new DateOnly())).IgnoreArguments().Return(vPeriod2).Repeat.AtLeastOnce();
            Expect.Call(person3.VirtualSchedulePeriod(new DateOnly())).IgnoreArguments().Return(vPeriod3).Repeat.AtLeastOnce();
            Expect.Call(person4.VirtualSchedulePeriod(new DateOnly())).IgnoreArguments().Return(vPeriod4).Repeat.AtLeastOnce();
            Expect.Call(vPeriod1.IsValid).Return(true).Repeat.AtLeastOnce();
            Expect.Call(vPeriod2.IsValid).Return(true).Repeat.AtLeastOnce();
            Expect.Call(vPeriod3.IsValid).Return(true).Repeat.AtLeastOnce();
            Expect.Call(vPeriod4.IsValid).Return(true).Repeat.AtLeastOnce();

            Expect.Call(person1.EmploymentNumber).Return("2").Repeat.AtLeastOnce();
            Expect.Call(person2.EmploymentNumber).Return("1").Repeat.AtLeastOnce();
            Expect.Call(person3.EmploymentNumber).Return("4").Repeat.AtLeastOnce();
            Expect.Call(person4.EmploymentNumber).Return("3").Repeat.AtLeastOnce();

            Expect.Call(person1.TerminalDate).Return(null).Repeat.AtLeastOnce();
            Expect.Call(person2.TerminalDate).Return(null).Repeat.AtLeastOnce();
            Expect.Call(person3.TerminalDate).Return(null).Repeat.AtLeastOnce();
            Expect.Call(person4.TerminalDate).Return(null).Repeat.AtLeastOnce();

            Expect.Call(vPeriod1.DaysOff()).Return(4).Repeat.AtLeastOnce();
            Expect.Call(vPeriod2.DaysOff()).Return(3).Repeat.AtLeastOnce();
            Expect.Call(vPeriod3.DaysOff()).Return(2).Repeat.AtLeastOnce();
            Expect.Call(vPeriod4.DaysOff()).Return(1).Repeat.AtLeastOnce();

            Expect.Call(_viewBase.IsOverviewColumnsHidden).Return(false).Repeat.Any();

            _mocks.ReplayAll();
            _schedulerState.FilteredPersonDictionary.Add(person1.Id.Value, person1);
            _schedulerState.FilteredPersonDictionary.Add(person3.Id.Value, person3);
            _schedulerState.FilteredPersonDictionary.Add(person2.Id.Value, person2);
            _schedulerState.FilteredPersonDictionary.Add(person4.Id.Value, person4);
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;

            _target.SortColumn((int)ColumnType.RowHeaderColumn);
            Assert.AreEqual(_schedulerState.FilteredPersonDictionary.ElementAt(0).Value.Name, person4.Name);
            Assert.AreEqual(_schedulerState.FilteredPersonDictionary.ElementAt(1).Value.Name, person3.Name);
            Assert.AreEqual(_schedulerState.FilteredPersonDictionary.ElementAt(2).Value.Name, person2.Name);
            Assert.AreEqual(_schedulerState.FilteredPersonDictionary.ElementAt(3).Value.Name, person1.Name);

            _target.SortColumn((int)ColumnType.RowHeaderColumn);
            Assert.AreEqual(_schedulerState.FilteredPersonDictionary.ElementAt(0).Value.Name, person1.Name);
            Assert.AreEqual(_schedulerState.FilteredPersonDictionary.ElementAt(1).Value.Name, person2.Name);
            Assert.AreEqual(_schedulerState.FilteredPersonDictionary.ElementAt(2).Value.Name, person3.Name);
            Assert.AreEqual(_schedulerState.FilteredPersonDictionary.ElementAt(3).Value.Name, person4.Name);

            _schedulerState.CommonNameDescription.AliasFormat = "{EmployeeNumber}";
            _target.SortColumn((int)ColumnType.RowHeaderColumn);
            Assert.AreEqual(_schedulerState.FilteredPersonDictionary.ElementAt(0).Value.Name, person3.Name);
            Assert.AreEqual(_schedulerState.FilteredPersonDictionary.ElementAt(1).Value.Name, person4.Name);
            Assert.AreEqual(_schedulerState.FilteredPersonDictionary.ElementAt(2).Value.Name, person1.Name);
            Assert.AreEqual(_schedulerState.FilteredPersonDictionary.ElementAt(3).Value.Name, person2.Name);

            _target.SortColumn((int)ColumnType.TargetDayOffColumn);
            Assert.AreEqual(_schedulerState.FilteredPersonDictionary.ElementAt(0).Value.Name, person4.Name);

            _target.SortCommand = new SortByStartAscendingCommand(_schedulerState);
            _target.SortColumn((int)ColumnType.TargetDayOffColumn);
            Assert.AreEqual(_schedulerState.FilteredPersonDictionary.ElementAt(0).Value.Name, person1.Name);
            Assert.IsTrue(_target.SortCommand is NoSortCommand);
            _mocks.VerifyAll();
        }




        [Test]
        public void ShouldSetStyleToNaOnDayOffCellWhenSchedulePeriodAndOpenPeriodDoNotMatch()
        {
            var person = _mocks.StrictMock<IPerson>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            var scheduleRange = _mocks.StrictMock<IScheduleRange>();
            var schedulePeriod = _mocks.StrictMock<ISchedulePeriod>();
            IList<ISchedulePeriod> schedulePeriods = new List<ISchedulePeriod> { schedulePeriod };

            using (_mocks.Record())
            {
                Expect.Call(_viewBase.TheGrid).Return(_grid).Repeat.AtLeastOnce();
                Expect.Call(_viewBase.IsOverviewColumnsHidden).Return(false);
                Expect.Call(scheduleDictionary[person]).Return(scheduleRange);
                Expect.Call(person.PersonSchedulePeriods(new DateOnlyPeriod(2009, 2, 2, 2009, 3, 1))).Return(schedulePeriods);
                Expect.Call(schedulePeriod.GetSchedulePeriod(new DateOnly(2009, 2, 2))).IgnoreArguments().Return((new DateOnlyPeriod(2009, 2, 2, 2009, 4, 1))).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                using (var gridStyleInfo = new GridStyleInfo())
                {
                    _target.QueryOverviewStyleInfo(gridStyleInfo, person, ColumnType.TargetDayOffColumn);
                    Assert.AreEqual(UserTexts.Resources.NA, gridStyleInfo.CellValue);
                }
            }
        }

        [Test]
        public void ShouldNotSetStyleToNaOnDayOffCellWhenSchedulePeriodAndOpenPeriodMatch()
        {
            var person = _mocks.StrictMock<IPerson>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            var scheduleRange = _mocks.StrictMock<IScheduleRange>();
            ISchedulePeriod schedulePeriod = new SchedulePeriod(new DateOnly(2009, 2, 2), SchedulePeriodType.Day, 2);
            schedulePeriod.SetParent(person);
            IList<ISchedulePeriod> schedulePeriods = new List<ISchedulePeriod> { schedulePeriod };
            var virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            var permissionInformation = _mocks.StrictMock<IPermissionInformation>();

            using (_mocks.Record())
            {
                Expect.Call(_viewBase.TheGrid).Return(_grid).Repeat.AtLeastOnce();
                Expect.Call(_viewBase.IsOverviewColumnsHidden).Return(false);
                Expect.Call(scheduleDictionary[person]).Return(scheduleRange);
                Expect.Call(person.PersonSchedulePeriods(new DateOnlyPeriod(2009, 2, 2, 2009, 3, 1))).Return(schedulePeriods).Repeat.AtLeastOnce();
                Expect.Call(scheduleRange.CalculatedTargetScheduleDaysOff).Return(0).Repeat.AtLeastOnce();
                Expect.Call(person.VirtualSchedulePeriod(new DateOnly(2009, 2, 2))).IgnoreArguments().Return(virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(virtualSchedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(person.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(permissionInformation.Culture()).Return(CultureInfo.CurrentCulture).Repeat.AtLeastOnce();
                Expect.Call(person.TerminalDate).Return(null).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                using (var gridStyleInfo = new GridStyleInfo())
                {
                    _target.QueryOverviewStyleInfo(gridStyleInfo, person, ColumnType.TargetDayOffColumn);
                    Assert.AreNotEqual(UserTexts.Resources.NA, gridStyleInfo.CellValue);
                }
            }
        }

        [Test]
        public void ShouldSetStyleToNaOnContractCellWhenSchedulePeriodAndOpenPeriodDoNotMatch()
        {
            var person = _mocks.StrictMock<IPerson>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            var scheduleRange = _mocks.StrictMock<IScheduleRange>();
            var schedulePeriod = _mocks.StrictMock<ISchedulePeriod>();
            IList<ISchedulePeriod> schedulePeriods = new List<ISchedulePeriod> { schedulePeriod };
            var virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

            using (_mocks.Record())
            {
                Expect.Call(_viewBase.TheGrid).Return(_grid).Repeat.AtLeastOnce();
                Expect.Call(_viewBase.IsOverviewColumnsHidden).Return(false);
                Expect.Call(scheduleDictionary[person]).Return(scheduleRange).Repeat.AtLeastOnce();
                Expect.Call(person.PersonSchedulePeriods(new DateOnlyPeriod(2009, 2, 2, 2009, 3, 1))).Return(schedulePeriods);
                Expect.Call(schedulePeriod.GetSchedulePeriod(new DateOnly(2009, 2, 2))).IgnoreArguments().Return((new DateOnlyPeriod(2009, 2, 2, 2009, 4, 1))).Repeat.AtLeastOnce();
                Expect.Call(person.VirtualSchedulePeriod(new DateOnly(2009, 2, 2))).Return(virtualSchedulePeriod);
                Expect.Call(virtualSchedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();

            }

            using (_mocks.Playback())
            {
                using (var gridStyleInfo = new GridStyleInfo())
                {

                    _target.QueryOverviewStyleInfo(gridStyleInfo, person, ColumnType.TargetContractTimeColumn);
                    Assert.AreEqual(UserTexts.Resources.NA, gridStyleInfo.CellValue);
                }
            }
        }

        [Test]
        public void ShouldNotSetStyleToNaOnContractCellWhenSchedulePeriodAndOpenPeriodMatch()
        {
            var person = _mocks.StrictMock<IPerson>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            var scheduleRange = _mocks.StrictMock<IScheduleRange>();
            ISchedulePeriod schedulePeriod = new SchedulePeriod(new DateOnly(2009, 2, 2), SchedulePeriodType.Day, 2);
            schedulePeriod.SetParent(person);
            IList<ISchedulePeriod> schedulePeriods = new List<ISchedulePeriod> { schedulePeriod };
            var virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            var permissionInformation = _mocks.StrictMock<IPermissionInformation>();

            using (_mocks.Record())
            {
                Expect.Call(_viewBase.TheGrid).Return(_grid).Repeat.AtLeastOnce();
                Expect.Call(_viewBase.IsOverviewColumnsHidden).Return(false);
                Expect.Call(scheduleDictionary[person]).Return(scheduleRange).Repeat.AtLeastOnce();
                Expect.Call(person.PersonSchedulePeriods(new DateOnlyPeriod(2009, 2, 2, 2009, 3, 1))).Return(schedulePeriods).Repeat.AtLeastOnce();
                Expect.Call(person.VirtualSchedulePeriod(new DateOnly(2009, 2, 2))).IgnoreArguments().Return(virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(virtualSchedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(person.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(permissionInformation.Culture()).Return(CultureInfo.CurrentCulture).Repeat.AtLeastOnce();
                Expect.Call(scheduleRange.CalculatedTargetTimeHolder).Return(TimeSpan.Zero).Repeat.AtLeastOnce();
                Expect.Call(person.TerminalDate).Return(null).Repeat.AtLeastOnce();
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
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
            Expect.Call(_viewBase.DayHeaderTooltipText(null, DateTime.MinValue)).IgnoreArguments().Return("test").Repeat.Once();
            Expect.Call(_viewBase.TheGrid).Return(_grid).Repeat.AtLeastOnce();

            _mocks.ReplayAll();

            _target.ColWeekMap.Add((int)ColumnType.StartScheduleColumns, 45);
            _target.SelectedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(_date,_date), TeleoptiPrincipal.Current.Regional.TimeZone);
            _schedulerState.FilteredPersonDictionary.Add(person1.Id.Value, person1);
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
            var personPeriod = _mocks.StrictMock<IPersonPeriod>();

            Expect.Call(person1.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(scheduleDictionary[person1]).IgnoreArguments().Return(range).Repeat.AtLeastOnce();
            Expect.Call(range.ScheduledDay(new DateOnly(_date))).IgnoreArguments().Return(schedulePart).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.FullAccess).Return(true).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.Person).Return(person1).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2008, 11, 4),
                                                                                           _timeZoneInfo)).Repeat.Times(2);

            schedulePart.Clear<IPersonAbsence>();
            LastCall.Repeat.Once();
            schedulePart.Clear<IPersonAssignment>();
            LastCall.Repeat.Once();
            schedulePart.Clear<IPersonDayOff>();
            LastCall.Repeat.Once();
            Expect.Call(schedulePart.PersonDayOffCollection()).Return(
                new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.PersonAssignmentConflictCollection).Return(
                new List<IPersonAssignment>()).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>())).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.PersonAbsenceCollection()).Return(
                new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence>())).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.PersonMeetingCollection()).Return(
                new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>())).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.BusinessRuleResponseCollection).Return(
                new List<IBusinessRuleResponse>()).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.ProjectionService()).Return(new VisualLayerProjectionService(person1)).Repeat.Any();
            Expect.Call(_viewBase.RowHeaders).Return(1).Repeat.AtLeastOnce();
            Expect.Call(_viewBase.ColHeaders).Return(1).Repeat.AtLeastOnce();
            _viewBase.SetCellBackTextAndBackColor(null, _date, false, false, null);
            LastCall.IgnoreArguments().Repeat.AtLeastOnce();
            Expect.Call(_viewBase.TheGrid).Return(_grid).Repeat.AtLeastOnce();
            Expect.Call(person1.Period(new DateOnly(2008, 11, 04))).Return(personPeriod).Repeat.Times(2);

            _mocks.ReplayAll();

            _target.SelectedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(_date,_date), TeleoptiPrincipal.Current.Regional.TimeZone);
            if (person1.Id != null) _schedulerState.FilteredPersonDictionary.Add(person1.Id.Value, person1);
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;

            var eventArgs = new GridQueryCellInfoEventArgs(2, (int)ColumnType.StartScheduleColumns, new GridStyleInfo());
            _target.QueryCellInfo(null, eventArgs);
            Assert.AreEqual(schedulePart, eventArgs.Style.CellValue);
            Assert.AreEqual(GridMergeCellDirection.None, eventArgs.Style.MergeCell);

            _target = new SchedulePresenterBase(_viewBase, _schedulerState, _gridlockManager, _clipHandlerSchedulePart, SchedulePartFilter.Meetings, _overriddenBusinessRulesHolder,
                _scheduleDayChangeCallback, NullScheduleTag.Instance);
            _target.SelectedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(_date,_date), TeleoptiPrincipal.Current.Regional.TimeZone);

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
            Expect.Call(_viewBase.TheGrid).Return(_grid).Repeat.AtLeastOnce();

            _mocks.ReplayAll();

            _target.ColWeekMap.Add((int)ColumnType.StartScheduleColumns, 45);
            _target.SelectedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(_date,_date), TeleoptiPrincipal.Current.Regional.TimeZone);
            _schedulerState.FilteredPersonDictionary.Add(person1.Id.Value, person1);
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
            Expect.Call(_viewBase.TheGrid).Return(_grid);
            Expect.Call(businessRuleResponse.Overridden).Return(false);

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
            Expect.Call(_viewBase.TheGrid).Return(_grid);
            Expect.Call(businessRuleResponse.Overridden).Return(false);

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
            Expect.Call(_viewBase.TheGrid).Return(_grid);
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
            Expect.Call(_viewBase.TheGrid).Return(_grid);
            Expect.Call(businessRuleResponse.Overridden).Return(false);
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
            allRules.Remove(typeof(NightlyRestRule));

            Expect.Call(businessRuleResponse.Overridden = true);
            Expect.Call(businessRuleResponse.TypeOfRule).Return(typeof(NightlyRestRule)).Repeat.AtLeastOnce();
            Expect.Call(scheduleDictionary.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay>(), allRules, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).Return(
                new List<IBusinessRuleResponse>()).IgnoreArguments();
            Expect.Call(_viewBase.TheGrid).Return(_grid);

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
            Expect.Call(_viewBase.TheGrid).Return(_grid);

            _mocks.ReplayAll();
            _gridlockManager.AddLock(person, new DateOnly(_date), LockType.Normal, schedulePart.DateOnlyAsPeriod.Period());
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

            Expect.Call(schedulePart.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { personAssignment }));
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
            Expect.Call(_viewBase.TheGrid).Return(_grid);

            Expect.Call(businessRuleResponse.Mandatory).Return(true);
            Expect.Call(businessRuleResponse.Message).Return("testar");
            Expect.Call(_scheduleDictionary.Modify(ScheduleModifier.Scheduler, (IEnumerable<IScheduleDay>)null, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(_businessRuleResponses);
            Expect.Call(schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(_date), new CccTimeZoneInfo(TimeZoneInfo.Utc)));
            Expect.Call(schedulePart.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { personAssignment })).Repeat.AtLeastOnce();
            Expect.Call(personAssignment.CheckRestrictions).Repeat.AtLeastOnce();
            Expect.Call(businessRuleResponse.Overridden).Return(false);
        }

        [Test]
        public void VerifyUpdateFromEditor()
        {
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
            var person = _mocks.StrictMock<IPerson>();
            var personAssignment = _mocks.StrictMock<IPersonAssignment>();
            IScheduleDictionary scheduleDictionary = CreateExpectationForModifySchedulePart(schedulePart, person);
            Expect.Call(schedulePart.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { personAssignment })).Repeat.AtLeastOnce();
            Expect.Call(personAssignment.CheckRestrictions).Repeat.AtLeastOnce();
            Expect.Call(_viewBase.OnPasteCompleted);
            Expect.Call(_viewBase.TheGrid).Return(_grid);

            _mocks.ReplayAll();
            _target.SchedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            _target.LastUnsavedSchedulePart = schedulePart;
            _target.UpdateFromEditor();
            _mocks.VerifyAll();
            Assert.IsNull(_target.LastUnsavedSchedulePart);
        }

        [Test]
        public void VerifyUpdateRestriction()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IList<IBusinessRuleResponse> businessRuleResponses = new List<IBusinessRuleResponse>();
            IScheduleDictionary scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            Expect.Call(scheduleDictionary.Modify(ScheduleModifier.Scheduler, (IEnumerable<IScheduleDay>)null, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(
                businessRuleResponses);

            _mocks.ReplayAll();
            _target.SchedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            _target.LastUnsavedSchedulePart = schedulePart;
            _target.UpdateRestriction();
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
        public void VerifyAddAbsence()
        {
            _day1 = _mocks.StrictMock<IScheduleDay>();
            _day2 = _mocks.StrictMock<IScheduleDay>();
        	const int numberOfDays = 2;
            var period = new DateTimePeriod(_date, _date.AddDays(1));

            ExpectCallsDialogOnVerifyAddAbsence(period);
            ExpectCallsViewBaseOnVerifyAddAbsence(period);
            Expect.Call(_day2.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_day1.Period).Return(new DateTimePeriod(2008, 11, 04, 2008, 11, 05)).Repeat.Any();
			Expect.Call(() => _day1.CreateAndAddAbsence(null)).IgnoreArguments();
			Expect.Call(() => _ass.CheckRestrictions()).Repeat.Times(numberOfDays);
			Expect.Call(_day1.PersonAssignmentCollection()).Return(new List<IPersonAssignment> { _ass }.AsReadOnly()).Repeat.AtLeastOnce();
            var scheduleDictionary = CreateExpectationForModifySchedulePart(_day1, _person);
            Expect.Call(_day1.TimeZone).Return(CccTimeZoneInfoFactory.StockholmTimeZoneInfo());

            // <expect that we call for tags for each days
            IScheduleRange range1 = _mocks.StrictMock<IScheduleRange>();
            Expect.Call(scheduleDictionary[_person]).Return(range1).Repeat.AtLeastOnce();
            Expect.Call(range1.ScheduledDay(new DateOnly(_date)))
                .Return(_day1);
            Expect.Call(range1.ScheduledDay(new DateOnly(_date.AddDays(1))))
                .Return(_day1);
            // >

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
            Expect.Call(_viewBase.CreateAddAbsenceViewModel(null, null)).IgnoreArguments().Return(_dialog);
            Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(_person, period)).IgnoreArguments().Repeat.AtLeastOnce();
            Expect.Call(_viewBase.TheGrid).Return(_grid);
        }

        [Test]
        public void VerifyAddAbsenceMultipleDays()
        {
            var periodPart1 = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_date, _date.AddDays(1),_timeZoneInfo);
			var periodPart2 = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_date.AddDays(1), _date.AddDays(2),_timeZoneInfo);
            var periodTotal = new DateTimePeriod(periodPart1.StartDateTime, periodPart2.EndDateTime);

            ExpectCallsDialogOnVerifyAddAbsenceMultipleDays(periodTotal);
            ExpectCallsViewBaseOnVerifyAddAbsenceMultipleDays(periodTotal);
            ExpectCallsSchedulePartsOnVerifyAddAbsenceMultipleDays(periodPart1, periodPart2);
            Expect.Call(() => _ass.CheckRestrictions()).Repeat.AtLeastOnce();
            var scheduleDictionary = CreateExpectationForModifySchedulePart(_day1, _person);

            // <expect that we call for tags for each days
            IScheduleRange range1 = _mocks.StrictMock<IScheduleRange>();
            Expect.Call(scheduleDictionary[_person]).Return(range1).Repeat.AtLeastOnce();
            Expect.Call(range1.ScheduledDay(_date))
                .Return(_day1);
            Expect.Call(range1.ScheduledDay(_date.AddDays(1)))
                .Return(_day1);
            Expect.Call(range1.ScheduledDay(_date.AddDays(2)))
                .Return(_day1);
            Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(_person, periodPart1)).IgnoreArguments().Repeat.AtLeastOnce();
        
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
            Expect.Call(_day1.PersistableScheduleDataCollection()).Return(new List<IPersistableScheduleData> { null }).Repeat.AtLeastOnce();

			Expect.Call(_day2.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(() => _day1.CreateAndAddAbsence(null)).IgnoreArguments();
            Expect.Call(_day1.PersonAssignmentCollection()).Return(new List<IPersonAssignment> { _ass }.AsReadOnly()).Repeat.AtLeastOnce();
            Expect.Call(_day1.TimeZone).Return(CccTimeZoneInfoFactory.StockholmTimeZoneInfo());
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
            Expect.Call(_viewBase.CreateAddAbsenceViewModel(null, null)).Constraints(Is.Anything(), Is.Matching(new Predicate<ISetupDateTimePeriod>(t => t.Period == period.ChangeEndTime(TimeSpan.FromMinutes(-1))))).Return(_dialog);
            Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(_person, period)).IgnoreArguments().Repeat.Once();
            Expect.Call(_viewBase.TheGrid).Return(_grid);
        }

        [Test]
        public void ShouldNotAddAbsenceOnLockedDay()
        {
            var rangePeriod = new DateTimePeriod(2011, 1, 1, 2011, 1, 3);
            IScheduleRange range1 = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay day1 = _mocks.StrictMock<IScheduleDay>();
			IScheduleDay day2 = _mocks.StrictMock<IScheduleDay>();
			IScheduleDay day3 = _mocks.StrictMock<IScheduleDay>();

            IDateOnlyAsDateTimePeriod periodPart1 = new DateOnlyAsDateTimePeriod(new DateOnly(2011, 1, 1), _timeZoneInfo);
			IDateOnlyAsDateTimePeriod periodPart2 = new DateOnlyAsDateTimePeriod(new DateOnly(2011, 1, 2), _timeZoneInfo);
			IDateOnlyAsDateTimePeriod periodPart3 = new DateOnlyAsDateTimePeriod(new DateOnly(2011, 1, 3), _timeZoneInfo);

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
				Expect.Call(range1.ScheduledDay(new DateOnly(2011, 1, 3)))
					.Return(day3);
                Expect.Call(day1.Person)
                    .Return(_person).Repeat.AtLeastOnce();
				Expect.Call(day2.Person)
					.Return(_person).Repeat.AtLeastOnce();
				Expect.Call(day3.Person)
					.Return(_person).Repeat.AtLeastOnce();
                Expect.Call(day1.DateOnlyAsPeriod)
                    .Return(periodPart1).Repeat.AtLeastOnce();
				Expect.Call(day2.DateOnlyAsPeriod)
					.Return(periodPart2).Repeat.AtLeastOnce();
				Expect.Call(day3.DateOnlyAsPeriod)
					.Return(periodPart3).Repeat.AtLeastOnce();

				const int numberOfAbsences = 2;
				
				Expect.Call(day2.Period).Return(new DateTimePeriod(2011, 01, 02, 2011, 01, 03)).Repeat.AtLeastOnce();
				Expect.Call(day3.Period).Return(new DateTimePeriod(2011, 01, 03, 2011, 01, 04)).Repeat.AtLeastOnce();
				// the absence is addes to the last day
				Expect.Call(() => day3.CreateAndAddAbsence(null)).IgnoreArguments();
				Expect.Call(day2.PersonAssignmentCollection()).Return(new List<IPersonAssignment> { _ass }.AsReadOnly()).Repeat.AtLeastOnce();
				Expect.Call(day3.PersonAssignmentCollection()).Return(new List<IPersonAssignment> { _ass }.AsReadOnly()).Repeat.AtLeastOnce();
            	Expect.Call(_ass.CheckRestrictions).Repeat.Times(numberOfAbsences);

				Expect.Call(_scheduleDictionary.Scenario).Return(_scen).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDictionary.Modify(ScheduleModifier.Scheduler, (IEnumerable<IScheduleDay>)_day2, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(_businessRuleResponses);
 

            }

            using (_mocks.Playback())
            {
                _schedulerState.SchedulingResultState.Schedules = _scheduleDictionary;
                _day1 = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(2011, 1, 1));
                _day2 = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(2011, 1, 2));
                _selectedSchedules = new List<IScheduleDay> { _day1, _day2 };
                _gridlockManager.AddLock(new List<IScheduleDay> { _day1 }, LockType.Normal);
                _target.AddAbsence(_selectedSchedules, null);
            }
        }

        private void ExpectCallsViewBaseOnShouldNotAddAbsenceOnLockedDay(DateTimePeriod period)
        {
            Expect.Call(_viewBase.SelectedSchedules()).Return(_selectedSchedules);
            Expect.Call(_viewBase.CreateAddAbsenceViewModel(null, null)).Constraints(Is.Anything(), Is.Matching(new Predicate<ISetupDateTimePeriod>(t => t.Period == period.ChangeEndTime(TimeSpan.FromMinutes(-1))))).Return(_dialog);
            Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(_person, period)).IgnoreArguments().Repeat.AtLeastOnce();
            Expect.Call(_viewBase.TheGrid).Return(_grid);
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
			var ass = _mocks.StrictMock<IPersonAssignment>();
			var schedulePart = _mocks.StrictMock<IScheduleDay>();
			var scheduleRange = _mocks.StrictMock<IScheduleRange>();

			_selectedSchedules = new List<IScheduleDay> { schedulePart };
			var startDateTime = _schedulerState.RequestedPeriod.Period().StartDateTime;
			var period = new DateTimePeriod(startDateTime.AddHours(3), startDateTime.AddHours(3.5));
			Expect.Call(schedulePart.Period).Return(DateTimeFactory.CreateDateTimePeriod(DateTime.SpecifyKind(_date, DateTimeKind.Utc), 0)).Repeat.Any();
			ExpectCallsDialogOnShouldAddAbsenceWithDefaultPeriod(period);
			ExpectCallsViewBaseOnShouldAddAbsenceWithDefaultPeriod(period);

			Expect.Call(schedulePart.PersonAssignmentCollection()).Return(new List<IPersonAssignment> { ass }.AsReadOnly()).Repeat.AtLeastOnce();
			Expect.Call(ass.CheckRestrictions);
			var scheduleDictionary = CreateExpectationForModifySchedulePart(schedulePart, _person);
			Expect.Call(schedulePart.TimeZone).Return(CccTimeZoneInfoFactory.StockholmTimeZoneInfo());

			Expect.Call(scheduleDictionary[_person])
				.Return(scheduleRange);
			Expect.Call(scheduleRange.ScheduledDay(new DateOnly(2009, 02, 02)))
				.Return(schedulePart);


			_mocks.ReplayAll();
			_schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
			_target.AddAbsence(_selectedSchedules, period);
			_mocks.VerifyAll();
		}

        private void ExpectCallsViewBaseOnShouldAddAbsenceWithDefaultPeriod(DateTimePeriod period)
        {
            Expect.Call(_viewBase.SelectedSchedules()).Return(_selectedSchedules);
            Expect.Call(_viewBase.CreateAddAbsenceViewModel(null, null)).Constraints(Is.Anything(), Is.Matching(new Predicate<ISetupDateTimePeriod>(t => t.Period == period))).Return(_dialog);
            Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(_person, period)).IgnoreArguments().Repeat.Once();
            Expect.Call(_viewBase.TheGrid).Return(_grid);
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
            var ass = _mocks.StrictMock<IPersonAssignment>();
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
			var scheduleRange = _mocks.StrictMock<IScheduleRange>();
            var selectedItem = _mocks.StrictMock<IAbsence>();
            var dialog = _mocks.StrictMock<IAddLayerViewModel<IAbsence>>();
			var period = new DateTimePeriod(2009, 2, 1, 2009, 2, 2);
            Expect.Call(schedulePart.Period).Return(period).Repeat.Any();
            Expect.Call(_viewBase.CurrentColumnSelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(_viewBase.CreateAddAbsenceViewModel(null, null)).IgnoreArguments().Return(dialog);
            Expect.Call(dialog.Result).Return(true);
            Expect.Call(dialog.SelectedItem).Return(selectedItem);
            Expect.Call(dialog.SelectedPeriod).Return(period);
            Expect.Call(schedulePart.PersonAssignmentCollection()).Return(new List<IPersonAssignment> { ass }.AsReadOnly()).Repeat.AtLeastOnce();
            IScheduleDictionary scheduleDictionary = CreateExpectationForModifySchedulePart(schedulePart, _person);
            
            Expect.Call(_viewBase.TheGrid).Return(_grid);
            Expect.Call(schedulePart.TimeZone).Return(CccTimeZoneInfoFactory.StockholmTimeZoneInfo());

			Expect.Call(scheduleDictionary[_person])
				.Return(scheduleRange).Repeat.AtLeastOnce();
			Expect.Call(scheduleRange.ScheduledDay(new DateOnly())).IgnoreArguments()
				.Return(schedulePart).Repeat.AtLeastOnce();
        	const int numberOfAbsences = 2;
			Expect.Call(() => schedulePart.CreateAndAddAbsence(null)).IgnoreArguments(); // a new absence created, that why ignore arguments
			Expect.Call(ass.CheckRestrictions).Repeat.Times(numberOfAbsences);
			Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(_person, period)).Repeat.Times(numberOfAbsences);
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
            Expect.Call(_viewBase.CurrentColumnSelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(schedulePart.Period).Return(period).Repeat.Any();
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(_viewBase.CreateAddAbsenceViewModel(null, null)).IgnoreArguments().Return(dialog);
            Expect.Call(dialog.Result).Return(false);
            LastCall.Repeat.Once();
            Expect.Call(schedulePart.TimeZone).Return(CccTimeZoneInfoFactory.StockholmTimeZoneInfo());

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
                    _schedulerState.TimeZoneInfo)).IgnoreArguments().Return(dialog);

            Expect.Call(() => scheduleDay.CreateAndAddActivity(null, shiftCategory)).IgnoreArguments().Repeat.Once();
            Expect.Call(scheduleDay.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { pa }));
            Expect.Call(scheduleDay.Period).Return(new DateTimePeriod()).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.AssignmentHighZOrder()).Return(pa).Repeat.AtLeastOnce();
            Expect.Call(pa.Period).Return(new DateTimePeriod()).Repeat.AtLeastOnce();
            Expect.Call(dialog.Result).Return(true);
            Expect.Call(dialog.SelectedItem).Return(selectedItem);
            Expect.Call(dialog.SelectedShiftCategory).Return(shiftCategory);
            Expect.Call(dialog.SelectedPeriod).Return(new DateTimePeriod());
            Expect.Call(() => pa.CheckRestrictions()).Throw(new ValidationException());
            Expect.Call(() => _viewBase.ShowErrorMessage(string.Empty, string.Empty)).IgnoreArguments();

            _mocks.ReplayAll();
            _target.AddActivity(list, null);
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
            IScheduleDay part = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2001, 1, 1));

            DateTimePeriod period = AddActivityCommand.GetDefaultPeriodFromPart(part);

            Assert.AreEqual(TimeZoneHelper.ConvertToUtc(new DateTime(2000, 1, 1, 8, 0, 0), part.TimeZone).Hour, period.StartDateTime.Hour);
            Assert.AreEqual(TimeZoneHelper.ConvertToUtc(new DateTime(2000, 1, 1, 17, 0, 0), part.TimeZone).Hour, period.EndDateTime.Hour);

            IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario, parameters.Person, assPeriod);

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
            Expect.Call(_viewBase.CreateAddActivityViewModel(null, new List<IShiftCategory> { ShiftCategoryFactory.CreateShiftCategory("Test") }, new DateTimePeriod(2001, 1, 1, 2001, 1, 2), null)).IgnoreArguments().Return(dialog);
            Expect.Call(dialog.Result).Return(false);

            LastCall.Repeat.Once();
            Expect.Call(schedulePart.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.AssignmentHighZOrder()).Return(pa).Repeat.AtLeastOnce();
            Expect.Call(pa.Period).Return(new DateTimePeriod()).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.Period).Return(new DateTimePeriod()).Repeat.AtLeastOnce();

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
            _target.AddActivity(new List<IScheduleDay> { schedulePart }, period);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyAddOvertimeTheNewWay()
        {
            IMainShift mainShift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();
            _multiplicatorDefinitionSets.Add(_definitionSet);
            _day1 = _mocks.StrictMock<IScheduleDay>();

            var period1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2008, 1, 1));
            period1.PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime));
            _person.AddPersonPeriod(period1);
            var period = _schedulerState.RequestedPeriod.Period();

            ExpectCallsViewBaseOnVerifyAddOvertimeTheNewWay(period);
            ExpectCallsDialogOnVerifyAddOvertimeTheNewWay(period);
            ExpectCallsScheduleDayOnVerifyAddOvertimeTheNewWay();
            Expect.Call(_day1.Period).Return(new DateTimePeriod(2001, 1, 1, 2001, 1, 2)).Repeat.Twice();
            Expect.Call(_ass.CheckRestrictions);
            Expect.Call(_ass.Period).Return(period);
            Expect.Call(_ass.MainShift).Return(mainShift).Repeat.AtLeastOnce();

            var scheduleDictionary = CreateExpectationForModifySchedulePart(_day1, _person);

            _mocks.ReplayAll();
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            _target.AddOvertime(_multiplicatorDefinitionSets);
            _mocks.VerifyAll();
        }

        private void ExpectCallsScheduleDayOnVerifyAddOvertimeTheNewWay()
        {
            Expect.Call(_day1.PersonAssignmentCollection()).Return(new List<IPersonAssignment> { _ass }.AsReadOnly()).Repeat.AtLeastOnce();
            Expect.Call(() => _day1.CreateAndAddOvertime(null)).IgnoreArguments();
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
            Expect.Call(_viewBase.CreateAddOvertimeViewModel(null, null, _multiplicatorDefinitionSets, null, new DateTimePeriod(2001, 1, 1, 2001, 1, 2))).IgnoreArguments().Return(_overtimeDialog);

            Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(_person, period));
            Expect.Call(_viewBase.TheGrid).Return(_grid);
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
            var schedulePartPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 1, 1), new CccTimeZoneInfo(TimeZoneInfo.Utc));
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
            var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2001, 1, 1), new CccTimeZoneInfo());
            var person = _mocks.StrictMock<IPerson>();
            var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            var ass = _mocks.StrictMock<IPersonAssignment>();
            IMainShift mainShift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();
            var period = _schedulerState.RequestedPeriod.Period();
            IList<IMultiplicatorDefinitionSet> multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
            var dialog = _mocks.StrictMock<IAddOvertimeViewModel>();
            Expect.Call(schedulePart.Period).Return(new DateTimePeriod(2001, 1, 1, 2001, 1, 2)).Repeat.Twice();
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(_viewBase.CreateAddOvertimeViewModel(null, null, multiplicatorDefinitionSets, null, new DateTimePeriod(2001, 1, 1, 2001, 1, 2))).IgnoreArguments().Return(dialog);
            Expect.Call(schedulePart.PersonAssignmentCollection()).Return(new List<IPersonAssignment> { ass }.AsReadOnly());
            Expect.Call(ass.Period).Return(period);
            Expect.Call(ass.MainShift).Return(mainShift).Repeat.Twice();
            Expect.Call(dialog.Result).Return(false);
            LastCall.Repeat.Once();
            Expect.Call(schedulePart.Person).Return(person);
            Expect.Call(schedulePart.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod);
            Expect.Call(person.Period(new DateOnly(2001, 1, 1))).IgnoreArguments().Return(personPeriod);

            _mocks.ReplayAll();
            _target.AddOvertime(multiplicatorDefinitionSets);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldNotAddOvertimeWhenNoPeriod()
        {
            var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2001, 1, 1), new CccTimeZoneInfo());
            var person = _mocks.StrictMock<IPerson>();
            var ass = _mocks.StrictMock<IPersonAssignment>();
            IMainShift mainShift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();
            var period = DateTimeFactory.CreateDateTimePeriod(DateTime.SpecifyKind(_date,DateTimeKind.Utc), 0);
            IList<IMultiplicatorDefinitionSet> multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(schedulePart.PersonAssignmentCollection()).Return(new List<IPersonAssignment> { ass }.AsReadOnly());
            Expect.Call(ass.Period).Return(period);
            Expect.Call(ass.MainShift).Return(mainShift).Repeat.Twice();
            Expect.Call(schedulePart.Period).Return(new DateTimePeriod(2001, 1, 1, 2001, 1, 2)).Repeat.Twice();
            Expect.Call(schedulePart.Person).Return(person);
            Expect.Call(schedulePart.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod);
            Expect.Call(person.Period(new DateOnly(2001, 1, 1))).IgnoreArguments().Return(null);
            Expect.Call(() => _viewBase.ShowInformationMessage(UserTexts.Resources.CouldNotAddOverTimeNoPersonPeriods, UserTexts.Resources.Information));

            _mocks.ReplayAll();
            _target.AddOvertime(multiplicatorDefinitionSets);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyAddPersonalShift()
        {
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
            var selectedItem = _mocks.StrictMock<IActivity>();
            var dialog = _mocks.StrictMock<IAddLayerViewModel<IActivity>>();
            var person = _mocks.StrictMock<IPerson>();
        	var period = _schedulerState.RequestedPeriod.Period();
            Expect.Call(schedulePart.Period).Return(new DateTimePeriod(2001, 1, 1, 2001, 1, 2)).Repeat.Twice();
            Expect.Call(schedulePart.AssignmentHighZOrder()).Return(null);
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(_viewBase.CreateAddPersonalActivityViewModel(null, new DateTimePeriod(2001, 1, 1, 2001, 1, 2), new CccTimeZoneInfo())).IgnoreArguments().Return(dialog);
            Expect.Call(dialog.Result).Return(true);
            Expect.Call(dialog.SelectedItem).Return(selectedItem);
            Expect.Call(dialog.SelectedPeriod).Return(period);
            Expect.Call(() => schedulePart.CreateAndAddPersonalActivity(null)).IgnoreArguments();
            Expect.Call(_viewBase.TheGrid).Return(_grid);
            var personAssignment = _mocks.StrictMock<IPersonAssignment>();

            IScheduleDictionary scheduleDictionary = CreateExpectationForModifySchedulePart(schedulePart, person);
            Expect.Call(schedulePart.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { personAssignment })).Repeat.AtLeastOnce();
            Expect.Call(personAssignment.CheckRestrictions);

            Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(person, period));
            Expect.Call(schedulePart.TimeZone).Return(CccTimeZoneInfoFactory.StockholmTimeZoneInfo());
            _mocks.ReplayAll();
            _schedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            _target.AddPersonalShift();
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
                Expect.Call(schedulePartDayOne.Period).Return(defaultPeriod).Repeat.Twice();
                Expect.Call(schedulePartDayOne.SignificantPart()).Return(SchedulePartView.None);
                AddActivityDialogExpectations(dialog, defaultPeriod);
                Expect.Call(() => schedulePartDayOne.CreateAndAddActivity(null, null)).IgnoreArguments();
                Expect.Call(schedulePartDayOne.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { personAssignment })).Repeat.Twice();
                Expect.Call(personAssignment.CheckRestrictions).Repeat.Twice();
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
            Expect.Call(_viewBase.CreateAddActivityViewModel(null, null, defaultPeriod, new CccTimeZoneInfo())).IgnoreArguments().Return(dialog);
            Expect.Call(_viewBase.TheGrid).Return(_grid);
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
            var personAssignment = _mocks.StrictMock<IPersonAssignment>();

        	var startDateTime = _schedulerState.RequestedPeriod.Period().StartDateTime;
            var period = new DateTimePeriod(startDateTime.AddHours(3), startDateTime.AddHours(3.5));

            Expect.Call(schedulePart.Period).Return(new DateTimePeriod(2001, 1, 1, 2001, 1, 2)).Repeat.Twice();
            Expect.Call(schedulePart.AssignmentHighZOrder()).Return(personAssignment).Repeat.AtLeastOnce();
            Expect.Call(personAssignment.Period).Return(period);
            Expect.Call(_viewBase.SelectedSchedules()).Return(new List<IScheduleDay> { schedulePart });
            Expect.Call(_viewBase.CreateAddPersonalActivityViewModel(null, new DateTimePeriod(2001, 1, 1, 2001, 1, 2),
                                                                    new CccTimeZoneInfo())).Constraints(Is.Anything(),
                                                                                                        Is.Equal(period), Is.Anything()).Return(dialog);
            Expect.Call(dialog.Result).Return(true);
            Expect.Call(dialog.SelectedItem).Return(selectedItem);
            Expect.Call(dialog.SelectedPeriod).Return(period);
            Expect.Call(() => schedulePart.CreateAndAddPersonalActivity(null)).IgnoreArguments();
            Expect.Call(schedulePart.PersonAssignmentCollection()).Return(new List<IPersonAssignment> { ass }.AsReadOnly()).Repeat.AtLeastOnce();
            Expect.Call(ass.CheckRestrictions);
            IScheduleDictionary scheduleDictionary = CreateExpectationForModifySchedulePart(schedulePart, person);
            Expect.Call(() => _viewBase.RefreshRangeForAgentPeriod(person, period));
            Expect.Call(_viewBase.TheGrid).Return(_grid);
            Expect.Call(schedulePart.TimeZone).Return(CccTimeZoneInfoFactory.StockholmTimeZoneInfo());
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
                businessRuleResponses);
            Expect.Call(schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(_date), new CccTimeZoneInfo(TimeZoneInfo.Utc))).Repeat.AtLeastOnce();
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
            Expect.Call(schedulePart.AssignmentHighZOrder()).Return(null);
            Expect.Call(_viewBase.CreateAddPersonalActivityViewModel(null, new DateTimePeriod(2001, 1, 1, 2001, 1, 2), new CccTimeZoneInfo())).IgnoreArguments().Return(dialog);
            Expect.Call(dialog.Result).Return(false);
            Expect.Call(schedulePart.TimeZone).Return(CccTimeZoneInfoFactory.StockholmTimeZoneInfo());

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

        [Test]
        public void ShouldHandleNullValuesInPersonNameComparer()
        {
            Assert.AreEqual(0, _personNameComparer.Compare(null, null));
            Assert.AreEqual(-1, _personNameComparer.Compare(null, "a"));
            Assert.AreEqual(1, _personNameComparer.Compare("a", null));
            Assert.AreEqual(-1, _personNameComparer.Compare("a", "b"));
        }
    }
}
