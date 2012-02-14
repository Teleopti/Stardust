using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class SkillDataMapperTest: MapperTest<global::Domain.SkillData>
    {
    
       private global::Domain.ISystemSetting _settingMock;
       private MockRepository _mocks;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 11; }
        }

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _settingMock = _mocks.StrictMock<global::Domain.ISystemSetting>();
        }

        [Test]
        public void CanMapSkillData6X()
        {
            Percent percent = new Percent(0.12);
            double seconds = 12;
            ServiceLevel serviceLevel = new ServiceLevel(percent, seconds);

            Percent minOccupancy = new Percent(0.10);
            Percent maxOccupancy = new Percent(0.80);
            int intervalLength = 15;

            Expect.On(_settingMock)
                .Call(_settingMock.IntervalLength())
                .Return(intervalLength);

            LastCall.Repeat.AtLeastOnce();
            _mocks.ReplayAll();


            SkillDataMapper skillDataMap = new SkillDataMapper();

            global::Domain.SkillData oldSKillData = new global::Domain.SkillData(15, 12, 13, 12, 32, 44, (int)serviceLevel.Seconds, (int)(serviceLevel.Percent.Value*100), true, 99, 33, (int)(minOccupancy.Value*100), (int)(maxOccupancy.Value*100), _settingMock);

            ServiceAgreement newServiceAgreement = skillDataMap.Map(oldSKillData);

            Assert.AreEqual(serviceLevel.Percent, newServiceAgreement.ServiceLevel.Percent);
            Assert.AreEqual(serviceLevel.Seconds, newServiceAgreement.ServiceLevel.Seconds);
            Assert.AreEqual(minOccupancy, newServiceAgreement.MinOccupancy);
            Assert.AreEqual(maxOccupancy, newServiceAgreement.MaxOccupancy);
        }

        [Test]
        public void CanHandleZeroAsMaxOccupancy()
        {
            Percent percent = new Percent(0.12);
            double seconds = 12;
            ServiceLevel serviceLevel = new ServiceLevel(percent, seconds);

            Percent minOccupancy = new Percent(0.50);
            Percent maxOccupancy = new Percent(1.00);
            int intervalLength = 15;

            Expect.On(_settingMock)
                .Call(_settingMock.IntervalLength())
                .Return(intervalLength);

            LastCall.Repeat.AtLeastOnce();
            _mocks.ReplayAll();


            SkillDataMapper skillDataMap = new SkillDataMapper();

            global::Domain.SkillData oldSKillData = new global::Domain.SkillData(15, 12, 13, 12, 32, 44, (int)serviceLevel.Seconds, (int)(serviceLevel.Percent.Value*100), true, 99, 33, 50, 0, _settingMock);

            ServiceAgreement newServiceAgreement = skillDataMap.Map(oldSKillData);

            Assert.AreEqual(minOccupancy, newServiceAgreement.MinOccupancy);
            Assert.AreEqual(maxOccupancy, newServiceAgreement.MaxOccupancy);
        }
    }
}