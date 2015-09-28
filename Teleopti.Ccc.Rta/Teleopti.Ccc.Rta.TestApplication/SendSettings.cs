using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using log4net;

namespace Teleopti.Ccc.Rta.TestApplication
{
	public class SendSettings
	{
		private readonly ILog _loggingSvc = LogManager.GetLogger(ClientHandler.LogName);
		private readonly string _authenticationKey;
		private readonly IList<string> _logOnCollection = new List<string>();
		private readonly IList<string> _stateCodeCollection = new List<string>();
		private readonly IList<Guid> _personIdForScheduleUpdate = new List<Guid>();
		private int _sendCount = 1;
		private readonly Guid _platformId;
		private readonly int _sourceId;
		private readonly int _minDistributionMilliseconds;
		private readonly int _maxDistributionMilliseconds;
		private readonly string _endSequenceCode;
		private readonly bool _snapshotMode;
		private readonly int _intervalForScheduleUpdate;
		private readonly Guid _businessUnitId;
		private readonly bool _removeOneByOne;
		private readonly bool _randomPersonsInSnapshot;
		private readonly bool _useMultiThread;
		private readonly int _numberOfThreads;
		private readonly Random random;

		public SendSettings()
		{
			_authenticationKey = ConfigurationManager.AppSettings["AuthenticationKey"];
			_logOnCollection = new List<string>(ConfigurationManager.AppSettings["LogOn"].Split(','));
			_stateCodeCollection = new List<string>(ConfigurationManager.AppSettings["StateCode"].Split(','));
			_endSequenceCode = ConfigurationManager.AppSettings["LogOffCode"];

			foreach (var personId in ConfigurationManager.AppSettings["PersonIdsForScheduleUpdate"].Split(','))
			{
				Guid personGuid;
				if (Guid.TryParse(personId, out personGuid))
					_personIdForScheduleUpdate.Add(personGuid);
				else
					_loggingSvc.WarnFormat(CultureInfo.CurrentCulture,
					                       "Property PersonIdsForScheduleUpdate was not read from configuration");
			}

			var strPlatformTypeId = (ConfigurationManager.AppSettings["PlatformTypeId"]);
			if (!Guid.TryParse(strPlatformTypeId, out _platformId))
				_loggingSvc.WarnFormat(CultureInfo.CurrentCulture,
				                       "Property PlatformTypeId was not read from configuration");

			var setting = ConfigurationManager.AppSettings["SendCount"];
			if (!int.TryParse(setting, out _sendCount))
				_loggingSvc.WarnFormat(CultureInfo.CurrentCulture,
				                       "Property SendCount was not read from configuration");

			setting = ConfigurationManager.AppSettings["SourceId"];
			if (!int.TryParse(setting, out _sourceId))
				_loggingSvc.WarnFormat(CultureInfo.CurrentCulture,
				                       "Property SourceId was not read from configuration (the value must be numeric)");

			setting = ConfigurationManager.AppSettings["MinDistributionMilliseconds"];
			if (!int.TryParse(setting, out _minDistributionMilliseconds))
				_loggingSvc.WarnFormat(CultureInfo.CurrentCulture,
				                       "Property MinDistributionMilliseconds was not read from configuration");

			setting = ConfigurationManager.AppSettings["MaxDistributionMilliseconds"];
			if (!int.TryParse(setting, out _maxDistributionMilliseconds))
				_loggingSvc.WarnFormat(CultureInfo.CurrentCulture,
				                       "Property MaxDistributionMilliseconds was not read from configuration");

			setting = ConfigurationManager.AppSettings["SnapshotMode"];
			if (!bool.TryParse(setting, out _snapshotMode))
				_loggingSvc.WarnFormat(CultureInfo.CurrentCulture,
				                       "Property Snapshot was not read from configuration, the default value of false will be used.");

			setting = ConfigurationManager.AppSettings["IntervalForScheduleUpdate"];
			if (!int.TryParse(setting, out _intervalForScheduleUpdate))
				_loggingSvc.WarnFormat(CultureInfo.CurrentCulture,
				                       "Property IntervalForScheduleUpdate was not read from configuration.");

			setting = ConfigurationManager.AppSettings["BusinessUnitId"];
			if (!Guid.TryParse(setting, out _businessUnitId))
				_loggingSvc.WarnFormat(CultureInfo.CurrentCulture,
				                       "Property BusinessUnitId was not read from configuration");

			setting = ConfigurationManager.AppSettings["UseMultiThread"];
			if (!bool.TryParse(setting, out _useMultiThread))
				_loggingSvc.WarnFormat(CultureInfo.CurrentCulture,
				                       "Property UseMultiThread was not read from configuration");

			setting = ConfigurationManager.AppSettings["NumberOfThreads"];
			if (!int.TryParse(setting, out _numberOfThreads))
				_loggingSvc.WarnFormat(CultureInfo.CurrentCulture,
				                       "Property NumberOfThreads was not read from configuration");

			setting = ConfigurationManager.AppSettings["RemoveOneByOne"];
			if (!bool.TryParse(setting, out _removeOneByOne))
				_loggingSvc.WarnFormat(CultureInfo.CurrentCulture,
				                       "Property RemoveOneByOne was not read from configuration");

			setting = ConfigurationManager.AppSettings["RandomPersonsInSnapshot"];
			if (!bool.TryParse(setting, out _randomPersonsInSnapshot))
				_loggingSvc.WarnFormat(CultureInfo.CurrentCulture,
				                       "Property RandomPersonsInSnapshot was not read from configuration");

			_loggingSvc.InfoFormat(
				"Loaded test sequence with {0} calls using {1} different log on and {2} different state codes. The SourceId is {3}. Snapshot mode is {4}.",
				_sendCount, _logOnCollection.Count, _stateCodeCollection.Count, _sourceId, _snapshotMode);
			random = new Random();
		}

