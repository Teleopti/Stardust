using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
    [TestFixture]
    public class AdvanceDayOffSchedulingServiceTest
    {
        private MockRepository _mock;
        private AdvanceDaysOffSchedulingService _target;
        private IAbsencePreferenceScheduler _absencePreferenceScheduler;
        //private IDayOffScheduler _dayOffScheduler;
        private IMissingDaysOffScheduler _missingDaysOffScheduler;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IList<IScheduleMatrixPro> _matrixList;
        private ISchedulingOptions _schedulingOptions;
        private bool _cancelTarget;

        [SetUp]
        public void SetUp()
        {
            _mock = new MockRepository();
            _absencePreferenceScheduler = _mock.StrictMock<IAbsencePreferenceScheduler>();
            //_dayOffScheduler = _mock.StrictMock<IDayOffScheduler>();
            _missingDaysOffScheduler = _mock.StrictMock<IMissingDaysOffScheduler>();
            _target = new AdvanceDaysOffSchedulingService(_absencePreferenceScheduler,_missingDaysOffScheduler);
            _rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            _matrixList = new List<IScheduleMatrixPro>();
            _schedulingOptions = new SchedulingOptions();
        }

        [Test]
        public void ShouldExecuteAndFireEvent()
        {
            using (_mock.Record())
            {
                Expect.Call(() => _absencePreferenceScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(() => _absencePreferenceScheduler.AddPreferredAbsence(_matrixList, _schedulingOptions));
                Expect.Call(() => _absencePreferenceScheduler.DayScheduled -= null).IgnoreArguments();

                Expect.Call(() => _missingDaysOffScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_missingDaysOffScheduler.Execute(_matrixList, _schedulingOptions, _rollbackService)).Return(true);
                Expect.Call(() => _missingDaysOffScheduler.DayScheduled -= null).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.DayScheduled += targetDayScheduled;
                _target.Execute(_matrixList, _matrixList, _rollbackService, _schedulingOptions);
                _target.DayScheduled -= targetDayScheduled;
            }
        }

        [Test]
        public void ShouldCancelAfterFirstIfAsked()
        {
            SchedulingServiceBaseEventArgs args = new SchedulingServiceBaseEventArgs(null);
            args.Cancel = true;
            using (_mock.Record())
            {
                Expect.Call(() => _absencePreferenceScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(() => _absencePreferenceScheduler.AddPreferredAbsence(_matrixList, _schedulingOptions));
                Expect.Call(() => _absencePreferenceScheduler.Raise(x => x.DayScheduled += _target.RaiseEventForTest, this, args));
                Expect.Call(() => _absencePreferenceScheduler.DayScheduled -= null).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Execute(_matrixList, _matrixList, _rollbackService, _schedulingOptions);
            }
        }

        [Test]
        public void ShouldListenToEventsFromAllClasses()
        {
            SchedulingServiceBaseEventArgs args = new SchedulingServiceBaseEventArgs(null);
            args.Cancel = true;

            using (_mock.Record())
            { }
            using (_mock.Playback())
            {
                _absencePreferenceScheduler.Raise(x => x.DayScheduled += targetDayScheduled, this, args);
                Assert.IsTrue(_cancelTarget);
                _cancelTarget = false;
                //_dayOffScheduler.Raise(x => x.DayScheduled += targetDayScheduled, this, args);
                //Assert.IsTrue(_cancelTarget);
                //_cancelTarget = false;
                _missingDaysOffScheduler.Raise(x => x.DayScheduled += targetDayScheduled, this, args);
                Assert.IsTrue(_cancelTarget);
            }
        }


        void targetDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
        {
            _cancelTarget = true;
        }
    }

   
}
