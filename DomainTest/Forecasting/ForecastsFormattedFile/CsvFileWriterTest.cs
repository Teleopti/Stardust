using System.IO;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting.ForecastFormattedFile;

namespace Teleopti.Ccc.DomainTest.Forecasting.ForecastsFormattedFile
{
    [TestFixture]
    public class CsvFileWriterTest
    {
        private CsvFileWriter _target;
        private MockRepository _mocks;
        private StreamWriter _streamWriter;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _streamWriter = _mocks.StrictMock<StreamWriter>();
            _target = new CsvFileWriter(_streamWriter);
        }

        [Test]
        public void ShouldWriteFile()
        {
            var row = new CsvFileRow {"Insurance", "20120301 12:45", "20120301 01:00", "17", "179", "0", "4.75"};

            _target.WriteRow(row);
            Assert.That(row.LineText, Is.EqualTo("Insurance,20120301 12:45,20120301 01:00,17,179,0,4.75"));
        }
    }
}
