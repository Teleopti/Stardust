using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Collection
{
	[TestFixture]
	public class CachedNumberOfEachCategoryPerPersonTest
	{
		private MockRepository _mocks;
		private ICachedNumberOfEachCategoryPerPerson _target;
		private IScheduleDictionary _dic;
	    private IPerson _person;
	    private IScheduleRange _range;
	    private IScheduleDay _scheduleDay;
	    private IPersonAssignment _personAssignment;
	    private IShiftCategory _shiftCategory;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dic = _mocks.StrictMock<IScheduleDictionary>();
	        _person = PersonFactory.CreatePerson();
            _shiftCategory = ShiftCategoryFactory.CreateShiftCategory("AM");
	        _range = _mocks.StrictMock<IScheduleRange>();
	        _scheduleDay = _mocks.StrictMock<IScheduleDay>();
	        _personAssignment = _mocks.StrictMock<IPersonAssignment>();
		}

		[Test]
		public void ShouldReturnValueForKey()
		{
			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 13);
			var person = PersonFactory.CreatePerson();
			var range = _mocks.StrictMock<IScheduleRange>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej");
			var period = new DateTimePeriod(2013, 09, 12, 8, 2013, 09, 12, 9);
			var assWithShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, new Scenario("hej"), period, shiftCategory);
			
			using (_mocks.Record())
			{
				Expect.Call(() => _dic.PartModified += null).IgnoreArguments();
				//key was not found
				Expect.Call(_dic[person]).Return(range);
				Expect.Call(range.ScheduledDayCollection(periodToMonitor)).Return(new [] {scheduleDay,scheduleDay});
				Expect.Call(scheduleDay.PersonAssignment()).Return(assWithShift);
				Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(periodToMonitor.StartDate,person.PermissionInformation.DefaultTimeZone()));
				Expect.Call(scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
				//now the key have been added
				Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(periodToMonitor.StartDate.AddDays(1), person.PermissionInformation.DefaultTimeZone()));
				Expect.Call(scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.None);
			}

			using (_mocks.Playback())
			{
				_target = new CachedNumberOfEachCategoryPerPerson(_dic, periodToMonitor);
				IDictionary<IShiftCategory, int> value = _target.GetValue(person);
				Assert.AreEqual(1, value[shiftCategory]);
				//and if we call it again with the same person the key will be found
				value = _target.GetValue(person);
				Assert.AreEqual(1, value[shiftCategory]);
			}
		}

		[Test]
		public void ShouldClearKeyIfScheduleForPersonWithinMonitoringPeriodChanges()
		{
			IEventRaiser eventRaiser;

			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 12);
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var range = _mocks.StrictMock<IScheduleRange>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej");
			var period = new DateTimePeriod(2013, 9, 12, 8, 2013, 9, 12, 9);
			var assWithShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, new Scenario("hej"),
																			period, shiftCategory);
			
			using (_mocks.Record())
			{
				Expect.Call(() => _dic.PartModified += null).IgnoreArguments();
				eventRaiser = LastCall.GetEventRaiser();
				//add values for one person
				Expect.Call(_dic[person1]).Return(range);
				Expect.Call(range.ScheduledDayCollection(periodToMonitor)).Return(new []{scheduleDay});
				Expect.Call(scheduleDay.PersonAssignment()).Return(assWithShift);
				Expect.Call(scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
				//add values for another person
				Expect.Call(_dic[person2]).Return(range);
				Expect.Call(range.ScheduledDayCollection(periodToMonitor)).Return(new []{scheduleDay});
				Expect.Call(scheduleDay.PersonAssignment()).Return(assWithShift);
				Expect.Call(scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
				//fire event that removes person1
				Expect.Call(scheduleDay.Person).Return(person1);
				Expect.Call(scheduleDay.DateOnlyAsPeriod)
					.Return(new DateOnlyAsDateTimePeriod(periodToMonitor.StartDate, person1.PermissionInformation.DefaultTimeZone()))
					.Repeat.AtLeastOnce();

			}

			using (_mocks.Playback())
			{
				_target = new CachedNumberOfEachCategoryPerPerson(_dic, periodToMonitor);
				//add values for one person
				_target.GetValue(person1);
				//add values for another person
				_target.GetValue(person2);
				Assert.AreEqual(2, _target.ItemCount);
				//fire event
				eventRaiser.Raise(_dic, new ModifyEventArgs(ScheduleModifier.Scheduler, null, new DateTimePeriod(), scheduleDay));
				Assert.AreEqual(1, _target.ItemCount);
			}
		}

		[Test]
		public void ShouldClearAllIfUnknownChanges()
		{
			IEventRaiser eventRaiser;

			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 12);
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var range = _mocks.StrictMock<IScheduleRange>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej");
			var period = new DateTimePeriod(2013, 9, 12, 8, 2013, 9, 12, 9);
			var assWithShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, new Scenario("hej"),
																			period, shiftCategory);

			using (_mocks.Record())
			{
				Expect.Call(() => _dic.PartModified += null).IgnoreArguments();
				eventRaiser = LastCall.GetEventRaiser();
				//add values for one person
				Expect.Call(_dic[person1]).Return(range);
				Expect.Call(range.ScheduledDayCollection(periodToMonitor)).Return(new []{scheduleDay});
				Expect.Call(scheduleDay.PersonAssignment()).Return(assWithShift);
				Expect.Call(scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
				//add values for another person
				Expect.Call(_dic[person2]).Return(range);
				Expect.Call(range.ScheduledDayCollection(periodToMonitor)).Return(new[] { scheduleDay });
				Expect.Call(scheduleDay.PersonAssignment()).Return(assWithShift);
				Expect.Call(scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
				Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(periodToMonitor.StartDate, person1.PermissionInformation.DefaultTimeZone())).Repeat.AtLeastOnce();
				//fire event that removes all

			}

			using (_mocks.Playback())
			{
				_target = new CachedNumberOfEachCategoryPerPerson(_dic, periodToMonitor);
				//add values for one person
				_target.GetValue(person1);
				//add values for another person
				_target.GetValue(person2);
				Assert.AreEqual(2, _target.ItemCount);
				//fire event without enough information
				eventRaiser.Raise(_dic, new ModifyEventArgs(ScheduleModifier.Scheduler, null, new DateTimePeriod(), null));
				Assert.AreEqual(0, _target.ItemCount);
			}
		}

        [Test]
        public void ShouldNotConsiderPersonWhoHaveLeft()
        {
            var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 13);
            var dateToMonitor = new DateOnly(2013, 09, 13);
            
			_person.TerminatePerson(dateToMonitor.AddDays(-1),new PersonAccountUpdaterDummy());
            using (_mocks.Record())
            {
                Expect.Call(() => _dic.PartModified += null).IgnoreArguments();
                
                Expect.Call(_dic[_person]).Return(_range);

            	Expect.Call(_range.ScheduledDayCollection(periodToMonitor)).Return(new[] { _scheduleDay, _scheduleDay });
                Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
	            Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(periodToMonitor.StartDate.AddDays(-1), _person.PermissionInformation.DefaultTimeZone()));
                Expect.Call(_personAssignment.ShiftCategory).Return(_shiftCategory);

            	Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
	            Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(periodToMonitor.StartDate, _person.PermissionInformation.DefaultTimeZone()));
                Expect.Call(_personAssignment.ShiftCategory).Return(_shiftCategory);
            }

            using (_mocks.Playback())
            {
                _target = new CachedNumberOfEachCategoryPerPerson( _dic, periodToMonitor);
                var result = _target.GetValue(_person);
                Assert.AreEqual(1, result.Count);
                Assert.IsTrue(result.ContainsKey(_shiftCategory ) );
                Assert.AreEqual(2,result[_shiftCategory ]);
            }
        }
	}
}