using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class BlockPeriodFinderBetweenDayOffTest
    {
        private MockRepository _mocks;
        private IScheduleMatrixPro _matrix;
        private IBlockPeriodFinderBetweenDayOff _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _target = new BlockPeriodFinderBetweenDayOff();
            
        }

        [Test]
        public void ShouldReturnBlockBetweenDayOffInsideMatrix()
        {
            //period was 2013-03-31 to 2013-04-03
            //2013-04-03 and 2013-03-31 is DO should return from 01 to 02
            DateOnly dateOnly = new DateOnly(2013,04,01);
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
                      .Return(new DateOnlyPeriod(dateOnly.AddDays(-1), dateOnly.AddDays(2))).Repeat.AtLeastOnce();

                Expect.Call(_matrix.GetScheduleDayByKey(dateOnly.AddDays(1))).Return(scheduleDayProToday);
                Expect.Call(scheduleDayProToday.DaySchedulePart()).Return(scheduleDayToday);
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.MainShift );

                Expect.Call(_matrix.GetScheduleDayByKey(dateOnly.AddDays(2))).Return(scheduleDayProToday);
                Expect.Call(scheduleDayProToday.DaySchedulePart()).Return(scheduleDayToday);
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.DayOff);
                

            }
            using (_mocks.Playback())
            {
                var period = _target.GetBlockPeriod(_matrix,dateOnly);
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

        [Test]
        public void ShouldReturnBlockBetweenDayOffOuterMatrixRightSide()
        {
            //period was 2013-03-31 to 2013-04-04
            //should return from 01 to 02
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
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.MainShift );

                Expect.Call(_matrix.GetScheduleDayByKey(dateOnly.AddDays(3))).Return(scheduleDayProToday);
                Expect.Call(scheduleDayProToday.DaySchedulePart()).Return(scheduleDayToday);
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.DayOff);


            }
            using (_mocks.Playback())
            {
                var period = _target.GetBlockPeriod(_matrix, dateOnly);
                if (period.HasValue)
                {
                    Assert.AreEqual(period.Value.StartDate, dateOnly);
                    Assert.AreEqual(period.Value.EndDate, dateOnly.AddDays(2));
                }
                else
                {
                    Assert.Fail();
                }

            }
        }

        [Test]
        public void ShouldReturnBlockBetweenDayOffInsideMatrixWithContractDayOff()
        {
            //period was 2013-03-31 to 2013-04-03
            //2013-04-03 and 2013-03-31 is DO should return from 01 to 02
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
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.ContractDayOff );

                Expect.Call(_matrix.SchedulePeriod).Return(virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(dateOnly.AddDays(-1), dateOnly.AddDays(2))).Repeat.AtLeastOnce();

                Expect.Call(_matrix.GetScheduleDayByKey(dateOnly.AddDays(1))).Return(scheduleDayProToday);
                Expect.Call(scheduleDayProToday.DaySchedulePart()).Return(scheduleDayToday);
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.MainShift);

                Expect.Call(_matrix.GetScheduleDayByKey(dateOnly.AddDays(2))).Return(scheduleDayProToday);
                Expect.Call(scheduleDayProToday.DaySchedulePart()).Return(scheduleDayToday);
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.ContractDayOff );


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

        [Test]
        public void ShouldReturnBlockBetweenDayOffOutsideMatrixLeftSide()
        {
            //period was 2013-03-30 to 2013-04-03
            //2013-04-03 and 2013-03-31 is DO should return from 01 to 02
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
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.MainShift);

                Expect.Call(_matrix.GetScheduleDayByKey(dateOnly.AddDays(-2))).Return(scheduleDayProToday);
                Expect.Call(scheduleDayProToday.DaySchedulePart()).Return(scheduleDayToday);
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.DayOff);

                Expect.Call(_matrix.SchedulePeriod).Return(virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(dateOnly, dateOnly.AddDays(2))).Repeat.AtLeastOnce();
                
                Expect.Call(_matrix.GetScheduleDayByKey(dateOnly)).Return(scheduleDayProToday);
                Expect.Call(scheduleDayProToday.DaySchedulePart()).Return(scheduleDayToday);
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.MainShift);

                Expect.Call(_matrix.GetScheduleDayByKey(dateOnly.AddDays(1))).Return(scheduleDayProToday);
                Expect.Call(scheduleDayProToday.DaySchedulePart()).Return(scheduleDayToday);
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.MainShift);

                Expect.Call(_matrix.GetScheduleDayByKey(dateOnly.AddDays(2))).Return(scheduleDayProToday);
                Expect.Call(scheduleDayProToday.DaySchedulePart()).Return(scheduleDayToday);
                Expect.Call(scheduleDayToday.SignificantPart()).Return(SchedulePartView.DayOff);


            }
            using (_mocks.Playback())
            {
                var period = _target.GetBlockPeriod(_matrix, dateOnly);
                if (period.HasValue)
                {
                    Assert.AreEqual(period.Value.StartDate, dateOnly.AddDays(-1));
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
