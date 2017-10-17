﻿using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using log4net;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Rta.WebService;

namespace Teleopti.Ccc.Web.Areas.Rta
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class TeleoptiRtaService : ITeleoptiRtaService
	{
		private readonly Domain.ApplicationLayer.Rta.Service.Rta _rta;
		private readonly IToggleManager _toggles;
		private readonly IRtaTracer _rtaTracer;
		private static readonly ILog Log = LogManager.GetLogger(typeof(TeleoptiRtaService));

		public TeleoptiRtaService(Domain.ApplicationLayer.Rta.Service.Rta rta, IToggleManager toggles, IRtaTracer rtaTracer)
		{
			_rta = rta;
			_toggles = toggles;
			_rtaTracer = rtaTracer;
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
			_rtaTracer.ProcessReceived();
			return handleRtaExceptions(() =>
			{
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
				if (_toggles.IsEnabled(Toggles.RTA_AsyncOptimization_43924))
					_rta.Enqueue(input);
				else
					_rta.Process(input);
			});
		}

		public int SaveBatchExternalUserState(
			string authenticationKey,
			string platformTypeId,
			string sourceId,
			ICollection<ExternalUserState> externalUserStateBatch)
		{
			_rtaTracer.ProcessReceived();
			return handleRtaExceptions(() =>
			{
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
				if (_toggles.IsEnabled(Toggles.RTA_AsyncOptimization_43924))
					_rta.Enqueue(input);
				else
					_rta.Process(input);
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

		private int handleRtaExceptions(Action rtaCall)
		{
			try
			{
				rtaCall.Invoke();
				if (_toggles.IsEnabled(Toggles.RTA_AsyncOptimization_43924))
					return 1;
				return 2;
			}
			catch (InvalidSourceException e)
			{
				Log.Error("Source id was invalid.", e);
				return -300;
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
						.Where(x => x.GetType() != typeof(AggregateException))
						.All(x => x.GetType() == typeof(InvalidUserCodeException));
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