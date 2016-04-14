using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class BusinessUnitDtoTest
    {
        [Test]
        public void CanCreateInstance()
        {
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TheUnit").WithId();
			var target = new BusinessUnitDto { Id = businessUnit.Id, Name = businessUnit.Name };

			Assert.IsNotNull(target);
            Assert.AreEqual("TheUnit", target.Name);
            Assert.AreEqual(businessUnit.Id, target.Id);
        }
    }
}