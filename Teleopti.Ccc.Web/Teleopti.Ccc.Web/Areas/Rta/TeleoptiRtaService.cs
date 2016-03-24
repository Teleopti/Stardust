﻿using System;
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
				_rta.SaveState(new ExternalUserStateInputModel
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
				});
			});
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
				_rta.SaveStateBatch(states.ToArray());
			});
		}

		public void GetUpdatedScheduleChange(Guid personId, Guid businessUnitId, DateTime timestamp, string tenant)
		{
			_rta.ReloadSchedulesOnNextCheckForActivityChanges(tenant, personId);
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
			catch (BatchTooBigException e)
			{
				throw new FaultException<BatchTooBigException>(e);
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