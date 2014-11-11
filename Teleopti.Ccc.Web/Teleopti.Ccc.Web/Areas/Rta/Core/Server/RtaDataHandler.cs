using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class RtaDataHandler
	{
		private static readonly ILog loggingSvc = LogManager.GetLogger(typeof(RtaDataHandler));
		private readonly IAdherenceAggregator _adherenceAggregator;
		private readonly IRtaEventPublisher _eventPublisher;

		private readonly ActualAgentAssembler _agentAssembler;
		private readonly IDatabaseWriter _databaseWriter;
		private readonly IMbCacheFactory _mbCacheFactory;
		private readonly ISignalRClient _messageClient;
		private readonly IMessageSender _messageSender;
		private readonly IDatabaseReader _databaseReader;
		private readonly DataSourceResolver _dataSourceResolver;
		private readonly PersonResolver _personResolver;

		public RtaDataHandler(ISignalRClient messageClient, IMessageSender messageSender, IDatabaseReader databaseReader,
			IDatabaseWriter databaseWriter, IMbCacheFactory mbCacheFactory, IAdherenceAggregator adherenceAggregator,
			IRtaEventPublisher eventPublisher)
		{
			_messageClient = messageClient;
			_messageSender = messageSender;
			_databaseReader = databaseReader;
			_dataSourceResolver = new DataSourceResolver(databaseReader);
			_personResolver = new PersonResolver(databaseReader);
			_agentAssembler = new ActualAgentAssembler(databaseReader, databaseWriter, mbCacheFactory);
			_databaseWriter = databaseWriter;
			_mbCacheFactory = mbCacheFactory;
			_adherenceAggregator = adherenceAggregator;
			_eventPublisher = eventPublisher;
		}

		public void ProcessScheduleUpdate(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			_mbCacheFactory.Invalidate(_databaseReader, x => x.GetCurrentSchedule(personId), true);
			var state = getState(personId, businessUnitId, null, TimeSpan.Zero, timestamp, Guid.Empty, null, null);
			send(state);
		}

		public bool IsAlive
		{
			get { return _messageClient.IsAlive; }
		}

		public int ProcessRtaData(string logOn, string stateCode, TimeSpan timeInState, DateTime timestamp, Guid platformTypeId, string sourceId, DateTime batchId, bool isSnapshot)
		{
			int dataSourceId;
			var batch = isSnapshot
				? batchId
				: (DateTime?)null;

			if (!_dataSourceResolver.TryResolveId(sourceId, out dataSourceId))
			{
				loggingSvc.WarnFormat(
					"No data source available for source id = {0}. Event will not be handled before data source is set up.", sourceId);
				return 0;
			}

			if (isSnapshot && string.IsNullOrEmpty(logOn))
			{
				loggingSvc.InfoFormat("Last of batch detected, initializing handling for batch id: {0}, source id: {1}", batchId, sourceId);
				handleClosingOfSnapshot(batchId, sourceId);
				return 0;
			}

			IEnumerable<PersonWithBusinessUnit> personWithBusinessUnits;
			if (!_personResolver.TryResolveId(dataSourceId, logOn, out personWithBusinessUnits))
			{
				loggingSvc.InfoFormat("No person available for datasource id = {0} and log on {1}. Event will not be handled before person is set up.", dataSourceId, logOn);
				return 0;
			}

			foreach (var p in personWithBusinessUnits)
			{
				loggingSvc.DebugFormat("ACD-Logon: {0} is connected to PersonId: {1}", logOn, p.PersonId);
				var state = getState(p.PersonId, p.BusinessUnitId, stateCode, timeInState, timestamp, platformTypeId, sourceId, batch);
				send(state);
			}
			return 1;
		}

		private void handleClosingOfSnapshot(DateTime batchId, string sourceId)
		{
			var missingAgents = _agentAssembler.GetAgentStatesForMissingAgents(batchId, sourceId);
			foreach (var agent in missingAgents.Where(agent => agent != null))
			{
				_databaseWriter.PersistActualAgentState(agent);
				send(new StateInfo { NewState = agent });
			}
		}

		private StateInfo getState(
			Guid personId,
			Guid businessUnitId,
			string stateCode,
			TimeSpan timeInState,
			DateTime timestamp,
			Guid platformTypeId,
			string sourceId,
			DateTime? batch)
		{
			var info = new StateInfo
			{
				ScheduleLayers = _databaseReader.GetCurrentSchedule(personId),
				PreviousState = _databaseReader.GetCurrentActualAgentState(personId),
			};

			if (info.PreviousState == null)
				info.PreviousState = new ActualAgentState
				{
					PersonId = personId,
					StateId = Guid.NewGuid(),
				};

			info.CurrentActivity = activityForTime(info, timestamp);
			info.NextActivityInShift = nextAdjacentActivityForTime(info, timestamp);
			info.PreviousActivity = activityForTime(info, info.PreviousState.ReceivedTime);

			info.NewState = _agentAssembler.GetAgentState(
				info.CurrentActivity,
				info.NextActivityInShift,
				info.PreviousState,
				personId,
				businessUnitId,
				platformTypeId,
				stateCode,
				timestamp,
				timeInState,
				batch,
				sourceId);

			info.WasScheduled = info.PreviousState.ScheduledId != Guid.Empty;
			info.IsScheduled = info.NewState.ScheduledId != Guid.Empty;

			info.CurrentShiftStartTime = startTimeOfShift(info, info.CurrentActivity);
			info.CurrentShiftEndTime = endTimeOfShift(info, info.CurrentActivity);
			info.PreviousShiftStartTime = startTimeOfShift(info, info.PreviousActivity);
			info.PreviousShiftEndTime = endTimeOfShift(info, info.PreviousActivity);

			return info;
		}

		private static ScheduleLayer activityForTime(StateInfo info, DateTime time)
		{
			return info.ScheduleLayers.FirstOrDefault(l => l.EndDateTime >= time && l.StartDateTime <= time);
		}

		private static ScheduleLayer nextAdjacentActivityForTime(StateInfo info, DateTime time)
		{
			var nextActivity = (from l in info.ScheduleLayers where l.StartDateTime > time select l).FirstOrDefault();
			if (nextActivity == null)
				return null;
			if (info.CurrentActivity == null)
				return nextActivity;
			if (nextActivity.StartDateTime == info.CurrentActivity.EndDateTime)
				return nextActivity;
			return null;
		}

		private static DateTime startTimeOfShift(StateInfo info, ScheduleLayer activity)
		{
			if (activity == null)
				return DateTime.MinValue;
			return activitiesThisShift(info, activity).Select(x => x.StartDateTime).Min();
		}

		private static DateTime endTimeOfShift(StateInfo info, ScheduleLayer activity)
		{
			if (activity == null)
				return DateTime.MinValue;
			return activitiesThisShift(info, activity).Select(x => x.EndDateTime).Max();
		}

		private static IEnumerable<ScheduleLayer> activitiesThisShift(StateInfo info, ScheduleLayer activity)
		{
			return from l in info.ScheduleLayers
				   where l.BelongsToDate == activity.BelongsToDate
				   select l;
		}

		private void send(StateInfo state)
		{
			_databaseWriter.PersistActualAgentState(state.NewState);

			loggingSvc.InfoFormat("Sending message: {0} ", state.NewState);

			var notification = NotificationFactory.CreateNotification(state.NewState);

			if (state.Send)
				_messageSender.Send(notification);

			// this means closing of snapshot, and aggregation and events doesnt work here.
			// BUG
			if (state.ScheduleLayers == null)
				return;

			_adherenceAggregator.Aggregate(state.NewState);
			
			_eventPublisher.Publish(state);
		}
	}
}
