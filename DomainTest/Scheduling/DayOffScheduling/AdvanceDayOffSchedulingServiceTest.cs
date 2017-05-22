using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
    [TestFixture]
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public class AdvanceDaysOffSchedulingServiceTest
    {
        private MockRepository _mock;
        private AdvanceDaysOffSchedulingServiceOLD _target;
        private IAbsencePreferenceScheduler _absencePreferenceScheduler;
        private ITeamBlockMissingDayOffHandler _teamBlockMissingDayOffHandler;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IList<IScheduleMatrixPro> _matrixList;
        private SchedulingOptions _schedulingOptions;
        private bool _cancelTarget;
        private ITeamDayOffScheduler _teamDayOffScheduler;
	    private List<IPerson> _selectedPersons;
		private IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _absencePreferenceScheduler = _mock.StrictMock<IAbsencePreferenceScheduler>();
            _teamBlockMissingDayOffHandler = _mock.StrictMock<ITeamBlockMissingDayOffHandler>();
            _teamDayOffScheduler = _mock.StrictMock<ITeamDayOffScheduler>();
            _target = new AdvanceDaysOffSchedulingServiceOLD(_absencePreferenceScheduler, _teamDayOffScheduler, _teamBlockMissingDayOffHandler);
            _rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            _matrixList = new List<IScheduleMatrixPro>();
            _schedulingOptions = new SchedulingOptions();
		    _selectedPersons = new List<IPerson>();
		    _groupPersonBuilderWrapper = _mock.StrictMock<IGroupPersonBuilderWrapper>();
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
				Expect.Call(() => _teamDayOffScheduler.DayOffScheduling(_matrixList, _selectedPersons, _rollbackService, _schedulingOptions, _groupPersonBuilderWrapper));
                Expect.Call(() => _teamDayOffScheduler.DayScheduled -= null).IgnoreArguments();

                Expect.Call(() => _teamBlockMissingDayOffHandler.DayScheduled += null).IgnoreArguments();
                Expect.Call(() => _teamBlockMissingDayOffHandler.Execute(_matrixList, _schedulingOptions, _rollbackService));
                Expect.Call(() => _teamBlockMissingDayOffHandler.DayScheduled -= null).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.DayScheduled += targetDayScheduled;
				_target.Execute(_matrixList, _selectedPersons, _rollbackService, _schedulingOptions, _groupPersonBuilderWrapper, new DateOnlyPeriod());
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
				_absencePreferenceScheduler.DayScheduled += null;
				var eventRaiser = LastCall.IgnoreArguments().GetEventRaiser();

				Expect.Call(() => _absencePreferenceScheduler.DayScheduled -= null).IgnoreArguments();
				Expect.Call(() => _absencePreferenceScheduler.AddPreferredAbsence(_matrixList, _schedulingOptions))
					.Callback(new Func<IEnumerable<IScheduleMatrixPro>, SchedulingOptions, bool>((a, b) =>
					{
						eventRaiser.Raise(null, args);
						return true;
					}));
            }
            using (_mock.Playback())
            {
				_target.Execute(_matrixList, _selectedPersons, _rollbackService, _schedulingOptions, _groupPersonBuilderWrapper, new DateOnlyPeriod());
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
