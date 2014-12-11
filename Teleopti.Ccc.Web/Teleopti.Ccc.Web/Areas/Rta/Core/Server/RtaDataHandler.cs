using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
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
		private readonly IShiftEventPublisher _shiftEventPublisher;
		private readonly IActivityEventPublisher _activityEventPublisher;
		private readonly IStateEventPublisher _stateEventPublisher;
		private readonly INow _now;

		private readonly ActualAgentAssembler _agentAssembler;
		private readonly IDatabaseWriter _databaseWriter;
		private readonly IMbCacheFactory _mbCacheFactory;
		private readonly IMessageSender _messageSender;
		private readonly IDatabaseReader _databaseReader;
		private readonly PersonResolver _personResolver;
		
		public RtaDataHandler(
			IMessageSender messageSender,
			IDatabaseReader databaseReader,
			IDatabaseWriter databaseWriter,
			IMbCacheFactory mbCacheFactory,
			IAdherenceAggregator adherenceAggregator,
			IShiftEventPublisher shiftEventPublisher,
			IActivityEventPublisher activityEventPublisher,
			IStateEventPublisher stateEventPublisher,
			INow now)
		{
			_messageSender = messageSender;
			_databaseReader = databaseReader;
			_personResolver = new PersonResolver(databaseReader);
			_agentAssembler = new ActualAgentAssembler(databaseReader, databaseWriter, mbCacheFactory);
			_databaseWriter = databaseWriter;
			_mbCacheFactory = mbCacheFactory;
			_adherenceAggregator = adherenceAggregator;
			_shiftEventPublisher = shiftEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
			_now = now;
		}

		public void CheckForActivityChange(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			_mbCacheFactory.Invalidate(_databaseReader, x => x.GetCurrentSchedule(personId), true);
			process(
				new ExternalUserStateInputModel {Timestamp = timestamp},
				new PersonWithBusinessUnit {BusinessUnitId = businessUnitId, PersonId = personId}
				);
		}

		public int ProcessStateChange(ExternalUserStateInputModel input, int dataSourceId)
		{
			IEnumerable<PersonWithBusinessUnit> personWithBusinessUnits;
			if (!_personResolver.TryResolveId(dataSourceId, input.UserCode, out personWithBusinessUnits))
			{
				loggingSvc.InfoFormat("No person available for datasource id = {0} and log on {1}. Event will not be handled before person is set up.", dataSourceId, input.UserCode);
				return 0;
			}

			//GLHF
			foreach (var p in personWithBusinessUnits)
			{
				loggingSvc.DebugFormat("ACD-Logon: {0} is connected to PersonId: {1}", input.UserCode, p.PersonId);
				process(input, p);
			}
			return 1;
		}

		public int CloseSnapshot(ExternalUserStateInputModel input, int dataSourceId)
		{
			var sourceId = input.SourceId;
			var batchId = input.BatchId;

			loggingSvc.InfoFormat("Last of batch detected, initializing handling for batch id: {0}, source id: {1}", batchId, sourceId);
			var missingAgents = _agentAssembler.GetAgentStatesForMissingAgents(batchId, sourceId);
			foreach (var agent in missingAgents)
			{
				input.StateCode = "CCC Logged out";
				input.SecondsInState = 0;
				input.PlatformTypeId = Guid.Empty.ToString();
				process(input, new PersonWithBusinessUnit {BusinessUnitId = agent.BusinessUnitId, PersonId = agent.PersonId});
			}
			return 1;
		}

		private void process(
			ExternalUserStateInputModel input,
			PersonWithBusinessUnit person
			)
		{
			var info = new StateInfo(
				_databaseReader,
				_agentAssembler,
				person,
				input,
				_now.UtcDateTime()
				);

			_databaseWriter.PersistActualAgentState(info.NewState);

			if (info.Send)
				_messageSender.Send(NotificationFactory.CreateNotification(info.NewState));

			_adherenceAggregator.Aggregate(info.NewState);

			_shiftEventPublisher.Publish(info);
			_activityEventPublisher.Publish(info);
			_stateEventPublisher.Publish(info);
		}
	}
}
