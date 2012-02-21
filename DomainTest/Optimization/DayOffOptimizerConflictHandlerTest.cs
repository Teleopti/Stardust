﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
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
        private ISchedulingOptions _schedulingOptions;
        private IEffectiveRestriction _effectiveRestriction;
        private ISchedulePartModifyAndRollbackService _rollbackService;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _scheduleService = _mock.StrictMock<IScheduleService>();
            _effectiveRestrictionCreator = _mock.StrictMock<IEffectiveRestrictionCreator>();
            _schedulingOptions = _mock.StrictMock<ISchedulingOptions>();
            _rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            _target = new DayOffOptimizerConflictHandler(_scheduleMatrixPro, _scheduleService, _effectiveRestrictionCreator, _rollbackService);
            _dateOnly = new DateOnly(2011, 1, 10);
            _scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _effectiveRestriction = _mock.StrictMock<IEffectiveRestriction>();
        }

        [Test]
        public void ShouldReturnTrueWhenDayBeforeRescheduled()
        {
            using (_mock.Record())
            {
                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(-1))).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(() => _scheduleDay.DeleteMainShift(_scheduleDay));
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).Return(_effectiveRestriction);
                Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, true, _effectiveRestriction)).Return(true);
                //Expect.Call(_scheduleMatrixPro.UserLockedDates).Return(new List<DateOnly>());
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_scheduleDayPro}));
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
                Expect.Call(() => _scheduleDay.DeleteMainShift(_scheduleDay));
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).Return(_effectiveRestriction);
                Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, true, _effectiveRestriction)).Return(false);
                //Expect.Call(_scheduleMatrixPro.UserLockedDates).Return(new List<DateOnly>());
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_scheduleDayPro}));
                Expect.Call(() => _rollbackService.Rollback());

                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(_scheduleDayPro);
                //Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(() => _scheduleDay.DeleteMainShift(_scheduleDay));
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).Return(_effectiveRestriction);
                Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, true, _effectiveRestriction)).Return(true);
                //Expect.Call(_scheduleMatrixPro.UserLockedDates).Return(new List<DateOnly>());
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
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
                Expect.Call(() => _scheduleDay.DeleteMainShift(_scheduleDay));
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).Return(_effectiveRestriction);
                Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, true, _effectiveRestriction)).Return(false);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
                Expect.Call(() => _rollbackService.Rollback());

                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(_scheduleDayPro);
                Expect.Call(() => _scheduleDay.DeleteMainShift(_scheduleDay));
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).Return(_effectiveRestriction);
                Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay, true, _effectiveRestriction)).Return(false);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
                Expect.Call(() => _rollbackService.Rollback());
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
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> ())).Repeat.Twice();
            }

            using (_mock.Playback())
            {
                var result = _target.HandleConflict(_schedulingOptions, _dateOnly);
                Assert.IsFalse(result);
            }          
        }
    }
}
