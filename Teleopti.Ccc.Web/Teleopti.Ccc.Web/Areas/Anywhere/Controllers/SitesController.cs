using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class SitesController : Controller
	{
		private readonly ISiteRepository _siteRepository;
		private readonly INumberOfAgentsInSiteReader _numberOfAgentsInSiteReader;
		private readonly ISiteAdherenceAggregator _siteAdherenceAggregator;
		private readonly IGetSiteAdherence _getAdherence;
		private readonly ICurrentHttpContext _currentHttpContext;

		public SitesController(ISiteRepository siteRepository, INumberOfAgentsInSiteReader numberOfAgentsInSiteReader, ISiteAdherenceAggregator siteAdherenceAggregator, IGetSiteAdherence getAdherence, ICurrentHttpContext currentHttpContext)
		{
			_siteRepository = siteRepository;
			_numberOfAgentsInSiteReader = numberOfAgentsInSiteReader;
			_siteAdherenceAggregator = siteAdherenceAggregator;
			_getAdherence = getAdherence;
			_currentHttpContext = currentHttpContext;
		}

		[UnitOfWork, HttpGet]
		public virtual JsonResult Index()
		{
			var sites = _siteRepository.LoadAll();
			if (sites.IsNullOrEmpty())
			{
				return Json(new SiteViewModel
				{
					Id = "",
					Name = "",
					NumberOfAgents = 0
				}, JsonRequestBehavior.AllowGet);
			}
			var numberOfAgents = _numberOfAgentsInSiteReader.FetchNumberOfAgents(sites);

			return Json(sites.Select(site =>
				new SiteViewModel
				{
					Id = site.Id.Value.ToString(),
					Name = site.Description.Name,
					NumberOfAgents = numberOfAgents[site.Id.Value]
				}), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult Get(string siteId)
		{
			var site = _siteRepository.Get(new Guid(siteId));
			return Json(new SiteViewModel
			{
				Id = site.Id.Value.ToString(),
				Name = site.Description.Name
			}, JsonRequestBehavior.AllowGet);
		}
		
		[UnitOfWorkAction, HttpGet]
		public JsonResult GetOutOfAdherence(string siteId)
		{
			var outOfAdherence = _siteAdherenceAggregator.Aggregate(Guid.Parse(siteId));
			return Json(new SiteOutOfAdherence
			{
				Id = siteId,
				OutOfAdherence = outOfAdherence
			}, JsonRequestBehavior.AllowGet);
		}
		
		[UnitOfWorkAction, HttpGet]
		public JsonResult GetBusinessUnitId(string siteId)
		{
			var site = _siteRepository.Get(new Guid(siteId));
			return Json(site.BusinessUnit.Id.GetValueOrDefault(), JsonRequestBehavior.AllowGet);
		}

		[ReadModelUnitOfWork, HttpGet]
		public virtual JsonResult GetOutOfAdherenceForAllSites()
		{
			var id = UnitOfWorkAspect.BusinessUnitIdForRequest(_currentHttpContext);
			return Json(_getAdherence.ReadAdherenceForAllSites(id.Value), JsonRequestBehavior.AllowGet);
		}
	}
}