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

        [SetUp ]
        public void Setup()
        {
            _mock = new MockRepository();
            _effectiveRestriction = _mock.StrictMock<IEffectiveRestriction>();
            _shiftProjectionCache = _mock.StrictMock<IShiftProjectionCache>();
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
        public void ShouldExecute()
        {
            var today = DateOnly.Today;
            IList<DateOnly  > selectedDays = new List<DateOnly>{today};
            IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_scheduleMatrixPro };
            var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(today, TimeZoneInfo.Local);

            var person = new Person();
            var personList  = new ReadOnlyCollection<IPerson>(new List<IPerson>{person});
            var dateTime = new DateTimePeriod();
            using (_mock.Record())
            {
                Expect.Call(_groupPerson.GroupMembers).Return(personList);
                Expect.Call(_scheduleMatrixPro.Person).Return(person);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(
                    _scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart() ).Return(_scheduleDay);
                Expect.Call(() => _scheduleDay.AddMainShift(_mainShift)).IgnoreArguments();
                Expect.Call(_shiftProjectionCache.TheMainShift).Return(_mainShift);
                Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(dateTime);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod);
                Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(dateOnlyAsDateTimePeriod.DateOnly ,
                                                                        dateTime,
                                                                        new List<IScheduleDay> {_scheduleDay})).
                    IgnoreArguments().Return(true);
            }

            using(_mock.Playback()   )
            {
                _target.Execute(selectedDays,matrixList,_groupPerson ,_effectiveRestriction ,_shiftProjectionCache)  ;
            }
        }
    }

   
}
