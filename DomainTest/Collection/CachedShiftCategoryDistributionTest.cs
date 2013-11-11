﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Collection
{
	[TestFixture]
	public class CachedShiftCategoryDistributionTest
	{
		private MockRepository _mocks;
		private CachedShiftCategoryDistribution _target;
		private IScheduleDictionary _dic;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dic = _mocks.StrictMock<IScheduleDictionary>();
		}

		[Test]
		public void ShouldGetMinMaxValue()
		{
			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 12);
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var personList = new List<IPerson> { person1, person2 };
			var range = _mocks.StrictMock<IScheduleRange>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej");
			var assWithShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Scenario("hej"), person1, new DateTimePeriod(), shiftCategory);
			var assEmpty = PersonAssignmentFactory.CreatePersonAssignmentEmpty();
	
			using (_mocks.Record())
			{
				Expect.Call(() => _dic.PartModified += null).IgnoreArguments().Repeat.Twice();
				Expect.Call(_dic[person1]).Return(range);
				Expect.Call(range.ScheduledDay(new DateOnly(2013, 09, 12))).Return(scheduleDay);
				Expect.Call(scheduleDay.PersonAssignment(true)).Return(assWithShift);
				Expect.Call(_dic[person2]).Return(range);
				Expect.Call(range.ScheduledDay(new DateOnly(2013, 09, 12))).Return(scheduleDay);
				Expect.Call(scheduleDay.PersonAssignment(true)).Return(assEmpty);
			}
			using (_mocks.Playback())
			{
				var cachedNumberOfEachCategoryPerPerson = new CachedNumberOfEachCategoryPerPerson(_dic, periodToMonitor);
				_target = new CachedShiftCategoryDistribution(_dic, periodToMonitor, cachedNumberOfEachCategoryPerPerson, new List<IShiftCategory>{shiftCategory});
				_target.SetFilteredPersons(personList);
				var minMaxValue = _target.GetMinMaxDictionary(personList);
				Assert.AreEqual(0, minMaxValue[shiftCategory].Minimum);
				Assert.AreEqual(1, minMaxValue[shiftCategory].Maximum);
			}
		}

		[Test]
		public void ShouldGetCachedMinMaxWhenNoChangedAgents()
		{
			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 12);
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var personList = new List<IPerson> { person1, person2 };
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej");

			using (_mocks.Record())
			{
				Expect.Call(() => _dic.PartModified += null).IgnoreArguments().Repeat.Twice();
			}
			using (_mocks.Playback())
			{
				var cachedNumberOfEachCategoryPerPerson = new CachedNumberOfEachCategoryPerPerson(_dic, periodToMonitor);
				_target = new CachedShiftCategoryDistribution(_dic, periodToMonitor, cachedNumberOfEachCategoryPerPerson, new List<IShiftCategory> { shiftCategory });
				_target.SetFilteredPersons(new List<IPerson>());
				var minMaxValue = _target.GetMinMaxDictionary(personList);
				Assert.AreEqual(-1, minMaxValue[shiftCategory].Minimum);
				Assert.AreEqual(0, minMaxValue[shiftCategory].Maximum);
			}	
		}
	}
}
