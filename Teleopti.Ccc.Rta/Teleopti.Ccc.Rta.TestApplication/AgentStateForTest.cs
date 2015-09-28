using System;

namespace Teleopti.Ccc.Rta.TestApplication
{
    public class AgentStateForTest
    {
        private readonly string _logOn;
        private readonly string _stateCode;
        private readonly TimeSpan _waitTime;
        private readonly int _dataSourceId;
        private readonly bool _isSnapshot;
        private readonly DateTime _batchIdentifier;
	    private readonly Guid _personId;
	    private readonly Guid _businessUnitId;

	    public AgentStateForTest(string authenticationKey, string logOn, string stateCode, TimeSpan waitTime, int dataSourceId, bool isSnapshot, DateTime batchIdentifier, Guid personId, Guid businessUnitId)
	    {
		    AuthenticationKey = authenticationKey;
            _logOn = logOn;
            _isSnapshot = isSnapshot;
            _batchIdentifier = batchIdentifier;
            _stateCode = stateCode;
            _waitTime = waitTime;
            _dataSourceId = dataSourceId;
	        _personId = personId;
			_businessUnitId = businessUnitId;
        }

        public int DataSourceId
        {
            get { return _dataSourceId; }
        }

        public DateTime BatchIdentifier
        {
            get { return _batchIdentifier; }
        }

        public TimeSpan WaitTime
        {
            get { return _waitTime; }
        }

        public string StateCode
        {
            get { return _stateCode; }
        }

        public string LogOn
        {
            get { return _logOn; }
        }

        public bool IsSnapshot
        {
            get { return _isSnapshot; }
        }
		
	    public Guid PersonId
	    {
			get { return _personId; }
	    }
		
	    public Guid BusinessUnitId
	    {
			get { return _businessUnitId; }
	    }

	    public string AuthenticationKey { get; private set; }
    }
}