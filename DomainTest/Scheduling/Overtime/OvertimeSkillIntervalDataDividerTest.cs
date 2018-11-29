using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Overtime;


namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class OvertimeSkillIntervalDataDividerTest
    {
        private MockRepository _mock;
        private IOvertimeSkillIntervalDataDivider _target;
        private IList<IOvertimeSkillIntervalData> _skillIntervalDataList;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new OvertimeSkillIntervalDataDivider();
        }


        [Test]
        public void ShouldTestTheSplitForFifteenMin()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            _skillIntervalDataList = new List<IOvertimeSkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 30));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 60));
            _skillIntervalDataList.Add(createSkillIntervalData(60, 90));

            using (_mock.Playback())
            {
                var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 15);
                Assert.AreEqual(skillIntervalData.Count(), 6);
                int min = 0;
                foreach (var si in skillIntervalData)
                {
                    Assert.AreEqual(si.Period, new DateTimePeriod(startDateTime.AddMinutes(min), startDateTime.AddMinutes(min + 15)));
                    min = min + 15;
                }
            }
        }

        [Test]
        public void ShouldTestTheSplitForTenMin()
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            _skillIntervalDataList = new List<IOvertimeSkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 30));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 60));
            _skillIntervalDataList.Add(createSkillIntervalData(60, 90));

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
            _skillIntervalDataList = new List<IOvertimeSkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 30));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 60));
            _skillIntervalDataList.Add(createSkillIntervalData(60, 90));

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
            _skillIntervalDataList = new List<IOvertimeSkillIntervalData>();
            _skillIntervalDataList.Add(createSkillIntervalData(0, 30));
            _skillIntervalDataList.Add(createSkillIntervalData(30, 60));
            _skillIntervalDataList.Add(createSkillIntervalData(60, 90));

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
		public void ShouldReturnEmptyIntervalDataListIfEmptySkillIntervalDataList()
		{
			var result = _target.SplitSkillIntervalData(new List<IOvertimeSkillIntervalData>(), 15);
			Assert.AreEqual(0, result.Count);
		}

	    [Test]
	    public void ShouldHandle60MinutesInterval()
	    {
			_skillIntervalDataList = new List<IOvertimeSkillIntervalData>();
			_skillIntervalDataList.Add(createSkillIntervalData(0, 60));

			var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 60);

			Assert.AreEqual(1, skillIntervalData.Count);
	    }

	    private static OvertimeSkillIntervalData createSkillIntervalData(int startMin, int endMin)
        {
            var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
            return new OvertimeSkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(startMin), startDateTime.AddMinutes(endMin)), 0, 0);
        }
    }
}
