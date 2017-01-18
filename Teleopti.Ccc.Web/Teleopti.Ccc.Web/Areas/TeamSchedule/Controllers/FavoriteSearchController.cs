using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
	public class FavoriteSearchController : ApiController
	{
		private readonly IFavoriteSearchProvider _favoriteSearchProvider;
		private readonly IAuthorization _authorization;

		public FavoriteSearchController(IFavoriteSearchProvider favoriteSearchProvider, IAuthorization authorization)
		{
			_favoriteSearchProvider = favoriteSearchProvider;
			_authorization = authorization;
		}

		[UnitOfWork, HttpGet, Route("api/FavoriteSearch/FetchAvailableFavorites")]
		public virtual IList<FavoriteSearchViewModel> FetchAvailableFavorites()
		{
			return _favoriteSearchProvider.GetAllForCurrentUser();
		}

		[UnitOfWork, HttpGet, Route("api/FavoriteSearch/GetPermission")]
		public virtual JsonResult<bool> GetPermission()
		{
			return Json(_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SaveFavoriteSearch));
		}

		[UnitOfWork,HttpPost, Route("api/FavoriteSearch/AddFavorite")]
		public virtual FavoriteSearchViewModel AddSearchFavorite(FavoriteSearchFormData input)
		{
			var result = _favoriteSearchProvider.AddFavoriteSearch(input);
			return new FavoriteSearchViewModel
			{
				Id = result.Id,
				IsDefault = input.IsDefault,
				Name = input.Name,
				SearchTerm = input.SearchTerm,
				TeamIds = input.TeamIds
			} ;
		}

		[UnitOfWork, HttpPost, Route("api/FavoriteSearch/Update")]
		public virtual IHttpActionResult UpdateFavoriteSearch(FavoriteSearchFormData input)
		{
			_favoriteSearchProvider.UpdateFavoriteSearch(input);
			return Ok();
		}

		[UnitOfWork, HttpPost, Route("api/FavoriteSearch/ChangeDefault")]
		public virtual IHttpActionResult ChangeDefault(ChangeDefaultFormData input)
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

	public class ChangeDefaultFormData
	{
		public Guid CurrentDefaultId { get; set; }
		public Guid? PreDefaultId { get; set; }
	}

	public class FavoriteSearchFormData
	{
		public Guid? Id { get; set; }
		public string Name { get; set;  }
		public string SearchTerm { get; set; }
		public Guid[] TeamIds { get; set; }
		public bool IsDefault { get; set; }
	}
}