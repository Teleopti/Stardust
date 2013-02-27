using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
    [TestFixture]
    public class DynamicBlockFinderTest
    {
        private IDynamicBlockFinder _target;
        private SchedulingOptions  _schedulingOptions;
        private MockRepository _mock;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IScheduleMatrixPro _matrixPro;
        private IList<IScheduleMatrixPro> _matrixList;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDayPro _scheduleDayPro3;
        private ReadOnlyCollection<IScheduleDayPro> _scheduleDayReadOnlyProList;
        private IList<IScheduleDayPro> _scheduleDayProList;
        private IScheduleDayPro _scheduleDayPro4;
        private IGroupPerson   _groupPerson;
        private BaseLineData _baseLine;
        private IScheduleDay _schedulePart;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _schedulingOptions = new SchedulingOptions();
            _matrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _matrixList = new List<IScheduleMatrixPro> {_matrixPro};
            _schedulePart = _mock.StrictMock<IScheduleDay>();
            _groupPerson = _mock.StrictMock<IGroupPerson>();
            _baseLine = new BaseLineData();
        }

        [Test]
        public void FindSkillDayFromBlockUsingPeriod()
        {
            _scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDayPro3 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDayProList = new List<IScheduleDayPro> { _scheduleDayPro1, _scheduleDayPro2, _scheduleDayPro3};
            _scheduleDayReadOnlyProList = new ReadOnlyCollection<IScheduleDayPro>(_scheduleDayProList);
            
            _schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod ;
            _target = new DynamicBlockFinder(_schedulingOptions, _schedulingResultStateHolder,_matrixList);
            var date = new DateOnly(  DateTime.UtcNow );

            using (_mock.Record())
            {
                Expect.Call(_matrixPro.EffectivePeriodDays).Return( _scheduleDayReadOnlyProList ).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro1.Day).Return(date).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro2.Day).Return(date.AddDays(1)).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro3.Day).Return(date.AddDays(2)).Repeat.AtLeastOnce();
                Expect.Call(_matrixPro.Person).Return(_baseLine.Person1);
                Expect.Call(_groupPerson.GroupMembers).Return(_baseLine.ReadOnlyCollectionPersonList);
                Expect.Call(_matrixPro.GetScheduleDayByKey(date)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePart);
                Expect.Call(_schedulePart.IsScheduled()).Return(false);
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.ExtractBlockDays(date,_groupPerson ), new List<DateOnly> { new DateOnly(date), new DateOnly(date.AddDays(1)), new DateOnly(date.AddDays(2)) });    
            }
        }

		[Test]
		public void ShouldReturnSameDateAsAskedForIfBlockFinderTypeIsSingleDay()
		{	_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;
			var date = new DateOnly(2013, 02, 22);
			_target = new DynamicBlockFinder(_schedulingOptions, _schedulingResultStateHolder, _matrixList);
            using (_mock.Record())
            {
                
            }
            using (_mock.Playback())
			{
                IList<DateOnly> result = _target.ExtractBlockDays(date, _groupPerson);
				Assert.AreEqual(date, result[0]);
				Assert.AreEqual(1, result.Count);
			}
      
      
		}

        [Test]
        public void ShouldNotContinueIfTheDateIsScheduled()
        {
            _schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.Weeks ;
            var date = new DateOnly(2013, 02, 22);
            _target = new DynamicBlockFinder(_schedulingOptions, _schedulingResultStateHolder, _matrixList);
            _scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDayPro3 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDayProList = new List<IScheduleDayPro> { _scheduleDayPro1, _scheduleDayPro2, _scheduleDayPro3 };
            _scheduleDayReadOnlyProList = new ReadOnlyCollection<IScheduleDayPro>(_scheduleDayProList);
            using (_mock.Record())
            {
                Expect.Call(_scheduleDayPro1.Day).Return(date).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro2.Day).Return(date.AddDays(1)).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro3.Day).Return(date.AddDays(2)).Repeat.AtLeastOnce();

                Expect.Call(_matrixPro.Person).Return(_baseLine.Person1);
                Expect.Call(_groupPerson.GroupMembers).Return(_baseLine.ReadOnlyCollectionPersonList);
                Expect.Call(_matrixPro.GetScheduleDayByKey(date)).Return(_scheduleDayPro1);
                Expect.Call(_matrixPro.EffectivePeriodDays).Return(_scheduleDayReadOnlyProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePart);
                Expect.Call(_schedulePart.IsScheduled()).Return(true);
            }

            using (_mock.Playback())
            {
                IList<DateOnly> result = _target.ExtractBlockDays(date, _groupPerson);
                Assert.AreEqual(0, result.Count);
            }


        }

        [Test]
        public void FindSkillDayFromBlockUsingTwoDaysOff()
        {
            _scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDayPro3 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDayPro4 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDayProList = new List<IScheduleDayPro> { _scheduleDayPro1, _scheduleDayPro2, _scheduleDayPro3,_scheduleDayPro4  };
            _scheduleDayReadOnlyProList = new ReadOnlyCollection<IScheduleDayPro>(_scheduleDayProList);

            _schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff ;
            _target = new DynamicBlockFinder(_schedulingOptions, _schedulingResultStateHolder,_matrixList );
            var date = new DateOnly(DateTime.UtcNow);

            var scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
            var scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            var scheduleDayList1 = new List<IScheduleDay>() { scheduleDay1 };
            var scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            var scheduleDayList2 = new List<IScheduleDay>() { scheduleDay2 };
            using (_mock.Record())
            {
                Expect.Call(_matrixPro.EffectivePeriodDays).Return(_scheduleDayReadOnlyProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro1.Day).Return(date).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro2.Day).Return(date.AddDays(1)).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro3.Day).Return(date.AddDays(2)).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro4.Day).Return(date.AddDays(3)).Repeat.AtLeastOnce();
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.AtLeastOnce();
                Expect.Call(scheduleDictionary.SchedulesForDay(new DateOnly(date))).Return(new List<IScheduleDay>() );
                Expect.Call(scheduleDictionary.SchedulesForDay(new DateOnly(date.AddDays(1)))).Return(new List<IScheduleDay>() );
                Expect.Call(scheduleDictionary.SchedulesForDay(new DateOnly(date.AddDays(2)))).Return(scheduleDayList1);
                Expect.Call(scheduleDictionary.SchedulesForDay(new DateOnly(date.AddDays(3)))).Return(scheduleDayList2);
                Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
                Expect.Call(scheduleDay2.SignificantPart()).Return(SchedulePartView.DayOff);

                Expect.Call(_matrixPro.Person).Return(_baseLine.Person1);
                Expect.Call(_groupPerson.GroupMembers).Return(_baseLine.ReadOnlyCollectionPersonList);
                Expect.Call(_matrixPro.GetScheduleDayByKey(date)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePart);
                Expect.Call(_schedulePart.IsScheduled()).Return(false);
           }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.ExtractBlockDays(date,_groupPerson ), new List<DateOnly> { new DateOnly(date), new DateOnly(date.AddDays(1)) });
            }
        }

        [Test]
        public void FindSkillDayFromBlockUsingCalendarWeek()
        {
            _schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.Weeks ;
            _target = new DynamicBlockFinder(_schedulingOptions, _schedulingResultStateHolder,_matrixList );
            var startDate = new  DateOnly( new DateTime(2012, 11, 1, 0, 0, 0, DateTimeKind.Utc));
            _scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDayPro3 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDayProList = new List<IScheduleDayPro> { _scheduleDayPro1, _scheduleDayPro2, _scheduleDayPro3 };
            _scheduleDayReadOnlyProList = new ReadOnlyCollection<IScheduleDayPro>(_scheduleDayProList);

            using (_mock.Record())
            {
                Expect.Call(_matrixPro.EffectivePeriodDays).Return(_scheduleDayReadOnlyProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro1.Day).Return(startDate).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro2.Day).Return(startDate.AddDays(1)).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro3.Day).Return(startDate.AddDays(2)).Repeat.AtLeastOnce();

                Expect.Call(_matrixPro.Person).Return(_baseLine.Person1);
                Expect.Call(_groupPerson.GroupMembers).Return(_baseLine.ReadOnlyCollectionPersonList);
                Expect.Call(_matrixPro.GetScheduleDayByKey(startDate)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePart);
                Expect.Call(_schedulePart.IsScheduled()).Return(false);
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.ExtractBlockDays(startDate, _groupPerson  ), new List<DateOnly> { new DateOnly(startDate), new DateOnly(startDate.AddDays(1)) });
            }
        }
        
    }

    
}
