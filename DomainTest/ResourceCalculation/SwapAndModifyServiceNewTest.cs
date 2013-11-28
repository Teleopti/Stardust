using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class SwapAndModifyServiceNewTest
	{
		private SwapAndModifyServiceNew _swapAndModifyServiceNew;
		private MockRepository _mock;
		private ISwapServiceNew _swapService;
		private IPerson _person1;
		private IPerson _person2;
		private IList<DateOnly> _dates;
		private IScheduleDictionary _dictionary;
		private IScenario _scenario;
		private DateTimePeriod _d1;
		private DateTimePeriod _d2;
		private IScheduleDay _p1D1;
		private IScheduleDay _p1D2;
		private IScheduleDay _p2D1;
		private IScheduleDay _p2D2;
		private IScheduleRange _range1;
		private IScheduleRange _range2;
		private IList<DateOnly> _lockedDates;
	    private IScheduleDayChangeCallback _scheduleDayChangeCallback;

	    [SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_swapService = _mock.StrictMock<ISwapServiceNew>();
            _scheduleDayChangeCallback = _mock.DynamicMock<IScheduleDayChangeCallback>();
			_swapAndModifyServiceNew = new SwapAndModifyServiceNew(_swapService,_scheduleDayChangeCallback);
			_person1 = _mock.StrictMock<IPerson>();
			_person2 = _mock.StrictMock<IPerson>();
			_dictionary = _mock.StrictMock<IScheduleDictionary>();

			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_person1 = PersonFactory.CreatePerson();
			_person2 = PersonFactory.CreatePerson();
			_dates = new List<DateOnly>();
			_lockedDates = new List<DateOnly>();
			
			Expect.Call(_dictionary.Scenario).Return(_scenario).Repeat.AtLeastOnce();
			_mock.Replay(_dictionary);
			_range1 = _mock.StrictMock<IScheduleRange>();
			_range2 = _mock.StrictMock<IScheduleRange>();
			_d1 = new DateTimePeriod(new DateTime(2008, 2, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 2, 2, 0, 0, 0, DateTimeKind.Utc));
			_d2 = new DateTimePeriod(new DateTime(2008, 2, 2, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 2, 3, 0, 0, 0, DateTimeKind.Utc));
			IShiftCategory category = ShiftCategoryFactory.CreateShiftCategory("hej");
			IActivity activity = ActivityFactory.CreateActivity("hej");

			_p1D1 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person1, new DateOnly(2008, 2, 1));
			_p1D1.AddMainShift(MainShiftFactory.CreateMainShift(activity, _d1, category));
			_p2D1 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person2, new DateOnly(2008, 2, 1));
			_p2D1.AddMainShift(MainShiftFactory.CreateMainShift(activity, _d1, category));

			_p1D2 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person1, new DateOnly(2008, 2, 2));
			_p1D2.AddMainShift(MainShiftFactory.CreateMainShift(activity, _d2, category));
			_p2D2 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person2, new DateOnly(2008, 2, 2));
			_p2D2.AddMainShift(MainShiftFactory.CreateMainShift(activity, _d2, category));
			_mock.BackToRecord(_dictionary);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenSwapServiceIsNull()
		{
			_swapAndModifyServiceNew = new SwapAndModifyServiceNew(null,_scheduleDayChangeCallback);	
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenPersonOneIsNull()
		{
            _swapAndModifyServiceNew.Swap(null, _person2, _dates, _lockedDates, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance));
		}
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenPersonTwoIsNull()
		{
            _swapAndModifyServiceNew.Swap(_person1, null, _dates, _lockedDates, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance));
		}
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenDatesIsNull()
		{
            _swapAndModifyServiceNew.Swap(_person1, _person2, null, _lockedDates, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance));
		}
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenDictionaryIsNull()
		{
            _swapAndModifyServiceNew.Swap(_person1, _person2, _dates, _lockedDates, null, null, new ScheduleTagSetter(NullScheduleTag.Instance));
		}
		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ShouldThrowExceptionWhenNoDates()
		{
            _swapAndModifyServiceNew.Swap(_person1, _person2, _dates, _lockedDates, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance));
		}
		[Test, ExpectedException(typeof(ArgumentException))]
		public void ShouldThrowExceptionWhenPersonOneAndPersonTwoIsSame()
		{
			_dates = new List<DateOnly> { new DateOnly(2011, 1, 1) };
            _swapAndModifyServiceNew.Swap(_person1, _person1, _dates, _lockedDates, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance));
		}
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenLockedDatesIsNull()
		{
			_dates = new List<DateOnly> { new DateOnly(2011, 1, 1) };
            _swapAndModifyServiceNew.Swap(_person1, _person2, _dates, null, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance));
		}


		[Test]
		public void ShouldSwap()
		{
			_dates.Clear();
			_dates.Add(new DateOnly(2009, 02, 01));
			_dates.Add(new DateOnly(2009, 02, 02));
			using (_mock.Record())
			{
				Expect.Call(_dictionary[_person1]).Return(_range1).Repeat.Twice();
				Expect.Call(_range1.ScheduledDay(_dates[0])).Return(_p1D1);
				Expect.Call(_range1.ScheduledDay(_dates[1])).Return(_p1D2);
				Expect.Call(_dictionary[_person2]).Return(_range2).Repeat.Twice();
				Expect.Call(_range2.ScheduledDay(_dates[0])).Return(_p2D1);
				Expect.Call(_range2.ScheduledDay(_dates[1])).Return(_p2D2);
				LastCall.IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_swapService.Swap(_p1D1, _p2D1, _dictionary)).Return(new List<IScheduleDay> { _p1D1, _p2D1 });
				Expect.Call(_swapService.Swap(_p1D2, _p2D2, _dictionary)).Return(new List<IScheduleDay> { _p1D2, _p2D2 });
                Expect.Call(_dictionary.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay>(), null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(new List<IBusinessRuleResponse>());
			}
			using (_mock.Playback())
			{
                _swapAndModifyServiceNew.Swap(_person1, _person2, _dates, _lockedDates, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance));
			}
		}


        
		

		[Test]
		public void ShouldNotSwapDaysWithLocks()
		{
			_dates.Clear();
			_dates.Add(new DateOnly(2009, 02, 01));
			_dates.Add(new DateOnly(2009, 02, 02));
			_lockedDates.Add(new DateOnly(2009, 02, 02));

			using (_mock.Record())
			{
				Expect.Call(_dictionary[_person1]).Return(_range1).Repeat.Twice();
				Expect.Call(_range1.ScheduledDay(_dates[0])).Return(_p1D1);
				Expect.Call(_range1.ScheduledDay(_dates[1])).Return(_p1D2);
				Expect.Call(_dictionary[_person2]).Return(_range2).Repeat.Twice();
				Expect.Call(_range2.ScheduledDay(_dates[0])).Return(_p2D1);
				Expect.Call(_range2.ScheduledDay(_dates[1])).Return(_p2D2);
				LastCall.IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_swapService.Swap(_p1D1, _p2D1, _dictionary)).Return(new List<IScheduleDay> { _p1D1, _p2D1 });

                Expect.Call(_dictionary.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay>(), null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(new List<IBusinessRuleResponse>());
			}
			using (_mock.Playback())
			{
                _swapAndModifyServiceNew.Swap(_person1, _person2, _dates, _lockedDates, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance));
			}			
		}

		[Test]
		public void ShouldOnlyReturnNotOverriddenRules()
		{
			var response1 = _mock.StrictMock<IBusinessRuleResponse>();
			var response2 = _mock.StrictMock<IBusinessRuleResponse>();
			_dates.Clear();
			_dates.Add(new DateOnly(2009, 02, 01));
			using (_mock.Record())
			{
				Expect.Call(_dictionary[_person1]).Return(_range1);
				Expect.Call(_range1.ScheduledDay(_dates[0])).Return(_p1D1);
				Expect.Call(_dictionary[_person2]).Return(_range2);
				Expect.Call(_range2.ScheduledDay(_dates[0])).Return(_p2D1);
				LastCall.IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_swapService.Swap(_p1D1, _p2D1 ,_dictionary)).Return(new List<IScheduleDay> { _p1D1, _p2D1 });
                Expect.Call(_dictionary.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay>(), null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).
					IgnoreArguments().Return(new List<IBusinessRuleResponse> { response1, response2 });
				Expect.Call(response1.Overridden).Return(true);
				Expect.Call(response2.Overridden).Return(false);
			}
			using (_mock.Playback())
			{
                var result = _swapAndModifyServiceNew.Swap(_person1, _person2, _dates, _lockedDates, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance));
				Assert.AreEqual(response2, result.Single());
			}
		}
	}
}
