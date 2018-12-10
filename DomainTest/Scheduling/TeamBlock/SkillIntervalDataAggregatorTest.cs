using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class SkillIntervalDataAggregatorTest
    {
        private MockRepository _mock;
        private SkillIntervalDataAggregator _target;
        private IList<IList<ISkillIntervalData>> _multipleSkillIntervalDataList;
        private ISkillIntervalData _skillAIntervalData1;
        private ISkillIntervalData _skillAIntervalData2;
        private ISkillIntervalData _skillAIntervalData3;
        private IList<ISkillIntervalData> _skillAIntervalDataList;
        private ISkillIntervalData _skillAIntervalData4;

        private ISkillIntervalData _skillBIntervalData1;
        private ISkillIntervalData _skillBIntervalData2;
        private ISkillIntervalData _skillBIntervalData3;
        private IList<ISkillIntervalData> _skillBIntervalDataList;
        private ISkillIntervalData _skillBIntervalData4;

        [SetUp ]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new SkillIntervalDataAggregator();
        }

        

        [Test]
        public void ShouldAggregateForecastedDemand()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 8, 0, 0, 0, null);
            _skillAIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 9, 0, 0, 0, null);
            _skillAIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 21, 0, 0, null, null);
            _skillAIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 22, 0, 0, null, null);

            _skillAIntervalDataList = new List<ISkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3, _skillAIntervalData4 };


            //for skill B
            _skillBIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 7, 0, 0, 0, null);
            _skillBIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 19, 0, 0, 0, null);
            _skillBIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 11, 0, 0, 0, null);
            _skillBIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 23, 0, 0, 0, null);

            _skillBIntervalDataList = new List<ISkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };


            _multipleSkillIntervalDataList = new List<IList<ISkillIntervalData>> { _skillAIntervalDataList, _skillBIntervalDataList };
            using (_mock.Record())
            {

            }
            using (_mock.Playback())
            {
                var result = _target.AggregateSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(result.Count, 6);
                Assert.AreEqual(result[0].ForecastedDemand, 8);
                Assert.AreEqual(result[1].ForecastedDemand, 9);
                Assert.AreEqual(result[2].ForecastedDemand, 28);
                Assert.AreEqual(result[3].ForecastedDemand, 41);
                Assert.AreEqual(result[4].ForecastedDemand, 11);
                Assert.AreEqual(result[5].ForecastedDemand, 23);
                
            }
        }

        [Test]
        public void ShouldAggregateCurrentDemand()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 0,5, 0, 0, null);
            _skillAIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 0, 5, 0, 0, null);
            _skillAIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 0, 15, 0, null, null);
            _skillAIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 0, 15, 0, null, null);

            _skillAIntervalDataList = new List<ISkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3, _skillAIntervalData4 };


            //for skill B
            _skillBIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 7, 5, 0, 0, null);
            _skillBIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 19,15, 0, 0, null);
            _skillBIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 11, 5, 0, 0, null);
            _skillBIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 23, 15, 0, 0, null);

            _skillBIntervalDataList = new List<ISkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };

            _multipleSkillIntervalDataList = new List<IList<ISkillIntervalData>> { _skillAIntervalDataList, _skillBIntervalDataList };

            using (_mock.Playback())
            {
                var result = _target.AggregateSkillIntervalData(_multipleSkillIntervalDataList);
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
        public void ShouldAggregateCurrentHeads()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 0, 0, 2, 0, null);
            _skillAIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 0, 0, 8, 0, null);
            _skillAIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 0, 0, 6, null, null);
            _skillAIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 0, 0, 7, null, null);

            _skillAIntervalDataList = new List<ISkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3, _skillAIntervalData4 };


            //for skill B
            _skillBIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 0, 0, 1, 0, null);
            _skillBIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 0, 0, 5, 0, null);
            _skillBIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 0, 0, 3, 0, null);
            _skillBIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 0, 0, 9, 0, null);

            _skillBIntervalDataList = new List<ISkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };

            _multipleSkillIntervalDataList = new List<IList<ISkillIntervalData>> { _skillAIntervalDataList, _skillBIntervalDataList };
            using (_mock.Record())
            {

            }
            using (_mock.Playback())
            {
                var result = _target.AggregateSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(result.Count, 6);
                Assert.AreEqual(result[0].CurrentHeads, 2);
                Assert.AreEqual(result[1].CurrentHeads, 8);
                Assert.AreEqual(result[2].CurrentHeads, 7);
                Assert.AreEqual(result[3].CurrentHeads, 12);
                Assert.AreEqual(result[4].CurrentHeads, 3);
                Assert.AreEqual(result[5].CurrentHeads, 9);
                
            }
        }

        [Test]
        public void ShouldAggregateMinimumNullHeads()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 0, 0, 0, null, null);
            _skillAIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 0, 0, 0, null, null);
            _skillAIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 0, 0, 0, null, null);
            _skillAIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 0, 0, 0, null, null);

            _skillAIntervalDataList = new List<ISkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3, _skillAIntervalData4 };


            //for skill B
            _skillBIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 0, 0, 0, null, null);
            _skillBIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 0, 0, 0, null, null);
            _skillBIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 0, 0, 0, null, null);
            _skillBIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 0, 0, 0, null, null);

            _skillBIntervalDataList = new List<ISkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };

            _multipleSkillIntervalDataList = new List<IList<ISkillIntervalData>> { _skillAIntervalDataList, _skillBIntervalDataList };

            using (_mock.Playback())
            {
                var result = _target.AggregateSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(result.Count, 6);
                foreach (var aggInt in result)
                {
                    Assert.IsNull(aggInt.MinimumHeads);
                }

            }
        }

        [Test]
        public void ShouldAggregateMaximumNullHeads()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 0, 0, 0, null, null);
            _skillAIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 0, 0, 0, null, null);
            _skillAIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 0, 0, 0, null, null);
            _skillAIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 0, 0, 0, null, null);

            _skillAIntervalDataList = new List<ISkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3, _skillAIntervalData4 };


            //for skill B
            _skillBIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 0, 0, 0, null, null);
            _skillBIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 0, 0, 0, null, null);
            _skillBIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 0, 0, 0, null, null);
            _skillBIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 0, 0, 0, null, null);

            _skillBIntervalDataList = new List<ISkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };

            _multipleSkillIntervalDataList = new List<IList<ISkillIntervalData>> { _skillAIntervalDataList, _skillBIntervalDataList };

            using (_mock.Playback())
            {
                var result = _target.AggregateSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(result.Count, 6);
                foreach (var aggInt in result)
                {
                    Assert.IsNull(aggInt.MaximumHeads );
                }

            }
        }

        [Test]
        public void AggregateWithAllMinimumValues()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 0, 0, 0, 7, null);
            _skillAIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 0, 0, 0, 6, null);
            _skillAIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 0, 0, 0, 2, null);
            _skillAIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 0, 0, 0, 5, null);

            _skillAIntervalDataList = new List<ISkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3, _skillAIntervalData4 };


            //for skill B
            _skillBIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 0, 0, 0, 3, null);
            _skillBIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 0, 0, 0, 2, null);
            _skillBIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 0, 0, 0, 5, null);
            _skillBIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 0, 0, 0, 1, null);

            _skillBIntervalDataList = new List<ISkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };

            _multipleSkillIntervalDataList = new List<IList<ISkillIntervalData>> { _skillAIntervalDataList, _skillBIntervalDataList };
            using (_mock.Record())
            {

            }
            using (_mock.Playback())
            {
                var result = _target.AggregateSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(6, result.Count);
                Assert.AreEqual(7, result[0].MinimumHeads.GetValueOrDefault());
				Assert.AreEqual(6, result[1].MinimumHeads.GetValueOrDefault());
                Assert.IsFalse(result[2].MinimumHeads.HasValue);
				Assert.IsFalse(result[3].MinimumHeads.HasValue);
				Assert.AreEqual(5, result[4].MinimumHeads.GetValueOrDefault());
				Assert.AreEqual(1, result[5].MinimumHeads.GetValueOrDefault());
                

            }
        }

        [Test]
        public void AggregateWithAllMaximumValues()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 0, 0, 0,null, 7);
            _skillAIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 0, 0, 0, null,6);
            _skillAIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 0, 0, 0, null,2);
            _skillAIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 0, 0, 0, null,5);

            _skillAIntervalDataList = new List<ISkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3, _skillAIntervalData4 };


            //for skill B
            _skillBIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 0, 0, 0, null,3);
            _skillBIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 0, 0, 0, null,2);
            _skillBIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 0, 0, 0, null,5);
            _skillBIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 0, 0, 0, null,1);

            _skillBIntervalDataList = new List<ISkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };

            _multipleSkillIntervalDataList = new List<IList<ISkillIntervalData>> { _skillAIntervalDataList, _skillBIntervalDataList };
            using (_mock.Record())
            {

            }
            using (_mock.Playback())
            {
                var result = _target.AggregateSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(6, result.Count);
                Assert.AreEqual(7, result[0].MaximumHeads.GetValueOrDefault());
                Assert.AreEqual(6, result[1].MaximumHeads.GetValueOrDefault());
                Assert.IsFalse(result[2].MaximumHeads.HasValue);
                Assert.IsFalse(result[3].MaximumHeads.HasValue);
				Assert.AreEqual(5, result[4].MaximumHeads.GetValueOrDefault());
				Assert.AreEqual(1, result[5].MaximumHeads.GetValueOrDefault());
            }
        }
       
        [Test]
        public void ShouldAggregateSkillIntervalDataWithDemands()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 10, 5, 0, null, null);
            _skillAIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 10, 5, 0, null, null);
            _skillAIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 20, 15, 0, null, null);
            _skillAIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 20, 15, 0, null, null);

            _skillAIntervalDataList = new List<ISkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3, _skillAIntervalData4 };


            //for skill B
            _skillBIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 10, 5, 0, null, null);
            _skillBIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 20, 15, 0, null, null);
            _skillBIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 10, 5, 0, null, null);
            _skillBIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 20, 15, 0, null, null);

            _skillBIntervalDataList = new List<ISkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };

            _multipleSkillIntervalDataList = new List<IList<ISkillIntervalData>> { _skillAIntervalDataList, _skillBIntervalDataList };

            using (_mock.Playback())
            {
                var result = _target.AggregateSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(result.Count, 6);
                Assert.AreEqual(result[0].ForecastedDemand, 10);
                Assert.AreEqual(result[0].CurrentDemand, 5);
                Assert.AreEqual(result[1].ForecastedDemand, 10);
                Assert.AreEqual(result[1].CurrentDemand, 5);
                Assert.AreEqual(result[2].ForecastedDemand, 30);
                Assert.AreEqual(result[2].CurrentDemand, 20);
                Assert.AreEqual(result[3].ForecastedDemand, 40);
                Assert.AreEqual(result[3].CurrentDemand, 30);
                Assert.AreEqual(result[4].ForecastedDemand, 10);
                Assert.AreEqual(result[4].CurrentDemand, 5);
                Assert.AreEqual(result[5].ForecastedDemand, 20);
                Assert.AreEqual(result[5].CurrentDemand, 15);
            }
        }

        [Test]
        public void ShouldAggregateAllSkillIntervalData()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 8, 5, 2, 7, null);
            _skillAIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 9, 5, 8, 6, null);
            _skillAIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 21, 15, 6, null, 2);
            _skillAIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 22, 15, 7, null, 5);

            _skillAIntervalDataList = new List<ISkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3, _skillAIntervalData4 };


            //for skill B
            _skillBIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 7, 5, 1, null, 21);
            _skillBIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 19, 15, 5, null, 6);
            _skillBIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 11, 5, 3, 15, null);
            _skillBIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 23, 15, 9, 18, null);

            _skillBIntervalDataList = new List<ISkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };

            _multipleSkillIntervalDataList = new List<IList<ISkillIntervalData>> { _skillAIntervalDataList, _skillBIntervalDataList };
            using (_mock.Record())
            {

            }
            using (_mock.Playback())
            {
                var result = _target.AggregateSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(result.Count, 6);
                Assert.AreEqual(result[0].ForecastedDemand, 8);
                Assert.AreEqual(result[0].CurrentDemand, 5);
                Assert.AreEqual(result[0].CurrentHeads, 2);
                if (result[0].MinimumHeads.HasValue)
                    Assert.AreEqual(result[0].MinimumHeads.Value, 7);
                Assert.AreEqual(result[0].MaximumHeads, null);

                Assert.AreEqual(result[1].ForecastedDemand, 9);
                Assert.AreEqual(result[1].CurrentDemand, 5);
                Assert.AreEqual(result[1].CurrentHeads, 8);
                if (result[1].MinimumHeads.HasValue)
                    Assert.AreEqual(result[1].MinimumHeads.Value, 6);
                Assert.AreEqual(result[1].MaximumHeads, null);

                Assert.AreEqual(result[2].ForecastedDemand, 28);
                Assert.AreEqual(result[2].CurrentDemand, 20);
                Assert.AreEqual(result[2].CurrentHeads, 7);
                if (result[2].MaximumHeads.HasValue)
                    Assert.AreEqual(result[2].MaximumHeads.Value, 23);
                Assert.AreEqual(result[2].MinimumHeads, null);

                Assert.AreEqual(result[3].ForecastedDemand, 41);
                Assert.AreEqual(result[3].CurrentDemand, 30);
                Assert.AreEqual(result[3].CurrentHeads, 12);
                if (result[3].MaximumHeads.HasValue)
                    Assert.AreEqual(result[3].MaximumHeads.Value, 11);
                Assert.AreEqual(result[3].MinimumHeads, null);

                Assert.AreEqual(result[4].ForecastedDemand, 11);
                Assert.AreEqual(result[4].CurrentDemand, 5);
                Assert.AreEqual(result[4].CurrentHeads, 3);
                if (result[4].MinimumHeads.HasValue)
                    Assert.AreEqual(result[4].MinimumHeads.Value, 15);
                Assert.AreEqual(result[4].MaximumHeads, null);

                Assert.AreEqual(result[5].ForecastedDemand, 23);
                Assert.AreEqual(result[5].CurrentDemand, 15);
                Assert.AreEqual(result[5].CurrentHeads, 9);
                if (result[5].MinimumHeads.HasValue)
                    Assert.AreEqual(result[5].MinimumHeads.Value, 18);
                Assert.AreEqual(result[5].MaximumHeads, null);


            }
        }


        [Test]
        public void ShouldAggregateBoostedValueCorrectly()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 10, 5, 3, 0, 1); //bosted value should b -3
            _skillAIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 10, 5, 1, 2, 4); //bosted value should b 1
            _skillAIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 20, 15, 4, 2, 4); //bosted value should b -1
            _skillAIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 20, 15, 0, 2, 4); //bosted value should b 2

            _skillAIntervalDataList = new List<ISkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3, _skillAIntervalData4 };


            //for skill B
            _skillBIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 10, 5, 6, null, 5); //bosted value should b -2
            _skillBIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 20, 15, 6, null, 5);//bosted value should b -2
            _skillBIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 10, 5, 6, null, 5);//bosted value should b -2
            _skillBIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 20, 15, 0, 2, 4);//bosted value should b 2

            _skillBIntervalDataList = new List<ISkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };

            //for skill c
            var skillCIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 10, 5, 0, null, null);//bosted value should b 0
            var skillCIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 20, 15, 1, 0, 1);//bosted value should b -1
            var skillCIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 10, 5, 0, 2, 4);//bosted value should b 2
            var skillCIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 20, 15, 1, 2, 2);//bosted value should b 1

            var skillCIntervalDataList = new List<ISkillIntervalData> { skillCIntervalData1, skillCIntervalData2, skillCIntervalData3, skillCIntervalData4 };

            _multipleSkillIntervalDataList = new List<IList<ISkillIntervalData>> { _skillAIntervalDataList, _skillBIntervalDataList,skillCIntervalDataList  };

            using (_mock.Playback())
            {
                var result = _target.AggregateSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(6, result.Count);
                Assert.AreEqual(-3, result[0].MinMaxBoostFactor);
				Assert.AreEqual(1, result[1].MinMaxBoostFactor);
				Assert.AreEqual(-3, result[2].MinMaxBoostFactor);
				Assert.AreEqual(-1, result[3].MinMaxBoostFactor);
				Assert.AreEqual(0, result[4].MinMaxBoostFactor);
				Assert.AreEqual(3, result[5].MinMaxBoostFactor);

				Assert.AreEqual(-2, result[0].MinMaxBoostFactorForStandardDeviation);
				Assert.AreEqual(1, result[1].MinMaxBoostFactorForStandardDeviation);
				Assert.AreEqual(-1, result[2].MinMaxBoostFactorForStandardDeviation);
				Assert.AreEqual(1, result[3].MinMaxBoostFactorForStandardDeviation);
				Assert.AreEqual(1, result[4].MinMaxBoostFactorForStandardDeviation);
				Assert.AreEqual(3, result[5].MinMaxBoostFactorForStandardDeviation);
            }
        }

		[Test]
		public void VerifyTheExcelSheet()
		{
			var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
			
			_skillAIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 0, 0, 1, 2, 4);
			_skillAIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 0, 0, 6, null, 5);
			var result = _target.AggregateTwoIntervals(_skillAIntervalData1, _skillAIntervalData2);
			Assert.AreEqual(-1, result.MinMaxBoostFactor);
			_skillAIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 0, 0, 1, 2, 2);
			result = _target.AggregateTwoIntervals(result, _skillAIntervalData3);
			Assert.AreEqual(0, result.MinMaxBoostFactor);
		}
    }


}
