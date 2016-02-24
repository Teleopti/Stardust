using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class SitesController : ApiController
	{
		private readonly ISiteRepository _siteRepository;
		private readonly INumberOfAgentsInSiteReader _numberOfAgentsInSiteReader;
		private readonly IGetSiteAdherence _getAdherence;
		private readonly IPersonalAvailableDataProvider _personalAvailableDataProvider;
		private readonly INow _now;

		public SitesController(ISiteRepository siteRepository,
			INumberOfAgentsInSiteReader numberOfAgentsInSiteReader,
			IGetSiteAdherence getAdherence,
			IPersonalAvailableDataProvider personalAvailableDataProvider, 
			INow now)
		{
			_siteRepository = siteRepository;
			_numberOfAgentsInSiteReader = numberOfAgentsInSiteReader;
			_getAdherence = getAdherence;
			_personalAvailableDataProvider = personalAvailableDataProvider;
			_now = now;
		}

		[UnitOfWork, HttpGet, Route("api/Sites")]
		public virtual IHttpActionResult Index()
		{
			var sites = _personalAvailableDataProvider != null
				? _personalAvailableDataProvider.AvailableSites(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview,
					_now.LocalDateOnly()).ToArray()
				: _siteRepository.LoadAll();

			IDictionary<Guid, int> numberOfAgents = new Dictionary<Guid, int>();
			if (sites.Any())
				numberOfAgents = _numberOfAgentsInSiteReader.FetchNumberOfAgents(sites);

			return Ok(sites.Select(site =>
			{
				var valueOrDefault = site.Id.GetValueOrDefault();
				return new SiteViewModel
											   {
												   Id = valueOrDefault,
												   Name = site.Description.Name,
												   NumberOfAgents = numberOfAgents[valueOrDefault]
											   };
			}));
		}

		[UnitOfWork, HttpGet, Route("api/Sites/Get")]
		public virtual IHttpActionResult Get(Guid siteId)
		{
			var site = _siteRepository.Get(siteId);
			return Ok(new SiteViewModel
			{
				Id = site.Id.GetValueOrDefault(),
				Name = site.Description.Name
			});
		}
		
		[UnitOfWork, HttpGet, Route("api/Sites/GetBusinessUnitId")]
		public virtual IHttpActionResult GetBusinessUnitId(Guid siteId)
		{
			var site = _siteRepository.Get(siteId);
			return Ok(site.BusinessUnit.Id.GetValueOrDefault());
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Sites/GetOutOfAdherenceForAllSites")]
		public virtual IHttpActionResult GetOutOfAdherenceForAllSites()
		{
			return Ok(_getAdherence.OutOfAdherence());
		}
	}
}