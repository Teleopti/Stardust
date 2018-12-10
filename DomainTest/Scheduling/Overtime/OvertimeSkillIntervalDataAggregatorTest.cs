using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Overtime;


namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class OvertimeSkillIntervalDataAggregatorTest
    {
        private MockRepository _mock;
        private IOvertimeSkillIntervalDataAggregator _target;
        private IList<IList<IOvertimeSkillIntervalData>> _multipleSkillIntervalDataList;
        private IOvertimeSkillIntervalData _skillAIntervalData1;
        private IOvertimeSkillIntervalData _skillAIntervalData2;
        private IOvertimeSkillIntervalData _skillAIntervalData3;
        private IList<IOvertimeSkillIntervalData> _skillAIntervalDataList;
        private IOvertimeSkillIntervalData _skillAIntervalData4;

        private IOvertimeSkillIntervalData _skillBIntervalData1;
        private IOvertimeSkillIntervalData _skillBIntervalData2;
        private IOvertimeSkillIntervalData _skillBIntervalData3;
        private IList<IOvertimeSkillIntervalData> _skillBIntervalDataList;
        private IOvertimeSkillIntervalData _skillBIntervalData4;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new OvertimeSkillIntervalDataAggregator();
        }



        [Test]
        public void ShouldAggregateForecastedDemand()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 8, 20);
            _skillAIntervalData2 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 9, 8);
            _skillAIntervalData3 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 21, 15);
            _skillAIntervalData4 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 22, 11);

            _skillAIntervalDataList = new List<IOvertimeSkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3, _skillAIntervalData4 };


            //for skill B
            _skillBIntervalData1 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 7, 5);
            _skillBIntervalData2 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 19, 6);
            _skillBIntervalData3 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 11, 8);
            _skillBIntervalData4 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 23, 19);

            _skillBIntervalDataList = new List<IOvertimeSkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };


            _multipleSkillIntervalDataList = new List<IList<IOvertimeSkillIntervalData>> { _skillAIntervalDataList, _skillBIntervalDataList };
            using (_mock.Record())
            {

            }
            using (_mock.Playback())
            {
                var result = _target.AggregateOvertimeSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(result.Count, 6);
                Assert.AreEqual(8,result[0].ForecastedDemand);
                Assert.AreEqual(9,result[1].ForecastedDemand);
                Assert.AreEqual(28,result[2].ForecastedDemand);
                Assert.AreEqual(41,result[3].ForecastedDemand);
                Assert.AreEqual(11,result[4].ForecastedDemand);
                Assert.AreEqual(23,result[5].ForecastedDemand);

            }
        }

        [Test]
        public void ShouldAggregateCurrentDemand()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 2, 5);
            _skillAIntervalData2 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 3, 5);
            _skillAIntervalData3 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 10, 15);
            _skillAIntervalData4 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 9, 15);

            _skillAIntervalDataList = new List<IOvertimeSkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3, _skillAIntervalData4 };


            //for skill B
            _skillBIntervalData1 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 7, 5);
            _skillBIntervalData2 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 19, 15);
            _skillBIntervalData3 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 11, 5);
            _skillBIntervalData4 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 23, 15);

            _skillBIntervalDataList = new List<IOvertimeSkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };

            _multipleSkillIntervalDataList = new List<IList<IOvertimeSkillIntervalData>> { _skillAIntervalDataList, _skillBIntervalDataList };

            using (_mock.Playback())
            {
                var result = _target.AggregateOvertimeSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(result.Count, 6);
                Assert.AreEqual(result[0].CurrentDemand, 5);
                Assert.AreEqual(result[1].CurrentDemand, 5);
                Assert.AreEqual(result[2].CurrentDemand, 20);
                Assert.AreEqual(result[3].CurrentDemand, 30);
                Assert.AreEqual(result[4].CurrentDemand, 5);
                Assert.AreEqual(result[5].CurrentDemand, 15);

            }
        }

        [Test]
        public void ShouldAggregateSkillIntervalDataWithDemands()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 10, 5);
            _skillAIntervalData2 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 19, 5);
            _skillAIntervalData3 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 2, 5);
            _skillAIntervalData4 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 2, 1);

            _skillAIntervalDataList = new List<IOvertimeSkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3, _skillAIntervalData4 };


            //for skill B
            _skillBIntervalData1 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 1, 5);
            _skillBIntervalData2 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 3, 15);
            _skillBIntervalData3 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 1, 8);
            _skillBIntervalData4 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 2, 0);

            _skillBIntervalDataList = new List<IOvertimeSkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };

            _multipleSkillIntervalDataList = new List<IList<IOvertimeSkillIntervalData>> { _skillAIntervalDataList, _skillBIntervalDataList };

            using (_mock.Playback())
            {
                var result = _target.AggregateOvertimeSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(result.Count, 6);
                Assert.AreEqual(result[0].ForecastedDemand, 10);
                Assert.AreEqual(result[0].CurrentDemand, 5);
                Assert.AreEqual(result[1].ForecastedDemand, 19);
                Assert.AreEqual(result[1].CurrentDemand, 5);
                Assert.AreEqual(result[2].ForecastedDemand, 3);
                Assert.AreEqual(result[2].CurrentDemand, 10);
                Assert.AreEqual(result[3].ForecastedDemand, 5);
                Assert.AreEqual(result[3].CurrentDemand, 16);
                Assert.AreEqual(result[4].ForecastedDemand, 1);
                Assert.AreEqual(result[4].CurrentDemand, 8);
                Assert.AreEqual(result[5].ForecastedDemand, 2);
                Assert.AreEqual(result[5].CurrentDemand, 0);
            }
        }

        [Test]
        public void ShouldAggregateAllSkillIntervalData()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 8, 5);
            _skillAIntervalData2 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 9, 5);
            _skillAIntervalData3 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 21, 15);
            _skillAIntervalData4 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 7, 15);

            _skillAIntervalDataList = new List<IOvertimeSkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3, _skillAIntervalData4 };


            //for skill B
            _skillBIntervalData1 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 7, 5);
            _skillBIntervalData2 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 19, 15);
            _skillBIntervalData3 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 11, 5);
            _skillBIntervalData4 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 23, 15);

            _skillBIntervalDataList = new List<IOvertimeSkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };

            _multipleSkillIntervalDataList = new List<IList<IOvertimeSkillIntervalData>> { _skillAIntervalDataList, _skillBIntervalDataList };
            using (_mock.Record())
            {

            }
            using (_mock.Playback())
            {
                var result = _target.AggregateOvertimeSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(result.Count, 6);
                Assert.AreEqual(result[0].ForecastedDemand, 8);
                Assert.AreEqual(result[0].CurrentDemand, 5);

                Assert.AreEqual(result[1].ForecastedDemand, 9);
                Assert.AreEqual(result[1].CurrentDemand, 5);

                Assert.AreEqual(result[2].ForecastedDemand, 28);
                Assert.AreEqual(result[2].CurrentDemand, 20);

                Assert.AreEqual(result[3].ForecastedDemand, 26);
                Assert.AreEqual(result[3].CurrentDemand, 30);

                Assert.AreEqual(result[4].ForecastedDemand, 11);
                Assert.AreEqual(result[4].CurrentDemand, 5);

                Assert.AreEqual(result[5].ForecastedDemand, 23);
                Assert.AreEqual(result[5].CurrentDemand, 15);


            }
        }


       [Test]
        public void VerifyTwoIntervalsAreaggregated()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();

            _skillAIntervalData1 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 3, 5);
            _skillAIntervalData2 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 6, 9);
            var result = _target.AggregateTwoIntervals(_skillAIntervalData1, _skillAIntervalData2);
            Assert.AreEqual(9, result.ForecastedDemand);
            Assert.AreEqual(14, result.CurrentDemand );
            _skillAIntervalData3 = new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 2, 3);
            result = _target.AggregateTwoIntervals(result, _skillAIntervalData3);
            Assert.AreEqual(11, result.ForecastedDemand );
            Assert.AreEqual(17, result.CurrentDemand  );
        }
    }

    
}
