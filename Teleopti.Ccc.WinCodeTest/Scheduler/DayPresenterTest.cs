﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class DayPresenterTest
    {
        private DayPresenter target;
        private MockRepository mocks;
        private IScheduleViewBase viewBase;
        private IScenario scenario;
        private GridlockManager gridlockManager;
        private ClipHandler<IScheduleDay> clipHandlerSchedulePart;
        private SchedulerStateHolder schedulerState;
        private readonly DateOnly _date = new DateOnly(2008, 11, 04);
        private IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
        private IScheduleDayChangeCallback _scheduleDayChangeCallback;

        [SetUp]
        public void Setup()
        {
            
            mocks = new MockRepository();
            viewBase = mocks.StrictMock<IScheduleViewBase>();
            scenario = mocks.StrictMock<IScenario>();
            gridlockManager = new GridlockManager();
            clipHandlerSchedulePart = new ClipHandler<IScheduleDay>();
			schedulerState = new SchedulerStateHolder(scenario, new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(_date, _date), TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone), new List<IPerson>(), mocks.DynamicMock<IDisableDeletedFilter>(), new SchedulingResultStateHolder(), new TimeZoneGuardWrapper());
            _overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();
            _scheduleDayChangeCallback = mocks.DynamicMock<IScheduleDayChangeCallback>();
            target = new DayPresenter(viewBase, schedulerState, gridlockManager, clipHandlerSchedulePart,
                                      SchedulePartFilter.None, _overriddenBusinessRulesHolder, _scheduleDayChangeCallback, NullScheduleTag.Instance);
        }

        [Test]
        public void VerifyInstanceCreated()
        {
            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyCreateDayHeader()
        {
            target = new DayPresenterTestClass(viewBase, schedulerState, gridlockManager, clipHandlerSchedulePart,
                                      SchedulePartFilter.None);
            viewBase.SetCellBackTextAndBackColor(null, _date, false, false, null);
            LastCall.IgnoreArguments().Repeat.Once();

            mocks.ReplayAll();

            GridQueryCellInfoEventArgs eventArgs = new GridQueryCellInfoEventArgs(0, -1, new GridStyleInfo());
            ((DayPresenterTestClass)target).CreateDayHeaderTest(eventArgs);

            target.SelectedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(_date,_date), TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
            eventArgs = new GridQueryCellInfoEventArgs(0, (int)ColumnType.StartScheduleColumns,new GridStyleInfo());
            ((DayPresenterTestClass)target).CreateDayHeaderTest(eventArgs);
            Assert.AreEqual(target.SelectedPeriod.DateOnlyPeriod.StartDate, eventArgs.Style.Tag);

            eventArgs = new GridQueryCellInfoEventArgs(1, (int)ColumnType.StartScheduleColumns, new GridStyleInfo());
            ((DayPresenterTestClass)target).CreateDayHeaderTest(eventArgs);
            Assert.AreEqual(target.SelectedPeriod.DateOnlyPeriod.StartDate, eventArgs.Style.Tag);

            mocks.VerifyAll();
        }

        [Test]
        public void MergeHeaders()
        {
            target.MergeHeaders();
            Assert.AreEqual(0, target.ColWeekMap.Count);
        }
        
        [Test]
        public void GetColumnFromLocalDateShouldReturnColumn()
        {
            target.SelectedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(_date,_date), TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
            int column = target.GetColumnFromLocalDate(_date.AddDays(3));
            Assert.AreEqual((int)ColumnType.StartScheduleColumns + 3, column);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyPotentialOverflowIsFixed()
        {
            target.GetLocalDateFromColumn(-2);
        }

        private class DayPresenterTestClass : DayPresenter
        {
            internal DayPresenterTestClass(IScheduleViewBase view, SchedulerStateHolder schedulerState, GridlockManager lockManager, 
                ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter)
                : base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, new OverriddenBusinessRulesHolder(), new DoNothingScheduleDayChangeCallBack(), NullScheduleTag.Instance)
            {
            }

            internal void CreateDayHeaderTest(GridQueryCellInfoEventArgs e)
            {
                base.CreateDayHeader(e);
            }
        }
    }
}
