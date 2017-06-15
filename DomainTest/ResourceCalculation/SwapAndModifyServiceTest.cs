using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	public class SwapAndModifyServiceTest
    {
        private SwapAndModifyService _target;
        private IPerson _person1;
        private IPerson _person2;
        private IPerson _person3;
        private IList<DateOnly> _dates;
        private MockRepository _mocks;
        private IScheduleDictionary _dictionary;
        private ISwapService _swapService;
        private IScenario _scenario;
        private DateTimePeriod _d1;
        private DateTimePeriod _d2;
        private IScheduleDay _p1D1;
        private IScheduleDay _p1D2;
        private IScheduleDay _p2D1;
        private IScheduleDay _p2D2;
        private IScheduleRange _range1;
        private IScheduleRange _range2;
        private IScheduleDayChangeCallback _scheduleDayChangeCallback;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _swapService = _mocks.StrictMock<ISwapService>();
            _scheduleDayChangeCallback = _mocks.DynamicMock<IScheduleDayChangeCallback>();
			
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _target = new SwapAndModifyService(_swapService,_scheduleDayChangeCallback);
            _person1 = PersonFactory.CreatePerson();
            _person2 = PersonFactory.CreatePerson();
            _person3 = PersonFactory.CreatePerson();
            _dates = new List<DateOnly>();
            _dictionary = _mocks.StrictMock<IScheduleDictionary>();
            Expect.Call(_dictionary.Scenario).Return(_scenario).Repeat.AtLeastOnce();
	        Expect.Call(_dictionary.PermissionsEnabled).Return(true).Repeat.AtLeastOnce();
            _mocks.Replay(_dictionary);
            _range1 = _mocks.StrictMock<IScheduleRange>();
            _range2 = _mocks.StrictMock<IScheduleRange>();
            _d1 = new DateTimePeriod(new DateTime(2008, 2, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 2, 2, 0, 0, 0, DateTimeKind.Utc));
            _d2 = new DateTimePeriod(new DateTime(2008, 2, 2, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 2, 3, 0, 0, 0, DateTimeKind.Utc));
            IShiftCategory category = ShiftCategoryFactory.CreateShiftCategory("hej");
            IActivity activity = ActivityFactory.CreateActivity("hej");

            _p1D1 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person1, new DateOnly(2008,2,1));
            _p1D1.AddMainShift(EditableShiftFactory.CreateEditorShift(activity, _d1, category));
            _p2D1 = ExtractedSchedule.CreateScheduleDay(_dictionary,  _person2, new DateOnly(2008,2,1));
			_p2D1.AddMainShift(EditableShiftFactory.CreateEditorShift(activity, _d1, category));

            _p1D2 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person1, new DateOnly(2008,2,2));
			_p1D2.AddMainShift(EditableShiftFactory.CreateEditorShift(activity, _d2, category));
            _p2D2 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person2, new DateOnly(2008, 2, 2));
			_p2D2.AddMainShift(EditableShiftFactory.CreateEditorShift(activity, _d2, category));
            _mocks.BackToRecord(_dictionary);
        }

        [Test]
        public void VerifyCreate()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyPersonOneCannotBeNull()
        {
			Assert.Throws<ArgumentNullException>(() => _target.Swap(null, _person2, _dates, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance)));
        }
        [Test]
        public void VerifyPersonTwoCannotBeNull()
        {
			Assert.Throws<ArgumentNullException>(() => _target.Swap(_person1, null, _dates, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance)));
        }
        [Test]
        public void VerifyDatesCannotBeNull()
        {
			Assert.Throws<ArgumentNullException>(() => _target.Swap(_person1, _person2, null, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance)));
        }
        [Test]
        public void VerifyDictionaryCannotBeNull()
        {
			Assert.Throws<ArgumentNullException>(() => _target.Swap(_person1, _person2, _dates, null, null, new ScheduleTagSetter(NullScheduleTag.Instance)));
        }
        [Test]
        public void VerifyDatesCannotBeEmpty()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.Swap(_person1, _person2, _dates, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance)));
        }
        [Test]
        public void VerifyPersonsCannotSame()
        {
            _dates.Add(new DateOnly(2009,02,02));
			Assert.Throws<ArgumentException>(() => _target.Swap(_person1, _person1, _dates, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance)));
        }

        [Test]
        public void VerifySwap()
        {
            _dates.Add(new DateOnly(2009, 02, 01));
            _dates.Add(new DateOnly(2009, 02, 02));
            using (_mocks.Record())
            {
                Expect.Call(_dictionary[_person1]).Return(_range1).Repeat.Twice();
                Expect.Call(_range1.ScheduledDay(_dates[0])).Return(_p1D1);
                Expect.Call(_range1.ScheduledDay(_dates[1])).Return(_p1D2);
                Expect.Call(_dictionary[_person2]).Return(_range2).Repeat.Twice();
                Expect.Call(_range2.ScheduledDay(_dates[0])).Return(_p2D1);
                Expect.Call(_range2.ScheduledDay(_dates[1])).Return(_p2D2);
                _swapService.Init(null);
                LastCall.IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_swapService.SwapAssignments(_dictionary, false)).Return(new List<IScheduleDay> {_p1D1,_p2D1});
                Expect.Call(_swapService.SwapAssignments(_dictionary, false)).Return(new List<IScheduleDay> { _p1D2, _p2D2 });
                Expect.Call(_dictionary.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay>(), null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(new List<IBusinessRuleResponse>());
            }
            using (_mocks.Playback())
            {
                _target.Swap(_person1, _person2, _dates, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance));
            }
        }

        [Test]
        public void ShouldOnlyReturnNotOverriddenRules()
        {
            IBusinessRuleResponse response1 = _mocks.StrictMock<IBusinessRuleResponse>();
            IBusinessRuleResponse response2 = _mocks.StrictMock<IBusinessRuleResponse>();
            _dates.Add(new DateOnly(2009, 02, 01));
            using (_mocks.Record())
            {
                Expect.Call(_dictionary[_person1]).Return(_range1);
                Expect.Call(_range1.ScheduledDay(_dates[0])).Return(_p1D1);
                Expect.Call(_dictionary[_person2]).Return(_range2);
                Expect.Call(_range2.ScheduledDay(_dates[0])).Return(_p2D1);
                _swapService.Init(null);
                LastCall.IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_swapService.SwapAssignments(_dictionary, false)).Return(new List<IScheduleDay> { _p1D1, _p2D1 });
                Expect.Call(_dictionary.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay>(), null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).
                    IgnoreArguments().Return(new List<IBusinessRuleResponse>{response1,response2});
                Expect.Call(response1.Overridden).Return(true);
                Expect.Call(response2.Overridden).Return(false);
            }
            using (_mocks.Playback())
            {
                var result = _target.Swap(_person1, _person2, _dates, _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance));
                Assert.AreEqual(response2,result.Single());
            }
        }

        [Test]
        public void VerifySwapWithShiftTradeSwapDetails()
        {
            DateOnly date1 = new DateOnly(2009, 02, 01);
            DateOnly date2 = new DateOnly(2009, 02, 01);
            
            // person2 shall, on the second, have person ones shift on the first
            IShiftTradeSwapDetail detail1 = new ShiftTradeSwapDetail(_person1, _person2, date1, date2);
            // person2 shall, on the first, have person three shift on the first
            IShiftTradeSwapDetail detail2 = new ShiftTradeSwapDetail(_person2, _person3, date1, date2);
            
            IList<IShiftTradeSwapDetail> shiftTradeSwapDetails = new List<IShiftTradeSwapDetail> { detail1,detail2};
            IBusinessRuleResponse ruleResponse1 = _mocks.StrictMock<IBusinessRuleResponse>();
            IBusinessRuleResponse ruleResponse2 = _mocks.StrictMock<IBusinessRuleResponse>();

            using (_mocks.Record())
            {
                //detail1
                Expect.Call(_dictionary[_person1]).Return(_range1);
                Expect.Call(_range1.ScheduledDay(date1)).Return(_p1D1);
                Expect.Call(_dictionary[_person2]).Return(_range1);
                Expect.Call(_range1.ScheduledDay(date2)).Return(_p1D1);
                Expect.Call(()=>_swapService.Init(null)).IgnoreArguments();
                Expect.Call(_swapService.SwapAssignments(_dictionary, true)).Return(new List<IScheduleDay> { _p1D1, _p2D1 });
                //detail2
                Expect.Call(_dictionary[_person2]).Return(_range2);
                Expect.Call(_range2.ScheduledDay(date1)).Return(_p1D1);
                Expect.Call(_dictionary[_person3]).Return(_range1);
                Expect.Call(_range1.ScheduledDay(date2)).Return(_p1D1);

                Expect.Call(()=>_swapService.Init(null)).IgnoreArguments();
                Expect.Call(_swapService.SwapAssignments(_dictionary, true)).Return(new List<IScheduleDay> { _p1D1, _p2D1 });

                Expect.Call(ruleResponse1.Overridden).Return(true);
                Expect.Call(ruleResponse2.Overridden).Return(false);

				Expect.Call(ruleResponse1.DateOnlyPeriod).Return(new DateOnlyPeriod(date1, date2));
				Expect.Call(ruleResponse2.DateOnlyPeriod).Return(new DateOnlyPeriod(date1, date2));

                Expect.Call(_dictionary.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay>(), null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).
                    IgnoreArguments().Return(new List<IBusinessRuleResponse> {ruleResponse1, ruleResponse2});
            }

            using (_mocks.Playback())
            {
                var result =
                    _target.SwapShiftTradeSwapDetails(
                        new ReadOnlyCollection<IShiftTradeSwapDetail>(shiftTradeSwapDetails), _dictionary, null, new ScheduleTagSetter(NullScheduleTag.Instance));
                Assert.AreEqual(ruleResponse2,result.Single());
            }
        }

		[Test]
		public void TradeShouldBeApprovedIfNoValidationErrorOccursOnDaysOfTrade()
		{
			IShiftTradeSwapDetail shiftTradeSwapDetail1 = new ShiftTradeSwapDetail(_person1, _person2, new DateOnly(2010, 1, 1), new DateOnly(2010, 1, 1));
			IShiftTradeSwapDetail shiftTradeSwapDetail2 = new ShiftTradeSwapDetail(_person1, _person2, new DateOnly(2010, 1, 2), new DateOnly(2010, 1, 4));

			ReadOnlyCollection<IShiftTradeSwapDetail> shiftTradeSwapDetails = new ReadOnlyCollection<IShiftTradeSwapDetail>(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail1, shiftTradeSwapDetail2 });

			int numberOfshiftTradeSwapDetails = shiftTradeSwapDetails.Count;

			IEnumerable<IScheduleDay> scheduleDays = new List<IScheduleDay> { _p1D1, _p1D2 };

			IBusinessRuleResponse response1 = new BusinessRuleResponse(typeof(NewNightlyRestRule), "", true, true, new DateTimePeriod(), _person1, new DateOnlyPeriod(2010, 2, 1, 2010, 2, 2), "tjillevippen");
			IBusinessRuleResponse response2 = new BusinessRuleResponse(typeof(NewNightlyRestRule), "", true, true, new DateTimePeriod(), _person1, new DateOnlyPeriod(2010, 2, 2, 2010, 2, 3), "tjillevippen");
			// this rule will be the only in the returnlist
			IBusinessRuleResponse response3 = new BusinessRuleResponse(typeof(NewNightlyRestRule), "", true, true, new DateTimePeriod(), _person1, new DateOnlyPeriod(2010, 1, 1, 2010, 2, 3), "tjillevippen");

			IEnumerable<IBusinessRuleResponse> lista = new List<IBusinessRuleResponse> { response1, response2, response3 };

			using (_mocks.Record())
			{
				Expect.Call(() => _swapService.Init(null)).IgnoreArguments().Repeat.Times(numberOfshiftTradeSwapDetails);
				Expect.Call(_swapService.SwapAssignments(_dictionary, true)).Return(new List<IScheduleDay> { _p1D1, _p1D2 }).Repeat.Times(numberOfshiftTradeSwapDetails);

				Expect.Call(_dictionary.Modify(ScheduleModifier.Scheduler, scheduleDays, null, null, null)).IgnoreArguments()
					.Return(lista);
				Expect.Call(_dictionary[_person1])
					.Return(_range1).Repeat.Times(2);
				Expect.Call(_dictionary[_person2])
					.Return(_range2).Repeat.Times(2);
				Expect.Call(_range1.ScheduledDay(new DateOnly(2010, 1, 1))).Return(_p1D1);
				Expect.Call(_range2.ScheduledDay(new DateOnly(2010, 1, 1))).Return(_p1D2);
				Expect.Call(_range1.ScheduledDay(new DateOnly(2010, 1, 2))).Return(_p1D1);
				Expect.Call(_range2.ScheduledDay(new DateOnly(2010, 1, 4))).Return(_p1D2);
			}

			IEnumerable<IBusinessRuleResponse> result;

			using (_mocks.Playback())
			{
				result = _target.SwapShiftTradeSwapDetails(shiftTradeSwapDetails, _dictionary, null, null);
			}

			Assert.AreEqual(1, result.Count());

		}
    }
}
