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
    public class SkillStaffPeriodToSkillIntervalDataMapperTest
    {
        private MockRepository _mock;
        private SkillStaffPeriodToSkillIntervalDataMapper _target;
        private IList< ISkillStaffPeriod> _skillStaffPeriodList;
        private ISkillStaffPeriod _skillStaffPeriod1;
        private ISkillStaffPeriod _skillStaffPeriod2;
        private ISkillStaffPeriod _skillStaffPeriod3;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new SkillStaffPeriodToSkillIntervalDataMapper();
            _skillStaffPeriod1 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriod2 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriod3 = _mock.StrictMock<ISkillStaffPeriod>();
        }

        [Test]
        public void ShouldTestSkillStaffToSkillIntervalMapper()
        {
            _skillStaffPeriodList = new List<ISkillStaffPeriod>
                                        {_skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3};
            var dateTime = new DateTimePeriod();
            using(_mock.Record())
            {
                Expect.Call(_skillStaffPeriod1.Period).Return(dateTime);
                Expect.Call(_skillStaffPeriod1.FStaff).Return(10).Repeat.Twice();
                Expect.Call(_skillStaffPeriod1.CalculatedResource).Return(5);

                Expect.Call(_skillStaffPeriod2.Period).Return(dateTime);
                Expect.Call(_skillStaffPeriod2.FStaff).Return(22).Repeat.Twice();
                Expect.Call(_skillStaffPeriod2.CalculatedResource).Return(7);

                Expect.Call(_skillStaffPeriod3.Period).Return(dateTime);
                Expect.Call(_skillStaffPeriod3.FStaff).Return(11).Repeat.Twice();
                Expect.Call(_skillStaffPeriod3.CalculatedResource).Return(3);
            }
            using(_mock.Playback() )
            {
                var returnList = _target.MapSkillIntervalData(_skillStaffPeriodList);
                Assert.AreEqual(returnList.Count , 3);
                
                Assert.That(returnList[0].Period, Is.EqualTo(dateTime));
                Assert.That(returnList[0].ForecastedDemand , Is.EqualTo(10));
                Assert.That(returnList[0].CurrentDemand  , Is.EqualTo(5));
                
                Assert.That(returnList[1].Period, Is.EqualTo(dateTime));
                Assert.That(returnList[1].ForecastedDemand , Is.EqualTo(22));
                Assert.That(returnList[1].CurrentDemand  , Is.EqualTo(15));
                
                Assert.That(returnList[2].Period, Is.EqualTo(dateTime));
                Assert.That(returnList[2].ForecastedDemand , Is.EqualTo(11));
                Assert.That(returnList[2].CurrentDemand  , Is.EqualTo(8));

            }
        }
    }

    
}
