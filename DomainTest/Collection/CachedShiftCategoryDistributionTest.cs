using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


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
			var period = new DateTimePeriod(2013, 09, 12, 8, 2013, 09, 12, 9);
			var assWithShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, new Scenario("hej"), period, shiftCategory);
	
			using (_mocks.Record())
			{
				Expect.Call(() => _dic.PartModified += null).IgnoreArguments().Repeat.Twice();
				Expect.Call(_dic[person1]).Return(range);
				Expect.Call(range.ScheduledDayCollection(periodToMonitor)).Return(new []{scheduleDay});
				Expect.Call(scheduleDay.PersonAssignment()).Return(assWithShift);
				Expect.Call(scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
				Expect.Call(scheduleDay.DateOnlyAsPeriod)
					.Return(new DateOnlyAsDateTimePeriod(periodToMonitor.StartDate, TimeZoneInfo.Utc)).Repeat.AtLeastOnce();
				Expect.Call(_dic[person2]).Return(range);
				Expect.Call(range.ScheduledDayCollection(periodToMonitor)).Return(new[] { scheduleDay });
				Expect.Call(scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.None);
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

		[Test]
		public void ShouldHandleDeletedCategories()
		{
			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 12);
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var personList = new List<IPerson> { person1, person2 };
			var range = _mocks.StrictMock<IScheduleRange>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej");
			shiftCategory.SetDeleted();
			var period = new DateTimePeriod(2013, 9, 12, 8, 2013, 9, 12, 9);
			var assWithShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, new Scenario("hej"), period, shiftCategory);

			using (_mocks.Record())
			{
				Expect.Call(() => _dic.PartModified += null).IgnoreArguments().Repeat.Twice();
				Expect.Call(_dic[person1]).Return(range);
				Expect.Call(range.ScheduledDayCollection(periodToMonitor)).Return(new []{scheduleDay});
				Expect.Call(scheduleDay.PersonAssignment()).Return(assWithShift);
				Expect.Call(scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
				Expect.Call(_dic[person2]).Return(range);
				Expect.Call(range.ScheduledDayCollection(periodToMonitor)).Return(new[] { scheduleDay });
				Expect.Call(scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.None);
				Expect.Call(scheduleDay.DateOnlyAsPeriod)
	.Return(new DateOnlyAsDateTimePeriod(periodToMonitor.StartDate, TimeZoneInfo.Utc)).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				var cachedNumberOfEachCategoryPerPerson = new CachedNumberOfEachCategoryPerPerson(_dic, periodToMonitor);
				_target = new CachedShiftCategoryDistribution(_dic, periodToMonitor, cachedNumberOfEachCategoryPerPerson, new List<IShiftCategory>());
				_target.SetFilteredPersons(personList);
				var minMaxValue = _target.GetMinMaxDictionary(personList);
				Assert.IsFalse(minMaxValue.ContainsKey(shiftCategory));
			}
		}
	}
}
