using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GetSiteAdherence : IGetSiteAdherence
	{
		private readonly ISiteOutOfAdherenceReadModelReader _siteOutOfAdherenceReadModelPersister;
		private readonly ISiteRepository _siteRepository;

		public GetSiteAdherence(
			ISiteOutOfAdherenceReadModelReader siteOutOfAdherenceReadModelPersister,
			ISiteRepository siteRepository)
		{
			_siteOutOfAdherenceReadModelPersister = siteOutOfAdherenceReadModelPersister;
			_siteRepository = siteRepository;
		}

		public IEnumerable<SiteOutOfAdherence> OutOfAdherence()
		{
			var adherence = _siteOutOfAdherenceReadModelPersister.Read();
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