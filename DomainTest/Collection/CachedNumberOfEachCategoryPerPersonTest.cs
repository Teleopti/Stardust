﻿using System.Collections.Generic;
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
	public class CachedNumberOfEachCategoryPerPersonTest
	{
		private MockRepository _mocks;
		private ICachedNumberOfEachCategoryPerPerson _target;
		private IScheduleDictionary _dic;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dic = _mocks.StrictMock<IScheduleDictionary>();
		}

		[Test]
		public void ShouldReturnValueForKey()
		{
			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 13);
			var person = PersonFactory.CreatePerson();
			var range = _mocks.StrictMock<IScheduleRange>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej");
			var assWithShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Scenario("hej"), person, new DateTimePeriod(),
			                                                                shiftCategory);
			var assEmpty = PersonAssignmentFactory.CreatePersonAssignmentEmpty();

			using (_mocks.Record())
			{
				Expect.Call(() => _dic.PartModified += null).IgnoreArguments();
				//key was not found
				Expect.Call(_dic[person]).Return(range);
				Expect.Call(range.ScheduledDay(new DateOnly(2013, 09, 12))).Return(scheduleDay);
				Expect.Call(scheduleDay.PersonAssignment(true)).Return(assWithShift);
				//now the key have been added
				Expect.Call(range.ScheduledDay(new DateOnly(2013, 09, 13))).Return(scheduleDay);
				Expect.Call(scheduleDay.PersonAssignment(true)).Return(assEmpty);
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
			var assWithShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Scenario("hej"), person1, new DateTimePeriod(),
																			shiftCategory);
			var dateOnlyAsPeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();

			using (_mocks.Record())
			{
				Expect.Call(() => _dic.PartModified += null).IgnoreArguments();
				eventRaiser = LastCall.GetEventRaiser();
				//add values for one person
				Expect.Call(_dic[person1]).Return(range);
				Expect.Call(range.ScheduledDay(new DateOnly(2013, 09, 12))).Return(scheduleDay);
				Expect.Call(scheduleDay.PersonAssignment(true)).Return(assWithShift);
				//add values for another person
				Expect.Call(_dic[person2]).Return(range);
				Expect.Call(range.ScheduledDay(new DateOnly(2013, 09, 12))).Return(scheduleDay);
				Expect.Call(scheduleDay.PersonAssignment(true)).Return(assWithShift);
				//fire event that removes person1
				Expect.Call(scheduleDay.Person).Return(person1);
				Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsPeriod);
				Expect.Call(dateOnlyAsPeriod.DateOnly).Return(new DateOnly(2013, 09, 12));

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
			var assWithShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Scenario("hej"), person1, new DateTimePeriod(),
																			shiftCategory);

			using (_mocks.Record())
			{
				Expect.Call(() => _dic.PartModified += null).IgnoreArguments();
				eventRaiser = LastCall.GetEventRaiser();
				//add values for one person
				Expect.Call(_dic[person1]).Return(range);
				Expect.Call(range.ScheduledDay(new DateOnly(2013, 09, 12))).Return(scheduleDay);
				Expect.Call(scheduleDay.PersonAssignment(true)).Return(assWithShift);
				//add values for another person
				Expect.Call(_dic[person2]).Return(range);
				Expect.Call(range.ScheduledDay(new DateOnly(2013, 09, 12))).Return(scheduleDay);
				Expect.Call(scheduleDay.PersonAssignment(true)).Return(assWithShift);
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

	}
}