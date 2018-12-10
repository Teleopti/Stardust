using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	public class NewMaxWeekWorkTimeRuleTest
    {
        private MockRepository _mocks;
        private IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
        private NewMaxWeekWorkTimeRule _target;
        private IPermissionInformation _permissionInformation;
        private TimeZoneInfo _timeZone;
        private IContract _contract;
        private IPersonContract _personContract;
        private IPersonPeriod _personPeriod;
        private IPersonPeriod _personPeriod2;
        private IPersonContract _personContract2;
        private IContract _contract2;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _weeksFromScheduleDaysExtractor = _mocks.StrictMock<IWeeksFromScheduleDaysExtractor>();
            _target = new NewMaxWeekWorkTimeRule(_weeksFromScheduleDaysExtractor);
            _permissionInformation = _mocks.StrictMock<IPermissionInformation>();
            _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            var maxTimePerWeek = new TimeSpan(40, 0, 0);
            var minTimePerWeek = new TimeSpan(0, 0, 0);
            var nightlyRest = new TimeSpan(8, 0, 0);
            var weeklyRest = new TimeSpan(50, 0, 0);
            _contract = new Contract("for test");
			_contract.WorkTimeDirective = new WorkTimeDirective(minTimePerWeek, maxTimePerWeek,
                                                               nightlyRest,
                                                               weeklyRest);
            _personContract = _mocks.StrictMock<IPersonContract>();
            _personPeriod = _mocks.StrictMock<IPersonPeriod>();
            _personPeriod2 = _mocks.StrictMock<IPersonPeriod>();
            _personContract2 = _mocks.StrictMock<IPersonContract>();

            _contract2 = new Contract("for test")
            {
                WorkTimeDirective = new WorkTimeDirective(minTimePerWeek, maxTimePerWeek.Add(TimeSpan.FromHours(8)), nightlyRest, weeklyRest),
                EmploymentType = EmploymentType.FixedStaffNormalWorkTime
            };
        }

        [Test]
        public void CanCreateRuleAndAccessSimpleProperties()
        {
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.IsMandatory);
            Assert.IsTrue(_target.HaltModify);
            // ska man kunna ändra det??
            _target.HaltModify = false;
            Assert.IsFalse(_target.HaltModify);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ValidateReturnEmptyListWhenNotTooMuchWorkTime()
        {
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            var dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

            var lstOfDays = new List<IScheduleDay> { scheduleDay, scheduleDay2 };

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
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays)).Return(
                    personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

				Expect.Call(person.PersonPeriods(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new List<IPersonPeriod> { _personPeriod });

				Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.AtLeastOnce();
                Expect.Call(_personContract.Contract).Return(_contract).Repeat.AtLeastOnce();
	            Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29)))
	                  .Return(new[]
		                  {day8Hours, day8Hours, day8Hours, day8Hours, day8Hours, day0Hours, day0Hours});

                Expect.Call(day8Hours.ProjectionService()).Return(projService8Hours).Repeat.Times(5);
                Expect.Call(day0Hours.ProjectionService()).Return(projService0Hours).Repeat.Times(2);

                Expect.Call(projService8Hours.CreateProjection()).Return(visualLayers8Hours).Repeat.Times(5);
                Expect.Call(projService0Hours.CreateProjection()).Return(visualLayers0Hours).Repeat.Times(2);

                Expect.Call(visualLayers8Hours.WorkTime()).Return(TimeSpan.FromHours(8)).Repeat.Times(5);
                Expect.Call(visualLayers0Hours.WorkTime()).Return(TimeSpan.FromHours(0)).Repeat.Times(2);


            }

            using (_mocks.Playback())
            {
                var ret =_target.Validate(dic, lstOfDays);
                Assert.AreEqual(0,ret.Count());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ValidateReturnListWithSevenWhenTooMuchWorkTime()
        {
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            var dic = new Dictionary<IPerson, IScheduleRange> { { person, range } };
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

            var lstOfDays = new List<IScheduleDay> { scheduleDay, scheduleDay2 };

            var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
            var personWeek = new PersonWeek(person, weekPeriod);

            var personWeeks = new List<PersonWeek> { personWeek };
            var oldResponses = new List<IBusinessRuleResponse>();
            var day8Hours = _mocks.StrictMock<IScheduleDay>();
            var day1Hour = _mocks.StrictMock<IScheduleDay>();
            var projService8Hours = _mocks.StrictMock<IProjectionService>();
            var projService1Hour = _mocks.StrictMock<IProjectionService>();

            var visualLayers8Hours = _mocks.StrictMock<IVisualLayerCollection>();
            var visualLayers1Hour = _mocks.StrictMock<IVisualLayerCollection>();

            using (_mocks.Record())
            {
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays)).Return(
                    personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

				Expect.Call(person.PersonPeriods(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new List<IPersonPeriod> { _personPeriod });

				Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.AtLeastOnce();
                Expect.Call(_personContract.Contract).Return(_contract).Repeat.AtLeastOnce();
                Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(2010, 8, 23,2010,8,29))).Return(new[]{ day8Hours,day8Hours,day8Hours,day8Hours,day8Hours,day1Hour,day1Hour});

                Expect.Call(day8Hours.ProjectionService()).Return(projService8Hours).Repeat.Times(5);
                Expect.Call(day1Hour.ProjectionService()).Return(projService1Hour).Repeat.Times(2);

                Expect.Call(projService8Hours.CreateProjection()).Return(visualLayers8Hours).Repeat.Times(5);
                Expect.Call(projService1Hour.CreateProjection()).Return(visualLayers1Hour).Repeat.Times(2);

                Expect.Call(visualLayers8Hours.WorkTime()).Return(TimeSpan.FromHours(8)).Repeat.Times(5);
                Expect.Call(visualLayers1Hour.WorkTime()).Return(TimeSpan.FromHours(1)).Repeat.Times(2);
            }

            using (_mocks.Playback())
            {
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(7, ret.Count());
            }
        }

        [Test, SetUICulture("sv-SE")]
		public void ValidateReturnListWithSevenWhenNoPersonPeriod()
        {
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            var dic = new Dictionary<IPerson, IScheduleRange> { { person, range } };
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

            var lstOfDays = new List<IScheduleDay> { scheduleDay, scheduleDay2 };

            var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
            var personWeek = new PersonWeek(person, weekPeriod);

            var personWeeks = new List<PersonWeek> { personWeek };
            var oldResponses = new List<IBusinessRuleResponse>();
            
            using (_mocks.Record())
            {
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays)).Return(
                    personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

				Expect.Call(person.PersonPeriods(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new List<IPersonPeriod>());
				Expect.Call(person.Name).Return(new Name("nn","mm")).Repeat.Times(7);
            }

            using (_mocks.Playback())
            {
                var ret = _target.Validate(dic, lstOfDays).ToArray();
				foreach (var response in ret)
				{
					Assert.IsTrue(response.FriendlyName.StartsWith("Veckan har för"));
					Assert.IsTrue(response.Message.StartsWith("Person nn mm har"));
				}
				Assert.AreEqual(7, ret.Length);
            }
        }

        [Test]
        public void ShouldUseMaxWorkTimeFromMultiplePeriods()
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

				Expect.Call(person.PersonPeriods(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new List<IPersonPeriod> { _personPeriod,_personPeriod2 });

				Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.AtLeastOnce();
                Expect.Call(_personPeriod2.PersonContract).Return(_personContract2).Repeat.AtLeastOnce();
                Expect.Call(_personContract.Contract).Return(_contract).Repeat.AtLeastOnce();
                Expect.Call(_personContract2.Contract).Return(_contract2).Repeat.AtLeastOnce();
                Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new[] { day8Hours, day8Hours, day8Hours, day8Hours, day8Hours, day8Hours, day0Hours });

                Expect.Call(day8Hours.ProjectionService()).Return(projService8Hours).Repeat.Times(6);
                Expect.Call(day0Hours.ProjectionService()).Return(projService0Hours).Repeat.Times(1);

                Expect.Call(projService8Hours.CreateProjection()).Return(visualLayers8Hours).Repeat.Times(6);
                Expect.Call(projService0Hours.CreateProjection()).Return(visualLayers0Hours).Repeat.Times(1);

                Expect.Call(visualLayers8Hours.WorkTime()).Return(TimeSpan.FromHours(8)).Repeat.Times(6);
                Expect.Call(visualLayers0Hours.WorkTime()).Return(TimeSpan.FromHours(0)).Repeat.Times(1);
            }

            using (_mocks.Playback())
            {
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(0, ret.Count());
            }
        }
    }

    
}
