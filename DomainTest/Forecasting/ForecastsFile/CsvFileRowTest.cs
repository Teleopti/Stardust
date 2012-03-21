using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;

namespace Teleopti.Ccc.DomainTest.Forecasting.ForecastsFile
{
    [TestFixture]
    public class CsvFileRowTest
    {
        private IFileRow _target;
        const string fileContent = "Insurance,20120301 12:45,20120301 13:00,17,179,0";
        const string fileContentWithAgent = "Insurance,20120301 12:45,20120301 13:00,17,179,0,4.75";

        [Test]
        public void ShouldConstructFromLineContent()
        {
            _target = new CsvFileRow(fileContent);

            Assert.That(_target.ToString(), Is.EqualTo(fileContent));
        }

        [Test]
        public void ShouldHaveCorrectCount()
        {
            var target = new CsvFileRow(fileContent);
            var targetWithAgent = new CsvFileRow(fileContentWithAgent);

            Assert.That(target.Count, Is.EqualTo(6));
            Assert.That(targetWithAgent.Count, Is.EqualTo(7));
        }

        [Test]
        public void ShouldClearContent()
        {
            _target = new CsvFileRow(fileContent);
            _target.Clear();

            Assert.That(_target.Count, Is.EqualTo(0));
        }
    }
}
