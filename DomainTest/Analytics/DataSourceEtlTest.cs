using NUnit.Framework;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Analytics
{
    [TestFixture]
    public class DataSourceEtlTest
    {
        private IDataSourceEtl _dataSource1;
        private int _dataSourceId;
        private string _datasourceName;
        private int _timeZoneId;
        private string _timeZoneCode;
        private int _intervalLength;
        private bool _inactive;

        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _dataSourceId = 11;
            _datasourceName = "DS 1";
            _timeZoneId = 1;
            _timeZoneCode = "UTC";
            _intervalLength = 15;
            _inactive = false;

            _dataSource1 = new DataSourceEtl(_dataSourceId, _datasourceName, _timeZoneId, _timeZoneCode, _intervalLength, _inactive);
        }

        #endregion

        [Test]
        public void VerifyCanAccessDataSourceProperties()
        {
            Assert.AreEqual(_dataSourceId, _dataSource1.DataSourceId);
            Assert.AreEqual(_datasourceName, _dataSource1.DataSourceName);
            Assert.AreEqual(_timeZoneId, _dataSource1.TimeZoneId);
            Assert.AreEqual(_timeZoneCode, _dataSource1.TimeZoneCode);
            Assert.AreEqual(_intervalLength, _dataSource1.IntervalLength);
            Assert.AreEqual(_inactive, _dataSource1.Inactive);
        }

        [Test]
        public void VerifyCanSetDataSourceProperties()
        {
            _timeZoneId = 2;
            _timeZoneCode = "W. Europe Standard Time";
            _inactive = true;

            _dataSource1.TimeZoneId = _timeZoneId;
            _dataSource1.TimeZoneCode = _timeZoneCode;
            _dataSource1.Inactive = _inactive;

            Assert.AreEqual(_timeZoneId, _dataSource1.TimeZoneId);
            Assert.AreEqual(_timeZoneCode, _dataSource1.TimeZoneCode);
            Assert.AreEqual(_inactive, _dataSource1.Inactive);
        }
    }
}