using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.RestrictionSummary
{
    [TestFixture]
    public class RestrictionSummaryPresenterTest
    {
        private RestrictionSummaryPresenter _target;
        private MockRepository _mocks;
        private IRestrictionSummaryView _viewBase;
        private IScenario _scenario;
        private GridlockManager _gridlockManager;
        private ClipHandler<IScheduleDay> _clipHandlerSchedulePart;
        private SchedulerStateHolder _schedulerState;
        private readonly DateTime _date = new DateTime(2008, 11, 04, 0, 0, 0, DateTimeKind.Utc);
        private RestrictionSummaryModel _model;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IPerson _person;
        private RestrictionSchedulingOptions _schedulingOptions; 
        private ISchedulePeriod _schedulePeriod;
        private IWorkShiftWorkTime _workShiftWorkTime;
        private IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
        private IScheduleDayChangeCallback _scheduleDayChangeCallback;
    	private IPreferenceNightRestChecker _preferenceNightRestChecker;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingOptions = new RestrictionSchedulingOptions();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(_date.AddDays(-10)), new List<ISkill>());
            _viewBase = _mocks.StrictMock<IRestrictionSummaryView>();
            _scenario = _mocks.StrictMock<IScenario>();
            _scheduleDayChangeCallback = _mocks.DynamicMock<IScheduleDayChangeCallback>();
            _gridlockManager = new GridlockManager();
            _clipHandlerSchedulePart = new ClipHandler<IScheduleDay>();
            _schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(_date));
            _person.AddSchedulePeriod(_schedulePeriod);
            _schedulerState = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(), TeleoptiPrincipal.Current.Regional.TimeZone), new List<IPerson>());
        	_preferenceNightRestChecker = _mocks.StrictMock<IPreferenceNightRestChecker>();
			_model = new RestrictionSummaryModel(_schedulingResultStateHolder, new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService())), _schedulerState, _preferenceNightRestChecker);
            _overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();
            _target = new RestrictionSummaryPresenter(_viewBase, _schedulerState, _gridlockManager, _clipHandlerSchedulePart,
                                      SchedulePartFilter.None, _model, _overriddenBusinessRulesHolder, _scheduleDayChangeCallback, NullScheduleTag.Instance);
				_workShiftWorkTime = _mocks.StrictMock<IWorkShiftWorkTime>();
        }

        [Test]
        public void VerifyCanCreate()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void CanGetCellDataCollection()
        {
            Assert.IsNotNull(_target.CellDataCollection);
        }

        [Test]
        public void CanGetCellDataClipHandler()
        {
            Assert.IsNotNull(_target.CellDataClipHandler);
        }
        [Test]
        public void VerifyOnQueryCellInfo()
        {
            IPreferenceCellData cellData;
            bool result = _target.OnQueryCellInfo(0, 0, out cellData);
            Assert.IsFalse(result);
            Assert.IsNotNull(cellData);
            IPreferenceCellData cellData2;
            result = _target.OnQueryCellInfo(1, 1, out cellData2);
            Assert.IsFalse(result);
            Assert.IsNull(cellData2);
        }

        [Test]
        public void VerifyOnQueryColumnHeaderText()
        {
            string text = _target.OnQueryColumnHeaderText(1);
            Assert.IsNullOrEmpty(text);
        }
        [Test]
        public void VerifyCorrectMethodsAreCalledOnGetNextPeriod()
        {
            var dateTimePeriod = new DateTimePeriod(_date, _date.AddDays(30));
            var range = _mocks.StrictMock<IScheduleRange>();
            var schedulePartFactoryForDomain = new SchedulePartFactoryForDomain(_person, _scenario, dateTimePeriod,
                                                                                 SkillFactory.CreateSkill("SkilliDill"));
            IDictionary<IPerson, IScheduleRange> dictionary = new Dictionary<IPerson, IScheduleRange> {{_person, range}};
            IScheduleDateTimePeriod scheduleDateTimePeriod = new ScheduleDateTimePeriod(dateTimePeriod);
            IScheduleDictionary scheduleDictionary = new ScheduleDictionaryForTest(_scenario, scheduleDateTimePeriod, dictionary);

            IScheduleDay part = schedulePartFactoryForDomain.CreatePart();
            Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.AtLeastOnce();
            Expect.Call(range.ScheduledDay(new DateOnly(_date))).IgnoreArguments().Return(part).Repeat.AtLeastOnce();
            Expect.Call(() => _viewBase.CellDataLoaded());
            Expect.Call(() => _viewBase.UpdateRowCount());
			Expect.Call(() => _preferenceNightRestChecker.CheckNightlyRest(null)).IgnoreArguments();
            _mocks.ReplayAll();
            var helper = new AgentInfoHelper(_person, new DateOnly(_date), _schedulingResultStateHolder, _schedulingOptions, _workShiftWorkTime);
            helper.SchedulePeriodData();
            _target.GetNextPeriod(helper);
            _mocks.VerifyAll();
        }
        [Test]
        public void VerifyQueryCellInfo()
        {
            var dateTimePeriod = new DateTimePeriod(_date, _date.AddDays(1));
            var schedulePartFactoryForDomain = new SchedulePartFactoryForDomain(_person, _scenario, dateTimePeriod, SkillFactory.CreateSkill("SkilliDill"));
            var part = schedulePartFactoryForDomain.CreatePart();

            IPreferenceCellData cellData = new PreferenceCellData
                                               {
                                                   SchedulePart = part,
                                                   TheDate = part.DateOnlyAsPeriod.DateOnly,
                                                   ViolatesNightlyRest = true,
                                               };

            _target.CellDataCollection.Add(0, cellData);
            var info = new GridStyleInfo {CellValue = string.Empty};
            var e = new GridQueryCellInfoEventArgs(-1,-1, info);
            _target.QueryCellInfo(new object(), e);
            e = new GridQueryCellInfoEventArgs(0, 1, info);
            _target.QueryCellInfo(new object(), e);
            Assert.IsNotEmpty(e.Style.CellValue.ToString());
            e = new GridQueryCellInfoEventArgs(1, 0, info);
            Assert.IsNull(e.Style.CultureInfo);
            e = new GridQueryCellInfoEventArgs(1, 1, info);
            _target.QueryCellInfo(new object(), e);
            Assert.AreEqual("RestrictionSummaryViewCellModel", e.Style.CellType);
            Assert.AreEqual(new DateOnly(cellData.SchedulePart.Period.LocalStartDateTime.Date), e.Style.Tag);

            e = new GridQueryCellInfoEventArgs(1, 0, info);
            _target.QueryCellInfo(new object(), e);
            Assert.AreEqual("RestrictionWeekHeaderViewCellModel", e.Style.CellType);
        }
        [Test]
        public void CanGetRowAndColCount()
        {
            IPreferenceCellData cellData = new PreferenceCellData();
            _target.CellDataCollection.Add(0, cellData);
            Expect.Call(_viewBase.RestrictionGridRowCount).Return(1).Repeat.AtLeastOnce();
            _mocks.ReplayAll();
            Assert.IsTrue(_target.RowCount == 2);
            Assert.IsTrue(_target.ColCount == 7);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyOnQueryWeekHeader()
        {
            var cellData = new PreferenceCellData();
            var startTimeLimitation = new StartTimeLimitation(new TimeSpan(7, 0, 0), null);
            var endTimeLimitation = new EndTimeLimitation(new TimeSpan(0, 20, 0), null);
            var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0));
            IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Natt");
            var effectiveRestriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
                                                                                  workTimeLimitation, shiftCategory,
                                                                                  null, null, new List<IActivityRestriction>());
            cellData.EffectiveRestriction = effectiveRestriction;
            _target.CellDataCollection.Add(0, cellData);
            IWeekHeaderCellData weekHeaderCellData = _target.OnQueryWeekHeader(1);

            Assert.IsNotNull(weekHeaderCellData);
            cellData.EffectiveRestriction = null;
            weekHeaderCellData = _target.OnQueryWeekHeader(1);
            Assert.IsNotNull(weekHeaderCellData);
        }

    }
}
