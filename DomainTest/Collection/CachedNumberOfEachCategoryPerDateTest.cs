using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Collection
{
	[TestFixture]
	public class CachedNumberOfEachCategoryPerDateTest
	{
		private MockRepository _mocks;
		private ICachedNumberOfEachCategoryPerDate _target;
		private IScheduleDictionary _dic;
	    private IPerson _person;
	    private IScheduleRange _range;
	    private IScheduleDay _scheduleDay;
	    private IShiftCategory _shiftCategory;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dic = _mocks.StrictMock<IScheduleDictionary>();
	        _person = _mocks.StrictMock<IPerson>();
            _range = _mocks.StrictMock<IScheduleRange>();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _shiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej"); 
		}

		[Test]
		public void ShouldReturnValueForKey()
		{
			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 13);
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var personList = new List<IPerson> {person1, person2};
			var assWithShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Scenario("hej"), person1, new DateTimePeriod(),
																			_shiftCategory);
			var assEmpty = PersonAssignmentFactory.CreatePersonAssignmentEmpty();

			using (_mocks.Record())
			{
				Expect.Call(() => _dic.PartModified += null).IgnoreArguments();
				//key was not found
				Expect.Call(_dic[person1]).Return(_range);
				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 09, 12))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.PersonAssignment(true)).Return(assWithShift);
				Expect.Call(_dic[person2]).Return(_range);
				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 09, 12))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.PersonAssignment(true)).Return(assEmpty);
			}

			using (_mocks.Playback())
			{
				_target = new CachedNumberOfEachCategoryPerDate(_dic, periodToMonitor);
				_target.SetFilteredPersons(personList);
				IDictionary<IShiftCategory, int> value = _target.GetValue(new DateOnly(2013, 09, 12));
				Assert.AreEqual(1, value[_shiftCategory]);
				//and if we call it again with the same date the key will be found
				value = _target.GetValue(new DateOnly(2013, 09, 12));
				Assert.AreEqual(1, value[_shiftCategory]);
			}
		}

		[Test]
		public void SettingFilteredPersonsShouldClearTheCache()
		{
			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 13);
			var person1 = PersonFactory.CreatePerson();
			var personList = new List<IPerson> { person1 };
			var assWithShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Scenario("hej"), person1, new DateTimePeriod(),
																			_shiftCategory);
			var assEmpty = PersonAssignmentFactory.CreatePersonAssignmentEmpty();

			using (_mocks.Record())
			{
				Expect.Call(() => _dic.PartModified += null).IgnoreArguments();
				//add two dates
				Expect.Call(_dic[person1]).Return(_range);
				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 09, 12))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.PersonAssignment(true)).Return(assWithShift);
				Expect.Call(_dic[person1]).Return(_range);
				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 09, 13))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.PersonAssignment(true)).Return(assEmpty);
			}

			using (_mocks.Playback())
			{
				_target = new CachedNumberOfEachCategoryPerDate(_dic, periodToMonitor);
				_target.SetFilteredPersons(personList);
				_target.GetValue(new DateOnly(2013, 09, 12));
				_target.GetValue(new DateOnly(2013, 09, 13));
				Assert.AreEqual(2, _target.ItemCount);
				_target.SetFilteredPersons(personList);
				Assert.AreEqual(0, _target.ItemCount);
			}
		}

		[Test]
		public void ShouldClearKeyIfScheduleForPersonWithinMonitoringPeriodChanges()
		{
			IEventRaiser eventRaiser;

			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 13);
			var person1 = PersonFactory.CreatePerson();
			var personList = new List<IPerson> { person1 };
			var assWithShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Scenario("hej"), person1, new DateTimePeriod(),
																			_shiftCategory);
			var assEmpty = PersonAssignmentFactory.CreatePersonAssignmentEmpty();
			var dateOnlyAsPeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();

			using (_mocks.Record())
			{
				Expect.Call(() => _dic.PartModified += null).IgnoreArguments();
				eventRaiser = LastCall.GetEventRaiser();
				//add two dates
				Expect.Call(_dic[person1]).Return(_range);
				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 09, 12))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.PersonAssignment(true)).Return(assWithShift);
				Expect.Call(_dic[person1]).Return(_range);
				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 09, 13))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.PersonAssignment(true)).Return(assEmpty);
				//event is rised
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsPeriod);
				Expect.Call(dateOnlyAsPeriod.DateOnly).Return(new DateOnly(2013, 09, 12));
			}

			using (_mocks.Playback())
			{
				_target = new CachedNumberOfEachCategoryPerDate(_dic, periodToMonitor);
				_target.SetFilteredPersons(personList);
				_target.GetValue(new DateOnly(2013, 09, 12));
				_target.GetValue(new DateOnly(2013, 09, 13));
				Assert.AreEqual(2, _target.ItemCount);
				//fire event for a change on one day
				eventRaiser.Raise(_dic, new ModifyEventArgs(ScheduleModifier.Scheduler, null, new DateTimePeriod(), _scheduleDay));
				Assert.AreEqual(1, _target.ItemCount);
			}
		}

		[Test]
		public void ShouldClearAllIfUnknownChanges()
		{
			IEventRaiser eventRaiser;

			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 13);
			var person1 = PersonFactory.CreatePerson();
			var personList = new List<IPerson> { person1 };
			var assWithShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Scenario("hej"), person1, new DateTimePeriod(),
																			_shiftCategory);
			var assEmpty = PersonAssignmentFactory.CreatePersonAssignmentEmpty();

			using (_mocks.Record())
			{
				Expect.Call(() => _dic.PartModified += null).IgnoreArguments();
				eventRaiser = LastCall.GetEventRaiser();
				//add two dates
				Expect.Call(_dic[person1]).Return(_range);
				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 09, 12))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.PersonAssignment(true)).Return(assWithShift);
				Expect.Call(_dic[person1]).Return(_range);
				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 09, 13))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.PersonAssignment(true)).Return(assEmpty);
				//event is rised

			}

			using (_mocks.Playback())
			{
				_target = new CachedNumberOfEachCategoryPerDate(_dic, periodToMonitor);
				_target.SetFilteredPersons(personList);
				_target.GetValue(new DateOnly(2013, 09, 12));
				_target.GetValue(new DateOnly(2013, 09, 13));
				Assert.AreEqual(2, _target.ItemCount);
				//fire event for a change on one day
				eventRaiser.Raise(_dic, new ModifyEventArgs(ScheduleModifier.Scheduler, null, new DateTimePeriod(), null));
				Assert.AreEqual(0, _target.ItemCount);
			}
		}

        [Test]
        public void ShouldNotConsiderPersonWhoHaveLeft()
        {
            var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 16);
            var dateToMonitor = new DateOnly(2013, 09, 13);
            var personList = new List<IPerson> { _person };
           
            using (_mocks.Record())
            {
                Expect.Call(() => _dic.PartModified += null).IgnoreArguments();
                Expect.Call(_person.TerminalDate).Return(dateToMonitor);
            }

            using (_mocks.Playback())
            {
                _target = new CachedNumberOfEachCategoryPerDate(_dic, periodToMonitor);
                _target.SetFilteredPersons(personList);
                var result = _target.GetValue(dateToMonitor.AddDays(1));
                Assert.AreEqual(0, result.Count);
            }
        }

	}
}