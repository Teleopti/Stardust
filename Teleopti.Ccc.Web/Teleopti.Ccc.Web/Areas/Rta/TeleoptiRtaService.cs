using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using log4net;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Rta.WebService;

namespace Teleopti.Ccc.Web.Areas.Rta
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class TeleoptiRtaService : ITeleoptiRtaService
	{
		private readonly Domain.ApplicationLayer.Rta.Service.Rta _rta;
		private static readonly ILog Log = LogManager.GetLogger(typeof(TeleoptiRtaService));
		private static readonly ILog _logger = LogManager.GetLogger("PerfLog.Rta");

		public TeleoptiRtaService(Domain.ApplicationLayer.Rta.Service.Rta rta)
		{
			_rta = rta;
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
			return handleRtaExceptions(() =>
			{
				userCode = fixUserCode(userCode);
				stateCode = fixStateCode(stateCode, isLoggedOn);
				if (isClosingSnapshot(userCode, isSnapshot))
				{
					_rta.CloseSnapshot(new CloseSnapshotInputModel
					{
						AuthenticationKey = authenticationKey,
						SnapshotId = batchId,
						SourceId = sourceId
					});
				}
				else
				{
					_rta.SaveState(new StateInputModel
					{
						AuthenticationKey = authenticationKey,
						UserCode = userCode,
						StateCode = stateCode,
						StateDescription = stateDescription,
						PlatformTypeId = platformTypeId,
						SourceId = sourceId,
						SnapshotId = fixSnapshotId(batchId, isSnapshot)
					});
				}
			});
		}

		public int SaveBatchExternalUserState(string authenticationKey, string platformTypeId, string sourceId, ICollection<ExternalUserState> externalUserStateBatch)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var response = handleRtaExceptions(() =>
			{
				IEnumerable<BatchStateInputModel> states = (
					from s in externalUserStateBatch
					select new BatchStateInputModel
					{
						UserCode = fixUserCode(s.UserCode),
						StateCode = fixStateCode(s.StateCode, s.IsLoggedOn),
						StateDescription = s.StateDescription,
					})
					.ToArray();

				var mayCloseSnapshot = externalUserStateBatch.Last();
				var isSnapshot = mayCloseSnapshot.IsSnapshot;
				var closeSnapshot = isClosingSnapshot(mayCloseSnapshot.UserCode, isSnapshot);
				if (closeSnapshot)
					states = states.Take(externalUserStateBatch.Count - 1);

				_rta.SaveStateBatch(new BatchInputModel
				{
					AuthenticationKey = authenticationKey,
					PlatformTypeId = platformTypeId,
					SourceId = sourceId,
					SnapshotId = fixSnapshotId(externalUserStateBatch.First().BatchId, isSnapshot),
					States = states
				});

				if (closeSnapshot)
					_rta.CloseSnapshot(new CloseSnapshotInputModel
					{
						AuthenticationKey = authenticationKey,
						SnapshotId = mayCloseSnapshot.BatchId,
						SourceId = sourceId
					});
			});

			stopwatch.Stop();
			_logger.Info($"Request completed, batchsize: {externalUserStateBatch.Count}, time: {stopwatch.ElapsedMilliseconds} => {stopwatch.ElapsedMilliseconds / externalUserStateBatch.Count} ms / state");

			return response;
		}

		private static bool isClosingSnapshot(string userCode, bool isSnapshot)
		{
			return isSnapshot && string.IsNullOrEmpty(userCode);
		}

		private static string fixUserCode(string userCode)
		{
			return userCode.Trim();
		}

		private static string fixStateCode(string stateCode, bool isLoggedOn)
		{
			if (!isLoggedOn)
				return "LOGGED-OFF";
			if (stateCode == null)
				return null;
			stateCode = stateCode.Trim();
			const int stateCodeMaxLength = 25;
			if (stateCode.Length > stateCodeMaxLength)
				return stateCode.Substring(0, stateCodeMaxLength);
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

		private int handleRtaExceptions(Action rtaCall)
		{
			try
			{
				rtaCall.Invoke();
				return 1;
			}
			catch (InvalidSourceException e)
			{
				Log.Error("Source id was invalid.", e);
				return -300;
			}
			catch (InvalidPlatformException e)
			{
				Log.Error("Platform id was invalid.", e);
				return -200;
			}
			catch (InvalidUserCodeException e)
			{
				Log.Info("User code was invalid.", e);
				return -100;
			}
			catch (AggregateException e)
			{
				var onlyInvalidUserCode =
					e.AllExceptions()
						.Where(x => x.GetType() != typeof (AggregateException))
						.All(x => x.GetType() == typeof (InvalidUserCodeException));
				if (onlyInvalidUserCode)
				{
					Log.Info("Batch contained invalid user code.", e);
					return -100;
				}
				throw;
			}
		}
	}
}