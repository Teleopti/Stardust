using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class MinWeeklyRestRuleTest
    {
        private MockRepository _mocks;
        private IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
        private MinWeeklyRestRule _target;
        private IPermissionInformation _permissionInformation;
        private TimeZoneInfo _timeZone;
        private IContract _contract;
        private IPersonContract _personContract;
        private IPersonPeriod _personPeriod;
    	private IWorkTimeStartEndExtractor _workTimeStartEndExtractor;

    	[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _weeksFromScheduleDaysExtractor = _mocks.StrictMock<IWeeksFromScheduleDaysExtractor>();
			_workTimeStartEndExtractor = _mocks.StrictMock<IWorkTimeStartEndExtractor>();
            _target = new MinWeeklyRestRule(_weeksFromScheduleDaysExtractor,_workTimeStartEndExtractor);
            _permissionInformation = _mocks.StrictMock<IPermissionInformation>();
            _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
            var maxTimePerWeek = new TimeSpan(40, 0, 0);
            var nightlyRest = new TimeSpan(8, 0, 0);
            var weeklyRest = new TimeSpan(50, 0, 0);
            _contract = new Contract("for test")
                        	{
                        		WorkTimeDirective = new WorkTimeDirective(maxTimePerWeek,
                        		                                          nightlyRest,
                        		                                          weeklyRest)
                        	};
    		_personContract = _mocks.StrictMock<IPersonContract>();
            _personPeriod = _mocks.StrictMock<IPersonPeriod>();
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
            Assert.AreEqual("", _target.ErrorMessage);
        }

        [Test]
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
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays,true)).Return(
                    personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

                Expect.Call(person.Period(new DateOnly(2010, 8, 23))).Return(null);
				Expect.Call(person.Period(new DateOnly(2010, 8, 29))).Return(null);
                Expect.Call(person.Name).Return(new Name("nn", "mm")).Repeat.Times(7);
            }

            using (_mocks.Playback())
            {
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(7, ret.Count());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ValidateReturnEmptyListWhenNoAssignments()
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
            var day0Hours = _mocks.StrictMock<IScheduleDay>();

            using (_mocks.Record())
            {
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true)).Return(
                    personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

                Expect.Call(person.Period(new DateOnly(2010, 8, 23))).Return(_personPeriod);

                Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(1);
                Expect.Call(_personContract.Contract).Return(_contract);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 22))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 23))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 24))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 25))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 26))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 27))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 28))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 29))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 30))).Return(day0Hours);
                Expect.Call(day0Hours.PersonAssignment()).Return(null).Repeat.Times(9);

            }
            using (_mocks.Playback())
            {
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(0, ret.Count());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ValidateReturnEmptyListWhenFirstAssignmentsHasRestBefore()
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
            var day0Hours = _mocks.StrictMock<IScheduleDay>();
            var day1 = _mocks.StrictMock<IScheduleDay>();

            var firstlayerCollectionPeriod = new DateTimePeriod(new DateTime(2010, 8, 24, 12, 0, 0, DateTimeKind.Utc),
                                                          new DateTime(2010, 8, 24, 22, 0, 0, DateTimeKind.Utc));

            var personAss = _mocks.StrictMock<IPersonAssignment>();

            using (_mocks.Record())
            {
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true)).Return(
                    personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

                Expect.Call(person.Period(new DateOnly(2010, 8, 23))).Return(_personPeriod);

                Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(1);
                Expect.Call(_personContract.Contract).Return(_contract);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 22))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 23))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 24))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 25))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 26))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 27))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 28))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 29))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 30))).Return(day0Hours);
								Expect.Call(day0Hours.PersonAssignment()).Return(null).Repeat.Times(8);
								Expect.Call(day1.PersonAssignment()).Return(personAss).Repeat.Times(1);
				mockShift(personAss, firstlayerCollectionPeriod, WorkTimeOptions.Start);
			}
            using (_mocks.Playback())
            {
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(0, ret.Count());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ValidateReturnEmptyListWhenLastRestBetweenAssignments()
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
            var day0Hours = _mocks.StrictMock<IScheduleDay>();
            var day1 = _mocks.StrictMock<IScheduleDay>();
            var day2 = _mocks.StrictMock<IScheduleDay>();

            var firstlayerCollectionPeriod = new DateTimePeriod(new DateTime(2010, 8, 24, 0, 0, 0, DateTimeKind.Utc),
                                                          new DateTime(2010, 8, 24, 22, 0, 0, DateTimeKind.Utc));

            var personAss = _mocks.StrictMock<IPersonAssignment>();
            var personAss2 = _mocks.StrictMock<IPersonAssignment>();

            using (_mocks.Record())
            {
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true)).Return(
                    personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

                Expect.Call(person.Period(new DateOnly(2010, 8, 23))).Return(_personPeriod);

                Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(1);
                Expect.Call(_personContract.Contract).Return(_contract);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 22))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 23))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 24))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 25))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 26))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 27))).Return(day2);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 28))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 29))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 30))).Return(day0Hours);
                Expect.Call(day0Hours.PersonAssignment()).Return(null).Repeat.Times(7);
								Expect.Call(day1.PersonAssignment()).Return(personAss).Repeat.Times(1);
				mockShift(personAss, firstlayerCollectionPeriod, WorkTimeOptions.Both);
				Expect.Call(day2.PersonAssignment()).Return(personAss2).Repeat.Times(1);
				mockShift(personAss2, firstlayerCollectionPeriod, WorkTimeOptions.Both);
            }
            using (_mocks.Playback())
            {
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(0, ret.Count());
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ValidateReturnEmptyListWhenLastAssignmentsHasRestAfter()
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
            var day0Hours = _mocks.StrictMock<IScheduleDay>();
            var day1 = _mocks.StrictMock<IScheduleDay>();

            var firstlayerCollectionPeriod = new DateTimePeriod(new DateTime(2010, 8, 24, 0, 0, 0, DateTimeKind.Utc),
                                                          new DateTime(2010, 8, 24, 22, 0, 0, DateTimeKind.Utc));

            var personAss = _mocks.StrictMock<IPersonAssignment>();

            using (_mocks.Record())
            {
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true)).Return(
                    personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

                Expect.Call(person.Period(new DateOnly(2010, 8, 23))).Return(_personPeriod);

                Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(1);
                Expect.Call(_personContract.Contract).Return(_contract);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 22))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 23))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 24))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 25))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 26))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 27))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 28))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 29))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 30))).Return(day0Hours);
                Expect.Call(day0Hours.PersonAssignment()).Return(null).Repeat.Times(8);
								Expect.Call(day1.PersonAssignment()).Return(personAss).Repeat.Times(1);
				mockShift(personAss, firstlayerCollectionPeriod, WorkTimeOptions.Both);
            }
            using (_mocks.Playback())
            {
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(0, ret.Count());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ValidateReturnListOfErrorsWhenNotEnoughWeeklyRest()
        {
            var maxTimePerWeek = new TimeSpan(40, 0, 0);
            var nightlyRest = new TimeSpan(8, 0, 0);
            // four days weekly rest
            var weeklyRest = new TimeSpan(4,0, 0, 0);
            _contract = new Contract("for test")
                        	{
                        		WorkTimeDirective = new WorkTimeDirective(maxTimePerWeek,
                        		                                          nightlyRest,
                        		                                          weeklyRest)
                        	};
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
            var day0Hours = _mocks.StrictMock<IScheduleDay>();
            var day1 = _mocks.StrictMock<IScheduleDay>();

            var firstlayerCollectionPeriod = new DateTimePeriod(new DateTime(2010, 8, 24, 0, 0, 0, DateTimeKind.Utc),
                                                          new DateTime(2010, 8, 24, 22, 0, 0, DateTimeKind.Utc));
            var firstlayerCollectionPeriod2 = new DateTimePeriod(new DateTime(2010, 8, 28, 0, 0, 0, DateTimeKind.Utc),
                                                          new DateTime(2010, 8, 28, 22, 0, 0, DateTimeKind.Utc));

			var personAss = _mocks.StrictMock<IPersonAssignment>();
            var personAss2 = _mocks.StrictMock<IPersonAssignment>();

            using (_mocks.Record())
            {
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true)).Return(
                    personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

                Expect.Call(person.Period(new DateOnly(2010, 8, 23))).Return(_personPeriod);

                Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(1);
                Expect.Call(_personContract.Contract).Return(_contract);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 22))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 23))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 24))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 25))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 26))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 27))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 28))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 29))).Return(day0Hours);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 30))).Return(day0Hours);
                Expect.Call(day0Hours.PersonAssignment()).Return(null).Repeat.Times(7);
								Expect.Call(day1.PersonAssignment()).Return(personAss);
								Expect.Call(day1.PersonAssignment()).Return(personAss2);
				mockShift(personAss, firstlayerCollectionPeriod, WorkTimeOptions.Both);
				mockShift(personAss2, firstlayerCollectionPeriod2, WorkTimeOptions.Both);

            }
            using (_mocks.Playback())
            {
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(7, ret.Count());
            }
        }

		[Test]
		public void ShouldNotReturnListIfSundayAndNextMondayIsNotScheduled()
		{
			var maxTimePerWeek = new TimeSpan(40, 0, 0);
			var nightlyRest = new TimeSpan(8, 0, 0);
			// four days weekly rest
			var weeklyRest = TimeSpan.FromHours(36);
			_contract = new Contract("for test")
			            	{
			            		WorkTimeDirective = new WorkTimeDirective(maxTimePerWeek,
			            		                                          nightlyRest,
			            		                                          weeklyRest)
			            	};
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
			var day0Hours = _mocks.StrictMock<IScheduleDay>();
			var day1 = _mocks.StrictMock<IScheduleDay>();

			var firstlayerCollectionPeriod = new DateTimePeriod(new DateTime(2010, 8, 23, 8, 0, 0, DateTimeKind.Utc),
														  new DateTime(2010, 8, 23, 22, 0, 0, DateTimeKind.Utc));

			var personAss = _mocks.StrictMock<IPersonAssignment>();

			using (_mocks.Record())
			{
				Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true)).Return(
					personWeeks);
				Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
				Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

				Expect.Call(person.Period(new DateOnly(2010, 8, 23))).Return(_personPeriod);

				Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(1);
				Expect.Call(_personContract.Contract).Return(_contract);
				Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 22))).Return(day0Hours);
				Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 23))).Return(day1);
				Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 24))).Return(day1);
				Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 25))).Return(day1);
				Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 26))).Return(day1);
				Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 27))).Return(day1);
				Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 28))).Return(day1);
				Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 29))).Return(day0Hours);
				Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 30))).Return(day0Hours);
				Expect.Call(day0Hours.PersonAssignment()).Return(null).Repeat.Times(3);
				Expect.Call(day1.PersonAssignment()).Return(personAss).Repeat.Times(6);

				mockShift(personAss, firstlayerCollectionPeriod, WorkTimeOptions.Both);
				mockShift(personAss, firstlayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(1)), WorkTimeOptions.Both);
				mockShift(personAss, firstlayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(2)), WorkTimeOptions.Both);
				mockShift(personAss, firstlayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(3)), WorkTimeOptions.Both);
				mockShift(personAss, firstlayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(4)), WorkTimeOptions.Both);
				mockShift(personAss, firstlayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(5)), WorkTimeOptions.Both);
			}
			using (_mocks.Playback())
			{
				var ret = _target.Validate(dic, lstOfDays);
				Assert.AreEqual(0, ret.Count());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "RestOn"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldNotUseSameWeeklyRestOnTwoWeeks()
        {
            var maxTimePerWeek = new TimeSpan(40, 0, 0);
            var nightlyRest = new TimeSpan(8, 0, 0);
            // four days weekly rest
            var weeklyRest = TimeSpan.FromHours(36);
            _contract = new Contract("for test")
                        	{
                        		WorkTimeDirective = new WorkTimeDirective(maxTimePerWeek,
                        		                                          nightlyRest,
                        		                                          weeklyRest)
                        	};
        	//var person = _mocks.StrictMock<IPerson>();
            var person = new Person();
            person.PermissionInformation.SetDefaultTimeZone(_timeZone);
            person.AddPersonPeriod(new PersonPeriod(new DateOnly(2010, 1, 1), _personContract, new Team()));
            var range = _mocks.StrictMock<IScheduleRange>();
            var dic = new Dictionary<IPerson, IScheduleRange> { { person, range } };
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

            var lstOfDays = new List<IScheduleDay> { scheduleDay, scheduleDay2 };

            var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
            var weekPeriod2 = new DateOnlyPeriod(2010, 8, 30, 2010, 9, 5);
            var personWeek = new PersonWeek(person, weekPeriod);
            var personWeek2 = new PersonWeek(person, weekPeriod2);

            var personWeeks = new List<PersonWeek> { personWeek, personWeek2 };
            var oldResponses = new List<IBusinessRuleResponse>();
            
            var day0Hours = _mocks.StrictMock<IScheduleDay>();
            var day1 = _mocks.StrictMock<IScheduleDay>();

            var scheduledDayLayerCollectionPeriod = new DateTimePeriod(new DateTime(2010, 8, 22, 6, 0, 0, DateTimeKind.Utc),
                                                          new DateTime(2010, 8, 22, 18, 0, 0, DateTimeKind.Utc));
            

			var personAss = _mocks.StrictMock<IPersonAssignment>();
           
            
            using (_mocks.Record())
            {
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true)).Return(
                    personWeeks);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses).Repeat.Any();
 
                Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Any();
                Expect.Call(_personContract.Contract).Return(_contract).Repeat.Twice();
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 22))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 23))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 24))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 25))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 26))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 27))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 28))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 29))).Return(day0Hours).Repeat.Twice();
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 30))).Return(day1).Repeat.Twice();
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 8, 31))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 9, 1))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 9, 2))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 9, 3))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 9, 4))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 9, 5))).Return(day1);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 9, 6))).Return(day1);

                Expect.Call(day0Hours.PersonAssignment()).Return(null).Repeat.Twice();
								Expect.Call(day1.PersonAssignment()).Return(personAss).Repeat.AtLeastOnce();

                mockShift(personAss, scheduledDayLayerCollectionPeriod, WorkTimeOptions.Both);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(1)), WorkTimeOptions.Both);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(2)), WorkTimeOptions.Both);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(3)), WorkTimeOptions.Both);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(4)), WorkTimeOptions.Both);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(5)), WorkTimeOptions.Both);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(6)), WorkTimeOptions.Both);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(8)), WorkTimeOptions.Start);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(8)), WorkTimeOptions.Both);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(9)), WorkTimeOptions.Both);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(10)), WorkTimeOptions.Both);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(11)), WorkTimeOptions.Both);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(12)), WorkTimeOptions.Both);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(13)), WorkTimeOptions.Both);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(14)), WorkTimeOptions.Both);
                mockShift(personAss, scheduledDayLayerCollectionPeriod.MovePeriod(TimeSpan.FromDays(15)), WorkTimeOptions.Both);

            }
            using (_mocks.Playback())
            {
                var ret = _target.Validate(dic, lstOfDays);
                Assert.AreEqual(7, ret.Count());
            }
        }
		
		private enum WorkTimeOptions
		{
			Start,
			End,
			Both
		}
		private void mockShift(IPersonAssignment assignment, DateTimePeriod period, WorkTimeOptions workTimeOptions)
		{
			var projectionService = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
			Expect.Call(assignment.ProjectionService()).Return(projectionService);
			Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
			if (workTimeOptions == WorkTimeOptions.Start || workTimeOptions == WorkTimeOptions.Both)
				Expect.Call(_workTimeStartEndExtractor.WorkTimeStart(visualLayerCollection)).Return(period.StartDateTime);
			if (workTimeOptions == WorkTimeOptions.End || workTimeOptions == WorkTimeOptions.Both)
				Expect.Call(_workTimeStartEndExtractor.WorkTimeEnd(visualLayerCollection)).Return(period.EndDateTime);

		}
    }
        
    }

