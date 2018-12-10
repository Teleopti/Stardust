using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using log4net;
using Teleopti.Ccc.Rta.WebService;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Ccc.Web.Areas.Rta
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class TeleoptiRtaService : ITeleoptiRtaService
	{
		private readonly Wfm.Adherence.States.Rta _rta;
		private readonly IRtaTracer _tracer;
		private readonly ILog log = LogManager.GetLogger(typeof(TeleoptiRtaService));

		public TeleoptiRtaService(Wfm.Adherence.States.Rta rta, IRtaTracer tracer)
		{
			_rta = rta;
			_tracer = tracer;
		}

		public int SaveExternalUserState(
			string authenticationKey,
			string userCode,
			string stateCode,
			string stateDescription,
			bool isLoggedOn,
			int secondsInState,
			DateTime timestamp,
			string platformTypeId,
			string sourceId,
			DateTime batchId,
			bool isSnapshot)
		{
			_tracer.ProcessReceived("SaveExternalUserState", 1);
			validateParameters(authenticationKey, platformTypeId, sourceId);
			userCode = fixUserCode(userCode);
			stateCode = fixStateCode(stateCode, platformTypeId, isLoggedOn);
			BatchInputModel input = null;
			if (isClosingSnapshot(userCode, isSnapshot))
				input = new BatchInputModel
				{
					AuthenticationKey = authenticationKey,
					SourceId = sourceId,
					SnapshotId = batchId,
					CloseSnapshot = true
				};
			else
				input = new BatchInputModel
				{
					AuthenticationKey = authenticationKey,
					SourceId = sourceId,
					SnapshotId = fixSnapshotId(batchId, isSnapshot),
					States = new[]
					{
						new BatchStateInputModel
						{
							StateCode = stateCode,
							StateDescription = stateDescription,
							UserCode = userCode,
						}
					}
				};

			var exceptionHandler = new LegacyReturnValue();
			_rta.Enqueue(input, exceptionHandler);
			return exceptionHandler.ReturnValue;
		}

		public int SaveBatchExternalUserState(
			string authenticationKey,
			string platformTypeId,
			string sourceId,
			ICollection<ExternalUserState> externalUserStateBatch)
		{
			_tracer.ProcessReceived("SaveBatchExternalUserState", externalUserStateBatch?.Count);
			validateParameters(authenticationKey, platformTypeId, sourceId);

			if (externalUserStateBatch?.Count == 0) return 0;

			IEnumerable<BatchStateInputModel> states = (
					from s in externalUserStateBatch
					select new BatchStateInputModel
					{
						UserCode = fixUserCode(s.UserCode),
						StateCode = fixStateCode(s.StateCode, platformTypeId, s.IsLoggedOn),
						StateDescription = s.StateDescription
					})
				.ToArray();

			var mayCloseSnapshot = externalUserStateBatch.Last();
			var isSnapshot = mayCloseSnapshot.IsSnapshot;
			var closeSnapshot = isClosingSnapshot(mayCloseSnapshot.UserCode, isSnapshot);
			if (closeSnapshot)
				states = states.Take(externalUserStateBatch.Count - 1);

			var input = new BatchInputModel
			{
				AuthenticationKey = authenticationKey,
				SourceId = sourceId,
				SnapshotId = fixSnapshotId(externalUserStateBatch.First().BatchId, isSnapshot),
				CloseSnapshot = closeSnapshot,
				States = states
			};

			var exceptionHandler = new LegacyReturnValue();
			_rta.Enqueue(input, exceptionHandler);
			return exceptionHandler.ReturnValue;
		}

		private void validateParameters(string authenticationKey, string platformTypeId, string sourceId)
		{
			const string message = @" was not received.
Make sure the SOAP message contains this value and all parameters are in correct order according to the contract.";
			if (string.IsNullOrEmpty(authenticationKey))
				log.Warn($"authenticationKey{message}");
			if (string.IsNullOrEmpty(platformTypeId))
			{
				var m = $"platformTypeId{message}";
				log.Warn(m);
				_tracer.ProcessException(new PlatformTypeIdWarning(m));
			}

			if (string.IsNullOrEmpty(sourceId))
				log.Warn($"sourceId{message}");
		}

		private class PlatformTypeIdWarning : Exception
		{
			public PlatformTypeIdWarning(string message) : base(message)
			{
			}
		}

		private static bool isClosingSnapshot(string userCode, bool isSnapshot)
		{
			return isSnapshot && string.IsNullOrEmpty(userCode);
		}

		private static string fixUserCode(string userCode)
		{
			return userCode.Trim();
		}

		private string fixStateCode(string stateCode, string platformTypeId, bool isLoggedOn)
		{
			if (!isLoggedOn)
				stateCode = "LOGGED-OFF";

			if (stateCode == null)
				return null;

			stateCode = stateCode.Trim();

			if (!string.IsNullOrEmpty(platformTypeId) && platformTypeId != Guid.Empty.ToString())
				stateCode = $"{stateCode} ({platformTypeId})";

			return stateCode;
		}

		private static DateTime? fixSnapshotId(DateTime snapshotId, bool isSnapshot)
		{
			if (!isSnapshot)
				return null;
			if (snapshotId == DateTime.MinValue)
				return null;
			return snapshotId;
		}

		public void GetUpdatedScheduleChange(Guid personId, Guid businessUnitId, DateTime timestamp, string tenant)
		{
		}
	}
}