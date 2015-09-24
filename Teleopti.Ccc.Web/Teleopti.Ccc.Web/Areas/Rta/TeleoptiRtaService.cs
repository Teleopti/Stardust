using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Rta.WebService;

namespace Teleopti.Ccc.Web.Areas.Rta
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single),
	 AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class TeleoptiRtaService : ITeleoptiRtaService
	{
		private readonly Domain.ApplicationLayer.Rta.Service.Rta _rta;

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
			return handleRtaExceptions(() => _rta.SaveState(
				new ExternalUserStateInputModel
				{
					AuthenticationKey = authenticationKey,
					UserCode = userCode,
					StateCode = stateCode,
					StateDescription = stateDescription,
					IsLoggedOn = isLoggedOn,
					PlatformTypeId = platformTypeId,
					SourceId = sourceId,
					BatchId = batchId,
					IsSnapshot = isSnapshot
				}));
		}

		public int SaveBatchExternalUserState(string authenticationKey, string platformTypeId, string sourceId, ICollection<ExternalUserState> externalUserStateBatch)
		{
			return handleRtaExceptions(() =>
			{
				var states = from s in externalUserStateBatch
					select new ExternalUserStateInputModel
					{
						AuthenticationKey = authenticationKey,
						UserCode = s.UserCode,
						StateCode = s.StateCode,
						StateDescription = s.StateDescription,
						IsLoggedOn = s.IsLoggedOn,
						PlatformTypeId = platformTypeId,
						SourceId = sourceId,
						BatchId = s.BatchId,
						IsSnapshot = s.IsSnapshot
					};
				return _rta.SaveStateBatch(states.ToArray());
			});
		}

		public void GetUpdatedScheduleChange(Guid personId, Guid businessUnitId, DateTime timestamp, string tenant)
		{
			_rta.ReloadSchedulesOnNextCheckForActivityChanges(tenant, personId);
		}

		private int handleRtaExceptions(Func<int> rtaCall)
		{
			try
			{
				return rtaCall.Invoke();
			}
			catch (InvalidAuthenticationKeyException e)
			{
				throw new FaultException<InvalidAuthenticationKeyException>(e);
			}
			catch (LegacyAuthenticationKeyException e)
			{
				throw new FaultException<LegacyAuthenticationKeyException>(e);
			}
			catch (BatchTooBigException e)
			{
				throw new FaultException<BatchTooBigException>(e);
			}
		}

	}
}