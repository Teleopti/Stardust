using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class RestrictionPresenterTest
    {
        private RestrictionPresenter _target;
        private SchedulerStateHolder _schedulerState;
        private IScenario _scenario;
        private MockRepository _mocks;
        private DateOnlyPeriod _period;
        private IScheduleViewBase _viewBase;
        private IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
        private IScheduleDayChangeCallback _scheduleDayChangeCallback;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _viewBase = _mocks.StrictMock<IScheduleViewBase>();
            _scenario = _mocks.StrictMock<IScenario>();
            _period = new DateOnlyPeriod(2009, 2, 2, 2009, 3, 1);
            _schedulerState = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_period,TeleoptiPrincipal.Current.Regional.TimeZone), new List<IPerson>());
            _overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();
            _scheduleDayChangeCallback = _mocks.DynamicMock<IScheduleDayChangeCallback>();
            _target = new RestrictionPresenter(_viewBase, _schedulerState, null, null, SchedulePartFilter.None, _overriddenBusinessRulesHolder,_scheduleDayChangeCallback, NullScheduleTag.Instance);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsFalse(_target.UseAvailability);
            Assert.IsFalse(_target.UsePreference);
            Assert.IsFalse(_target.UseRotation);
            Assert.IsFalse(_target.UseSchedule);
            Assert.IsFalse(_target.UseStudent);
            _target.UseAvailability = true;
            _target.UsePreference = true;
            _target.UseRotation = true;
            _target.UseSchedule = true;
            _target.UseStudent = true;
            Assert.IsTrue(_target.UseAvailability);
            Assert.IsTrue(_target.UsePreference);
            Assert.IsTrue(_target.UseRotation);
            Assert.IsTrue(_target.UseSchedule);
            Assert.IsTrue(_target.UseStudent);
        }

        [Test]
        public void VerifyQueryCellInfo()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();

            using (_mocks.Record())
            {
                Expect.Call(_viewBase.RowHeaders).Return(1).Repeat.Twice();
            }

            using (_mocks.Playback())
            {
                GridStyleInfo style = new GridStyleInfo();
                style.CellValue = schedulePart;
                //style.CellValue = new RestrictionCellValue(null,false,false,false,false,false);
                GridQueryCellInfoEventArgs eventArgs = new GridQueryCellInfoEventArgs(2, (int)ColumnType.StartScheduleColumns, style);
                _target.QueryCellInfo(null, eventArgs);
                Assert.AreEqual(typeof(RestrictionCellValue), eventArgs.Style.CellValue.GetType());
            }
        }
    }
}