using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.SystemSetting
{
    [TestFixture]
    public class PersonalSettingDataTest
    {
        private PersonalSettingData _target;

        [SetUp]
        public void Setup()
        {
            _target = new PersonalSettingData("Micke65", PersonFactory.CreatePerson("Roger"));
        }

        [Test]
        public void VerifyPerson()
        {
            IPerson p = _target.OwnerPerson;
            Assert.AreEqual("Roger", p.Name.FirstName);
        }

        [Test]
        public void VerifyBelongsToNoBusinessUnit()
        {
            Assert.IsFalse(_target is IFilterOnBusinessUnit);
        }
    }
}
