using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Analytics
{
    public class DataSourceEtl : IDataSourceEtl
    {
        private readonly int _dataSourceId;
        private readonly string _dataSourceName;
        private readonly int _intervalLength;

        private DataSourceEtl()
        {
        }

        public DataSourceEtl(int dataSourceId, string dataSourceName, int timeZoneId, string timeZoneCode, int intervalLength, bool inactive)
            : this()
        {
            _dataSourceId = dataSourceId;
            _dataSourceName = dataSourceName;
            TimeZoneId = timeZoneId;
            TimeZoneCode = timeZoneCode;
            _intervalLength = intervalLength;
            Inactive = inactive;
        }

        public int DataSourceId
        {
            get { return _dataSourceId; }
        }

        public string DataSourceName
        {
            get { return _dataSourceName; }
        }

        public int TimeZoneId { get; set; }

        public string TimeZoneCode { get; set; }

        public int IntervalLength
        {
            get { return _intervalLength; }
        }

        public bool Inactive { get; set; }
    }
}