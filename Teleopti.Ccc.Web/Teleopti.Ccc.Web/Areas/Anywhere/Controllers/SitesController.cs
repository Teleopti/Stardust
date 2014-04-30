using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class SitesController : Controller
	{
		private readonly ISiteRepository _siteRepository;
		private readonly INumberOfAgentsInSiteReader _numberOfAgentsInSiteReader;
		private readonly ISiteAdherenceAggregator _siteAdherenceAggregator;

		public SitesController(ISiteRepository siteRepository, INumberOfAgentsInSiteReader numberOfAgentsInSiteReader, ISiteAdherenceAggregator siteAdherenceAggregator)
		{
			_siteRepository = siteRepository;
			_numberOfAgentsInSiteReader = numberOfAgentsInSiteReader;
			_siteAdherenceAggregator = siteAdherenceAggregator;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult Index()
		{
			var sites = _siteRepository.LoadAll();
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

		[HttpGet]
		public JsonResult GetOutOfAdherence(string siteId)
		{
			var outOfAdherence = _siteAdherenceAggregator.Aggregate(Guid.Parse(siteId));
			return Json(outOfAdherence);
		}
	}
}