using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ShiftCategoryLimitationCheckerTest
    {
        private ShiftCategoryLimitationChecker _target;
        private MockRepository _mocks;
        private IPersonAssignment _personAssignment;
        private DateTimePeriod _dateTimePeriod;
        private IShiftCategoryLimitation _shiftCategoryLimitation;
        private IPerson _person;
        IScheduleDictionary _dic;
        IScheduleRange _range;
        IScheduleDay _part;
        IScheduleDay _partWithoutShift;
        private ISchedulePeriod _schedulePeriod;
        private IPersonPeriod _personPeriod;
        private IPersonContract _personContract;
        private ISchedulingResultStateHolder _stateHolder;
        private SchedulePeriod _schedulePeriod2;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_dic = _mocks.DynamicMock<IScheduleDictionary>();
            _target = new ShiftCategoryLimitationChecker(_stateHolder);
            _dateTimePeriod = new DateTimePeriod(2010,1,1,2010,1,2);
            _person = PersonFactory.CreatePerson();
            _person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
            _person.FirstDayOfWeek = DayOfWeek.Monday;
            _schedulePeriod = new SchedulePeriod(new DateOnly(2010, 1, 1), SchedulePeriodType.Day, 1);
            _person.AddSchedulePeriod(_schedulePeriod);
            _personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,_dateTimePeriod);
            _shiftCategoryLimitation = new ShiftCategoryLimitation(_personAssignment.ShiftCategory);
            _schedulePeriod.AddShiftCategoryLimitation(_shiftCategoryLimitation);
            _schedulePeriod2 = new SchedulePeriod(new DateOnly(2010, 1, 1), SchedulePeriodType.Day, 1);
            _range = _mocks.DynamicMock<IScheduleRange>();
            _part = _mocks.DynamicMock<IScheduleDay>();
           _partWithoutShift = _mocks.DynamicMock<IScheduleDay>();
           IContract contract = ContractFactory.CreateContract("MyContract");
           IPartTimePercentage partTime = PartTimePercentageFactory.CreatePartTimePercentage("Full time");
           IContractSchedule contractSchedule = ContractScheduleFactory.CreateContractSchedule("Mon-Sat");
           IContractScheduleWeek week1 = new ContractScheduleWeek();
           week1.Add(DayOfWeek.Monday, true);
           week1.Add(DayOfWeek.Tuesday, true);
           week1.Add(DayOfWeek.Wednesday, true);
           week1.Add(DayOfWeek.Thursday, true);
           week1.Add(DayOfWeek.Friday, true);
           contractSchedule.AddContractScheduleWeek(week1);
           _personContract = new PersonContract(contract, partTime, contractSchedule);
           _personPeriod = new PersonPeriod(new DateOnly(200,1,1), _personContract, new Team());
            _person.AddPersonPeriod(_personPeriod);
        }

        [Test]
        public void VerifyIsThisDayCorrectCategory()
        {
            var part = _mocks.DynamicMock<IScheduleDay>();
            IShiftCategory shiftCategory = _personAssignment.ShiftCategory;

            using(_mocks.Record())
            {
                Expect.Call(part.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
                Expect.Call(part.PersonAssignment()).Return(_personAssignment).Repeat.Any();
            }

            Assert.IsTrue(ShiftCategoryLimitationChecker.IsThisDayCorrectCategory(part, shiftCategory));
        }

        [Test]
        public void VerifyIsThisDayCorrectCategoryWhenWrongCategory()
        {
            var part = _mocks.DynamicMock<IScheduleDay>();
            IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("xx");

            using (_mocks.Record())
            {
                Expect.Call(part.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
                Expect.Call(part.PersonAssignment()).Return(_personAssignment).Repeat.Any();
            }

            Assert.IsFalse(ShiftCategoryLimitationChecker.IsThisDayCorrectCategory(part, shiftCategory));
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void VerifyIsShiftCategoryOverOrAtPeriodLimitThrows()
        {
            _shiftCategoryLimitation.MaxNumberOf = 1;
            _shiftCategoryLimitation.Weekly = true;
            IList<DateOnly> datesWithCategory;
            mockExpections();

	        using (_mocks.Playback())
	        {
		        Assert.IsTrue(ShiftCategoryLimitationChecker.IsShiftCategoryOverOrAtPeriodLimit(_shiftCategoryLimitation,
		                                                                                        new DateOnlyPeriod(2010, 1, 1,
		                                                                                                           2010, 1, 1),
		                                                                                        _dic[_person],
		                                                                                        out datesWithCategory));
	        }
        }

        [Test]
        public void VerifyIsShiftCategoryOverOrAtPeriodLimit()
        {
            _shiftCategoryLimitation.MaxNumberOf = 1;
            IList<DateOnly> datesWithCategory;
            mockExpections();

	        using (_mocks.Playback())
	        {
		        Assert.IsTrue(ShiftCategoryLimitationChecker.IsShiftCategoryOverOrAtPeriodLimit(_shiftCategoryLimitation,
		                                                                                        new DateOnlyPeriod(2010, 1, 1,
		                                                                                                           2010, 1, 1),
		                                                                                        _dic[_person],
		                                                                                        out datesWithCategory));
	        }
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void VerifyIsShiftCategoryOverOrAtWeekLimitThrow()
        {
            _shiftCategoryLimitation.Weekly = false;
            _shiftCategoryLimitation.MaxNumberOf = 1;
            IList<DateOnly> datesWithCategory;
            mockExpections();

	        using (_mocks.Playback())
	        {
		        Assert.IsTrue(ShiftCategoryLimitationChecker.IsShiftCategoryOverOrAtWeekLimit(_shiftCategoryLimitation,
		                                                                                      _dic[_person],
		                                                                                      new DateOnlyPeriod(
			                                                                                      new DateOnly(2009, 12, 28),
			                                                                                      new DateOnly(2010, 1, 3)),
		                                                                                      out datesWithCategory));
	        }
        }

        [Test, SetCulture("en-GB")]
        public void VerifyIsShiftCategoryOverOrAtWeekLimit()
        {
            _shiftCategoryLimitation.Weekly = true;
            _shiftCategoryLimitation.MaxNumberOf = 1;
            IList<DateOnly> datesWithCategory;
            mockExpections();

	        using (_mocks.Playback())
	        {
		        Assert.IsTrue(ShiftCategoryLimitationChecker.IsShiftCategoryOverOrAtWeekLimit(_shiftCategoryLimitation,
		                                                                                      _dic[_person],
		                                                                                      new DateOnlyPeriod(2009, 12, 28,2010, 1, 3),
		                                                                                      out datesWithCategory));
		        Assert.AreNotEqual(0, datesWithCategory.Count);
	        }
        }

        [Test]
        public void VerifySetBlockedShiftCategoriesWhenNotUsingLimitations()
        {
            _shiftCategoryLimitation.Weekly = true;
            _shiftCategoryLimitation.MaxNumberOf = 1;
            IOptimizerOriginalPreferences preferences = new OptimizerOriginalPreferences(new SchedulingOptions())
                                                     	{
                                                     		SchedulingOptions = {UseShiftCategoryLimitations = false}
                                                     	};

            mockExpections();

	        using (_mocks.Playback())
	        {
		        _target.SetBlockedShiftCategories(preferences.SchedulingOptions, _person, new DateOnly(2010, 1, 1));

		        Assert.AreEqual(0, preferences.SchedulingOptions.NotAllowedShiftCategories.Count);
	        }
        }

        [Test]
        public void VerifySetBlockedShiftCategoriesWhenUsingLimitationsWeek()
        {
            _shiftCategoryLimitation.Weekly = true;
            _shiftCategoryLimitation.MaxNumberOf = 1;
            IOptimizerOriginalPreferences preferences = new OptimizerOriginalPreferences(new SchedulingOptions())
                                                     	{
                                                     		SchedulingOptions = {UseShiftCategoryLimitations = true}
                                                     	};

            mockExpections();

	        using (_mocks.Playback())
	        {
		        _target.SetBlockedShiftCategories(preferences.SchedulingOptions, _person, new DateOnly(2010, 1, 1));

		        Assert.AreEqual(1, preferences.SchedulingOptions.NotAllowedShiftCategories.Count);
	        }
        }

        [Test]
        public void VerifySetBlockedShiftCategoriesWhenUsingLimitationsPeriod()
        {
            _shiftCategoryLimitation.Weekly = false;
            _shiftCategoryLimitation.MaxNumberOf = 1;
            IOptimizerOriginalPreferences preferences = new OptimizerOriginalPreferences(new SchedulingOptions())
                                                     	{
                                                     		SchedulingOptions = {UseShiftCategoryLimitations = true}
                                                     	};

            mockExpections();

        	using (_mocks.Playback())
        	{
				_target.SetBlockedShiftCategories(preferences.SchedulingOptions, _person, new DateOnly(2010, 1, 1));

				Assert.AreEqual(1, preferences.SchedulingOptions.NotAllowedShiftCategories.Count);
        	}
        }

        private void mockExpections()
        {
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.Any();
				Expect.Call(_dic[_person]).Return(_range).Repeat.Any();
				Expect.Call(_range.ScheduledDayCollection(new DateOnlyPeriod(2009, 12, 28, 2010, 1, 3)))
					  .Return(new[]
		                  {
			                  _partWithoutShift, _partWithoutShift, _partWithoutShift, _partWithoutShift, _part,
			                  _partWithoutShift, _partWithoutShift
		                  })
					  .Repeat.Any();
	            Expect.Call(_range.ScheduledDayCollection(new DateOnlyPeriod(2010, 1, 1, 2010, 1, 1))).Return(new[] { _part }).Repeat.Any();
	            Expect.Call(_part.DateOnlyAsPeriod)
	                  .Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 1),
	                                                       _person.PermissionInformation.DefaultTimeZone())).Repeat.Any();
	            Expect.Call(_partWithoutShift.DateOnlyAsPeriod)
	                  .Return(new DateOnlyAsDateTimePeriod(new DateOnly(2009, 12, 29),
	                                                       _person.PermissionInformation.DefaultTimeZone())).Repeat.Any();
                Expect.Call(_part.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
                Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.Any();
                Expect.Call(_part.Period).Return(_dateTimePeriod).Repeat.Any();
                Expect.Call(_part.Person).Return(_person).Repeat.Any();
            }
        }

        [Test]
        public void VerifySetBlockedDiffShiftCategoryWhenUsingLimitationPeriodForGroup()
        {
            IPersonAssignment personAssignment2;
            IGroupPerson groupPerson;
            IScenario scenario;
            IPersonAssignment personAssignment1;
            var dateOnly = SetupPersonAndShiftCategory(out personAssignment2, out groupPerson, out scenario, out personAssignment1,false,false);

            IOptimizerOriginalPreferences preferences = new OptimizerOriginalPreferences(new SchedulingOptions())
            {
                SchedulingOptions = { UseShiftCategoryLimitations = true }
            };
            var range = _mocks.DynamicMock<IScheduleRange>();
            var dic = new ScheduleDictionary(scenario , new ScheduleDateTimePeriod(_dateTimePeriod));
            ((ScheduleRange)dic[personAssignment1 .Person]).Add(personAssignment1 );
            ((ScheduleRange)dic[personAssignment2 .Person]).Add(personAssignment2 );
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.Schedules).Return(dic).Repeat.Any();
                Expect.Call(range.ScheduledDay(new DateOnly(2011, 1, 1))).Return(_part).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                _target.SetBlockedShiftCategories(preferences.SchedulingOptions, groupPerson, dateOnly);
                Assert.AreEqual(2, preferences.SchedulingOptions.NotAllowedShiftCategories.Count);
            }
        }
        
        [Test]
        public void VerifySetBlockedSameShiftCategoryWhenUsingLimitationPeriodForGroup()
        {
            IPersonAssignment personAssignment2;
            IGroupPerson groupPerson;
            IScenario scenario;
            IPersonAssignment personAssignment1;
            var dateOnly = SetupPersonAndShiftCategory(out personAssignment2, out groupPerson, out scenario, out personAssignment1, true,false);

            IOptimizerOriginalPreferences preferences = new OptimizerOriginalPreferences(new SchedulingOptions())
            {
                SchedulingOptions = { UseShiftCategoryLimitations = true }
            };
            var range = _mocks.DynamicMock<IScheduleRange>();
            var dic = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(_dateTimePeriod));
            ((ScheduleRange)dic[personAssignment1.Person]).Add(personAssignment1);
            ((ScheduleRange)dic[personAssignment2.Person]).Add(personAssignment2);
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.Schedules).Return(dic).Repeat.Any();
                Expect.Call(range.ScheduledDay(new DateOnly(2011, 1, 1))).Return(_part).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                _target.SetBlockedShiftCategories(preferences.SchedulingOptions, groupPerson, dateOnly);
                Assert.AreEqual(1, preferences.SchedulingOptions.NotAllowedShiftCategories.Count);
            }

        }

        [Test]
        public void VerifySetBlockedDiffShiftCategoryWhenUsingLimitationWeekForGroup()
        {
            IPersonAssignment personAssignment2;
            IGroupPerson groupPerson;
            IScenario scenario;
            IPersonAssignment personAssignment1;
            var dateOnly = SetupPersonAndShiftCategory(out personAssignment2, out groupPerson, out scenario, out personAssignment1, false,true);

            IOptimizerOriginalPreferences preferences = new OptimizerOriginalPreferences(new SchedulingOptions())
            {
                SchedulingOptions = { UseShiftCategoryLimitations = true }
            };
            var range = _mocks.DynamicMock<IScheduleRange>();
            var dic = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(_dateTimePeriod));
            ((ScheduleRange)dic[personAssignment1.Person]).Add(personAssignment1);
            ((ScheduleRange)dic[personAssignment2.Person]).Add(personAssignment2);
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.Schedules).Return(dic).Repeat.Any();
                Expect.Call(range.ScheduledDay(new DateOnly(2011, 1, 1))).Return(_part).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                _target.SetBlockedShiftCategories(preferences.SchedulingOptions, groupPerson, dateOnly);
                Assert.AreEqual(2, preferences.SchedulingOptions.NotAllowedShiftCategories.Count);
            }
        }

        [Test]
        public void VerifySetBlockedSameShiftCategoryWhenUsingLimitationWeekForGroup()
        {
           IPersonAssignment personAssignment2;
            IGroupPerson groupPerson;
            IScenario scenario;
            IPersonAssignment personAssignment1;
            var dateOnly = SetupPersonAndShiftCategory(out personAssignment2, out groupPerson, out scenario, out personAssignment1, true, true);

            IOptimizerOriginalPreferences preferences = new OptimizerOriginalPreferences(new SchedulingOptions())
            {
                SchedulingOptions = { UseShiftCategoryLimitations = true }
            };
            var range = _mocks.DynamicMock<IScheduleRange>();
            var dic = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(_dateTimePeriod));
            ((ScheduleRange)dic[personAssignment1.Person]).Add(personAssignment1);
            ((ScheduleRange)dic[personAssignment2.Person]).Add(personAssignment2);
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.Schedules).Return(dic).Repeat.Any();
                Expect.Call(range.ScheduledDay(new DateOnly(2011, 1, 1))).Return(_part).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                _target.SetBlockedShiftCategories(preferences.SchedulingOptions, groupPerson, dateOnly);
                Assert.AreEqual(1, preferences.SchedulingOptions.NotAllowedShiftCategories.Count);
            }
        }

        private DateOnly SetupPersonAndShiftCategory(out IPersonAssignment personAssignment2, out IGroupPerson groupPerson,
                                                    out IScenario scenario, out IPersonAssignment personAssignment1, bool withSameShiftCategory, bool isWeekPerdiod)
        {
            var dateOnly = new DateOnly(2010, 1, 1);
            scenario = ScenarioFactory.CreateScenarioAggregate();
            var person1 = PersonFactory.CreatePerson("P1");
            person1.AddSchedulePeriod(_schedulePeriod2);
            person1.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(dateOnly));
            personAssignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person1, _dateTimePeriod);
            var shiftCategoryLimitation1 =
                new ShiftCategoryLimitation(personAssignment1.ShiftCategory) { MaxNumberOf = 1, Weekly = isWeekPerdiod };
            person1.SchedulePeriod(dateOnly).AddShiftCategoryLimitation(shiftCategoryLimitation1);
            var person2 = PersonFactory.CreatePerson("P2");
            person2.AddSchedulePeriod(_schedulePeriod2);
            person2.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(dateOnly));
            personAssignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person2, _dateTimePeriod);
            var shiftCategoryLimitation2 =
                new ShiftCategoryLimitation(personAssignment2.ShiftCategory) { MaxNumberOf = 1, Weekly = isWeekPerdiod };
            if (!withSameShiftCategory)
                person2.SchedulePeriod(dateOnly).AddShiftCategoryLimitation(shiftCategoryLimitation2);
            groupPerson = new GroupPersonFactory().CreateGroupPerson(new List<IPerson> { person1, person2 }, dateOnly, "gp1", null);
            return dateOnly;
        }

    }
}