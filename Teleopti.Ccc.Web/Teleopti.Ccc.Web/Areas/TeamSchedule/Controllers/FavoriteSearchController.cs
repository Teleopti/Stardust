using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
	public class FavoriteSearchController : ApiController
	{
		private readonly IFavoriteSearchProvider _favoriteSearchProvider;

		public FavoriteSearchController(IFavoriteSearchProvider favoriteSearchProvider)
		{
			_favoriteSearchProvider = favoriteSearchProvider;
		}

		[UnitOfWork, HttpGet, Route("api/FavoriteSearch/FetchAvailableFavorites")]
		public virtual IList<FavoriteSearchViewModel> FetchAvailableFavorites()
		{
			return _favoriteSearchProvider.GetAllForCurrentUser();
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