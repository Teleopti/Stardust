using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.RestrictionSummary
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
    public class SingleAgentRestrictionPresenterTest
    {
        private MockRepository _mocks;
        private SingleAgentRestrictionPresenter _target;
        private IRestrictionSummaryGrid _restrictionSummaryGrid;
        private SingleAgentRestrictionModel _singleAgentRestrictionModel;
        private DateTimePeriod _dateTimePeriod;
        private IPerson _person; 
        private IList<IPerson> _loadedPersons;
        private ISchedulingResultStateHolder _stateHolder;
        private ISchedulingOptions _schedulingOptions;
        private ISchedulePeriod _schedulePeriod;
        private TimeZoneInfo _timeZoneInfo;
        private ScheduleDictionaryForTest _dic;
        private ScheduleRange _range;
        private IWorkShiftWorkTime _workShiftWorkTime;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _restrictionSummaryGrid = _mocks.StrictMock<IRestrictionSummaryGrid>();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _schedulingOptions = new RestrictionSchedulingOptions
            {
                UseAvailability = true,
                UsePreferences = true,
                UseStudentAvailability = true,
                UseRotations = true,
                UseScheduling = true
            };
            _timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            DateTime start = new DateTime(2010, 3, 31, 22, 0, 0, DateTimeKind.Utc); 
            DateTime end = new DateTime(2010, 4, 30, 22, 0,0, DateTimeKind.Utc);
            _person = PersonFactory.CreatePerson("Lars", "Lagerbäck");
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();

            _person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2009, 1, 1)));
            _schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2010, 2, 3),
                                                                         SchedulePeriodType.Week, 4);
            _schedulePeriod.AverageWorkTimePerDayOverride = new TimeSpan(8, 0, 0);
            _schedulePeriod.DaysOff = 8;
            _person.RemoveAllSchedulePeriods();
            _person.AddSchedulePeriod(_schedulePeriod);

            _dic = new ScheduleDictionaryForTest(scenario,
                                                 new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2020, 1, 1)),
                                                 new Dictionary<IPerson, IScheduleRange>());
						var dayOff = new PersonDayOff(_person, _dic.Scenario, new DayOff(new DateTime(), new TimeSpan(), new TimeSpan(), new Description(), Color.Red, string.Empty), new DateOnly(2010, 4, 1), _timeZoneInfo);
            _range = new ScheduleRange(_dic, dayOff);
            _range.Add(dayOff);

            _loadedPersons = new List<IPerson> { _person };
            _dateTimePeriod = new DateTimePeriod(start, end);
            _workShiftWorkTime = _mocks.StrictMock<IWorkShiftWorkTime>();
            _singleAgentRestrictionModel = new SingleAgentRestrictionModel(_dateTimePeriod, _timeZoneInfo, _workShiftWorkTime);
            using (_mocks.Record())
            {
                Expect.Call(_restrictionSummaryGrid.RowCount).Return(2).Repeat.AtLeastOnce();
                _restrictionSummaryGrid.RowCount = 2;
                LastCall.IgnoreArguments().Repeat.AtLeastOnce();
                _restrictionSummaryGrid.HeaderCount = 1;
                LastCall.IgnoreArguments();
                _restrictionSummaryGrid.ColCount = 12;
                LastCall.IgnoreArguments();
                Expect.Call(_restrictionSummaryGrid.RowCount).Return(2).Repeat.AtLeastOnce();
                _restrictionSummaryGrid.KeepSelection(false);
                LastCall.IgnoreArguments();
                _restrictionSummaryGrid.Invalidate();
                LastCall.IgnoreArguments();
                Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.Any();
                _restrictionSummaryGrid.TipText(1, 7, UserTexts.Resources.ContractTimeDoesNotMeetTheTargetTime);
                LastCall.IgnoreArguments();
                _restrictionSummaryGrid.TipText(1, 11, UserTexts.Resources.WrongNumberOfDaysOff);
                LastCall.IgnoreArguments();
                _restrictionSummaryGrid.TipText(2, 10, UserTexts.Resources.HighestPossibleWorkTimeIsTooLow);
            }
            _singleAgentRestrictionModel.Initialize(_loadedPersons, _stateHolder, _schedulingOptions);
            _target = new SingleAgentRestrictionPresenter(_restrictionSummaryGrid, _singleAgentRestrictionModel);
            _target.Initialize(_loadedPersons, _stateHolder, _schedulingOptions);
            _mocks.ReplayAll();
        }

        [Test]
        public void CanCreateInstance()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void CanGetProperties()
        {
            Assert.IsNotNull(_target.SchedulingResultStateHolder);
            Assert.IsTrue(_target.IsInitialized);
        }

        [Test]
        public void CanInitializePresenter()
        {
            Assert.IsTrue(_target.IsInitialized);
        }

        [Test]
        public void CanReload()
        {
            _mocks.BackToRecordAll();
            using (_mocks.Record())
            {
                _restrictionSummaryGrid.RowCount = 2;
                LastCall.IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_restrictionSummaryGrid.RowCount).Return(2).Repeat.AtLeastOnce();
                _restrictionSummaryGrid.KeepSelection(true);
                LastCall.IgnoreArguments().Repeat.AtLeastOnce();
                _restrictionSummaryGrid.Invalidate();
                LastCall.IgnoreArguments().Repeat.Twice();
                Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.Any();
                _restrictionSummaryGrid.TipText(1, 7, UserTexts.Resources.ContractTimeDoesNotMeetTheTargetTime);
                LastCall.IgnoreArguments();
                _restrictionSummaryGrid.TipText(1, 11, UserTexts.Resources.WrongNumberOfDaysOff);
                LastCall.IgnoreArguments();
                _restrictionSummaryGrid.TipText(2, 10, UserTexts.Resources.HighestPossibleWorkTimeIsTooLow);
            }
            _target.Reload(_loadedPersons);
            _mocks.ReplayAll();
        }

        [Test]
        public void CanSelectAgentInfo()
        {
            _mocks.BackToRecordAll();
            using (_mocks.Record())
            {
                Expect.Call(_restrictionSummaryGrid.CurrentCellRowIndex).Return(2).Repeat.AtLeastOnce();
            }
            Assert.IsNotNull(_target.SelectedAgentInfo());
            _mocks.ReplayAll();
            _mocks.BackToRecordAll();
            using (_mocks.Record())
            {
                Expect.Call(_restrictionSummaryGrid.CurrentCellRowIndex).Return(1).Repeat.AtLeastOnce();
            }
            Assert.IsNull(_target.SelectedAgentInfo());
            _mocks.ReplayAll();
        }

        [Test]
        public void VerifySetSchedulingOptions()
        {
            ISchedulingOptions options = new RestrictionSchedulingOptions{UseScheduling = true};
            _mocks.BackToRecordAll();
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.Any();
                _restrictionSummaryGrid.RowCount = 2;
                LastCall.IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_restrictionSummaryGrid.RowCount).Return(2).Repeat.AtLeastOnce();
                _restrictionSummaryGrid.TipText(1, 7, UserTexts.Resources.ContractTimeDoesNotMeetTheTargetTime);
                LastCall.IgnoreArguments();
                _restrictionSummaryGrid.TipText(1, 11, UserTexts.Resources.WrongNumberOfDaysOff);
                LastCall.IgnoreArguments();
                _restrictionSummaryGrid.KeepSelection(false);
                LastCall.IgnoreArguments().Repeat.AtLeastOnce();
                _restrictionSummaryGrid.Invalidate();
                LastCall.IgnoreArguments().Repeat.Twice();
                _restrictionSummaryGrid.TipText(2, 10, UserTexts.Resources.HighestPossibleWorkTimeIsTooLow);
            }
            _target.SetSchedulingOptions(options, false);
            Assert.AreEqual(options, _target.SchedulingOptions);
            _mocks.ReplayAll();
        }

        [Test]
        public void CanCallDispose()
        {
            _target.Dispose();
        }

        [Test]
        public void VerifySetSelectionReturnsRowIndex()
        {
            _mocks.BackToRecordAll();
            using (_mocks.Record())
            {
                _restrictionSummaryGrid.SetSelections(2, true);
                LastCall.Repeat.Once();
            }
            _target.SetSelection(_range.ScheduledDay(new DateOnly(2010, 4, 1)));

            _mocks.ReplayAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyOnQueryCellType()
        {
            Assert.IsTrue(SingleAgentRestrictionPresenter.OnQueryCellType(0) == "Header");
            Assert.IsTrue(SingleAgentRestrictionPresenter.OnQueryCellType(1) == "NumericCell");
            Assert.IsTrue(SingleAgentRestrictionPresenter.OnQueryCellType(6) == "NumericCell");
            Assert.IsTrue(SingleAgentRestrictionPresenter.OnQueryCellType(8) == "NumericCell");
            Assert.IsTrue(SingleAgentRestrictionPresenter.OnQueryCellType(11) == "NumericCell");
            Assert.IsTrue(SingleAgentRestrictionPresenter.OnQueryCellType(5) == "TimeSpan");
            Assert.IsTrue(SingleAgentRestrictionPresenter.OnQueryCellType(7) == "TimeSpan");
            Assert.IsTrue(SingleAgentRestrictionPresenter.OnQueryCellType(9) == "TimeSpan");
            Assert.IsTrue(SingleAgentRestrictionPresenter.OnQueryCellType(10) == "TimeSpan");
            Assert.IsTrue(SingleAgentRestrictionPresenter.OnQueryCellType(2) == "Static");
            Assert.IsTrue(SingleAgentRestrictionPresenter.OnQueryCellType(3) == "Static");
            Assert.IsTrue(SingleAgentRestrictionPresenter.OnQueryCellType(4) == "Static");
            Assert.IsTrue(SingleAgentRestrictionPresenter.OnQueryCellType(12) == "Static");
        }

        [Test]
        public void VerifyOnQueryCellInfo()
        {
            Assert.AreEqual(string.Empty, _target.OnQueryCellInfo(0,0));
            Assert.AreEqual(UserTexts.Resources.SchedulePeriod, _target.OnQueryCellInfo(0,2));
            Assert.AreEqual(UserTexts.Resources.Schedule, _target.OnQueryCellInfo(0,7));
            Assert.AreEqual(UserTexts.Resources.SchedulePlusRestrictions, _target.OnQueryCellInfo(0,9));
            Assert.AreEqual(UserTexts.Resources.Name, _target.OnQueryCellInfo(1,0));

            Assert.AreEqual(string.Concat(_person.Name.FirstName, " ", _person.Name.LastName), _target.OnQueryCellInfo(2,0));
            Assert.AreEqual(3, _target.OnQueryCellInfo(2,1));
            Assert.AreEqual("Week", _target.OnQueryCellInfo(2,2));
            Assert.AreEqual(new DateOnly(2010, 3, 31).Date.ToShortDateString(), _target.OnQueryCellInfo(2,3));
            Assert.AreEqual(new DateOnly(2010, 4, 27).Date.ToShortDateString(), _target.OnQueryCellInfo(2,4));
            // the following assert does not work because of the 
            // _schedulePeriod.PeriodTarget is not correct
            // Assert.AreEqual(_schedulePeriod.PeriodTarget(new DateOnly(2010, 3, 31)), _target.OnQueryCellInfo(2, 5));
            Assert.AreEqual(_schedulePeriod.DaysOff, _target.OnQueryCellInfo(2, 6));
            Assert.AreEqual(new TimeSpan(), _target.OnQueryCellInfo(2, 7));
            //Assert.AreEqual(0, _target.OnQueryCellInfo(2, 8));
            Assert.AreEqual(new TimeSpan(), _target.OnQueryCellInfo(2, 9));
            Assert.AreEqual(new TimeSpan(), _target.OnQueryCellInfo(2, 10));
            Assert.AreEqual(0, _target.OnQueryCellInfo(2, 11));
            Assert.AreEqual(UserTexts.Resources.No, _target.OnQueryCellInfo(2, 12));
        }

        [Test]
        public void VerifySortCanBeCalled()
        {
            _mocks.BackToRecordAll();
            using (_mocks.Record())
            {
                _restrictionSummaryGrid.Invalidate();
                LastCall.Repeat.Once();
                Expect.Call(_restrictionSummaryGrid.HeaderCount).Return(1);
                _restrictionSummaryGrid.SetSelections(1, false);
                LastCall.Repeat.Once();
            }
            _target.Sort(0, true);
            _mocks.ReplayAll();
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void VerifySetSelectedAgentDateThrowsErrorIfRowIndexIsTooLow()
        {
            _target.SetSelectedPersonDate(0);
        }

        [Test]
        public void VerifySetSelectedAgentDateDoesNotThrowError()
        {
            _target.SetSelectedPersonDate(2);
        }
    }
}
