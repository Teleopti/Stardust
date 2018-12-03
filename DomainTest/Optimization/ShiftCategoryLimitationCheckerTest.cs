using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_dic = _mocks.DynamicMock<IScheduleDictionary>();
            _target = new ShiftCategoryLimitationChecker(()=>_stateHolder);
            _dateTimePeriod = new DateTimePeriod(2010,1,1,2010,1,2);
            _person = PersonFactory.CreatePerson();
            _person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
            _person.FirstDayOfWeek = DayOfWeek.Monday;
            _schedulePeriod = new SchedulePeriod(new DateOnly(2010, 1, 1), SchedulePeriodType.Day, 1);
            _person.AddSchedulePeriod(_schedulePeriod);
            _personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,_dateTimePeriod);
            _shiftCategoryLimitation = new ShiftCategoryLimitation(_personAssignment.ShiftCategory);
            _schedulePeriod.AddShiftCategoryLimitation(_shiftCategoryLimitation);
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

        [Test]
        public void VerifyIsShiftCategoryOverOrAtPeriodLimitThrows()
        {
            _shiftCategoryLimitation.MaxNumberOf = 1;
            _shiftCategoryLimitation.Weekly = true;
            IList<DateOnly> datesWithCategory;
            mockExpections();
	        Assert.Throws<ArgumentException>(() =>
	        {
				using (_mocks.Playback())
				{
					Assert.IsTrue(ShiftCategoryLimitationChecker.IsShiftCategoryOverOrAtPeriodLimit(_shiftCategoryLimitation,
																									new DateOnlyPeriod(2010, 1, 1,
																													   2010, 1, 1),
																									_dic[_person],
																									out datesWithCategory));
				}

			});
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

        [Test]
        public void VerifyIsShiftCategoryOverOrAtWeekLimitThrow()
        {
            _shiftCategoryLimitation.Weekly = false;
            _shiftCategoryLimitation.MaxNumberOf = 1;
            IList<DateOnly> datesWithCategory;
            mockExpections();
	        Assert.Throws<ArgumentException>(() =>
	        {
				using (_mocks.Playback())
				{
					Assert.IsTrue(ShiftCategoryLimitationChecker.IsShiftCategoryOverOrAtWeekLimit(_shiftCategoryLimitation,
																								  _dic[_person],
																								  new DateOnlyPeriod(
																									  new DateOnly(2009, 12, 28),
																									  new DateOnly(2010, 1, 3)),
																								  out datesWithCategory));
				}
			});
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
	        var preferences = new SchedulingOptions
		        {UseShiftCategoryLimitations = false};

            mockExpections();

	        using (_mocks.Playback())
	        {
		        _target.SetBlockedShiftCategories(preferences, _person, new DateOnly(2010, 1, 1));

		        Assert.AreEqual(0, preferences.NotAllowedShiftCategories.Count);
	        }
        }

        [Test]
        public void VerifySetBlockedShiftCategoriesWhenUsingLimitationsWeek()
        {
            _shiftCategoryLimitation.Weekly = true;
            _shiftCategoryLimitation.MaxNumberOf = 1;
	        var preferences = new SchedulingOptions
		        { UseShiftCategoryLimitations = true };

			mockExpections();

	        using (_mocks.Playback())
	        {
		        _target.SetBlockedShiftCategories(preferences, _person, new DateOnly(2010, 1, 1));

		        Assert.AreEqual(1, preferences.NotAllowedShiftCategories.Count);
	        }
        }

        [Test]
        public void VerifySetBlockedShiftCategoriesWhenUsingLimitationsPeriod()
        {
            _shiftCategoryLimitation.Weekly = false;
            _shiftCategoryLimitation.MaxNumberOf = 1;
	        var preferences = new SchedulingOptions
		        { UseShiftCategoryLimitations = true };

			mockExpections();

        	using (_mocks.Playback())
        	{
				_target.SetBlockedShiftCategories(preferences, _person, new DateOnly(2010, 1, 1));

				Assert.AreEqual(1, preferences.NotAllowedShiftCategories.Count);
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
    }
}