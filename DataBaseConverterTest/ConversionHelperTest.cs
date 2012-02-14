using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DatabaseConverterTest
{
    [TestFixture]
    public class ConversionHelperTest
    {
        private string _testString;

        [SetUp]
        public void Setup()
        {
            _testString = "123456789012345678901234567890";
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyHasEmptyConstructor()
        {

            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(ConversionHelper), true));
        }

        [Test]
        public void CanReturnWithoutShrink()
        {
            string ret = ConversionHelper.MapString(_testString, 50);
            Assert.AreEqual(_testString, ret);
        }

        [Test]
        public void CanReturnAndShrink()
        {
            string ret = ConversionHelper.MapString(_testString, 10);
            Assert.AreEqual("1234567890", ret);
        }
    }
}
