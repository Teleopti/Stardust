using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Overtime;


namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class OvertimeSkillStaffPeriodToSkillIntervalDataMapperTest
    {
        private MockRepository _mock;
        private OvertimeSkillStaffPeriodToSkillIntervalDataMapper _target;
        private IList<ISkillStaffPeriod> _skillStaffPeriodList;
        private ISkillStaffPeriod _skillStaffPeriod1;
        private ISkillStaffPeriod _skillStaffPeriod2;
        private IMergeOvertimeSkillIntervalData _mergeOvertimeSkillIntervalData;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _mergeOvertimeSkillIntervalData = _mock.StrictMock<IMergeOvertimeSkillIntervalData>();
            _target = new OvertimeSkillStaffPeriodToSkillIntervalDataMapper(_mergeOvertimeSkillIntervalData);
            _skillStaffPeriod1 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriod2 = _mock.StrictMock<ISkillStaffPeriod>();
        }

        [Test]
        public void ShouldMapWithSinglePeriod()
        {
            _skillStaffPeriodList = new List<ISkillStaffPeriod> { _skillStaffPeriod1 };
            var dateTime = new DateTimePeriod(new DateTime(2013, 7, 19, 0, 0, 0, DateTimeKind.Utc),
                                              new DateTime(2013, 7, 19, 0, 15, 0, DateTimeKind.Utc));
            using (_mock.Record())
            {
                Expect.Call(_skillStaffPeriod1.Period).Return(dateTime);
                Expect.Call(_skillStaffPeriod1.FStaff).Return(-8).Repeat.Twice();
                Expect.Call(_skillStaffPeriod1.CalculatedResource).Return(2);


            }
            using (_mock.Playback())
            {
                var returnList = _target.MapSkillIntervalData(_skillStaffPeriodList);
                Assert.AreEqual(returnList.Count, 1);

                Assert.That(returnList[0].Period, Is.EqualTo(dateTime));
                Assert.AreEqual(-8, returnList[0].ForecastedDemand);
                Assert.AreEqual(-10, returnList[0].CurrentDemand);


            }
        }

        [Test]
        public void ShouldMapWithTwoPeriods()
        {
            _skillStaffPeriodList = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2 };
            var dateTime = new DateTimePeriod(new DateTime(2013, 7, 19, 0, 0, 0, DateTimeKind.Utc),
                                              new DateTime(2013, 7, 19, 0, 15, 0, DateTimeKind.Utc));
            IOvertimeSkillIntervalData overtSkillStaff = new OvertimeSkillIntervalData(dateTime, 0, 0);
            using (_mock.Record())
            {
                Expect.Call(_skillStaffPeriod1.Period).Return(dateTime);
                Expect.Call(_skillStaffPeriod1.FStaff).Return(-8).Repeat.Twice();
                Expect.Call(_skillStaffPeriod1.CalculatedResource).Return(2);

                Expect.Call(_skillStaffPeriod2.Period).Return(dateTime).Repeat.Twice();
                Expect.Call(_skillStaffPeriod2.FStaff).Return(2).Repeat.Twice();
                Expect.Call(_skillStaffPeriod2.CalculatedResource).Return(5);


                Expect.Call(_mergeOvertimeSkillIntervalData.MergeSkillIntervalData(overtSkillStaff, overtSkillStaff))
                      .IgnoreArguments()
                      .Return(new OvertimeSkillIntervalData(dateTime, -6, -13));
            }
            using (_mock.Playback())
            {
                var returnList = _target.MapSkillIntervalData(_skillStaffPeriodList);
                Assert.AreEqual(returnList.Count, 1);

                Assert.That(returnList[0].Period, Is.EqualTo(dateTime));
                Assert.AreEqual(-6, returnList[0].ForecastedDemand);
                Assert.AreEqual(-13, returnList[0].CurrentDemand);


            }
        }

    }
}
