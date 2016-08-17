using System;
using System.Collections.Generic;
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
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class TeleoptiRtaService : ITeleoptiRtaService
	{
		private readonly Domain.ApplicationLayer.Rta.Service.Rta _rta;
		private static readonly ILog Log = LogManager.GetLogger(typeof(TeleoptiRtaService));

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
						SnapshotId = fixSnapshotId(batchId)
					});
				}
			});
		}

		public int SaveBatchExternalUserState(string authenticationKey, string platformTypeId, string sourceId, ICollection<ExternalUserState> externalUserStateBatch)
		{
			return handleRtaExceptions(() =>
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
				var closeSnapshot = isClosingSnapshot(mayCloseSnapshot.UserCode, mayCloseSnapshot.IsSnapshot);
				if (closeSnapshot)
					states = states.Take(externalUserStateBatch.Count - 1);

				_rta.SaveStateBatch(new BatchInputModel
				{
					AuthenticationKey = authenticationKey,
					PlatformTypeId = platformTypeId,
					SourceId = sourceId,
					SnapshotId = fixSnapshotId(externalUserStateBatch.First().BatchId),
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
				return Domain.ApplicationLayer.Rta.Service.Rta.LogOutStateCode;
			if (stateCode == null)
				return null;
			stateCode = stateCode.Trim();
			const int stateCodeMaxLength = 25;
			if (stateCode.Length > stateCodeMaxLength)
				return stateCode.Substring(0, stateCodeMaxLength);
			return stateCode;
		}

		private static DateTime? fixSnapshotId(DateTime batchId)
		{
			return batchId == DateTime.MinValue ? (DateTime?)null : batchId;
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
			catch (InvalidAuthenticationKeyException e)
			{
				throw new FaultException<InvalidAuthenticationKeyException>(e);
			}
			catch (LegacyAuthenticationKeyException e)
			{
				throw new FaultException<LegacyAuthenticationKeyException>(e);
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
				Log.Error("Exceptions while processing batch.", e);
				return -500;
			}
		}
	}
}