using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Forecasting.Import
{
    [TestFixture]
    public class ForecastFileTest
    {
        [Test]
        public void ShouldHaveForecastFileName()
        {
            const string fileName = "TestImportFile.csv";
            const string fileContent = "";
            var encoding = new System.Text.UTF8Encoding();
            var forecastFile = new ForecastFile(fileName , encoding.GetBytes(fileContent));
            Assert.AreEqual(forecastFile.FileName, fileName);
        }

        [Test]
        public void ShouldInitializeFileContents()
        {
            const string fileName = "TestImportFile.csv";
            const string fileContent = "hello World";
            var encoding = new System.Text.UTF8Encoding();
            var forecastFile = new ForecastFile(fileName, encoding.GetBytes(fileContent));
            Assert.IsNotEmpty(forecastFile.FileContent);
        }

        [Test]
        public void ShouldHaveDefaultConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof (ForecastFile)));
        }
    }
}
