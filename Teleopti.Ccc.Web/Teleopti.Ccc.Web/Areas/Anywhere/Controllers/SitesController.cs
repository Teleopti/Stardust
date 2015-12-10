using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class SitesController : Controller
	{
		private readonly ISiteRepository _siteRepository;
		private readonly INumberOfAgentsInSiteReader _numberOfAgentsInSiteReader;
		private readonly ISiteAdherenceAggregator _siteAdherenceAggregator;
		private readonly IGetSiteAdherence _getAdherence;
		private readonly IPersonalAvailableDataProvider _personalAvailableDataProvider;
		private readonly INow _now;

		public SitesController(ISiteRepository siteRepository,
			INumberOfAgentsInSiteReader numberOfAgentsInSiteReader,
			ISiteAdherenceAggregator siteAdherenceAggregator, 
			IGetSiteAdherence getAdherence,
			IPersonalAvailableDataProvider personalAvailableDataProvider, 
			INow now)
		{
			_siteRepository = siteRepository;
			_numberOfAgentsInSiteReader = numberOfAgentsInSiteReader;
			_siteAdherenceAggregator = siteAdherenceAggregator;
			_getAdherence = getAdherence;
			_personalAvailableDataProvider = personalAvailableDataProvider;
			_now = now;
		}

		[UnitOfWork, HttpGet]
		public virtual JsonResult Index()
		{
			var sites = _personalAvailableDataProvider != null
				? _personalAvailableDataProvider.AvailableSites(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview,
					_now.LocalDateOnly())
				: _siteRepository.LoadAll();

			IDictionary<Guid, int> numberOfAgents = new Dictionary<Guid, int>();
			if (sites.Any())
				numberOfAgents = _numberOfAgentsInSiteReader.FetchNumberOfAgents(sites);

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

		[ReadModelUnitOfWork, UnitOfWork, HttpGet]
		public virtual JsonResult GetOutOfAdherenceForAllSites()
		{
			return Json(_getAdherence.OutOfAdherence(), JsonRequestBehavior.AllowGet);
		}
	}
}