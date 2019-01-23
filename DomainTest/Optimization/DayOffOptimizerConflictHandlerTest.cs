using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;


namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
    public class DayOffOptimizerConflictHandlerTest
    {
        private DayOffOptimizerConflictHandler _target;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private DateOnly _dateOnly;
        private MockRepository _mock;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _scheduleDay;
        private IScheduleService _scheduleService;
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private SchedulingOptions _schedulingOptions;
        private IEffectiveRestriction _effectiveRestriction;
        private ISchedulePartModifyAndRollbackService _rollbackService;
    	private IResourceCalculateDelayer _resourceCalculateDelayer;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _scheduleService = _mock.StrictMock<IScheduleService>();
            _effectiveRestrictionCreator = _mock.StrictMock<IEffectiveRestrictionCreator>();
            _schedulingOptions = new SchedulingOptions();
            _rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
        	_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
			_target = new DayOffOptimizerConflictHandler(_scheduleMatrixPro, _scheduleService, _effectiveRestrictionCreator, _rollbackService, _resourceCalculateDelayer);
            _dateOnly = new DateOnly(2011, 1, 10);
            _scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _effectiveRestriction = _mock.StrictMock<IEffectiveRestriction>();
	        _dateOnlyAsDateTimePeriod = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
        }

        [Test]
        public void ShouldReturnTrueWhenDayBeforeRescheduled()
        {
            using (_mock.Record())
            {
                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(-1))).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(() => _scheduleDay.DeleteMainShift());
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
				_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null, false);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).Return(_effectiveRestriction);
				Expect.Call(_scheduleDay.ReFetch()).Return(_scheduleDay);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions,_effectiveRestriction, _resourceCalculateDelayer, _rollbackService)).Return(true);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new HashSet<IScheduleDayPro> { _scheduleDayPro});
            }

            using (_mock.Playback())
            { 
                var result = _target.HandleConflict(_schedulingOptions, _dateOnly);
                Assert.IsTrue(result);
            }   
        }

        [Test]
        public void ShouldReturnTrueWhenDayAfterRescheduled()
        {
            using (_mock.Record())
            {
                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(-1))).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(() => _scheduleDay.DeleteMainShift());
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).Return(_effectiveRestriction);
				Expect.Call(_scheduleDay.ReFetch()).Return(_scheduleDay);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, _rollbackService)).Return(false);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new HashSet<IScheduleDayPro> { _scheduleDayPro});
                Expect.Call(() => _rollbackService.Rollback());

                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(_scheduleDayPro);
                Expect.Call(() => _scheduleDay.DeleteMainShift());
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
                _resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null, false);
	            _resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null, false);
				LastCall.Repeat.Times(2);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).Return(_effectiveRestriction);
				Expect.Call(_scheduleDay.ReFetch()).Return(_scheduleDay);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, _rollbackService)).Return(true);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new HashSet<IScheduleDayPro> { _scheduleDayPro });
            }

            using (_mock.Playback())
            {
                var result = _target.HandleConflict(_schedulingOptions, _dateOnly);
                Assert.IsTrue(result);
            }        
        }

        [Test]
        public void ShouldReturnFalseOnUnhandledConflict()
        {
            using (_mock.Record())
            {

                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(-1))).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(() => _scheduleDay.DeleteMainShift());
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
	            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
	            Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).Return(_effectiveRestriction);
	            Expect.Call(_scheduleDay.ReFetch()).Return(_scheduleDay);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, _rollbackService)).Return(false);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new HashSet<IScheduleDayPro> { _scheduleDayPro });
                Expect.Call(() => _rollbackService.Rollback());

                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(_scheduleDayPro);
                Expect.Call(() => _scheduleDay.DeleteMainShift());
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
	            _resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null, false);
				LastCall.Repeat.Times(2);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).Return(_effectiveRestriction);
				Expect.Call(_scheduleDay.ReFetch()).Return(_scheduleDay);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, _rollbackService)).Return(false);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new HashSet<IScheduleDayPro> { _scheduleDayPro });
                Expect.Call(() => _rollbackService.Rollback());
	            _resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null, false);
				LastCall.Repeat.Times(2);
            }

            using (_mock.Playback())
            {
                var result = _target.HandleConflict(_schedulingOptions, _dateOnly);
                Assert.IsFalse(result);
            }       
        }

        [Test]
        public void ShouldNotTryToRescheduleLockedDates()
        {
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(-1))).Return(_scheduleDayPro);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(_scheduleDayPro);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new HashSet<IScheduleDayPro>()).Repeat.Twice();
            }

            using (_mock.Playback())
            {
                var result = _target.HandleConflict(_schedulingOptions, _dateOnly);
                Assert.IsFalse(result);
            }          
        }
    }
}
