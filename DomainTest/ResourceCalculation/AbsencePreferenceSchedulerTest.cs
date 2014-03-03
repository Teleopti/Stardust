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
		private IAbsencePreferenceFullDayLayerCreator _absencePreferenceFullDayLayerCreator;

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
			_absencePreferenceFullDayLayerCreator = _mocks.StrictMock<IAbsencePreferenceFullDayLayerCreator>();
			_target = new AbsencePreferenceScheduler(_effectiveRestrictionCreator,_rollBackService, _absencePreferenceFullDayLayerCreator);
		}

       [Test]
        public void ShouldDoNothingIfMatrixProIsEmpty()
        {
            var matrixProList = new List<IScheduleMatrixPro>();
            _target.AddPreferredAbsence(matrixProList, _options);
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
				_target.AddPreferredAbsence(matrixProList, _options);
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
				_target.AddPreferredAbsence(matrixProList, _options);    
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
				_target.AddPreferredAbsence(matrixProList, _options);
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
			var absenceLayer = new AbsenceLayer(absence, period);

            using(_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _options)).Return(effectiveRestriction);
				Expect.Call(_absencePreferenceFullDayLayerCreator.Create(_scheduleDay, absence)).Return(absenceLayer);
	            Expect.Call(() => _scheduleDay.CreateAndAddAbsence(absenceLayer));
                Expect.Call(() => _rollBackService.Modify(_scheduleDay));
            }

            using(_mocks.Playback())
            {
				_target.AddPreferredAbsence(matrixProList, _options);   
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldAddAbsenceOnFirstDayAndJumpOutIfCanceled()
        {
            var absence = new Absence();
            var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, absence, new List<IActivityRestriction>()) { IsPreferenceDay = true };
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro, _scheduleDayPro2 });
            var date = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
            var period = new DateTimePeriod(date, date.AddDays(1));
            _target.DayScheduled += targetDayScheduled;
			var absenceLayer = new AbsenceLayer(absence, period);


            using(_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _options)).Return(effectiveRestriction);
	            Expect.Call(_absencePreferenceFullDayLayerCreator.Create(_scheduleDay, absence)).Return(absenceLayer);
                Expect.Call(() => _scheduleDay.CreateAndAddAbsence(absenceLayer));
                Expect.Call(() => _rollBackService.Modify(_scheduleDay));
            }

            using(_mocks.Playback())
            {
				_target.AddPreferredAbsence(matrixProList, _options);    
            }

        }

		static void targetDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			e.Cancel = true;
		}
	}

	
}