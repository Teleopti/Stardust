using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class AdherenceDtoTest
    {
        [Test]
        public void VerifyPropertiesAndConstructor()
        {
			var target = new AdherenceDto();
			Assert.IsNotNull(target);
            ICollection<AdherenceDataDto> list = new List<AdherenceDataDto>();
            target.AdherenceDataDtos = list;
            Assert.AreEqual(list, target.AdherenceDataDtos);
        }
    }
}
