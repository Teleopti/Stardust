using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public interface IStateStreamSynchronizer
	{
		void Sync();
		void Initialize();
	}

	public class StateStreamSynchronizer : IStateStreamSynchronizer
	{
		private readonly ITeamOutOfAdherenceReadModelPersister _teamOutOfReadModelPersister;
		private readonly ISiteOutOfAdherenceReadModelPersister _siteOutOfReadModelPersister;
		private readonly IAdherenceDetailsReadModelPersister _detailsPresister;

		private readonly INow _now;
		private readonly RtaProcessor _processor;
		private readonly IPersonOrganizationProvider _personOrganizationProvider;
		private readonly IAgentStateMessageSender _agentStateMessageSender;
		private readonly IAdherenceAggregator _adherenceAggregator;
		private readonly IDatabaseReader _databaseReader;
		private readonly AgentStateAssembler _agentStateAssembler;
		private readonly ICurrentEventPublisher _currentEventPublisher;
		private readonly IResolve _resolve;

		public StateStreamSynchronizer(
			ITeamOutOfAdherenceReadModelPersister teamOutOfReadModelPersister, 
			ISiteOutOfAdherenceReadModelPersister siteOutOfReadModelPersister, 
			IAdherenceDetailsReadModelPersister detailsPresister,  
			INow now,
			RtaProcessor processor,
			IPersonOrganizationProvider personOrganizationProvider,
			IAgentStateMessageSender agentStateMessageSender,
			IAdherenceAggregator adherenceAggregator,
			IDatabaseReader databaseReader,
			AgentStateAssembler agentStateAssembler,
			ICurrentEventPublisher currentEventPublisher,
			IResolve resolve
			)
		{
			_teamOutOfReadModelPersister = teamOutOfReadModelPersister;
			_siteOutOfReadModelPersister = siteOutOfReadModelPersister;
			_detailsPresister = detailsPresister;
			_now = now;
			_processor = processor;
			_personOrganizationProvider = personOrganizationProvider;
			_agentStateMessageSender = agentStateMessageSender;
			_adherenceAggregator = adherenceAggregator;
			_databaseReader = databaseReader;
			_agentStateAssembler = agentStateAssembler;
			_currentEventPublisher = currentEventPublisher;
			_resolve = resolve;
		}

		public void Sync()
		{
			_teamOutOfReadModelPersister.Clear();
			processStates(new TeamOutOfAdherenceReadModelUpdater(_teamOutOfReadModelPersister));
			_siteOutOfReadModelPersister.Clear();
			processStates(new SiteOutOfAdherenceReadModelUpdater(_siteOutOfReadModelPersister));
		}

		public void Initialize()
		{
			if (!_teamOutOfReadModelPersister.HasData())
				processStates(new TeamOutOfAdherenceReadModelUpdater(_teamOutOfReadModelPersister));
			if (!_siteOutOfReadModelPersister.HasData())
				processStates(new SiteOutOfAdherenceReadModelUpdater(_siteOutOfReadModelPersister));
			if(!_detailsPresister.HasData())
				processStates(new AdherenceDetailsReadModelUpdater(_detailsPresister));
		}

		private void processStates(object handler)
		{
			_databaseReader.GetActualAgentStates()
				.ForEach(s =>
				{
					var context = new RtaProcessContext(
						null,
						s.PersonId,
						s.BusinessUnitId,
						_now.UtcDateTime(),
						_personOrganizationProvider,
						new DontUpdateAgentStateReadModel(),
						_agentStateMessageSender,
						_adherenceAggregator,
						_databaseReader,
						_agentStateAssembler,
						_currentEventPublisher,
						_resolve
						);
					context.SetPreviousMakeMethodToReturnEmptyState();
					context.SetCurrentMakeMethodToReturnPreviousState(s);
					context.PublisEventsTo(handler);
					_processor.Process(context);
				});
		}
	}
}