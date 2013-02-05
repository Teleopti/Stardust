using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
    [TestFixture]
    public class SkillIntervalDataAggregatorTest
    {
        private MockRepository _mock;
        private ISkillIntervalDataAggregator _target;
        private IList<IList< ISkillIntervalData>> _multipleSkillIntervalDataList;
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
        public void SetUp()
        {
            _mock = new MockRepository();
            _target = new SkillIntervalDataAggregator();
        }

        [Test]
        public void ShouldAggregateSkillIntervalData()
        {
            SetUpIntervalList();
            using(_mock.Record())
            {
                
            }
            using(_mock.Playback())
            {
                var result = _target.AggregateSkillIntervalData(_multipleSkillIntervalDataList);
                Assert.AreEqual(result[0].ForecastedDemand,10 );
                Assert.AreEqual(result[1].ForecastedDemand, 10);
                Assert.AreEqual(result[2].ForecastedDemand, 30);
                Assert.AreEqual(result[3].ForecastedDemand, 40);
                Assert.AreEqual(result[4].ForecastedDemand, 10);
                Assert.AreEqual(result[5].ForecastedDemand, 20);
            }
        }

        private void SetUpIntervalList()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            //for skill A
            _skillAIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(15)), 10, 5, 0, null, null);
            _skillAIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(15), startDateTime.AddMinutes(30)), 10, 5, 0, null, null);
            _skillAIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 20, 15, 0, null, null);
            _skillAIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 20, 15, 0, null, null);

            _skillAIntervalDataList = new List<ISkillIntervalData> { _skillAIntervalData1, _skillAIntervalData2, _skillAIntervalData3,_skillAIntervalData4  };


            //for skill B
            _skillBIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(30), startDateTime.AddMinutes(45)), 10, 5, 0, null, null);
            _skillBIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(45), startDateTime.AddMinutes(60)), 20, 15, 0, null, null);
            _skillBIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(60), startDateTime.AddMinutes(75)), 10, 5, 0, null, null);
            _skillBIntervalData4 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(75), startDateTime.AddMinutes(90)), 20, 15, 0, null, null);

            _skillBIntervalDataList = new List<ISkillIntervalData> { _skillBIntervalData1, _skillBIntervalData2, _skillBIntervalData3, _skillBIntervalData4 };

            _multipleSkillIntervalDataList = new List<IList<ISkillIntervalData>>{ _skillAIntervalDataList,_skillBIntervalDataList };
        }
    }


}
