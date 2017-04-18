using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;

namespace Teleopti.Ccc.WinCodeTest.Common.Chart
{
    [TestFixture]
    public class ChartSettingsManagerTest
    {
        private int _minimumCount = 1;

        [Test]
        public void VerifyDefaultConstructor()
        {
            Assert.IsNotNull(new ChartSettingsManager().ChartSettingsDefault);
            Assert.GreaterOrEqual(new ChartSettingsManager().ChartSettingsDefault.Count, _minimumCount);
        }
    }
}