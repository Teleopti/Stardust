using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class AgentsInAlarmForSiteViewModelBuilder
	{
		private readonly ISiteInAlarmReader _siteInAlarmReader;
		private readonly ISiteRepository _siteRepository;

		public AgentsInAlarmForSiteViewModelBuilder(
			ISiteInAlarmReader siteInAlarmReader,
			ISiteRepository siteRepository)
		{
			_siteInAlarmReader = siteInAlarmReader;
			_siteRepository = siteRepository;
		}

		public IEnumerable<SiteOutOfAdherence> Build()
		{
			var adherence = _siteInAlarmReader.Read().ToLookup(a => a.SiteId, v => v.Count);
			var sites = _siteRepository.LoadAll();
			return sites.Select(s =>
				new SiteOutOfAdherence
				{
					Id = s.Id.GetValueOrDefault(),
					OutOfAdherence = adherence[s.Id.GetValueOrDefault()].FirstOrDefault()
				}).ToArray();
		}

		public IEnumerable<SiteOutOfAdherence> ForSkills(Guid[] skillIds)
		{
			var adherence = _siteInAlarmReader.ReadForSkills(skillIds).ToLookup(a => a.SiteId, v => v.Count);
			var sites = _siteRepository.LoadAll();
			return from site in sites
				   select new SiteOutOfAdherence
				   {
					   Id = site.Id.GetValueOrDefault(),
					   OutOfAdherence = adherence[site.Id.GetValueOrDefault()].FirstOrDefault()
				   };
		}
	}
}