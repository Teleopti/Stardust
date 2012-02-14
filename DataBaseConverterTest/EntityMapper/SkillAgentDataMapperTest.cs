using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class SkillAgentDataMapperTest : MapperTest<global::Domain.SkillData>
    {
       private global::Domain.ISystemSetting _settingMock;
       private MockRepository _mocks;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 10; }
        }

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _settingMock = _mocks.CreateMock<global::Domain.ISystemSetting>();
        }

        [Test]
        public void CanMapSkillAgentData6X()
        {

            MinMax<int> agents = new MinMax<int>(10, 80);
            int intervalLength = 15;

            Expect.On(_settingMock)
                .Call(_settingMock.IntervalLength())
                .Return(intervalLength);

            LastCall.Repeat.AtLeastOnce();
            _mocks.ReplayAll();


            SkillAgentDataMapper skillAgentDataMap = new SkillAgentDataMapper();

            global::Domain.SkillData oldSKillData = new global::Domain.SkillData(15, 12, 13,agents.Minimum,agents.Maximum, 44, 22,12, true, 99, 33,8, 9, _settingMock);

            SkillPersonData newSkillPersonData = skillAgentDataMap.Map(oldSKillData);

            Assert.AreEqual(agents, newSkillPersonData.PersonCollection);
        }

        [Test]
        public void VerifyMinimumValueIsNotTooHigh()
        {
            //What happens if the minimum is higher than maximum,
            //For now I set it to integer max
         
            int intervalLength = 15;

            Expect.On(_settingMock)
                .Call(_settingMock.IntervalLength())
                .Return(intervalLength);

            LastCall.Repeat.AtLeastOnce();
            _mocks.ReplayAll();


            SkillAgentDataMapper skillAgentDataMap = new SkillAgentDataMapper();

            global::Domain.SkillData oldSKillData = new global::Domain.SkillData(15, 12, 13, 2, 0, 44, 22, 12, true, 99, 33, 8, 9, _settingMock);

            SkillPersonData newSkillPersonData = skillAgentDataMap.Map(oldSKillData);
            Assert.AreEqual(int.MaxValue, newSkillPersonData.PersonCollection.Maximum);
        }
    }
}