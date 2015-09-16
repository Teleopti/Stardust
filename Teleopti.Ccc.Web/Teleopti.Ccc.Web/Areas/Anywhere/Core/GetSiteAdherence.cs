using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GetSiteAdherence : IGetSiteAdherence
	{
		private readonly ISiteOutOfAdherenceReadModelReader _siteOutOfAdherenceReadModelPersister;
		private readonly IPersonalAvailableDataProvider _availableSitesProvider;
		private readonly INow _now;

		public GetSiteAdherence(ISiteOutOfAdherenceReadModelReader siteOutOfAdherenceReadModelPersister, 
			IPersonalAvailableDataProvider availableSitesProvider,
			INow now)
		{
			_siteOutOfAdherenceReadModelPersister = siteOutOfAdherenceReadModelPersister;
			_availableSitesProvider = availableSitesProvider;
			_now = now;
		}

		public IEnumerable<SiteOutOfAdherence> ReadAdherenceForAllPermittedSites()
		{
			var permittedSites = _availableSitesProvider.AvailableSites(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview,
				_now.LocalDateOnly());
			if (permittedSites.IsEmpty()) return Enumerable.Empty<SiteOutOfAdherence>();
			return	_siteOutOfAdherenceReadModelPersister.Read(permittedSites.Select(x => x.Id.Value).ToArray())
				.Select(x => new SiteOutOfAdherence { Id = x.SiteId.ToString(), OutOfAdherence = x.Count });
		}
	}
}