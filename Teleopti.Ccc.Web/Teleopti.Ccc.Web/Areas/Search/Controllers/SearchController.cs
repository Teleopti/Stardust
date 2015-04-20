using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.Web.Areas.Search.Controllers
{
	public class SearchController : ApiController
	{
		private readonly INextPlanningPeriodProvider _planningPeriodProvider;
		private readonly IToggleManager _toggleManager;

		public SearchController(INextPlanningPeriodProvider planningPeriodProvider, IToggleManager toggleManager)
		{
			_planningPeriodProvider = planningPeriodProvider;
			_toggleManager = toggleManager;
		}

		[UnitOfWork]
		[HttpGet, Route("api/Search/Global")]
		public virtual IHttpActionResult GetResult(string keyword)
		{
			var searchResultModel = new List<SearchResultModel>();

			if (_toggleManager.IsEnabled(Toggles.Wfm_ResourcePlanner_32892) && UserTexts.Resources.NextPlanningPeriod.IndexOf(keyword,StringComparison.CurrentCultureIgnoreCase) > -1)
			{
				var currentPplanningPeriodRange = _planningPeriodProvider.Current().Range;
				searchResultModel.AddRange(new[]
				{
					new SearchResultModel
					{
						Name =
							UserTexts.Resources.NextPlanningPeriod + " " + currentPplanningPeriodRange.StartDate.ToShortDateString() + "-" +
							currentPplanningPeriodRange.EndDate.ToShortDateString(),
						Url = "/resourceplanner",
						SearchGroup = UserTexts.Resources.PlanningPeriod
					}
				});
			}

			return Ok(searchResultModel.AsEnumerable());
		}
	}

	public class SearchResultModel
	{
		public string Name { get; set; }
		public string Url { get; set; }
		//can be refactorred to some enum?
		public string SearchGroup { get; set; }
	}

}