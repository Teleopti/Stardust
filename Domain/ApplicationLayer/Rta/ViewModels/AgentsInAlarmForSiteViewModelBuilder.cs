using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class AgentsInAlarmForSiteViewModelBuilder
	{
		private readonly INow _now;
		private readonly ISiteInAlarmReader _siteInAlarmReader;
		private readonly ISiteRepository _siteRepository;
		private readonly ICurrentAuthorization _authorization;
		private readonly IUserUiCulture _uiCulture;

		public AgentsInAlarmForSiteViewModelBuilder(
			ISiteInAlarmReader siteInAlarmReader,
			ISiteRepository siteRepository,
			ICurrentAuthorization authorization,
			IUserUiCulture uiCulture,
			INow now)
		{
			_siteInAlarmReader = siteInAlarmReader;
			_siteRepository = siteRepository;
			_authorization = authorization;
			_uiCulture = uiCulture;
			_now = now;
			
		}

		public IEnumerable<SiteOutOfAdherence> Build()
		{
			var adherence = _siteInAlarmReader.Read().ToLookup(a => a.SiteId, v => v.Count);
			var sites = allPermittedSites();
			var sitesOrdered = sites.OrderBy(x => x.Description.Name, StringComparer.Create(_uiCulture.GetUiCulture(), false));
			return sitesOrdered.Select(s =>
				new SiteOutOfAdherence
				{
					Id = s.Id.GetValueOrDefault(),
					OutOfAdherence = adherence[s.Id.GetValueOrDefault()].FirstOrDefault()
				}).ToArray();
		}

		public IEnumerable<SiteOutOfAdherence> ForSkills(Guid[] skillIds)
		{
			var adherence = _siteInAlarmReader.ReadForSkills(skillIds).ToLookup(a => a.SiteId, v => v.Count);
			var sites = allPermittedSites();
			var sitesOrdered = sites.OrderBy(x => x.Description.Name, StringComparer.Create(_uiCulture.GetUiCulture(), false)); 
			return from site in sitesOrdered
				   select new SiteOutOfAdherence
				   {
					   Id = site.Id.GetValueOrDefault(),
					   OutOfAdherence = adherence[site.Id.GetValueOrDefault()].FirstOrDefault()
				   };
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
	}
}