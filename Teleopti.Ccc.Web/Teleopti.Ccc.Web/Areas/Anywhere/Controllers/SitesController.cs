using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class SitesController : ApiController
	{
		private readonly ISiteRepository _siteRepository;
		private readonly IGetSiteAdherence _getAdherence;
		private readonly SiteViewModelBuilder _siteViewModelBuilder;

		public SitesController(
			ISiteRepository siteRepository,
			IGetSiteAdherence getAdherence,
			SiteViewModelBuilder siteViewModelBuilder)
		{
			_siteRepository = siteRepository;
			_getAdherence = getAdherence;
			_siteViewModelBuilder = siteViewModelBuilder;
		}

		[UnitOfWork, HttpGet, Route("api/Sites")]
		public virtual IHttpActionResult Index()
		{
			return Ok(_siteViewModelBuilder.Build());
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