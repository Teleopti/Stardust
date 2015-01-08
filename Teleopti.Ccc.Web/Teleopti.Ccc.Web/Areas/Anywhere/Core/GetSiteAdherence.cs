using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GetSiteAdherence : IGetSiteAdherence
	{
		private readonly ISiteAdherencePersister _siteAdherencePersister;

		public GetSiteAdherence(ISiteAdherencePersister siteAdherencePersister)
		{
			_siteAdherencePersister = siteAdherencePersister;
		}

		public IEnumerable<SiteOutOfAdherence> ReadAdherenceForAllSites(Guid businessUnitId)
		{
			return
				_siteAdherencePersister.GetAll(businessUnitId)
					.Select(x => new SiteOutOfAdherence {Id = x.SiteId.ToString(), OutOfAdherence = x.AgentsOutOfAdherence});
		}
	}
}