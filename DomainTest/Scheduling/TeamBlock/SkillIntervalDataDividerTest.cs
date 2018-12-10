using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class SkillIntervalDataDividerTest
    {
        private MockRepository _mock;
        private ISkillIntervalDataDivider _target;
        private IList<ISkillIntervalData> _skillIntervalDataList;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new SkillIntervalDataDivider();
        }

       
        [Test]
        public void ShouldTestTheSplitForFifteenMin()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 30, 8, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 60, 9, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(60, 90, 11, null, 7, 0, 0));

            using (_mock.Playback())
            {
                var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 15);
                Assert.AreEqual(skillIntervalData.Count(), 6);
                int min = 0;
                foreach (var si in skillIntervalData)
                {
                    Assert.AreEqual(si.Period, new DateTimePeriod(startDateTime.AddMinutes(min), startDateTime.AddMinutes(min+15)));
                    min = min + 15;
                }
            }
        }

        [Test]
        public void ShouldTestTheSplitForTenMin()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 30, 8, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 60, 9, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(60, 90, 11, null, 7, 0, 0));

            using (_mock.Playback())
            {
                var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 10);
                Assert.AreEqual(skillIntervalData.Count(), 9);
                int min = 0;
                foreach (var si in skillIntervalData)
                {
                    Assert.AreEqual(si.Period, new DateTimePeriod(startDateTime.AddMinutes(min), startDateTime.AddMinutes(min + 10)));
                    min = min + 10;
                }
            }
        }

        [Test]
        public void ShouldTestTheSplitForFiveMin()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 30, 8, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 60, 9, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(60, 90, 11, null, 7, 0, 0));

            using (_mock.Playback())
            {
                var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 5);
                Assert.AreEqual(skillIntervalData.Count(), 18);
                int min = 0;
                foreach (var si in skillIntervalData)
                {
                    Assert.AreEqual(si.Period, new DateTimePeriod(startDateTime.AddMinutes(min), startDateTime.AddMinutes(min + 5)));
                    min = min + 5;
                }
            }
        }

        [Test]
        public void ShouldTestTheSplitForTenMinResolutionWith15MinInterval()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 15, 8, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(15, 30, 9, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 45, 11, null, 7, 0, 0));

            using (_mock.Playback())
            {
                var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList,10);
                Assert.AreEqual(skillIntervalData.Count(), 6);
                int min = 0;
                foreach (var si in skillIntervalData)
                {
                    Assert.AreEqual(si.Period, new DateTimePeriod(startDateTime.AddMinutes(min), startDateTime.AddMinutes(min + 10)));
                    min = min + 10;
                }
            }
        }

        [Test]
        public void VerifyMinimumHeadsArePopulatedWithCorrectSplit()
        {
            
            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 15, 8, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(15, 30, 9, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 45, 11, null, 7, 0, 0));

            using (_mock.Playback())
            {
                var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 10);
                Assert.AreEqual(skillIntervalData.Count(), 6);
                Assert.IsTrue(verifyResult(skillIntervalData[0].MinimumHeads, 8.5));
                Assert.IsTrue(verifyResult(skillIntervalData[1].MinimumHeads, 8.5));
                Assert.IsTrue(verifyResult(skillIntervalData[2].MinimumHeads, 8.5));
                Assert.IsTrue(verifyResult(skillIntervalData[3].MinimumHeads, 11));
                Assert.IsTrue(verifyResult(skillIntervalData[4].MinimumHeads, 11));
                Assert.IsTrue(verifyResult(skillIntervalData[5].MinimumHeads, 11));
                

            }
        }

        [Test]
        public void VerifyMinimumHeadsArePopulatedWithNullWithCorrectSplit()
        {
            
            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 15, 8, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(15, 30, null, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 45, 11, null, 7, 0, 0));

            using (_mock.Playback())
            {
                var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 10);
                Assert.AreEqual(skillIntervalData.Count(), 6);
                Assert.IsTrue(verifyResult(skillIntervalData[0].MinimumHeads, 8));
                Assert.IsTrue(verifyResult(skillIntervalData[1].MinimumHeads, 8));
                Assert.IsTrue(verifyResult(skillIntervalData[2].MinimumHeads, 8));
                Assert.IsTrue(verifyResult(skillIntervalData[3].MinimumHeads, 11));
                Assert.IsTrue(verifyResult(skillIntervalData[4].MinimumHeads, 11));
                Assert.IsTrue(verifyResult(skillIntervalData[5].MinimumHeads, 11));


            }
        }

        [Test]
        public void MinimumHeadsPopulatedWithFirstIntervalWithoutMinHead()
        {

            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 15, null, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(15, 30, 8, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 45, 11, null, 7, 0, 0));

            using (_mock.Playback())
            {
                var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 10);
                Assert.AreEqual(skillIntervalData.Count(), 6);
                Assert.IsTrue(verifyResult(skillIntervalData[0].MinimumHeads, 8));
                Assert.IsTrue(verifyResult(skillIntervalData[1].MinimumHeads, 8));
                Assert.IsTrue(verifyResult(skillIntervalData[2].MinimumHeads, 8));
                Assert.IsTrue(verifyResult(skillIntervalData[3].MinimumHeads, 11));
                Assert.IsTrue(verifyResult(skillIntervalData[4].MinimumHeads, 11));
                Assert.IsTrue(verifyResult(skillIntervalData[5].MinimumHeads, 11));


            }
        }

        [Test]
        public void VerifyMaximumHeadsArePopulatedWithCorrectSplit()
        {
            
            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 15, null, 12, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(15, 30, null, 9, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 45, null, 4, 7, 0, 0));

            using (_mock.Playback())
            {
                var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 10);
                Assert.AreEqual(skillIntervalData.Count(), 6);
                Assert.IsTrue(verifyResult(skillIntervalData[0].MaximumHeads, 10.5));
                Assert.IsTrue(verifyResult(skillIntervalData[1].MaximumHeads, 10.5));
                Assert.IsTrue(verifyResult(skillIntervalData[2].MaximumHeads, 10.5));
                Assert.IsTrue(verifyResult(skillIntervalData[3].MaximumHeads, 4));
                Assert.IsTrue(verifyResult(skillIntervalData[4].MaximumHeads, 4));
                Assert.IsTrue(verifyResult(skillIntervalData[5].MaximumHeads, 4));


            }
        }

        [Test]
        public void MaximumHeadsPopulatedWithFirstIntervalWithoutMinHead()
        {

            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 15, null,null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(15, 30, null, 9, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 45, null, 4, 7, 0, 0));

            using (_mock.Playback())
            {
                var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 10);
                Assert.AreEqual(skillIntervalData.Count(), 6);
                Assert.IsTrue(verifyResult(skillIntervalData[0].MaximumHeads, 9));
                Assert.IsTrue(verifyResult(skillIntervalData[1].MaximumHeads, 9));
                Assert.IsTrue(verifyResult(skillIntervalData[2].MaximumHeads, 9));
                Assert.IsTrue(verifyResult(skillIntervalData[3].MaximumHeads, 4));
                Assert.IsTrue(verifyResult(skillIntervalData[4].MaximumHeads, 4));
                Assert.IsTrue(verifyResult(skillIntervalData[5].MaximumHeads, 4));


            }
        }

        [Test]
        public void MaximumHeadsPopulatedWithSecondIntervalWithoutMinHead()
        {

            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 15, null, 3, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(15, 30, null, null, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 45, null, 4, 7, 0, 0));

            using (_mock.Playback())
            {
                var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 10);
                Assert.AreEqual(skillIntervalData.Count(), 6);
                Assert.IsTrue(verifyResult(skillIntervalData[0].MaximumHeads, 3));
                Assert.IsTrue(verifyResult(skillIntervalData[1].MaximumHeads, 3));
                Assert.IsTrue(verifyResult(skillIntervalData[2].MaximumHeads, 3));
                Assert.IsTrue(verifyResult(skillIntervalData[3].MaximumHeads, 4));
                Assert.IsTrue(verifyResult(skillIntervalData[4].MaximumHeads, 4));
                Assert.IsTrue(verifyResult(skillIntervalData[5].MaximumHeads, 4));


            }
        }

        [Test]
        public void VerifyMaximumHeadsArePopulatedWithNullWithCorrectSplit()
        {
            
            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 15, null, 6, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(15, 30, null, 5, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 45, null, null, 7, 0, 0));
            using (_mock.Playback())
            {
                var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 10);
                Assert.AreEqual(skillIntervalData.Count(), 6);
                Assert.IsTrue(verifyResult(skillIntervalData[0].MaximumHeads, 5.5));
                Assert.IsTrue(verifyResult(skillIntervalData[1].MaximumHeads, 5.5));
                Assert.IsTrue(verifyResult(skillIntervalData[2].MaximumHeads, 5.5));
                Assert.IsFalse( skillIntervalData[3].MaximumHeads.HasValue );
                Assert.IsFalse( skillIntervalData[4].MaximumHeads.HasValue );
                Assert.IsFalse( skillIntervalData[5].MaximumHeads.HasValue );
                

            }
        }

        [Test]
        public void VerifyMinMaxHeadsArePopulated()
        {
            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 15, 8, 6, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(15, 30, 2, 5, 7, 0, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 45, 8, 15, 7, 0, 0));

            using (_mock.Playback())
            {
                var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 10);
                Assert.AreEqual(skillIntervalData.Count(), 6);

                Assert.IsTrue(verifyResult(skillIntervalData[0].MinimumHeads, 5));
                Assert.IsTrue(verifyResult(skillIntervalData[1].MinimumHeads, 5));
                Assert.IsTrue(verifyResult(skillIntervalData[2].MinimumHeads, 5));
                Assert.IsTrue(verifyResult(skillIntervalData[3].MinimumHeads, 8));
                Assert.IsTrue(verifyResult(skillIntervalData[4].MinimumHeads, 8));
                Assert.IsTrue(verifyResult(skillIntervalData[5].MinimumHeads, 8));

                Assert.IsTrue(verifyResult(skillIntervalData[0].MaximumHeads, 5.5));
                Assert.IsTrue(verifyResult(skillIntervalData[1].MaximumHeads, 5.5));
                Assert.IsTrue(verifyResult(skillIntervalData[2].MaximumHeads, 5.5));
                Assert.IsTrue(verifyResult(skillIntervalData[3].MaximumHeads, 15));
                Assert.IsTrue(verifyResult(skillIntervalData[4].MaximumHeads, 15));
                Assert.IsTrue(verifyResult(skillIntervalData[5].MaximumHeads, 15));
                
            }
        }

        [Test]
        public void ForecastedDemandShouldBeSplitCorrectly()
        {
            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 15, null,null,10,0,0));
            _skillIntervalDataList.Add(createSkillIntervalData(15, 30,null,null,5,0,0));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 45, null,null,7,0,0));
            var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 10);
            Assert.AreEqual(skillIntervalData.Count(), 6);
            Assert.AreEqual(skillIntervalData[0].ForecastedDemand,7.5);
            Assert.AreEqual(skillIntervalData[1].ForecastedDemand,7.5);
            Assert.AreEqual(skillIntervalData[2].ForecastedDemand,7.5);
            Assert.AreEqual(skillIntervalData[3].ForecastedDemand,7);
            Assert.AreEqual(skillIntervalData[4].ForecastedDemand,7);
            Assert.AreEqual(skillIntervalData[5].ForecastedDemand,7);
         }

        [Test]
        public void CurrentDemandShouldBeSplitCorrectly()
        {
            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 15, null, null, 0, 11, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(15, 30, null, null, 0, 19, 0));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 45, null, null, 0, 5, 0));
            var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 10);
            Assert.AreEqual(skillIntervalData.Count(), 6);
            Assert.AreEqual(skillIntervalData[0].CurrentDemand, 15);
            Assert.AreEqual(skillIntervalData[1].CurrentDemand, 15);
            Assert.AreEqual(skillIntervalData[2].CurrentDemand, 15);
            Assert.AreEqual(skillIntervalData[3].CurrentDemand, 5);
            Assert.AreEqual(skillIntervalData[4].CurrentDemand, 5);
            Assert.AreEqual(skillIntervalData[5].CurrentDemand, 5);
        }

        [Test]
        public void CurrentHeadsShouldBeSplitCorrectly()
        {
            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 15, null, null, 0, 0, 7));
            _skillIntervalDataList.Add(createSkillIntervalData(15, 30, null, null, 0, 0, 1));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 45, null, null, 0, 0, 3));
            var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 10);
            Assert.AreEqual(skillIntervalData.Count(), 6);
            Assert.AreEqual(skillIntervalData[0].CurrentHeads , 4);
            Assert.AreEqual(skillIntervalData[1].CurrentHeads, 4);
            Assert.AreEqual(skillIntervalData[2].CurrentHeads, 4);
            Assert.AreEqual(skillIntervalData[3].CurrentHeads, 3);
            Assert.AreEqual(skillIntervalData[4].CurrentHeads, 3);
            Assert.AreEqual(skillIntervalData[5].CurrentHeads, 3);
        }

        [Test]
        public void VerifyThatTheSplitWhenIntervalIsInHour()
        {
            _skillIntervalDataList = new List<ISkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalDataHour(0, 1, null, null, 0, 0, 7));
            _skillIntervalDataList.Add(createSkillIntervalDataHour(1, 2, null, null, 0, 0, 1));
            _skillIntervalDataList.Add(createSkillIntervalDataHour(2, 3, null, null, 0, 0, 3));
            var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 30);
            Assert.AreEqual(skillIntervalData.Count(), 6);
            Assert.AreEqual(skillIntervalData[0].CurrentHeads, 7);
            Assert.AreEqual(skillIntervalData[1].CurrentHeads, 7);
            Assert.AreEqual(skillIntervalData[2].CurrentHeads, 1);
            Assert.AreEqual(skillIntervalData[3].CurrentHeads, 1);
            Assert.AreEqual(skillIntervalData[4].CurrentHeads, 3);
            Assert.AreEqual(skillIntervalData[5].CurrentHeads, 3);
        }

        private static bool verifyResult(double? valueToVerify,double resultValue )
        {
            if (!valueToVerify.HasValue) return false;
            if (valueToVerify.Value == resultValue)
                return true;
            return false;
        }

        private static  SkillIntervalData createSkillIntervalData(int startMin, int endMin, double? minHead, double? maxHead, double forecastedDemand, double currentDemand, double currentHeads)
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            return new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(startMin), startDateTime.AddMinutes(endMin)), forecastedDemand, currentDemand, currentHeads, minHead, maxHead);
        }

        private static SkillIntervalData createSkillIntervalDataHour(int startHour, int endHour, double? minHead, double? maxHead, double forecastedDemand, double currentDemand, double currentHeads)
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            return new SkillIntervalData(new DateTimePeriod(startDateTime.AddHours(startHour), startDateTime.AddHours(endHour)), forecastedDemand, currentDemand, currentHeads, minHead, maxHead);
        }

        
    }

    
}
