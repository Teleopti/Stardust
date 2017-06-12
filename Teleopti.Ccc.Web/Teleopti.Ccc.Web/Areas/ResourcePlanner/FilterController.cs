﻿using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class FilterController : ApiController
	{
		private readonly FindFilter _findFilter;

		public FilterController(FindFilter findFilter)
		{
			_findFilter = findFilter;
		}

		[HttpGet, Route("api/filters"), UnitOfWork]
		public virtual IHttpActionResult FindFilters(string searchString, int maxHits)
		{
			return Ok(_findFilter.Search(searchString, maxHits));
		}

		[HttpGet, Route("api/filtersplanninggroup"), UnitOfWork]
		public virtual IHttpActionResult FindFiltersForPlanningGroup(string searchString, int maxHits)
		{
			return Ok(_findFilter.SearchForPlanningGroup(searchString, maxHits));
		}
	}
}