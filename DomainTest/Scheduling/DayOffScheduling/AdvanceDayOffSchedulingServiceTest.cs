using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
    [TestFixture]
	public class AdvanceDaysOffSchedulingServiceTest
    {
        private MockRepository _mock;
        private AdvanceDaysOffSchedulingService _target;
        private IAbsencePreferenceScheduler _absencePreferenceScheduler;
        private ITeamBlockMissingDayOffHandler _teamBlockMissingDayOffHandler;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IList<IScheduleMatrixPro> _matrixList;
        private ISchedulingOptions _schedulingOptions;
        private bool _cancelTarget;
        private ITeamDayOffScheduler _teamDayOffScheduler;
	    private IGroupPersonBuilderForOptimization _groupPersonBuilder;
	    private List<IPerson> _selectedPersons;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _absencePreferenceScheduler = _mock.StrictMock<IAbsencePreferenceScheduler>();
            _teamBlockMissingDayOffHandler = _mock.StrictMock<ITeamBlockMissingDayOffHandler>();
            _teamDayOffScheduler = _mock.StrictMock<ITeamDayOffScheduler>();
            _target = new AdvanceDaysOffSchedulingService(_absencePreferenceScheduler, _teamDayOffScheduler, _teamBlockMissingDayOffHandler);
            _rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
	        _groupPersonBuilder = _mock.StrictMock<IGroupPersonBuilderForOptimization>();
            _matrixList = new List<IScheduleMatrixPro>();
            _schedulingOptions = new SchedulingOptions();
		    _selectedPersons = new List<IPerson>();
        }

        [Test]
        public void ShouldExecuteAndFireEvent()
        {
            using (_mock.Record())
            {
                Expect.Call(() => _absencePreferenceScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(() => _absencePreferenceScheduler.AddPreferredAbsence(_matrixList, _schedulingOptions));
                Expect.Call(() => _absencePreferenceScheduler.DayScheduled -= null).IgnoreArguments();
                
                Expect.Call(() => _teamDayOffScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(() => _teamDayOffScheduler.DayOffScheduling(_matrixList, _selectedPersons, _rollbackService, _schedulingOptions, _groupPersonBuilder));
                Expect.Call(() => _teamDayOffScheduler.DayScheduled -= null).IgnoreArguments();

                Expect.Call(() => _teamBlockMissingDayOffHandler.DayScheduled += null).IgnoreArguments();
                Expect.Call(() => _teamBlockMissingDayOffHandler.Execute(_matrixList, _schedulingOptions, _rollbackService));
                Expect.Call(() => _teamBlockMissingDayOffHandler.DayScheduled -= null).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.DayScheduled += targetDayScheduled;
				_target.Execute(_matrixList, _selectedPersons, _rollbackService, _schedulingOptions, _groupPersonBuilder);
                _target.DayScheduled -= targetDayScheduled;
            }
        }

        [Test]
        public void ShouldCancelAfterFirstIfAsked()
        {
			SchedulingServiceBaseEventArgs args = new SchedulingServiceSuccessfulEventArgs(null);
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
				_target.Execute(_matrixList, _selectedPersons, _rollbackService, _schedulingOptions, _groupPersonBuilder);
            }
        }

        [Test]
        public void ShouldListenToEventsFromAllClasses()
        {
			SchedulingServiceBaseEventArgs args = new SchedulingServiceSuccessfulEventArgs(null);
            args.Cancel = true;

            using (_mock.Record())
            { }
            using (_mock.Playback())
            {
                _absencePreferenceScheduler.Raise(x => x.DayScheduled += targetDayScheduled, this, args);
                Assert.IsTrue(_cancelTarget);
                _cancelTarget = false;
                _teamDayOffScheduler.Raise(x => x.DayScheduled += targetDayScheduled, this, args);
                Assert.IsTrue(_cancelTarget);
                _cancelTarget = false;
                _teamBlockMissingDayOffHandler.Raise(x => x.DayScheduled += targetDayScheduled, this, args);
                Assert.IsTrue(_cancelTarget);
            }
        }


        void targetDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
        {
            _cancelTarget = true;
        }
    }

   
}
