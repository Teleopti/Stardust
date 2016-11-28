using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class SitesController : ApiController
	{
		private readonly ISiteRepository _siteRepository;
		private readonly AgentsInAlarmForSiteViewModelBuilder _inAlarmForSites;
		private readonly SiteViewModelBuilder _siteViewModelBuilder;

		public SitesController(
			ISiteRepository siteRepository,
			AgentsInAlarmForSiteViewModelBuilder inAlarmForSites,
			SiteViewModelBuilder siteViewModelBuilder)
		{
			_siteRepository = siteRepository;
			_inAlarmForSites = inAlarmForSites;
			_siteViewModelBuilder = siteViewModelBuilder;
		}

		[UnitOfWork, HttpGet, Route("api/Sites")]
		public virtual IHttpActionResult Index()
		{
			return Ok(_siteViewModelBuilder.Build());
		}

		[UnitOfWork, ReadModelUnitOfWork, HttpGet, Route("api/Sites/Organization")]
		public virtual IHttpActionResult GetOrganization()
		{
			return Ok(_siteViewModelBuilder.ForOrganization());
		}

		[UnitOfWork, ReadModelUnitOfWork, HttpGet, Route("api/Sites/ForSkills")]
		public virtual IHttpActionResult SitesForSkill([FromUri]Guid[] skillIds)
		{
			return Ok(_siteViewModelBuilder.ForSkills(skillIds));
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
			return Ok(_inAlarmForSites.Build());
		}

		[UnitOfWork, ReadModelUnitOfWork, HttpGet, Route("api/Sites/InAlarmCountForSkills")]
		public virtual IHttpActionResult InAlarmCountForSkills([FromUri]Guid[] skillIds)
		{
			return Ok(_inAlarmForSites.ForSkills(skillIds));
		}

	}
}