using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
	[TestFixture]
	public class DaysOffSchedulingServiceTest
	{
		private MockRepository _mocks;
		private DaysOffSchedulingService _target;
		private IAbsencePreferenceScheduler _absencePreferenceScheduler;
		private IDayOffScheduler _dayOffScheduler;
		private IMissingDaysOffScheduler _missingDaysOffScheduler;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IList<IScheduleMatrixPro> _matrixList;
		private ISchedulingOptions _schedulingOptions;
		private IScheduleTagSetter _scheduleTagSetter;

		private bool _cancelTarget;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_absencePreferenceScheduler = _mocks.StrictMock<IAbsencePreferenceScheduler>();
			_dayOffScheduler = _mocks.StrictMock<IDayOffScheduler>();
			_missingDaysOffScheduler = _mocks.StrictMock<IMissingDaysOffScheduler>();
			_target = new DaysOffSchedulingService(()=>_absencePreferenceScheduler, ()=>_dayOffScheduler, ()=>_missingDaysOffScheduler);
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_matrixList = new List<IScheduleMatrixPro>();
			_schedulingOptions = new SchedulingOptions();
			_scheduleTagSetter = new FakeScheduleTagSetter();
		}

		[Test]
		public void ShouldExecuteAndFireEvent()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _absencePreferenceScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(() => _absencePreferenceScheduler.AddPreferredAbsence(_matrixList, _schedulingOptions));
				Expect.Call(() => _absencePreferenceScheduler.DayScheduled -= null).IgnoreArguments();

				Expect.Call(() => _dayOffScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(() => _dayOffScheduler.DayOffScheduling(_matrixList, _matrixList, _rollbackService, _schedulingOptions, _scheduleTagSetter));
				Expect.Call(() => _dayOffScheduler.DayScheduled -= null).IgnoreArguments();

				Expect.Call(() => _missingDaysOffScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_missingDaysOffScheduler.Execute(_matrixList, _schedulingOptions, _rollbackService)).Return(true);
				Expect.Call(() => _missingDaysOffScheduler.DayScheduled -= null).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				_target.DayScheduled += targetDayScheduled;
				_target.Execute(_matrixList, _matrixList, _rollbackService, _schedulingOptions, _scheduleTagSetter);
				_target.DayScheduled -= targetDayScheduled;
			}
		}

		[Test]
		public void ShouldCancelAfterFirstIfAsked()
		{
			SchedulingServiceBaseEventArgs args = new SchedulingServiceSuccessfulEventArgs(null);
			args.Cancel = true;
			using (_mocks.Record())
			{
				_absencePreferenceScheduler.DayScheduled += null;
				var raiser = LastCall.IgnoreArguments().GetEventRaiser();
				Expect.Call(() => _absencePreferenceScheduler.AddPreferredAbsence(_matrixList, _schedulingOptions)).WhenCalled(_ => raiser.Raise(null,args));
				Expect.Call(() => _absencePreferenceScheduler.DayScheduled -= null).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				_target.Execute(_matrixList, _matrixList, _rollbackService, _schedulingOptions, _scheduleTagSetter);
			}
		}

		[Test]
		public void ShouldListenToEventsFromAllClasses()
		{
			SchedulingServiceBaseEventArgs args = new SchedulingServiceSuccessfulEventArgs(null);
			args.Cancel = true;

			using (_mocks.Record())
			{}
			using (_mocks.Playback())
			{
				_absencePreferenceScheduler.Raise(x => x.DayScheduled += targetDayScheduled, this, args);
				Assert.IsTrue(_cancelTarget);
				_cancelTarget = false;
				_dayOffScheduler.Raise(x => x.DayScheduled += targetDayScheduled, this, args);
				Assert.IsTrue(_cancelTarget);
				_cancelTarget = false;
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