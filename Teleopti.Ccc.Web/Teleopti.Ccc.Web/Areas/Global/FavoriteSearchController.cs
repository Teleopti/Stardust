using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class FavoriteSearchController : ApiController
	{
		private readonly IFavoriteSearchProvider _favoriteSearchProvider;
		private readonly IAuthorization _authorization;
		private readonly Dictionary<string, WfmArea> _areaMap = new Dictionary<string, WfmArea>()
		{
			{"Teams", WfmArea.Teams},
			{"Requests", WfmArea.Requests}
		};

		public FavoriteSearchController(IFavoriteSearchProvider favoriteSearchProvider, IAuthorization authorization)
		{
			_favoriteSearchProvider = favoriteSearchProvider;
			_authorization = authorization;
		}

		[UnitOfWork, HttpGet, Route("api/FavoriteSearch/GetPermission")]
		public virtual JsonResult<bool> GetPermission()
		{
			return Json(_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SaveFavoriteSearch));
		}

		[UnitOfWork, HttpGet, Route("api/FavoriteSearch/{area}/FetchAvailableFavorites")]
		public virtual IList<FavoriteSearchViewModel> FetchAvailableFavorites([FromUri]string area)
		{
			return _favoriteSearchProvider.GetAllForCurrentUser(_areaMap[area]);
		}

		[UnitOfWork,HttpPost, Route("api/FavoriteSearch/{area}/AddFavorite")]
		public virtual FavoriteSearchViewModel AddSearchFavorite([FromUri]string area, [FromBody]FavoriteSearchFormData input)
		{
			var result = _favoriteSearchProvider.AddFavoriteSearch(input, _areaMap[area]);
			return new FavoriteSearchViewModel
			{
				Id = result.Id,
				IsDefault = input.IsDefault,
				Name = input.Name,
				SearchTerm = input.SearchTerm,
				TeamIds = input.TeamIds,
			};
		}

		[UnitOfWork, HttpPost, Route("api/FavoriteSearch/Update")]
		public virtual IHttpActionResult UpdateFavoriteSearch([FromBody]FavoriteSearchFormData input)
		{
			_favoriteSearchProvider.UpdateFavoriteSearch(input);
			return Ok();
		}

		[UnitOfWork, HttpPost, Route("api/FavoriteSearch/ChangeDefault")]
		public virtual IHttpActionResult ChangeDefault([FromBody]ChangeDefaultFormData input)
		{
			_favoriteSearchProvider.ChangeDefaultFavorite(input);
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/FavoriteSearch/Delete")]
		public virtual IHttpActionResult DeleteFavoriteSearch(Guid id)
		{
			_favoriteSearchProvider.DeleteFavoriteSearch(id);
			return Ok();
		}

	}
}