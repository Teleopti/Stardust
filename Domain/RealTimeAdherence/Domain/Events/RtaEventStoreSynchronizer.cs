using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events
{
	public interface IRtaEventStoreSynchronizer
	{
		void Synchronize(Guid personId, DateTime startTime);
	}


	public class NoRtaEventStoreSynchronizer : IRtaEventStoreSynchronizer
	{
		public void Synchronize(Guid personId, DateTime startTime)
		{
			
		}
	}

	public class RtaEventStoreSynchronizer : IRtaEventStoreSynchronizer
	{
		private readonly IRtaEventStoreReader _events;
		private readonly IHistoricalOverviewReadModelPersister _readModels;
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;
		private readonly IAgentAdherenceDayLoader _adherenceDayLoader;
		private readonly INow _now;

		public RtaEventStoreSynchronizer(IRtaEventStoreReader events, IHistoricalOverviewReadModelPersister readModels, IAgentStateReadModelReader agentStateReadModelReader, IAgentAdherenceDayLoader adherenceDayLoader, INow now)
		{
			_events = events;
			_readModels = readModels;
			_agentStateReadModelReader = agentStateReadModelReader;
			_adherenceDayLoader = adherenceDayLoader;
			_now = now;
		}

		public void Synchronize(Guid personId, DateTime startTime)
		{
//			var agent = _agentStateReadModelReader.Read(new[] {personId}).FirstOrDefault();
//			if (agent != null)
//			{
//				UpdateReadModel(agent, startTime);
//			}
		}


		private void UpdateReadModel(AgentStateReadModel agent, DateTime startTime)
		{
			//UTC??
			//var sevenDays = startTime.Date.AddDays(-6).DateRange(7);
//			var sevenDays = _now.UtcDateTime().Date.AddDays(-7).DateRange(7);
//
//			sevenDays.ForEach(day => _readModels.Upsert(
//						new HistoricalOverviewReadModel
//						{
//							PersonId = agent.PersonId,
//							// change later, must get for the current date 
//							FirstName = agent?.FirstName,
//							LastName = agent?.LastName,
//							Date = day,
//							SiteName = agent?.SiteName,  
//							TeamId = agent?.TeamId,      
//							TeamName = agent?.TeamName, 
//							Adherence = 0 // _adherenceDayLoader.Load(agent.PersonId, day.ToDateOnly()).Percentage()
//						}
//					)
//				);


//			var teams =
//				from agent in agents
//				group agent by agent.SiteAndTeam
//				into teamGroupedAgents
//				select new HistoricalOverviewTeamViewModel
//				{
//					Name = teamGroupedAgents.First().SiteAndTeam,
//					Agents = (from agent in teamGroupedAgents
//						let adherenceDays =
//							from d in sevenDays
//							let loadedDay = _agentAdherenceDayLoader.Load(agent.Id, d.ToDateOnly())
//							let change = loadedDay.Changes().FirstOrDefault(change => change.LateForWork != null)
//							let lateForWorkText = change != null ? change.LateForWork : "0"
//							let minutesLateForWork = int.Parse(Regex.Replace(lateForWorkText, "[^0-9.]", ""))
//							select new
//							{
//								LoadedDay = loadedDay,
//								MinutesLateForWork = minutesLateForWork,
//								d.Date
//							}
//						select new HistoricalOverviewAgentViewModel()
//						{
//							Id = agent.Id,
//							Name = agent.Name,
//							Days = (from day in adherenceDays
//								select new HistoricalOverviewDayViewModel
//								{
//									Date = day.Date.ToString("yyyyMMdd"),
//									DisplayDate = day.Date.ToString("MM") + "/" + day.Date.ToString("dd"),
//									Adherence = day.LoadedDay.Percentage(),
//									WasLateForWork = day.LoadedDay.Changes().Any(x => x.LateForWork != null)
//								}).ToArray(),
//							LateForWork = new HistoricalOverviewLateForWorkViewModel
//							{
//								Count = adherenceDays.Count(ad => ad.MinutesLateForWork > 0),
//								TotalMinutes = adherenceDays.Sum(ad => ad.MinutesLateForWork)
//							}
//						}).ToArray(),
//				};			
		}
	}
}