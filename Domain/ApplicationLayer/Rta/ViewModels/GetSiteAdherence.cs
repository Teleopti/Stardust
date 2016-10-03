using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class GetSiteAdherence : IGetSiteAdherence
	{
		private readonly ISiteInAlarmReader _siteInAlarmReader;
		private readonly ISiteRepository _siteRepository;

		public GetSiteAdherence(
			ISiteInAlarmReader siteInAlarmReader,
			ISiteRepository siteRepository)
		{
			_siteInAlarmReader = siteInAlarmReader;
			_siteRepository = siteRepository;
		}

		public IEnumerable<SiteOutOfAdherence> OutOfAdherence()
		{
			var adherence = _siteInAlarmReader.Read();
			var sites = _siteRepository.LoadAll();
			return sites.Select(s =>
				new SiteOutOfAdherence
				{
					Id = s.Id.GetValueOrDefault(),
					OutOfAdherence = adherence
						.Where(a => a.SiteId == s.Id.Value)
						.Select(a => a.Count)
						.SingleOrDefault()
				}).ToArray();
		}
	}
}