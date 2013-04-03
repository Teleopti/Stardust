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
	        _person = PersonFactory.CreatePerson();
	        _scheduleDay = _mocks.StrictMock<IScheduleDay>();
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
				var period = _target.GetBlockPeriod(_matrix, new DateOnly(2013, 4, 2));
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
				var period = _target.GetBlockPeriod(_matrix, new DateOnly(2013, 4, 2));
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

				for (int i = 0; i < 3; i++)
				{
					Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 1).AddDays(i))).Return(_scheduleDay);
					Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);
				}	
			}

			using (_mocks.Playback())
			{
				var period = _target.GetBlockPeriod(_matrix, new DateOnly(2013, 4, 2));
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

				for (int i = 0; i < 365; i++)
				{
					Expect.Call(_range.ScheduledDay(new DateOnly(2013, 1, 1).AddDays(i))).Return(_scheduleDay).Repeat.Any();
				}
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None).Repeat.Times(21);
			}

			using (_mocks.Playback())
			{
				var period = _target.GetBlockPeriod(_matrix, new DateOnly(2013, 4, 11));
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

				//1 is first because of nullcheck
				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 1))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 3, 31))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 2))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);
			}

			using (_mocks.Playback())
			{
				var period = _target.GetBlockPeriod(_matrix, new DateOnly(2013, 4, 1));
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

				//3 is first because of nullcheck
				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 3))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 2))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.ContractDayOff);

				Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 4))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.ContractDayOff);
			}

			using (_mocks.Playback())
			{
				var period = _target.GetBlockPeriod(_matrix, new DateOnly(2013, 4, 3));
				Assert.AreEqual(new DateOnlyPeriod(2013, 4, 3, 2013, 4, 3), period);
			}
		}

		[Test]
		public void AbsenceShouldNotCountAsBlockDelimiterWhenUsingAnythingButSingleAgentTeam()
		{
			Assert.Fail("Not implemented");
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

        [Test]
        public void ShouldReturnTheLastDateOfRangeFromRightSide()
        {
            //period was 2013-03-31 to 2013-04-02
            //2013-04-04 and 2013-03-31 is DO should return from 01 to 03
            DateOnly dateOnly = new DateOnly(2013, 04, 01);
            IVirtualSchedulePeriod virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            IScheduleDayPro scheduleDayProToday = _mocks.StrictMock<IScheduleDayPro>();
            IScheduleDay scheduleDayToday = _mocks.StrictMock<IScheduleDay>();

            using (_mocks.Record())
            {

                Expect.Call(_matrix.GetScheduleDayByKey(dateOnly)).Return(scheduleDayProToday);
                Expect.Call(scheduleDayProToday.DaySchedulePart()).Return(scheduleDayToday);
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.MainShift);

                Expect.Call(_matrix.GetScheduleDayByKey(dateOnly.AddDays(-1))).Return(scheduleDayProToday);
                Expect.Call(scheduleDayProToday.DaySchedulePart()).Return(scheduleDayToday);
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.DayOff);

                Expect.Call(_matrix.SchedulePeriod).Return(virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(dateOnly.AddDays(-1), dateOnly.AddDays(1))).Repeat.AtLeastOnce();

                Expect.Call(_matrix.GetScheduleDayByKey(dateOnly.AddDays(1))).Return(scheduleDayProToday);
                Expect.Call(scheduleDayProToday.DaySchedulePart()).Return(scheduleDayToday);
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.MainShift);

                Expect.Call(_matrix.GetScheduleDayByKey(dateOnly.AddDays(2))).Return(scheduleDayProToday);
                Expect.Call(scheduleDayProToday.DaySchedulePart()).Return(scheduleDayToday);
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.None);


            }
            using (_mocks.Playback())
            {
                var period = _target.GetBlockPeriod(_matrix, dateOnly);
                if (period.HasValue)
                {
                    Assert.AreEqual(period.Value.StartDate, dateOnly);
                    Assert.AreEqual(period.Value.EndDate, dateOnly.AddDays(1));
                }
                else
                {
                    Assert.Fail();
                }

            }
        }

    }

   
}
