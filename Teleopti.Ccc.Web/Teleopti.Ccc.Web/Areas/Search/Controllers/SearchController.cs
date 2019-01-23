using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.Web.Areas.Search.Controllers
{
	public class SearchController : ApiController
	{
		private readonly INextPlanningPeriodProvider _planningPeriodProvider;
		private readonly IToggleManager _toggleManager;
		private readonly IApplicationRoleRepository _applicationRoleRepository;

		public SearchController(INextPlanningPeriodProvider planningPeriodProvider, IToggleManager toggleManager, IApplicationRoleRepository applicationRoleRepository)
		{
			_planningPeriodProvider = planningPeriodProvider;
			_toggleManager = toggleManager;
			_applicationRoleRepository = applicationRoleRepository;
		}

		[UnitOfWork]
		[HttpGet, Route("api/Search/Global")]
		public virtual IHttpActionResult GetResult(string keyword)
		{
			var searchResultModel = new List<SearchResultModel>();

			if (_toggleManager.IsEnabled(Toggles.Wfm_WebPlan_Pilot_46815))
			{
				searchResultModel.AddRange(searchPlanningPeriods(keyword));	
			}
			searchResultModel.AddRange(searchApplicationRoles(keyword));

			return Ok(searchResultModel.AsEnumerable());

		}

		private IEnumerable<SearchResultModel> searchApplicationRoles(string keyword)
		{
			var result =  _applicationRoleRepository.LoadAllRolesByDescription(keyword);
			return result.Select(item => new SearchResultModel
			{
				Name = item.DescriptionText,
				Url = "permissions", 
				SearchGroup = "Permission",
				Id = item.Id
			});
		}


		private IEnumerable<SearchResultModel> searchPlanningPeriods(string keyword)
		{
			var searchResultModel = new List<SearchResultModel>();
			if (UserTexts.Resources.NextPlanningPeriod.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) > -1)
			{
				var currentPplanningPeriodRange = _planningPeriodProvider.Current(null).Range;
				searchResultModel.AddRange(new[]
				{
					new SearchResultModel
					{
						Name =
							UserTexts.Resources.NextPlanningPeriod + " " +
							currentPplanningPeriodRange.StartDate.ToShortDateString(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture) +
							"-" +
							currentPplanningPeriodRange.EndDate.ToShortDateString(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture),
						Url = "resourceplanner.planningperiod",
						SearchGroup = "Planning Period",
						Id = _planningPeriodProvider.Current(null).Id
					}
				});
			}

			return searchResultModel.AsEnumerable();
		}
	}

	public class SearchResultModel
	{
		public string Name { get; set; }
		public string Url { get; set; }
		public Guid? Id { get; set; }
		//can be refactorred to some enum?
		public string SearchGroup { get; set; }
	}

}