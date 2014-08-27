using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class MinWeekWorkTimeRuleTest
    {
        private MockRepository _mocks;
        private IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
        private MinWeekWorkTimeRule _target;
        private IPermissionInformation _permissionInformation;
        private TimeZoneInfo _timeZone;
        private IContract _contract1;
        private IContract _contract2;
        private IContract _contract3;
        private IPersonContract _personContract1;
        private IPersonContract _personContract2;
        private IPersonPeriod _personPeriod1;
        private IPersonPeriod _personPeriod2;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _weeksFromScheduleDaysExtractor = _mocks.StrictMock<IWeeksFromScheduleDaysExtractor>();
            _target = new MinWeekWorkTimeRule(_weeksFromScheduleDaysExtractor);
            _permissionInformation = _mocks.StrictMock<IPermissionInformation>();
            _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            var maxTimePerWeek = new TimeSpan(40, 0, 0);
            var minTimePerWeek1 = new TimeSpan(40, 0, 0);
            var minTimePerWeek2 = new TimeSpan(32, 0, 0);
            var nightlyRest = new TimeSpan(8, 0, 0);
            var weeklyRest = new TimeSpan(50, 0, 0);
            _contract1 = new Contract("for test")
            {
                WorkTimeDirective = new WorkTimeDirective(minTimePerWeek1, maxTimePerWeek,nightlyRest,weeklyRest),
                EmploymentType = EmploymentType.FixedStaffNormalWorkTime
                
            };

            _contract2 = new Contract("for test")
            {
                WorkTimeDirective = new WorkTimeDirective(minTimePerWeek2, maxTimePerWeek, nightlyRest, weeklyRest),
                EmploymentType = EmploymentType.FixedStaffNormalWorkTime
            };

            _contract3 = new Contract("for test")
            {
                WorkTimeDirective = new WorkTimeDirective(minTimePerWeek2, maxTimePerWeek, nightlyRest, weeklyRest),
                EmploymentType = EmploymentType.HourlyStaff
            };

            _personContract1 = _mocks.StrictMock<IPersonContract>();
            _personContract2 = _mocks.StrictMock<IPersonContract>();
            _personPeriod1 = _mocks.StrictMock<IPersonPeriod>();
            _personPeriod2 = _mocks.StrictMock<IPersonPeriod>();
        }

        [Test]
        public void ShouldCreateRuleAndAccessSimpleProperties()
        {
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.IsMandatory);
            Assert.IsTrue(_target.HaltModify);
            _target.HaltModify = false;
            Assert.IsFalse(_target.HaltModify);
            Assert.AreEqual("", _target.ErrorMessage);
            Assert.IsFalse(_target.ShouldValidate);
        }

        [Test]
        public void ShouldReturnEmptyListWhenNotTooLittleWorkTime()
        {
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            var dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var lstOfDays = new List<IScheduleDay> { scheduleDay};

            var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
            var personWeek = new PersonWeek(person, weekPeriod);

            var personWeeks = new List<PersonWeek> { personWeek };
            var oldResponses = new List<IBusinessRuleResponse>();
            var day8Hours = _mocks.StrictMock<IScheduleDay>();
            var day0Hours = _mocks.StrictMock<IScheduleDay>();
            var projService8Hours = _mocks.StrictMock<IProjectionService>();
            var projService0Hours = _mocks.StrictMock<IProjectionService>();

            var visualLayers8Hours = _mocks.StrictMock<IVisualLayerCollection>();
            var visualLayers0Hours = _mocks.StrictMock<IVisualLayerCollection>();

            using (_mocks.Record())
            {
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays)).Return(personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

                Expect.Call(person.Period(new DateOnly(2010, 8, 23))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 24))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 25))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 26))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 27))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 28))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 29))).Return(_personPeriod1);

                Expect.Call(_personPeriod1.PersonContract).Return(_personContract1).Repeat.AtLeastOnce();
                Expect.Call(_personContract1.Contract).Return(_contract1).Repeat.AtLeastOnce();
	            Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new[]{day8Hours, day8Hours, day8Hours, day8Hours, day8Hours, day0Hours, day0Hours});

                Expect.Call(day8Hours.ProjectionService()).Return(projService8Hours).Repeat.Times(5);
                Expect.Call(day0Hours.ProjectionService()).Return(projService0Hours).Repeat.Times(2);

                Expect.Call(projService8Hours.CreateProjection()).Return(visualLayers8Hours).Repeat.Times(5);
                Expect.Call(projService0Hours.CreateProjection()).Return(visualLayers0Hours).Repeat.Times(2);

                Expect.Call(visualLayers8Hours.WorkTime()).Return(TimeSpan.FromHours(8)).Repeat.Times(5);
                Expect.Call(visualLayers0Hours.WorkTime()).Return(TimeSpan.FromHours(0)).Repeat.Times(2);

                Expect.Call(day8Hours.IsScheduled()).Return(true).Repeat.AtLeastOnce();
                Expect.Call(day0Hours.IsScheduled()).Return(true).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.ShouldValidate = true;
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(0, ret.Count());
            }
        }

        [Test]
        public void ShouldReturnFilledListWhenTooLittleWorkTime()
        {
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            var dic = new Dictionary<IPerson, IScheduleRange> { { person, range } };
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var lstOfDays = new List<IScheduleDay> { scheduleDay };

            var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
            var personWeek = new PersonWeek(person, weekPeriod);

            var personWeeks = new List<PersonWeek> { personWeek };
            var oldResponses = new List<IBusinessRuleResponse>();
            var day8Hours = _mocks.StrictMock<IScheduleDay>();
            var day0Hours = _mocks.StrictMock<IScheduleDay>();
            var projService8Hours = _mocks.StrictMock<IProjectionService>();
            var projService0Hours = _mocks.StrictMock<IProjectionService>();

            var visualLayers8Hours = _mocks.StrictMock<IVisualLayerCollection>();
            var visualLayers0Hours = _mocks.StrictMock<IVisualLayerCollection>();

            using (_mocks.Record())
            {
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays)).Return(personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

                Expect.Call(person.Period(new DateOnly(2010, 8, 23))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 24))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 25))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 26))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 27))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 28))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 29))).Return(_personPeriod1);

                Expect.Call(_personPeriod1.PersonContract).Return(_personContract1).Repeat.AtLeastOnce();
                Expect.Call(_personContract1.Contract).Return(_contract1).Repeat.AtLeastOnce();
                Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new[] { day8Hours, day8Hours, day8Hours, day8Hours, day0Hours, day0Hours, day0Hours });

                Expect.Call(day8Hours.ProjectionService()).Return(projService8Hours).Repeat.Times(4);
                Expect.Call(day0Hours.ProjectionService()).Return(projService0Hours).Repeat.Times(3);

                Expect.Call(projService8Hours.CreateProjection()).Return(visualLayers8Hours).Repeat.Times(4);
                Expect.Call(projService0Hours.CreateProjection()).Return(visualLayers0Hours).Repeat.Times(3);

                Expect.Call(visualLayers8Hours.WorkTime()).Return(TimeSpan.FromHours(8)).Repeat.Times(4);
                Expect.Call(visualLayers0Hours.WorkTime()).Return(TimeSpan.FromHours(0)).Repeat.Times(3);

                Expect.Call(day8Hours.IsScheduled()).Return(true).Repeat.AtLeastOnce();
                Expect.Call(day0Hours.IsScheduled()).Return(true).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.ShouldValidate = true;
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(7, ret.Count());
            }
        }

        [Test]
        public void ShouldUseMinWorkTimeFromMultiplePeriods()
        {
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            var dic = new Dictionary<IPerson, IScheduleRange> { { person, range } };
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var lstOfDays = new List<IScheduleDay> { scheduleDay };

            var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
            var personWeek = new PersonWeek(person, weekPeriod);

            var personWeeks = new List<PersonWeek> { personWeek };
            var oldResponses = new List<IBusinessRuleResponse>();
            var day8Hours = _mocks.StrictMock<IScheduleDay>();
            var day0Hours = _mocks.StrictMock<IScheduleDay>();
            var projService8Hours = _mocks.StrictMock<IProjectionService>();
            var projService0Hours = _mocks.StrictMock<IProjectionService>();

            var visualLayers8Hours = _mocks.StrictMock<IVisualLayerCollection>();
            var visualLayers0Hours = _mocks.StrictMock<IVisualLayerCollection>();

            using (_mocks.Record())
            {
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays)).Return(personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

                Expect.Call(person.Period(new DateOnly(2010, 8, 23))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 24))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 25))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 26))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 27))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 28))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 29))).Return(_personPeriod2);

                Expect.Call(_personPeriod1.PersonContract).Return(_personContract1).Repeat.AtLeastOnce();
                Expect.Call(_personPeriod2.PersonContract).Return(_personContract2).Repeat.AtLeastOnce();
                Expect.Call(_personContract1.Contract).Return(_contract1).Repeat.AtLeastOnce();
                Expect.Call(_personContract2.Contract).Return(_contract2).Repeat.AtLeastOnce();
                Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new[] { day8Hours, day8Hours, day8Hours, day8Hours, day0Hours, day0Hours, day0Hours });

                Expect.Call(day8Hours.ProjectionService()).Return(projService8Hours).Repeat.Times(4);
                Expect.Call(day0Hours.ProjectionService()).Return(projService0Hours).Repeat.Times(3);

                Expect.Call(projService8Hours.CreateProjection()).Return(visualLayers8Hours).Repeat.Times(4);
                Expect.Call(projService0Hours.CreateProjection()).Return(visualLayers0Hours).Repeat.Times(3);

                Expect.Call(visualLayers8Hours.WorkTime()).Return(TimeSpan.FromHours(8)).Repeat.Times(4);
                Expect.Call(visualLayers0Hours.WorkTime()).Return(TimeSpan.FromHours(0)).Repeat.Times(3);

                Expect.Call(day8Hours.IsScheduled()).Return(true).Repeat.AtLeastOnce();
                Expect.Call(day0Hours.IsScheduled()).Return(true).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.ShouldValidate = true;
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(0, ret.Count());
            }
        }

        [Test]
        public void ShouldReturnEmptyListWhenMissingSchedule()
        {
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            var dic = new Dictionary<IPerson, IScheduleRange> { { person, range } };
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var lstOfDays = new List<IScheduleDay> { scheduleDay };

            var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
            var personWeek = new PersonWeek(person, weekPeriod);

            var personWeeks = new List<PersonWeek> { personWeek };
            var oldResponses = new List<IBusinessRuleResponse>();
            var day8Hours = _mocks.StrictMock<IScheduleDay>();
            var day0Hours = _mocks.StrictMock<IScheduleDay>();
            var projService8Hours = _mocks.StrictMock<IProjectionService>();
            var projService0Hours = _mocks.StrictMock<IProjectionService>();

            var visualLayers8Hours = _mocks.StrictMock<IVisualLayerCollection>();
            var visualLayers0Hours = _mocks.StrictMock<IVisualLayerCollection>();

            using (_mocks.Record())
            {
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays)).Return(personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

                Expect.Call(person.Period(new DateOnly(2010, 8, 23))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 24))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 25))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 26))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 27))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 28))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 29))).Return(_personPeriod1);

                Expect.Call(_personPeriod1.PersonContract).Return(_personContract1).Repeat.AtLeastOnce();
                Expect.Call(_personContract1.Contract).Return(_contract1).Repeat.AtLeastOnce();
                Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new[] { day8Hours, day8Hours, day8Hours, day0Hours, day0Hours, day0Hours, day0Hours });

                Expect.Call(day8Hours.ProjectionService()).Return(projService8Hours).Repeat.Times(3);
                Expect.Call(day0Hours.ProjectionService()).Return(projService0Hours).Repeat.Times(4);

                Expect.Call(projService8Hours.CreateProjection()).Return(visualLayers8Hours).Repeat.Times(3);
                Expect.Call(projService0Hours.CreateProjection()).Return(visualLayers0Hours).Repeat.Times(4);

                Expect.Call(visualLayers8Hours.WorkTime()).Return(TimeSpan.FromHours(8)).Repeat.Times(3);
                Expect.Call(visualLayers0Hours.WorkTime()).Return(TimeSpan.FromHours(0)).Repeat.Times(4);

                Expect.Call(day8Hours.IsScheduled()).Return(true).Repeat.AtLeastOnce();
                Expect.Call(day0Hours.IsScheduled()).Return(false).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.ShouldValidate = true;
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(0, ret.Count());
            }
        }

        [Test]
        public void ShouldReturnEmptyListWhenNotFullTime()
        {
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            var dic = new Dictionary<IPerson, IScheduleRange> { { person, range } };
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var lstOfDays = new List<IScheduleDay> { scheduleDay };

            var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
            var personWeek = new PersonWeek(person, weekPeriod);

            var personWeeks = new List<PersonWeek> { personWeek };
            var oldResponses = new List<IBusinessRuleResponse>();

            using (_mocks.Record())
            {
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays)).Return(personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

                Expect.Call(person.Period(new DateOnly(2010, 8, 23))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 24))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 25))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 26))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 27))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 28))).Return(_personPeriod1);
                Expect.Call(person.Period(new DateOnly(2010, 8, 29))).Return(_personPeriod1);

                Expect.Call(_personPeriod1.PersonContract).Return(_personContract1).Repeat.AtLeastOnce();
                Expect.Call(_personContract1.Contract).Return(_contract3).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.ShouldValidate = true;
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(0, ret.Count());
            }
        }

        [Test]
        public void ShouldReturnEmptyListWhenSkippingValidation()
        {
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            var dic = new Dictionary<IPerson, IScheduleRange> { { person, range } };
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var lstOfDays = new List<IScheduleDay> { scheduleDay };

            var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
            var personWeek = new PersonWeek(person, weekPeriod);

            var personWeeks = new List<PersonWeek> { personWeek };
            var oldResponses = new List<IBusinessRuleResponse>();

            using (_mocks.Record())
            {
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays)).Return(personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();    
            }

            using (_mocks.Playback())
            {
                _target.ShouldValidate = false;
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(0, ret.Count());
            }
        }
    }
}
