using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
    [TestFixture]
    public class TeamSchedulingTest
    {

        private MockRepository _mock;
        private ITeamScheduling _target;
        private IEffectiveRestriction _effectiveRestriction;
        private IShiftProjectionCache _shiftProjectionCache;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IGroupPerson _groupPerson;
        private IResourceCalculateDelayer _resourceCalculateDelayer;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _scheduleDay;
        private IMainShift _mainShift;
        private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private IVirtualSchedulePeriod _virtualSchedulePeriod;

        [SetUp ]
        public void Setup()
        {
            _mock = new MockRepository();
            _effectiveRestriction = _mock.StrictMock<IEffectiveRestriction>();
            _shiftProjectionCache = _mock.StrictMock<IShiftProjectionCache>();
            _virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _groupPerson = _mock.StrictMock<IGroupPerson>();
            _resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
            _schedulePartModifyAndRollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            _target = new TeamScheduling(_resourceCalculateDelayer,_schedulePartModifyAndRollbackService);
            _scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _mainShift = _mock.StrictMock<IMainShift>();
        }

        [Test]
        public void ShouldExecuteWithMainShift()
        {
            DateOnly startDateOfBlock = DateOnly.Today;
            IList<DateOnly> selectedDays = new List<DateOnly> { startDateOfBlock };
            IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_scheduleMatrixPro };
            var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(startDateOfBlock, TimeZoneInfo.Local);
            var person = new Person();
            var personList  = new ReadOnlyCollection<IPerson>(new List<IPerson>{person});
            var dateTime = new DateTimePeriod();
            var dateOnlyPeriod = new DateOnlyPeriod(startDateOfBlock,startDateOfBlock.AddDays(2));
            
            using (_mock.Record())
            {
                ExpectCalls(dateOnlyAsDateTimePeriod, dateOnlyPeriod, dateTime, personList, person);

                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_shiftProjectionCache.TheMainShift).Return(_mainShift);
                Expect.Call(() => _scheduleDay.AddMainShift(_mainShift)).IgnoreArguments();
                Expect.Call(_mainShift.EntityClone()).Return(_mainShift);
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay)).IgnoreArguments();
                Expect.Call(() => _scheduleDay.Merge(_scheduleDay, false));
            }

            using(_mock.Playback()   )
            {

                _target.Execute( startDateOfBlock, selectedDays, matrixList, _groupPerson, _effectiveRestriction, _shiftProjectionCache, new List<DateOnly> { startDateOfBlock },new List<IPerson>());
            }
        }

        private void ExpectCalls(DateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod, DateOnlyPeriod dateOnlyPeriod,
                                 DateTimePeriod dateTime, ReadOnlyCollection<IPerson> personList, Person person)
        {
            Expect.Call(_groupPerson.GroupMembers).Return(personList);
            Expect.Call(_scheduleMatrixPro.Person).Return(person);
            Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(
                _scheduleDayPro).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
            Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(dateTime);
            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod);
            Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(dateOnlyAsDateTimePeriod.DateOnly,
                                                                    dateTime,
                                                                    new List<IScheduleDay> {_scheduleDay})).
                IgnoreArguments().Return(true);
            Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
            Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod);
            Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(
                new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_scheduleDayPro}));

        }

        [Test]
        public void ShouldNotContinueWithDayOff()
        {
            DateOnly startDateOfBlock = DateOnly.Today;
            IList<DateOnly> selectedDays = new List<DateOnly> { startDateOfBlock };
            IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(startDateOfBlock, TimeZoneInfo.Local);

            var person = new Person();
            var personList = new ReadOnlyCollection<IPerson>(new List<IPerson> { person });
            var dateTime = new DateTimePeriod();
            var dateOnlyPeriod = new DateOnlyPeriod(startDateOfBlock, startDateOfBlock.AddDays(2));
            using (_mock.Record())
            {
                ExpectCalls(dateOnlyAsDateTimePeriod, dateOnlyPeriod, dateTime, personList, person);
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff );
                
            }
            using (_mock.Playback())
            {
                _target.Execute(startDateOfBlock, selectedDays, matrixList, _groupPerson, _effectiveRestriction, _shiftProjectionCache, new List<DateOnly> { startDateOfBlock }, new List<IPerson>());
            }
        }

        [Test]
        public void ShouldNotContinueWithContractDayOff()
        {
            DateOnly startDateOfBlock = DateOnly.Today;
            IList<DateOnly> selectedDays = new List<DateOnly> { startDateOfBlock };
            IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(startDateOfBlock, TimeZoneInfo.Local);

            var person = new Person();
            var personList = new ReadOnlyCollection<IPerson>(new List<IPerson> { person });
            var dateTime = new DateTimePeriod();
            var dateOnlyPeriod = new DateOnlyPeriod(startDateOfBlock, startDateOfBlock.AddDays(2));
            using (_mock.Record())
            {
                ExpectCalls(dateOnlyAsDateTimePeriod, dateOnlyPeriod, dateTime, personList, person);
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.ContractDayOff );

            }
            using (_mock.Playback())
            {
                _target.Execute(startDateOfBlock, selectedDays, matrixList, _groupPerson, _effectiveRestriction, _shiftProjectionCache, new List<DateOnly> { startDateOfBlock }, new List<IPerson>());
            }
        }

        [Test]
        public void ShouldNotContinueWithFullDayAbsence()
        {
            DateOnly startDateOfBlock = DateOnly.Today;
            IList<DateOnly> selectedDays = new List<DateOnly> { startDateOfBlock };
            IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(startDateOfBlock, TimeZoneInfo.Local);

            var person = new Person();
            var personList = new ReadOnlyCollection<IPerson>(new List<IPerson> { person });
            var dateTime = new DateTimePeriod();
            var dateOnlyPeriod = new DateOnlyPeriod(startDateOfBlock, startDateOfBlock.AddDays(2));
            using (_mock.Record())
            {
                ExpectCalls(dateOnlyAsDateTimePeriod, dateOnlyPeriod, dateTime, personList, person);
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.FullDayAbsence );

            }
            using (_mock.Playback())
            {
                _target.Execute(startDateOfBlock, selectedDays, matrixList, _groupPerson, _effectiveRestriction, _shiftProjectionCache, new List<DateOnly> { startDateOfBlock }, new List<IPerson>());
            }
        }
    }

   
}
