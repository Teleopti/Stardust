using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters;

namespace Teleopti.Ccc.WinCodeTest.Converters
{
    [TestFixture()]
    public class SimpleConverterTest
    {
        readonly SimpleConverter _target = new SimpleConverter();
        [Test()]
        public void ConvertTest()
        {
            _target.Convert(55, typeof(string), null, null).Should().Be(55);
        }

        [Test()]
        public void ConvertBackTest()
        {
            _target.ConvertBack(55, typeof(string), null, null).Should().Be(55);
        }
    }
}
