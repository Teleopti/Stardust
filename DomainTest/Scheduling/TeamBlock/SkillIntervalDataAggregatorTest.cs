using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class SkillIntervalDataAggregatorTest
    {
        private MockRepository _mock;
        private ISkillIntervalDataAggregator _target;
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

            using(_mock.Record())
            {
                
            }
            using(_mock.Playback())
            {
                var result = _target.AggregateSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(result.Count,6);
                Assert.AreEqual(result[0].ForecastedDemand,10 );
                Assert.AreEqual(result[0].CurrentDemand ,5 );
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
        public void ShouldAggregateSkillIntervalDataWithHeadcount()
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
                if(result[0].MinimumHeads.HasValue)
                    Assert.AreEqual(result[0].MinimumHeads.Value  , 7);
                Assert.AreEqual(result[0].MaximumHeads , null);

                Assert.AreEqual(result[1].ForecastedDemand, 9);
                Assert.AreEqual(result[1].CurrentDemand, 5);
                Assert.AreEqual(result[1].CurrentHeads , 8);
                if (result[1].MinimumHeads.HasValue)
                    Assert.AreEqual(result[1].MinimumHeads.Value, 6);
                Assert.AreEqual(result[1].MaximumHeads, null);
                
                Assert.AreEqual(result[2].ForecastedDemand, 28);
                Assert.AreEqual(result[2].CurrentDemand, 20);
                Assert.AreEqual(result[2].CurrentHeads , 7);
                if (result[2].MaximumHeads.HasValue)
                    Assert.AreEqual(result[2].MaximumHeads.Value, 23);
                Assert.AreEqual(result[2].MinimumHeads , null);
                
                Assert.AreEqual(result[3].ForecastedDemand, 41);
                Assert.AreEqual(result[3].CurrentDemand, 30);
                Assert.AreEqual(result[3].CurrentHeads , 12);
                if (result[3].MaximumHeads .HasValue)
                    Assert.AreEqual(result[3].MaximumHeads .Value, 11);
                Assert.AreEqual(result[3].MinimumHeads  , null);
                
                Assert.AreEqual(result[4].ForecastedDemand, 11);
                Assert.AreEqual(result[4].CurrentDemand, 5);
                Assert.AreEqual(result[4].CurrentHeads , 3);
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

    }


}
