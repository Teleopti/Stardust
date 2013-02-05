using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
    [TestFixture]
    public class SkillIntervalDataDividerTest
    {
        private MockRepository _mock;
        private ISkillIntervalDataDivider _target;
        private IList<ISkillIntervalData> _skillIntervalDataList;
        private ISkillIntervalData _skillIntervalData1;
        private ISkillIntervalData _skillIntervalData2;
        private ISkillIntervalData _skillIntervalData3;

        [SetUp]
        public void SetUp()
        {
            _mock = new MockRepository();
            _target = new SkillIntervalDataDivider();
        }

        [Test]
        public void ShouldTestTheSplitForFifteenMin()
        {
            SetUpIntervalList(30);
            using(_mock.Record())
            {
                
            }
            using(_mock.Playback())
            {
                Assert.AreEqual(_target.SplitSkillIntervalData(_skillIntervalDataList, 15).Count(), 6);
            }
        }

        [Test]
        public void ShouldTestTheSplitForTenMin()
        {
            SetUpIntervalList(30);
            using (_mock.Record())
            {

            }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.SplitSkillIntervalData(_skillIntervalDataList, 10).Count(), 9);
            }
        }

        [Test]
        public void ShouldTestTheSplitForFiveMin()
        {
            SetUpIntervalList(30);
            using (_mock.Record())
            {

            }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.SplitSkillIntervalData(_skillIntervalDataList, 5).Count(), 18);
            }
        }

        [Test]
        public void ShouldTestTheSplitForTenMinResolutionWith15MinInterval()
        {
            SetUpIntervalList(15);
            using (_mock.Record())
            {

            }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.SplitSkillIntervalData(_skillIntervalDataList,10).Count(), 6);
            }
        }

        private void SetUpIntervalList(int min)
        {
            var startDateTime = new DateTime(2001,01,01,8,0,0).ToUniversalTime() ;
            _skillIntervalData1 = new SkillIntervalData(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(min)), 10, 20, 0, null, null);
            _skillIntervalData2 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(min), startDateTime.AddMinutes(min + min)), 10, 20, 0, null, null);
            _skillIntervalData3 = new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(min + min), startDateTime.AddMinutes(min + min + min)), 10, 20, 0, null, null);

            _skillIntervalDataList = new List<ISkillIntervalData> { _skillIntervalData1, _skillIntervalData2, _skillIntervalData3 };
        }
    }

    
}
