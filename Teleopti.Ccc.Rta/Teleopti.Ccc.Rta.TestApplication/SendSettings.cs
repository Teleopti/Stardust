using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using log4net;
using Teleopti.Ccc.Rta.LogClient;

namespace Teleopti.Ccc.Rta.TestApplication
{
    public class SendSettings
    {
        private readonly ILog _loggingSvc = LogManager.GetLogger(ClientHandler.LogName);
        private readonly IList<string> _logOnCollection = new List<string>();
        private readonly IList<string> _stateCodeCollection = new List<string>();
        private int _sendCount = 1;
        private readonly int _sourceId;
        private readonly int _minDistributionMilliseconds;
        private readonly int _maxDistributionMilliseconds;
        private readonly string _endSequenceCode;
        private readonly bool _snapshotMode;

        public SendSettings()
        {
            _logOnCollection = new List<string>(ConfigurationManager.AppSettings["LogOn"].Split(','));
            _stateCodeCollection = new List<string>(ConfigurationManager.AppSettings["StateCode"].Split(','));
            _endSequenceCode = ConfigurationManager.AppSettings["LogOffCode"];

            string setting = ConfigurationManager.AppSettings["SendCount"];
            if (!int.TryParse(setting, out _sendCount))
                _loggingSvc.WarnFormat(CultureInfo.CurrentCulture, "Property SendCount was not read from configuration");

            setting = ConfigurationManager.AppSettings["SourceId"];
            if (!int.TryParse(setting, out _sourceId))
                _loggingSvc.WarnFormat(CultureInfo.CurrentCulture, "Property SourceId was not read from configuration (the value must be numeric)");

            setting = ConfigurationManager.AppSettings["MinDistributionMilliseconds"];
            if (!int.TryParse(setting, out _minDistributionMilliseconds))
                _loggingSvc.WarnFormat(CultureInfo.CurrentCulture, "Property MinDistributionMilliseconds was not read from configuration");

            setting = ConfigurationManager.AppSettings["MaxDistributionMilliseconds"];
            if (!int.TryParse(setting, out _maxDistributionMilliseconds))
                _loggingSvc.WarnFormat(CultureInfo.CurrentCulture, "Property MaxDistributionMilliseconds was not read from configuration");

            setting = ConfigurationManager.AppSettings["SnapshotMode"];
            if (!bool.TryParse(setting, out _snapshotMode))
                _loggingSvc.WarnFormat(CultureInfo.CurrentCulture, "Property Snapshot was not read from configuration, the default value of false will be used.");

            _loggingSvc.InfoFormat(
                "Loaded test sequence with {0} calls using {1} different log on and {2} different state codes. The SourceId is {3}. Snapshot mode is {4}.",
                _sendCount, _logOnCollection.Count, _stateCodeCollection.Count, _sourceId, _snapshotMode);
        }

        public int RemainingCount
        {
            get { return _sendCount; }
        }

        public IEnumerable<AgentStateForTest> Read()
        {
            if (_sendCount<=0) throw new InvalidOperationException("The sequence is now finished. Start a new sequence to run test again.");
            Random random = new Random();
            if (_snapshotMode)
            {
                var numberOfAgentsToInclude = Math.Max(1, _logOnCollection.Count - 2);
                var selectedLogOns = new HashSet<int>();
                while (selectedLogOns.Count<numberOfAgentsToInclude)
                {
                    selectedLogOns.Add(random.Next(0, _logOnCollection.Count));
                }
                var batchIdentifier = DateTime.UtcNow;
                foreach (int selectedLogOn in selectedLogOns)
                {
                    int stateCodeIndex = random.Next(0, _stateCodeCollection.Count);
                    _sendCount--;

                    yield return
                        new AgentStateForTest(_logOnCollection[selectedLogOn], _stateCodeCollection[stateCodeIndex],
                                              TimeSpan.Zero, _sourceId, true, batchIdentifier);
                }

                int waitTime = random.Next(_minDistributionMilliseconds, _maxDistributionMilliseconds);
                yield return new AgentStateForTest("", "", TimeSpan.FromMilliseconds(waitTime), _sourceId, true, batchIdentifier); //Snapshot end signal - with delay to add delay between snapshots
            }
            else
            {
                int logOnIndex = random.Next(0, _logOnCollection.Count);
                int stateCodeIndex = random.Next(0, _stateCodeCollection.Count);
                int waitTime = random.Next(_minDistributionMilliseconds, _maxDistributionMilliseconds);
                _sendCount--;

                yield return
                    new AgentStateForTest(_logOnCollection[logOnIndex], _stateCodeCollection[stateCodeIndex],
                                          TimeSpan.FromMilliseconds(waitTime), _sourceId, false, DateTime.UtcNow);
            }
        }

        public IList<AgentStateForTest> EndSequence()
        {
            IList<AgentStateForTest> result = new List<AgentStateForTest>();
            if (string.IsNullOrEmpty(_endSequenceCode)) return result;
            foreach (string logOn in _logOnCollection)
            {
                result.Add(new AgentStateForTest(logOn, _endSequenceCode, TimeSpan.Zero, _sourceId, false,
                                                 DateTime.UtcNow));
            }
            return result;
        }
    }
}
