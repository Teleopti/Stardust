using System.IO;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting.ForecastFormattedFile;

namespace Teleopti.Ccc.DomainTest.Forecasting.ForecastsFormattedFile
{
    [TestFixture]
    public class CsvFileReaderTest
    {
        private CsvFileReader _target;
        private MockRepository _mocks;
        private StreamReader _streamReader;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _streamReader = _mocks.StrictMock<StreamReader>();
            _target = new CsvFileReader(_streamReader);
        }

        [Test]
        public void ShouldReadRowWhenThereIsRow()
        {
            var row = new CsvFileRow();
            using(_mocks.Record())
            {
                Expect.Call(_streamReader.ReadLine()).Return("Insurance,20120301 12:45,20120301 01:00,17,179,0,4.75");
            }
            using(_mocks.Playback())
            {
                Assert.That(_target.ReadNextRow(row), Is.True);
            }
        }
        
        [Test]
        public void ShouldAwareOfTheEndOfFile()
        {
            var row = new CsvFileRow();
            using(_mocks.Record())
            {
                Expect.Call(_streamReader.ReadLine()).Return("");
            }
            using(_mocks.Playback())
            {
                Assert.That(_target.ReadNextRow(row), Is.False);
            }
        }
    }
}
