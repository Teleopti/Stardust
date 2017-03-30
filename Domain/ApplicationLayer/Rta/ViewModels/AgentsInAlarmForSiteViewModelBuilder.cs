using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class AgentsInAlarmForSiteViewModelBuilder
	{
		private readonly INow _now;
		private readonly ISiteInAlarmReader _siteInAlarmReader;
		private readonly ISiteRepository _siteRepository;
		private readonly ICurrentAuthorization _authorization;
		private readonly IUserUiCulture _uiCulture;
		private readonly INumberOfAgentsInSiteReader _numberOfAgentsInSiteReader;

		public AgentsInAlarmForSiteViewModelBuilder(
			ISiteInAlarmReader siteInAlarmReader,
			ISiteRepository siteRepository,
			INumberOfAgentsInSiteReader numberOfAgentsInSiteReader,
			ICurrentAuthorization authorization,
			IUserUiCulture uiCulture,
			INow now
			)
		{
			_siteInAlarmReader = siteInAlarmReader;
			_siteRepository = siteRepository;
			_numberOfAgentsInSiteReader = numberOfAgentsInSiteReader;
			_authorization = authorization;
			_uiCulture = uiCulture;
			_now = now;
		}

		public IEnumerable<SiteOutOfAdherence> Build()
		{
			var adherence = _siteInAlarmReader.Read().ToLookup(a => a.SiteId, v => v.Count);
			var sites = allPermittedSites();
			var numberOfAgents = sites.Any() ? _numberOfAgentsInSiteReader.FetchNumberOfAgents(sites.Select(x => x.Id.Value)) : new Dictionary<Guid, int>();
			var sitesOrdered = sites.OrderBy(x => x.Description.Name, StringComparer.Create(_uiCulture.GetUiCulture(), false));
	
			return sitesOrdered
				.Select(s =>
					{
						int tempNumberOfAgents;
						int ooa = adherence[s.Id.GetValueOrDefault()].FirstOrDefault();
						int noa = numberOfAgents.TryGetValue(s.Id.Value, out tempNumberOfAgents) ? tempNumberOfAgents : 0;
						return new SiteOutOfAdherence
						{
							Id = s.Id.GetValueOrDefault(),
							Name = s.Description.Name,
							NumberOfAgents = noa,
							OpenHours = openHours(s),
							OutOfAdherence = ooa,
							Color = getColor(ooa,noa)
						};
					}
				).ToArray();
		}

		public IEnumerable<SiteOutOfAdherence> ForSkills(Guid[] skillIds)
		{
			var adherence = _siteInAlarmReader.ReadForSkills(skillIds).ToLookup(a => a.SiteId, v => v.Count);
			var sites = allPermittedSites();
			var numberOfAgents = sites.Any() ? _numberOfAgentsInSiteReader.FetchNumberOfAgents(sites.Select(x => x.Id.Value)) : new Dictionary<Guid, int>();
			var sitesOrdered = sites.OrderBy(x => x.Description.Name, StringComparer.Create(_uiCulture.GetUiCulture(), false));
			int tempNumberOfAgents;
			var result = from site in sitesOrdered
				   let ooa = adherence[site.Id.GetValueOrDefault()].FirstOrDefault() 
				   let noa = numberOfAgents.TryGetValue(site.Id.Value, out tempNumberOfAgents) ? tempNumberOfAgents : 0
				   select new SiteOutOfAdherence
				   {
					   Id = site.Id.GetValueOrDefault(),
					   Name = site.Description.Name,
					   NumberOfAgents = noa,
					   OpenHours = openHours(site),
					   OutOfAdherence = ooa,
					   Color = getColor(ooa, noa)
				   };
			return result.ToArray();
		}

		private IEnumerable<ISite> allPermittedSites()
		{
			return _siteRepository.LoadAll()
				.Where(
					s =>
						_authorization.Current()
							.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, _now.LocalDateOnly(), s)

			); 
		}

		private static IEnumerable<SiteOpenHourViewModel> openHours(ISite site)
		{
			return site.OpenHourCollection.Select(
				openHour =>
					new SiteOpenHourViewModel()
					{
						WeekDay = openHour.WeekDay,
						StartTime = openHour.TimePeriod.StartTime,
						EndTime = openHour.TimePeriod.EndTime,
						IsClosed = openHour.IsClosed
					}).ToList();
		}

		private string getColor(int OutOfAdherence , int NumberOfAgents )
		{

			if (NumberOfAgents == 0)
				return null;
			var adherencePercent = Math.Floor(((double)OutOfAdherence / (double)NumberOfAgents) * 100);
			if (adherencePercent >= 67)
				return "danger";
			if (adherencePercent >= 34 && adherencePercent <= 66)
				return "warning";
			return "good";	

		}
	}
}