using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;

namespace Teleopti.Ccc.DomainTest.Forecasting.ForecastsFile
{
    [TestFixture]
    public class CsvFileRowTest
    {
        private readonly IFileRow _target = new CsvFileRow();

        [Test]
        public void ShouldHoldLineText()
        {
            const string fileContent = "Insurance,20120301 12:45,20120301 01:00,17,179,0,4.75";
            _target.LineText = fileContent;

            Assert.That(_target.LineText, Is.EqualTo(fileContent));
        }
    }
}
