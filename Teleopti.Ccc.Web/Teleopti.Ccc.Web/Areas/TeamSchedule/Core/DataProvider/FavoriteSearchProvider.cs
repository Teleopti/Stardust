using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public class FavoriteSearchProvider : IFavoriteSearchProvider
	{
		private readonly ILoggedOnUser _loggonUser;
		private readonly IFavoriteSearchRepository _favoriteSearchRepo;

		public FavoriteSearchProvider(ILoggedOnUser loggonUser, IFavoriteSearchRepository favoriteSearchRepo)
		{
			_loggonUser = loggonUser;
			_favoriteSearchRepo = favoriteSearchRepo;
		}

		public IList<FavoriteSearchViewModel> GetAllForCurrentUser(WfmArea area)
		{
			var currentUser = _loggonUser.CurrentUser();
			return _favoriteSearchRepo.FindAllForPerson(currentUser, area).Select(f => new FavoriteSearchViewModel
			{
				Id =f.Id.GetValueOrDefault(),
				Name = f.Name,
				SearchTerm = f.SearchTerm,
				IsDefault = f.Status == FavoriteSearchStatus.Default,
				TeamIds = f.TeamIds?.Split(',').Select(t => new Guid(t)).ToArray()
			}).OrderBy(f => f.Name).ToList();
		}

		public IFavoriteSearch AddFavoriteSearch(FavoriteSearchFormData input, WfmArea area)
		{
			var fav = new FavoriteSearch(input.Name)
			{
				SearchTerm = input.SearchTerm,
				TeamIds = string.Join(",", input.TeamIds.Select(t => t.ToString())),
				Creator = _loggonUser.CurrentUser(),
				Status = FavoriteSearchStatus.Normal,
				WfmArea = area
			};
			_favoriteSearchRepo.Add(fav);

			return fav;
		}

		public void UpdateFavoriteSearch(FavoriteSearchFormData input)
		{
			var search = _favoriteSearchRepo.Get(input.Id.Value);
			search.SearchTerm = input.SearchTerm;
			search.TeamIds = String.Join(",", input.TeamIds.Select(t => t.ToString()));
			search.Status = input.IsDefault ? FavoriteSearchStatus.Default : FavoriteSearchStatus.Normal;
		}

		public void DeleteFavoriteSearch(Guid id)
		{
			var search = _favoriteSearchRepo.Get(id);
			_favoriteSearchRepo.Remove(search);
		}

		public void ChangeDefaultFavorite(ChangeDefaultFormData input)
		{
			var newDefault = _favoriteSearchRepo.Get(input.CurrentDefaultId);
			newDefault.Status = FavoriteSearchStatus.Default;
			if (input.PreDefaultId != null)
			{
				var preDefault = _favoriteSearchRepo.Get(input.PreDefaultId.Value);
				preDefault.Status = FavoriteSearchStatus.Normal;
			}
		}
	}

	public interface IFavoriteSearchProvider
	{
		IList<FavoriteSearchViewModel> GetAllForCurrentUser(WfmArea area);
		IFavoriteSearch AddFavoriteSearch(FavoriteSearchFormData input, WfmArea area);
		void UpdateFavoriteSearch(FavoriteSearchFormData input);
		void DeleteFavoriteSearch(Guid id);
		void ChangeDefaultFavorite(ChangeDefaultFormData input);
	}
}