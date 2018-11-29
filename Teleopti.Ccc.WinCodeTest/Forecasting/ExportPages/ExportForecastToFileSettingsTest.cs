using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;


namespace Teleopti.Ccc.WinCodeTest.Forecasting.ExportPages
{
    [TestFixture]
    public class ExportForecastToFileSettingsTest
    {

        private IExportForecastToFileSettings _target;

        [SetUp]
        public void Setup()
        {
            _target = new ExportForecastToFileSettings();
        }

        [Test]
        public void ShouldKeepSelectionPeriod()
        {
            var selectedPeriod = new DateOnlyPeriod(2012, 1, 1, 2012, 2, 1);

            _target.Period = selectedPeriod;

            Assert.That(_target.Period, Is.EqualTo(selectedPeriod));
        }
    }
}
