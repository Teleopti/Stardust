using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Teleopti.Ccc.Rta.WebService;

namespace Teleopti.Ccc.Web.Areas.Rta
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single),
	 AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class TeleoptiRtaService : ITeleoptiRtaService
	{
		private readonly IRta _rta;

		public TeleoptiRtaService(IRta rta)
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
			return _rta.SaveState(
				authenticationKey, 
				userCode, 
				stateCode, 
				stateDescription,
				isLoggedOn, 
				secondsInState, 
				timestamp, 
				platformTypeId, 
				sourceId, 
				batchId,
				isSnapshot);
		}

		public int SaveBatchExternalUserState(string authenticationKey, string platformTypeId, string sourceId, ICollection<ExternalUserState> externalUserStateBatch)
		{
			return _rta.SaveStateBatch(authenticationKey, platformTypeId, sourceId, externalUserStateBatch);
		}

		public void GetUpdatedScheduleChange(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			_rta.CheckForActivityChange(personId, businessUnitId, timestamp);
		}
	}
}