		public int RemainingCount
		{
			get { return _sendCount; }
		}

		public Guid PlatformId
		{
			get { return _platformId; }
		}

		public int IntervalForScheduleUpdate
		{
			get { return _intervalForScheduleUpdate; }
		}

		public int NumberOfThreads
		{
			get { return _numberOfThreads; }
		}

		public bool UseMultiThread
		{
			get { return _useMultiThread; }
		}

		public IEnumerable<AgentStateForTest> Read()
		{
			if (_sendCount <= 0)
				throw new InvalidOperationException("The sequence is now finished. Start a new sequence to run test again.");

			return _snapshotMode
				       ? getSnapshot()
				       : new[] {getSingleState()};
		}

		private IEnumerable<AgentStateForTest> getSnapshot()
		{
			var batchIdentifier = DateTime.UtcNow;
			var selectedLogOns = new List<int>();

			
			if (_removeOneByOne && _logOnCollection.Count > 1)
				_logOnCollection.RemoveAt(0);

			for (var i = 0; i < _logOnCollection.Count; i++)
				selectedLogOns.Add(i);

			if (_randomPersonsInSnapshot && !_removeOneByOne)
			{
				selectedLogOns.Clear();
				while (selectedLogOns.Count < Math.Max(1, _logOnCollection.Count - 2))
					selectedLogOns.Add(random.Next(0, _logOnCollection.Count));
			}

			return buildSnapshot(selectedLogOns, batchIdentifier);
		}

		private IEnumerable<AgentStateForTest> buildSnapshot(IEnumerable<int> selectedLogOns, DateTime batchIdentifier)
		{
			foreach (var selectedLogOn in selectedLogOns)
			{
				var stateCodeIndex = random.Next(0, _stateCodeCollection.Count);
				var personIdIndex = random.Next(0, _personIdForScheduleUpdate.Count);

				_sendCount--;
				yield return
					new AgentStateForTest(_authenticationKey, _logOnCollection[selectedLogOn], _stateCodeCollection[stateCodeIndex],
					                      TimeSpan.Zero, _sourceId, true, batchIdentifier, _personIdForScheduleUpdate[personIdIndex],
					                      _businessUnitId);
			}

			var waitTime = random.Next(_minDistributionMilliseconds, _maxDistributionMilliseconds);
			yield return new AgentStateForTest(_authenticationKey, "", "", TimeSpan.FromMilliseconds(waitTime), _sourceId, true, batchIdentifier,
			                                   _personIdForScheduleUpdate[0], _businessUnitId);
		}

		private AgentStateForTest getSingleState()
		{
			var logOnIndex = random.Next(0, _logOnCollection.Count);
			var stateCodeIndex = random.Next(0, _stateCodeCollection.Count);
			var personIdIndex = random.Next(0, _personIdForScheduleUpdate.Count);
			var waitTime = random.Next(_minDistributionMilliseconds, _maxDistributionMilliseconds);

			_sendCount--;
			return new AgentStateForTest(_authenticationKey, _logOnCollection[logOnIndex], _stateCodeCollection[stateCodeIndex],
			                             TimeSpan.FromMilliseconds(waitTime), _sourceId, false, DateTime.UtcNow,
			                             _personIdForScheduleUpdate[personIdIndex], _businessUnitId);
		}

		public IList<AgentStateForTest> EndSequence()
		{
			var result = new List<AgentStateForTest>();
			if (string.IsNullOrEmpty(_endSequenceCode)) return result;

			result.AddRange(
				_logOnCollection.Select(
					logOn =>
					new AgentStateForTest(_authenticationKey, logOn, _endSequenceCode, TimeSpan.Zero, _sourceId, false, DateTime.UtcNow, Guid.Empty,
					                      Guid.Empty)));
			return result;
		}
	}
}
