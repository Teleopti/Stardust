using System;
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
using Teleopti.Ccc.TestCommon.FakeData;

using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon;

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
        private readonly DateOnly _date = new DateOnly(2008, 11, 04);
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
			schedulerState = new SchedulerStateHolder(scenario, new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone), new List<IPerson>(), mocks.DynamicMock<IDisableDeletedFilter>(), new SchedulingResultStateHolder());
            _overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();
            _scheduleDayChangeCallback = mocks.DynamicMock<IScheduleDayChangeCallback>();
            _scaleCalculator = mocks.StrictMock<IDayPresenterScaleCalculator>();
            target = new DayPresenterNew(viewBase, schedulerState, gridlockManager, clipHandlerSchedulePart,
                                      SchedulePartFilter.None, _overriddenBusinessRulesHolder, _scheduleDayChangeCallback, _scaleCalculator, NullScheduleTag.Instance, new UndoRedoContainer(), new FakeTimeZoneGuard());
        }

        [Test]
        public void VerifyCreateDayHeader()
        {
            target = new DayPresenterNewTestClass(viewBase, schedulerState, gridlockManager, clipHandlerSchedulePart,
                                      SchedulePartFilter.None, _scaleCalculator);
			var fakeTimeZoneGuard = new FakeTimeZoneGuard();

			using (mocks.Record())
            {
                Expect.Call(_scaleCalculator.CalculateScalePeriod(schedulerState, new DateOnly(2011, 1, 1), fakeTimeZoneGuard)).Return(new DateTimePeriod(2011, 1, 1, 2011, 1, 2));
                Expect.Call(() => viewBase.SetCellBackTextAndBackColor(null, _date, false, false, null)).IgnoreArguments
                    ();
				Expect.Call(viewBase.TimeZoneGuard).Return(fakeTimeZoneGuard).Repeat.Any();
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

        [Test]
        public void VerifyQueryCellInfo()
        {
            IPerson person = PersonFactory.CreatePerson();
            IDictionary<Guid, IPerson> persons = new Dictionary<Guid, IPerson>();
            persons.Add(Guid.NewGuid(), person);
            IScheduleDictionary scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();
            IScheduleRange range = mocks.StrictMock<IScheduleRange>();
            IScheduleDay scheduleDay1 = mocks.StrictMock<IScheduleDay>();
            ISchedulerStateHolder schedulerState1 = mocks.StrictMock<ISchedulerStateHolder>();
            DateTimePeriod assPeriod2 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 16, 0, 0, 0, DateTimeKind.Utc));
            IPersonAssignment ass2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, assPeriod2);
	        IProjectionService projectionService = mocks.StrictMock<IProjectionService>();
			IVisualLayerCollection visualLayerCollection = new VisualLayerCollection(new List<IVisualLayer>(), null);
			var fakeTimeZoneGuard = new FakeTimeZoneGuard();

            using (mocks.Record())
            {
                Expect.Call(_scaleCalculator.CalculateScalePeriod(schedulerState1, new DateOnly(2011, 1, 1), fakeTimeZoneGuard)).IgnoreArguments().Return(
                    new DateTimePeriod(2011, 1, 1, 2011, 1, 2));
                Expect.Call(() => viewBase.SetCellBackTextAndBackColor(null, _date, false, false, null)).IgnoreArguments
                    ();
				Expect.Call(viewBase.TimeZoneGuard).Return(fakeTimeZoneGuard).Repeat.Any();
				Expect.Call(viewBase.RowHeaders).Return(1).Repeat.AtLeastOnce();
                Expect.Call(schedulerState1.RequestedPeriod).Return(new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(), fakeTimeZoneGuard.CurrentTimeZone()));
                Expect.Call(schedulerState1.FilteredCombinedAgentsDictionary).Return(persons).Repeat.AtLeastOnce();
                Expect.Call(schedulerState1.Schedules).Return(scheduleDictionary);
                Expect.Call(scheduleDictionary[person]).Return(range);
                Expect.Call(range.ScheduledDay(new DateOnly(2011, 01, 01))).Return(scheduleDay1);
                Expect.Call(scheduleDay1.FullAccess).Return(true).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay1.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay1.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2011, 1, 1), person.PermissionInformation.DefaultTimeZone())).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay1.PersonAssignment()).Return(ass2).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay1.PersonAbsenceCollection()).Return(new IPersonAbsence[0]);
                Expect.Call(scheduleDay1.BusinessRuleResponseCollection).Return(new List<IBusinessRuleResponse>());
                Expect.Call(scheduleDay1.PersonMeetingCollection()).Return(new IPersonMeeting[0]);
	            Expect.Call(scheduleDay1.ProjectionService()).Return(projectionService);
	            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
				//Expect.Call(viewBase.TimeZoneGuard).Return(fakeTimeZoneGuard).Repeat.Any();
			}

            using (mocks.Playback())
            {
				using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
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
				}
			}
        }

		[Test]
	    public void ShouldSelectDayAndDayAfter()
	    {
			var scaleCalculator = new DayPresenterScaleCalculator();
		    var presenter = new DayPresenterNew(new FakeScheduleView(), schedulerState, null,null, SchedulePartFilter.None, new OverriddenBusinessRulesHolder(), new DoNothingScheduleDayChangeCallBack(), scaleCalculator, NullScheduleTag.Instance, new UndoRedoContainer(), new FakeTimeZoneGuard());
			presenter.SelectDate(DateOnly.Today);
			Assert.AreEqual(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1)), presenter.SelectedPeriod.DateOnlyPeriod);
	    }

        private class DayPresenterNewTestClass : DayPresenterNew
        {
            internal DayPresenterNewTestClass(IScheduleViewBase view, ISchedulerStateHolder schedulerState, GridlockManager lockManager,
                ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter, IDayPresenterScaleCalculator scaleCalculator)
                : base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, new OverriddenBusinessRulesHolder(), new DoNothingScheduleDayChangeCallBack(), scaleCalculator, NullScheduleTag.Instance, new UndoRedoContainer(), new FakeTimeZoneGuard())
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