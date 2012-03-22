using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class AbsencePreferenceSchedulerTest
	{
		private MockRepository _mocks;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private ISchedulingOptions _options;
		private ISchedulePartModifyAndRollbackService _rollBackService;
		private AbsencePreferenceScheduler _target;
	    private IScheduleMatrixPro _scheduleMatrixPro;
	    private IScheduleDayPro _scheduleDayPro;
	    private IScheduleDayPro _scheduleDayPro2;
	    private IScheduleDay _scheduleDay;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_rollBackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_options = new SchedulingOptions();
		    _scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
		    _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
		    _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
		    _scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_target = new AbsencePreferenceScheduler(_effectiveRestrictionCreator, _options,_rollBackService);
		}

       [Test]
        public void ShouldDoNothingIfMatrixProIsEmpty()
        {
            var matrixProList = new List<IScheduleMatrixPro>();
            _target.AddPreferredAbsence(matrixProList);
        }

        [Test]
        public void ShouldDoNothingIfNoUnlockedDays()
        {
            var matrixProList = new List<IScheduleMatrixPro> {_scheduleMatrixPro};

            using(_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>()));
            }

            using(_mocks.Playback())
            {
                _target.AddPreferredAbsence(matrixProList);
            }
        }

        [Test]
        public void ShouldDoNotingIfNoAbsence()
        {
            var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation() , null, null, null, new List<IActivityRestriction>()) { IsPreferenceDay = true };
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_scheduleDayPro});

            using(_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _options)).Return(effectiveRestriction);
            }

            using(_mocks.Playback())
            {
                _target.AddPreferredAbsence(matrixProList);    
            }
        }

        [Test]
        public void ShouldDoNothingIfNoPreferenceDay()
        {
            var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation() , null, null, null, new List<IActivityRestriction>()) { IsPreferenceDay = false };
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _options)).Return(effectiveRestriction);
            }

            using (_mocks.Playback())
            {
                _target.AddPreferredAbsence(matrixProList);
            }
        }

        [Test]
        public void ShouldAddAbsenceIfPreferred()
        {
            var absence = new Absence();
            var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, absence,new List<IActivityRestriction>()) { IsPreferenceDay = true };
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
            var date = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
            var period = new DateTimePeriod(date, date.AddDays(1));

            using(_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _options)).Return(effectiveRestriction);
                Expect.Call(_scheduleDay.Period).Return(period);
                Expect.Call(() => _scheduleDay.CreateAndAddAbsence(new AbsenceLayer(absence, period))).IgnoreArguments();
                Expect.Call(() => _rollBackService.Modify(_scheduleDay));
            }

            using(_mocks.Playback())
            {
                _target.AddPreferredAbsence(matrixProList);   
            }
        }

        [Test]
        public void ShouldAddAbsenceOnFirstDayAndJumpOutIfCanceled()
        {
            var absence = new Absence();
            var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, absence, new List<IActivityRestriction>()) { IsPreferenceDay = true };
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro, _scheduleDayPro2 });
            var date = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
            var period = new DateTimePeriod(date, date.AddDays(1));
            _target.DayScheduled += targetDayScheduled;

            using(_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _options)).Return(effectiveRestriction);
                Expect.Call(_scheduleDay.Period).Return(period);
                Expect.Call(() => _scheduleDay.CreateAndAddAbsence(new AbsenceLayer(absence, period))).IgnoreArguments();
                Expect.Call(() => _rollBackService.Modify(_scheduleDay));
            }

            using(_mocks.Playback())
            {
                _target.AddPreferredAbsence(matrixProList);    
            }

        }

		static void targetDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			e.Cancel = true;
		}
	}

	
}