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
		private readonly ISiteAdherencePersister _sitePersister;
		private readonly IAdherenceDetailsReadModelPersister _detailsPresister;

		private readonly IPersonOrganizationReader _reader;
		private readonly IDatabaseReader _reader2;
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
			ISiteAdherencePersister sitePersister, 
			IAdherenceDetailsReadModelPersister detailsPresister,  
			IPersonOrganizationReader reader, 
			IDatabaseReader reader2, 
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
			_sitePersister = sitePersister;
			_detailsPresister = detailsPresister;
			_reader = reader;
			_reader2 = reader2;
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
			persistReadModelForTeam();
			_sitePersister.Clear();
			persistReadModelForSite();

		}

		public void Initialize()
		{
			handleTeam();
			handleSite();
			handleDetails();
		}

		private void handleDetails()
		{
			
			var agentStates = _reader2.GetActualAgentStates();
			if (!agentStates.Any()) return;
			var personActivities = _reader2.GetCurrentSchedule(agentStates.First().PersonId);
			if (!personActivities.Any()) return;

			_detailsPresister.Add(new AdherenceDetailsReadModel
			{
				Date = _now.UtcDateTime().Date,
				PersonId = agentStates.First().PersonId,
				Model = new AdherenceDetailsModel
				{
					Details = new List<AdherenceDetailModel>(new[]
					{
						new AdherenceDetailModel
						{
							StartTime = personActivities.First().StartDateTime
						}
					})
				}
			});
		}

		private void handleTeam()
		{
			if (!_teamOutOfReadModelPersister.HasData())
				persistReadModelForTeam();
		}

		private void handleSite()
		{
			if (!_sitePersister.HasData())
				persistReadModelForSite();
		}

		private void persistReadModelForSite()
		{
			var data = from s in _reader2.GetActualAgentStates()
				group s by organizationalData(s).SiteId
				into states
				select new
				{
					siteId = states.Key,
					outOfAdherence = countOutOfAdherence(states)
				};

			data.ForEach(s => _sitePersister.Persist(new SiteAdherenceReadModel
			{
				SiteId = s.siteId,
				AgentsOutOfAdherence = s.outOfAdherence
			}));
		}

		private void persistReadModelForTeam()
		{
			_reader2.GetActualAgentStates()
				.OrderBy(s => StateInfo.AdherenceFor(s) == Domain.ApplicationLayer.Rta.Adherence.In ? 0 : 1)
				.ToList()
				.ForEach(s =>
				{
					var context = new RtaProcessContext(
						null,
						s.PersonId,
						s.BusinessUnitId,
						s.ReceivedTime,
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
					context.PublisEventsTo(new TeamOutOfAdherenceReadModelUpdater(_teamOutOfReadModelPersister));
					_processor.Process(context);
				});
		}

		private static int countOutOfAdherence(IEnumerable<AgentStateReadModel> states)
		{
			return states.Count(s => StateInfo.AdherenceFor(s) == Domain.ApplicationLayer.Rta.Adherence.Out);
		}

		private PersonOrganizationData organizationalData(AgentStateReadModel agentStateReadModel)
		{
			return (from d in _reader.PersonOrganizationData()
				where d.PersonId == agentStateReadModel.PersonId
				select d).Single();
		}

	}
}