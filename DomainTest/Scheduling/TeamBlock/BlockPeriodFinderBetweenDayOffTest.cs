using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class BlockPeriodFinderBetweenDayOffTest
    {
        private MockRepository _mocks;
        private IScheduleMatrixPro _matrix;
        private IBlockPeriodFinderBetweenDayOff _target;
	    private IScheduleRange _range;
	    private ISchedulingResultStateHolder _stateHolder;
	    private IScheduleDictionary _scheduleDictionary;
	    private IPerson _person;
	    private IScheduleDay _scheduleDay;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IScheduleDay _scheduleDay3;
	    private IVirtualSchedulePeriod _schedulePeriod;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _target = new BlockPeriodFinderBetweenDayOff();
	        _range = _mocks.StrictMock<IScheduleRange>();
	        _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
	        _scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
	        _person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
	        _scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay3 = _mocks.StrictMock<IScheduleDay>();
	        _schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
        }

		[Test]
		public void ShouldReturnNullIfRequestedDateIsDayOff()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.SchedulingStateHolder).Return(_stateHolder);
				Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_range);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 2))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);
			}

			using (_mocks.Playback())
			{
                var period = _target.GetBlockPeriod(_matrix, new DateOnly(2013, 4, 2), false);
				Assert.IsNull(period);
			}
		}

		[Test]
		public void ShouldReturnNullIfRequestedDateIsContractDayOff()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.SchedulingStateHolder).Return(_stateHolder);
				Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_range);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 2))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.ContractDayOff);
			}

			using (_mocks.Playback())
			{
                var period = _target.GetBlockPeriod(_matrix, new DateOnly(2013, 4, 2), false);
				Assert.IsNull(period);
			}
		}

		[Test]
		public void ShouldNeverTryToCheckOutsideLoadedPeriod()
		{
			//range period is 3 days
			var rangePeriod = new DateTimePeriod(2013, 4, 1, 2013, 4, 3);
			//matrix is 1 day
			var matrixPeriod = new DateOnlyPeriod(2013, 4, 2, 2013, 4, 2);

			using (_mocks.Record())
			{
				commonMocks(rangePeriod, matrixPeriod);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 1))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 2))).Return(_scheduleDay1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.None).Repeat.AtLeastOnce();
				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 3))).Return(_scheduleDay2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.None).Repeat.AtLeastOnce();

			}

			using (_mocks.Playback())
			{
                var period = _target.GetBlockPeriod(_matrix, new DateOnly(2013, 4, 2), false);
				Assert.AreEqual(new DateOnlyPeriod(2013, 4, 1, 2013, 4, 3), period);
			}
		}

	    [Test]
		public void ShouldCheckMaxTenDaysOutsideCurrentMatrix()
		{
			//range period is 365 days
			var rangePeriod = new DateTimePeriod(2013, 1, 1, 2013, 12, 31);
			//matrix is 1 day
			var matrixPeriod = new DateOnlyPeriod(2013, 4, 11, 2013, 4, 11);

		    using (_mocks.Record())
		    {
			    commonMocks(rangePeriod, matrixPeriod);

			    Expect.Call(_range.ScheduledDay(new DateOnly(2013, 1, 1)))
			          .IgnoreArguments()
			          .Return(_scheduleDay)
			          .Repeat.Any();

			    Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None).Repeat.Times(23);
		    }

		    using (_mocks.Playback())
			{
                var period = _target.GetBlockPeriod(_matrix, new DateOnly(2013, 4, 11), false);
				Assert.AreEqual(new DateOnlyPeriod(2013, 4, 1, 2013, 4, 21), period);
			}
		}

		[Test]
		public void ShouldReturnCorrectPeriodBetweenDaysOff()
		{
			//range period is 365 days
			var rangePeriod = new DateTimePeriod(2013, 1, 1, 2013, 12, 31);
			//matrix is 2 days
			var matrixPeriod = new DateOnlyPeriod(2013, 4, 1, 2013, 4, 2);

			using (_mocks.Record())
			{
				commonMocks(rangePeriod, matrixPeriod);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 1))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 1))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);

				//1 is first because of nullcheck
				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 1))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 3, 31))).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 2))).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.DayOff);
			}

			using (_mocks.Playback())
			{
                var period = _target.GetBlockPeriod(_matrix, new DateOnly(2013, 4, 1), false);
				Assert.AreEqual(new DateOnlyPeriod(2013, 4, 1, 2013, 4, 1), period);
			}
		}

		[Test]
		public void ShouldReturnCorrectPeriodIfContractDayOff()
		{
			//range period is 365 days
			var rangePeriod = new DateTimePeriod(2013, 1, 1, 2013, 12, 31);
			//matrix is 5 days
			var matrixPeriod = new DateOnlyPeriod(2013, 4, 1, 2013, 4, 5);

			using (_mocks.Record())
			{
				commonMocks(rangePeriod, matrixPeriod);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 3))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);

				//3 is first because of nullcheck
				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 3))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 2))).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.None);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 1))).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.ContractDayOff);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 3))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 4))).Return(_scheduleDay3);
				Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.ContractDayOff);
			}

			using (_mocks.Playback())
			{
                var period = _target.GetBlockPeriod(_matrix, new DateOnly(2013, 4, 3), false);
				Assert.AreEqual(new DateOnlyPeriod(2013, 4, 2, 2013, 4, 3), period);
			}
		}

        [Test]
        public void AbsenceShouldNotCountAsBlockDelimiterWithAbsence()
        {
            //range period is 365 days
            var rangePeriod = new DateTimePeriod(2013, 1, 1, 2013, 12, 31);
            //matrix is 2 days
            var matrixPeriod = new DateOnlyPeriod(2013, 4, 1, 2013, 4, 2);

            using (_mocks.Record())
            {
                commonMocks(rangePeriod, matrixPeriod);

                //1 is first because of nullcheck
                Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 1))).Return(_scheduleDay).Repeat.Times(3);
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.Absence).Repeat.AtLeastOnce();

                Expect.Call(_range.ScheduledDay(new DateOnly(2013, 3, 31))).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);

                Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 2))).Return(_scheduleDay2);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.DayOff);
            }

            using (_mocks.Playback())
            {
                var period = _target.GetBlockPeriod(_matrix, new DateOnly(2013, 4, 1), false);
                Assert.AreEqual(new DateOnlyPeriod(2013, 4, 1, 2013, 4, 1), period);
            }
        }

        [Test]
        public void AbsenceShouldNotCountAsBlockDelimiterWithFullDayAbsence()
        {
            //range period is 365 days
            var rangePeriod = new DateTimePeriod(2013, 1, 1, 2013, 12, 31);
            //matrix is 2 days
            var matrixPeriod = new DateOnlyPeriod(2013, 4, 1, 2013, 4, 2);

            using (_mocks.Record())
            {
                commonMocks(rangePeriod, matrixPeriod);

                //1 is first because of nullcheck
                Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 1))).Return(_scheduleDay).Repeat.Times(3);
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.AtLeastOnce();

                Expect.Call(_range.ScheduledDay(new DateOnly(2013, 3, 31))).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);

                Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 2))).Return(_scheduleDay2);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.DayOff);
            }

            using (_mocks.Playback())
            {
                var period = _target.GetBlockPeriod(_matrix, new DateOnly(2013, 4, 1), false);
                Assert.AreEqual(new DateOnlyPeriod(2013, 4, 1, 2013, 4, 1), period);
            }
        }

		private void commonMocks(DateTimePeriod rangePeriod, DateOnlyPeriod matrixPeriod)
		{
			Expect.Call(_matrix.Person).Return(_person);
			Expect.Call(_matrix.SchedulingStateHolder).Return(_stateHolder);
			Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
			Expect.Call(_scheduleDictionary[_person]).Return(_range);
			Expect.Call(_range.Period).Return(rangePeriod);
			Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
			Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(matrixPeriod);
		}

        

    }

   
}
