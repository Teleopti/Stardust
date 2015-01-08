using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.WebService;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class RtaModule : Module
	{
		private readonly IIocConfiguration _config;

		public RtaModule(IIocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TeleoptiRtaService>().AsSelf().As<ITeleoptiRtaService>().SingleInstance();
			builder.RegisterType<Rta>().As<IRta>().SingleInstance();

			builder.RegisterType<DatabaseConnectionStringHandler>().As<IDatabaseConnectionStringHandler>();
			builder.RegisterType<DatabaseConnectionFactory>().As<IDatabaseConnectionFactory>();
			//mark activityalarms and stategroups to be cached
			_config.Args().CacheBuilder
				.For<DatabaseReader>()
				.CacheMethod(x => x.ActivityAlarms())
				.CacheMethod(x => x.StateGroups())
				.CacheMethod(x => x.GetCurrentSchedule(Guid.NewGuid()))
				.CacheMethod(x => x.Datasources())
				.CacheMethod(x => x.ExternalLogOns())
				.As<IDatabaseReader>();
			builder.RegisterMbCacheComponent<DatabaseReader, IDatabaseReader>();

			builder.Register<IReadActualAgentStates>(c => c.Resolve<DatabaseReader>());
			builder.RegisterType<DatabaseWriter>().As<IDatabaseWriter>().SingleInstance();

			if (_config.Toggle(Toggles.RTA_NoBroker_31237))
			{
				builder.RegisterType<NoMessagge>().As<IAgentStateMessageSender>().SingleInstance();
				builder.RegisterType<NoAggregation>().As<IAdherenceAggregator>().SingleInstance();
			}
			else
			{
				builder.RegisterType<AgentStateMessageSender>().As<IAgentStateMessageSender>().SingleInstance();
				builder.RegisterType<AdherenceAggregator>().As<IAdherenceAggregator>().SingleInstance();
			}

			builder.RegisterType<CalculateAdherence>().SingleInstance().As<ICalculateAdherence>();
			builder.RegisterType<CalculateAdherenceDetails>().SingleInstance().As<ICalculateAdherenceDetails>();

			if (_config.Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783) ||
				_config.Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285))
			{
				builder.RegisterType<ShiftEventPublisher>().SingleInstance().As<IShiftEventPublisher>();
				builder.RegisterType<AdherenceEventPublisher>().SingleInstance().As<IAdherenceEventPublisher>();
			}
			else
			{
				builder.RegisterType<NoEvents>().SingleInstance().As<IShiftEventPublisher>();
				builder.RegisterType<NoEvents>().SingleInstance().As<IAdherenceEventPublisher>();
			}

			if (_config.Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783) ||
				_config.Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285))
			{
				builder.RegisterType<StateEventPublisher>().SingleInstance().As<IStateEventPublisher>();
				builder.RegisterType<ActivityEventPublisher>().SingleInstance().As<IActivityEventPublisher>();
			}
			else
			{
				builder.RegisterType<NoEvents>().SingleInstance().As<IStateEventPublisher>();
				builder.RegisterType<NoEvents>().SingleInstance().As<IActivityEventPublisher>();
			}

			builder.RegisterType<OrganizationForPerson>().SingleInstance().As<IOrganizationForPerson>();

			builder.RegisterType<StateStreamSynchronizer>().As<IStateStreamSynchronizer>().SingleInstance();

			_config.Args().CacheBuilder
				.For<PersonOrganizationProvider>()
				.CacheMethod(svc => svc.PersonOrganizationData())
				.As<IPersonOrganizationProvider>();
			builder.RegisterMbCacheComponent<PersonOrganizationProvider, IPersonOrganizationProvider>().SingleInstance();

			//messy for now
			builder.Register(c => new PersonOrganizationReader(c.Resolve<INow>(), c.Resolve<IDatabaseConnectionStringHandler>().AppConnectionString()))
				.SingleInstance().As<IPersonOrganizationReader>();
		}
	}

	public interface IStateStreamSynchronizer
	{
		void Sync();
		void Initialize();
	}

	public class StateStreamSynchronizer : IStateStreamSynchronizer
	{
		private readonly ITeamAdherencePersister _teamPersister;
		private readonly ISiteAdherencePersister _sitePersister;
		private readonly IAdherenceDetailsReadModelPersister _detailsPresister;

		private readonly IPersonOrganizationReader _reader;
		private readonly IDatabaseReader _reader2;
		private readonly INow _now;

		public StateStreamSynchronizer(ITeamAdherencePersister teamPersister, ISiteAdherencePersister sitePersister, 
						IAdherenceDetailsReadModelPersister detailsPresister,  IPersonOrganizationReader reader, IDatabaseReader reader2, INow now)
		{
			_teamPersister = teamPersister;
			_sitePersister = sitePersister;
			_detailsPresister = detailsPresister;
			_reader = reader;
			_reader2 = reader2;
			_now = now;
		}

		public void Sync()
		{
			_teamPersister.Clear();
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
			if (!_teamPersister.HasData())
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
			var data = from s in _reader2.GetActualAgentStates()
					   group s by organizationalData(s).TeamId
						   into states
						   select new
						   {
							   teamId = states.Key,
							   outOfAdherence = countOutOfAdherence(states)
						   };
			data.ForEach(s => _teamPersister.Persist(new TeamAdherenceReadModel
			{
				TeamId = s.teamId,
				AgentsOutOfAdherence = s.outOfAdherence
			}));
		}

		private static int countOutOfAdherence(IEnumerable<IActualAgentState> states)
		{
			return states.Count(s => StateInfo.AdherenceFor(s) == Domain.ApplicationLayer.Rta.Adherence.Out);
		}

		private PersonOrganizationData organizationalData(IActualAgentState actualAgentState)
		{
			return (from d in _reader.PersonOrganizationData()
					where d.PersonId == actualAgentState.PersonId
					select d).Single();
		}

	}
}