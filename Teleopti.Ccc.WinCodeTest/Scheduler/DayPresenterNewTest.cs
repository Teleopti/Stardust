using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Common.Clipboard;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class DayPresenterNewTest
    {
        private DayPresenterNew target;
        private MockRepository mocks;
        private IScheduleViewBase viewBase;
        private IScenario scenario;
        private GridlockManager gridlockManager;
        private ClipHandler<IScheduleDay> clipHandlerSchedulePart;
        private ISchedulerStateHolder schedulerState;
        private readonly DateTime _date = new DateTime(2008, 11, 04, 0, 0, 0, DateTimeKind.Utc);
        private IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
        private IScheduleDayChangeCallback _scheduleDayChangeCallback;
        private IDayPresenterScaleCalculator _scaleCalculator;

        [SetUp]
        public void Setup()
        {

            mocks = new MockRepository();
            viewBase = mocks.StrictMock<IScheduleViewBase>();
            scenario = mocks.StrictMock<IScenario>();
            gridlockManager = new GridlockManager();
            clipHandlerSchedulePart = new ClipHandler<IScheduleDay>();
            schedulerState = new SchedulerStateHolder(scenario, new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(), TeleoptiPrincipal.Current.Regional.TimeZone), new List<IPerson>());
            _overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();
            _scheduleDayChangeCallback = mocks.DynamicMock<IScheduleDayChangeCallback>();
            _scaleCalculator = mocks.StrictMock<IDayPresenterScaleCalculator>();
            target = new DayPresenterNew(viewBase, schedulerState, gridlockManager, clipHandlerSchedulePart,
                                      SchedulePartFilter.None, _overriddenBusinessRulesHolder, _scheduleDayChangeCallback, _scaleCalculator, NullScheduleTag.Instance);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void VerifyCreateDayHeader()
        {
            target = new DayPresenterNewTestClass(viewBase, schedulerState, gridlockManager, clipHandlerSchedulePart,
                                      SchedulePartFilter.None, _scaleCalculator);
            using (mocks.Record())
            {
                Expect.Call(_scaleCalculator.CalculateScalePeriod(schedulerState, new DateOnly(2011, 1, 1))).Return(new DateTimePeriod(2011, 1, 1, 2011, 1, 2));
                Expect.Call(() => viewBase.SetCellBackTextAndBackColor(null, _date, false, false, null)).IgnoreArguments
                    ();
            }

            using (mocks.Playback())
            {
                target.SelectDate(new DateOnly(2011, 1, 1));
                GridQueryCellInfoEventArgs eventArgs = new GridQueryCellInfoEventArgs(0, -1, new GridStyleInfo());
                ((DayPresenterNewTestClass)target).CreateDayHeaderTest(eventArgs);

                eventArgs = new GridQueryCellInfoEventArgs(0, (int)ColumnType.StartScheduleColumns, new GridStyleInfo());
                ((DayPresenterNewTestClass)target).CreateDayHeaderTest(eventArgs);
                Assert.AreEqual(new DateOnly(2011, 1, 1), eventArgs.Style.Tag);

                eventArgs = new GridQueryCellInfoEventArgs(1, (int)ColumnType.StartScheduleColumns, new GridStyleInfo());
                eventArgs.Style.Tag = target.SelectedPeriod.DateOnlyPeriod.StartDate;
                ((DayPresenterNewTestClass)target).CreateDayHeaderTest(eventArgs);
                Assert.AreEqual(new DateOnly(2011, 1, 1), eventArgs.Style.Tag);
            }

            Assert.AreEqual(new DateTimePeriod(2011, 1, 1, 2011, 1, 2), target.ScalePeriod);
            Assert.AreEqual(6, target.ColCount);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyQueryCellInfo()
        {
            IDictionary<Guid, IPerson> persons;
            IPerson person;
            IScheduleDictionary scheduleDictionary;
            IScheduleRange range;
            IScheduleDay scheduleDay1;
            person = PersonFactory.CreatePerson();
            persons = new Dictionary<Guid, IPerson>();
            persons.Add(Guid.NewGuid(), person);
            scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();
            range = mocks.StrictMock<IScheduleRange>();
            scheduleDay1 = mocks.StrictMock<IScheduleDay>();
            IProjectionService projectionService = mocks.StrictMock<IProjectionService>();
            ISchedulerStateHolder schedulerState1 = mocks.StrictMock<ISchedulerStateHolder>();
            DateTimePeriod assPeriod2 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 16, 0, 0, 0, DateTimeKind.Utc));
            IPersonAssignment ass2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, assPeriod2);
            IVisualLayerCollection visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(person,
                                                                                                           TimeSpan.
                                                                                                               FromHours
                                                                                                               (8),
                                                                                                           TimeSpan.
                                                                                                               FromHours
                                                                                                               (16));
            using (mocks.Record())
            {
                Expect.Call(_scaleCalculator.CalculateScalePeriod(schedulerState1, new DateOnly(2011, 1, 1))).Return(
                    new DateTimePeriod(2011, 1, 1, 2011, 1, 2));
                Expect.Call(() => viewBase.SetCellBackTextAndBackColor(null, _date, false, false, null)).IgnoreArguments
                    ();
                Expect.Call(viewBase.RowHeaders).Return(1).Repeat.AtLeastOnce();
                Expect.Call(schedulerState1.RequestedPeriod).Return(new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(), TeleoptiPrincipal.Current.Regional.TimeZone));
                Expect.Call(schedulerState1.FilteredPersonDictionary).Return(persons).Repeat.AtLeastOnce();
                Expect.Call(schedulerState1.Schedules).Return(scheduleDictionary);
                Expect.Call(scheduleDictionary[person]).Return(range);
                Expect.Call(range.ScheduledDay(new DateOnly(2011, 01, 01))).Return(scheduleDay1);
                Expect.Call(scheduleDay1.FullAccess).Return(true).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay1.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay1.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2011, 1, 1), person.PermissionInformation.DefaultTimeZone())).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay1.Period).Return(
                    new DateOnlyAsDateTimePeriod(new DateOnly(2011, 1, 1),
                                                 person.PermissionInformation.DefaultTimeZone()).Period());
                Expect.Call(scheduleDay1.PersonAssignmentConflictCollection).Return(new List<IPersonAssignment>());
                Expect.Call(scheduleDay1.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>{ass2})).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay1.TimeZone).Return(person.PermissionInformation.DefaultTimeZone());
                Expect.Call(scheduleDay1.PersonAbsenceCollection()).Return(new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence>()));
                Expect.Call(scheduleDay1.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
                Expect.Call(scheduleDay1.BusinessRuleResponseCollection).Return(new List<IBusinessRuleResponse>());
                Expect.Call(scheduleDay1.PersonMeetingCollection()).Return(new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>()));
                Expect.Call(scheduleDay1.ProjectionService()).Return(projectionService);
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
            }

            using (mocks.Playback())
            {

                target = new DayPresenterNewTestClass(viewBase, schedulerState1, gridlockManager,
                                                      clipHandlerSchedulePart,
                                                      SchedulePartFilter.None, _scaleCalculator);
                target.SelectDate(new DateOnly(2011, 1, 1));
                GridQueryCellInfoEventArgs eventArgs = new GridQueryCellInfoEventArgs(2,
                                                                                      (int)
                                                                                      ColumnType.StartScheduleColumns,
                                                                                      new GridStyleInfo());
                target.QueryCellInfo(this, eventArgs);

                //eventArgs = new GridQueryCellInfoEventArgs(0, (int)ColumnType.StartScheduleColumns, new GridStyleInfo());
                //((DayPresenterNewTestClass)target).CreateDayHeaderTest(eventArgs);
                //Assert.AreEqual(new DateOnly(2011, 1, 1).Date, eventArgs.Style.Tag);

                //eventArgs = new GridQueryCellInfoEventArgs(1, (int)ColumnType.StartScheduleColumns, new GridStyleInfo());
                //eventArgs.Style.Tag = target.SelectedPeriod.LocalStartDateTime.Date;
                //((DayPresenterNewTestClass)target).CreateDayHeaderTest(eventArgs);
                //Assert.AreEqual(new DateOnly(2011, 1, 1).Date, eventArgs.Style.Tag);
            }
        }

        //[Test]
        //public void VerifyCreateDayHeader()
        //{
        //    target = new DayPresenterNewTestClass(viewBase, schedulerState, gridlockManager, clipHandlerSchedulePart,
        //                              SchedulePartFilter.None, _scaleCalculator);

        //    viewBase.SetCellBackTextAndBackColor(null, _date, false, false, null);
        //    LastCall.IgnoreArguments().Repeat.Once();

        //    mocks.ReplayAll();
        //    GridQueryCellInfoEventArgs eventArgs = new GridQueryCellInfoEventArgs(0, -1, new GridStyleInfo());
        //    ((DayPresenterNewTestClass)target).CreateDayHeaderTest(eventArgs);

        //    target.SelectedPeriod = DateTimeFactory.CreateDateTimePeriod(_date, 1);
        //    eventArgs = new GridQueryCellInfoEventArgs(0, (int)ColumnType.StartScheduleColumns, new GridStyleInfo());
        //    ((DayPresenterNewTestClass)target).CreateDayHeaderTest(eventArgs);
        //    Assert.AreEqual(target.SelectedPeriod, eventArgs.Style.Tag);

        //    eventArgs = new GridQueryCellInfoEventArgs(1, (int)ColumnType.StartScheduleColumns, new GridStyleInfo());
        //    eventArgs.Style.Tag = target.SelectedPeriod.LocalStartDateTime.Date;
        //    ((DayPresenterNewTestClass)target).CreateDayHeaderTest(eventArgs);
        //    Assert.AreEqual(target.SelectedPeriod.LocalStartDateTime.Date, eventArgs.Style.Tag);

        //    mocks.VerifyAll();
        //}

        private class DayPresenterNewTestClass : DayPresenterNew
        {
            internal DayPresenterNewTestClass(IScheduleViewBase view, ISchedulerStateHolder schedulerState, GridlockManager lockManager,
                ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter, IDayPresenterScaleCalculator scaleCalculator)
                : base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, new OverriddenBusinessRulesHolder(), new Domain.Scheduling.EmptyScheduleDayChangeCallback(), scaleCalculator, NullScheduleTag.Instance)
            {
                
            }

            internal void CreateDayHeaderTest(GridQueryCellInfoEventArgs e)
            {
                base.CreateDayHeader(e);
            }

            public override void QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
            {
                QuerySchedulePartCellInfo(e);
            }
        }
    }

    
}