using System;
using System.Collections.Generic;
using log4net;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers;
using Teleopti.Interfaces.Domain;

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
		private readonly IPersonOrganizationProvider _personOrganizationProvider;

		private readonly ActualAgentAssembler _agentAssembler;
		private readonly IDatabaseWriter _databaseWriter;
		private readonly IMbCacheFactory _mbCacheFactory;
		private readonly IAgentStateMessageSender _messageSender;
		private readonly IDatabaseReader _databaseReader;
		private readonly PersonResolver _personResolver;
		
		public RtaDataHandler(
			IAgentStateMessageSender messageSender,
			IAdherenceAggregator adherenceAggregator,
			IDatabaseReader databaseReader,
			IDatabaseWriter databaseWriter,
			IMbCacheFactory mbCacheFactory,
			IShiftEventPublisher shiftEventPublisher,
			IActivityEventPublisher activityEventPublisher,
			IStateEventPublisher stateEventPublisher,
			INow now,
			IPersonOrganizationProvider personOrganizationProvider)
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
			_personOrganizationProvider = personOrganizationProvider;
		}

		public void CheckForActivityChange(Guid personId, Guid businessUnitId)
		{
			_mbCacheFactory.Invalidate(_databaseReader, x => x.GetCurrentSchedule(personId), true);
			process(
				new ExternalUserStateInputModel(),
				personId,
				businessUnitId
				);
		}

		public int ProcessStateChange(ExternalUserStateInputModel input, int dataSourceId)
		{
			IEnumerable<ResolvedPerson> personWithBusinessUnits;
			if (!_personResolver.TryResolveId(dataSourceId, input.UserCode, out personWithBusinessUnits))
			{
				loggingSvc.InfoFormat("No person available for datasource id = {0} and log on {1}. Event will not be handled before person is set up.", dataSourceId, input.UserCode);
				return 0;
			}

			//GLHF
			foreach (var p in personWithBusinessUnits)
			{
				loggingSvc.DebugFormat("ACD-Logon: {0} is connected to PersonId: {1}", input.UserCode, p.PersonId);
				process(input, p.PersonId, p.BusinessUnitId);
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
				input.PlatformTypeId = Guid.Empty.ToString();
				process(input, agent.PersonId, agent.BusinessUnitId);
			}
			return 1;
		}

		private void process(
			ExternalUserStateInputModel input,
			Guid personId,
			Guid businessUnitId
			)
		{

			PersonOrganizationData person;
			if (!_personOrganizationProvider.PersonOrganizationData().TryGetValue(personId, out person))
				return;
			person.BusinessUnitId = businessUnitId;

			var info = new StateInfo(
				_databaseReader,
				_agentAssembler,
				person,
				input,
				_now.UtcDateTime()
				);

			_databaseWriter.PersistActualAgentState(info.NewState);

			if (info.Send)
				_messageSender.Send(info);

			_adherenceAggregator.Aggregate(info);

			_shiftEventPublisher.Publish(info);
			_activityEventPublisher.Publish(info);
			_stateEventPublisher.Publish(info);
		}

		public void Initialize()
		{
			_adherenceAggregator.Initialize();
		}
	}
}
