using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class ShiftDtoTest
    {
        private ShiftDto _shift;

        [SetUp]
        public void Setup()
        {
            _shift = new ShiftDto();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(0,_shift.LayerCollection.Count);

        }
    }
}