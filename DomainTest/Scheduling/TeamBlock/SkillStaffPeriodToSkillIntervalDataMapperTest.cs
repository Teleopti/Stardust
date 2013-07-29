using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
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
        private ISkillStaff _payload1;
        private ISkillStaff _payload2;
        private ISkillStaff _payload3;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new SkillStaffPeriodToSkillIntervalDataMapper();
            _skillStaffPeriod1 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriod2 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriod3 = _mock.StrictMock<ISkillStaffPeriod>();
            _payload1 = _mock.StrictMock<ISkillStaff>();
            _payload2 = _mock.StrictMock<ISkillStaff>();
            _payload3 = _mock.StrictMock<ISkillStaff>();
        }

        [Test]
        public void ShouldTestSkillStaffToSkillIntervalMapper()
        {
            _skillStaffPeriodList = new List<ISkillStaffPeriod>
                                        {_skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3};
            var dateTime = new DateTimePeriod();
            var skillPersonData1 = new SkillPersonData(0, 10);
            var skillPersonData2 = new SkillPersonData(2, 8);
            var skillPersonData3 = new SkillPersonData(3, 5);
            using(_mock.Record())
            {
                Expect.Call(_skillStaffPeriod1.Period).Return(dateTime);
                Expect.Call(_skillStaffPeriod1.FStaff).Return(10).Repeat.Twice();
                Expect.Call(_skillStaffPeriod1.CalculatedResource).Return(5);
                Expect.Call(_skillStaffPeriod1.Payload).Return(_payload1).Repeat.AtLeastOnce();
                Expect.Call(_payload1.SkillPersonData).Return(skillPersonData1).Repeat.AtLeastOnce();
                Expect.Call(_payload1.CalculatedLoggedOn).Return(1);

                Expect.Call(_skillStaffPeriod2.Period).Return(dateTime);
                Expect.Call(_skillStaffPeriod2.FStaff).Return(22).Repeat.Twice();
                Expect.Call(_skillStaffPeriod2.CalculatedResource).Return(7);
                Expect.Call(_skillStaffPeriod2.Payload).Return(_payload2).Repeat.AtLeastOnce();
                Expect.Call(_payload2.SkillPersonData).Return(skillPersonData2).Repeat.AtLeastOnce();
                Expect.Call(_payload2.CalculatedLoggedOn).Return(2);

                Expect.Call(_skillStaffPeriod3.Period).Return(dateTime);
                Expect.Call(_skillStaffPeriod3.FStaff).Return(11).Repeat.Twice();
                Expect.Call(_skillStaffPeriod3.CalculatedResource).Return(3);
                Expect.Call(_skillStaffPeriod3.Payload).Return(_payload3).Repeat.AtLeastOnce();
                Expect.Call(_payload3.SkillPersonData).Return(skillPersonData3).Repeat.AtLeastOnce();
                Expect.Call(_payload3.CalculatedLoggedOn).Return(3);
            }
            using(_mock.Playback() )
            {
                var returnList = _target.MapSkillIntervalData(_skillStaffPeriodList);
                Assert.AreEqual(returnList.Count , 3);
                
                Assert.That(returnList[0].Period, Is.EqualTo(dateTime));
                Assert.That(returnList[0].ForecastedDemand , Is.EqualTo(10));
                Assert.That(returnList[0].CurrentDemand  , Is.EqualTo(5));
                Assert.That(returnList[0].MinimumHeads ,Is.EqualTo(0) );
                Assert.That(returnList[0].MaximumHeads ,Is.EqualTo(10) );
                Assert.That(returnList[0].CurrentHeads  ,Is.EqualTo(1) );
                
                Assert.That(returnList[1].Period, Is.EqualTo(dateTime));
                Assert.That(returnList[1].ForecastedDemand , Is.EqualTo(22));
                Assert.That(returnList[1].CurrentDemand  , Is.EqualTo(15));
                Assert.That(returnList[1].MinimumHeads, Is.EqualTo(2));
                Assert.That(returnList[1].MaximumHeads, Is.EqualTo(8));
                Assert.That(returnList[1].CurrentHeads, Is.EqualTo(2));
                
                Assert.That(returnList[2].Period, Is.EqualTo(dateTime));
                Assert.That(returnList[2].ForecastedDemand , Is.EqualTo(11));
                Assert.That(returnList[2].CurrentDemand  , Is.EqualTo(8));
                Assert.That(returnList[2].MinimumHeads, Is.EqualTo(3));
                Assert.That(returnList[2].MaximumHeads, Is.EqualTo(5));
                Assert.That(returnList[2].CurrentHeads, Is.EqualTo(3));

            }
        }
    }

    
}
