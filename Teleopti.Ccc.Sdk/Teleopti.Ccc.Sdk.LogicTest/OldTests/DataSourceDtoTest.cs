using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class DataSourceDtoTest
    {
        [Test]
        public void VerifyProperties()
        {
			var target = new DataSourceDto(AuthenticationTypeOptionDto.Application) { Name = "Teleopti WFM" };
			Assert.AreEqual("Teleopti WFM", target.Name);
            Assert.AreEqual(AuthenticationTypeOptionDto.Application,target.AuthenticationTypeOptionDto);
        }
    }
}