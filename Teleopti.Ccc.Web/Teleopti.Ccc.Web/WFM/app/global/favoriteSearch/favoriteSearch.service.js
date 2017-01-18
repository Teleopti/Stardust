(function () {
	"use strict";

	angular.module('wfm.favoriteSearch').service('FavoriteSearchDataService', ['$http', FavoriteSearchService]);

	function FavoriteSearchService($http) {
		var svc = this;

		var fetchAvailableFavoritesUrl = '../api/FavoriteSearch/FetchAvailableFavorites';
		var addFavoriteUrl = '../api/FavoriteSearch/AddFavorite';
		var updateFavoriteUrl = '../api/FavoriteSearch/Update';
		var makeDefaultFavoriteUrl = '../api/FavoriteSearch/ChangeDefault';
		var deleteFavoriteUrl = '../api/FavoriteSearch/Delete';
		var getFavoriteSearchPermissionUrl = '../api/FavoriteSearch/GetPermission';

		svc.getPermission = function() {
			return $http.get(getFavoriteSearchPermissionUrl);
		}

		svc.getFavoriteSearchList = function () {
			return $http.get(fetchAvailableFavoritesUrl);
		};

		svc.add = function(name, curSearch) {
			return $http.post(addFavoriteUrl, { Name: name, TeamIds: curSearch.teamIds, SearchTerm: curSearch.searchTerm });
		};

		svc.update = function(curFavorite) {
			return $http.post(updateFavoriteUrl, curFavorite);
		};

		svc.changeDefault = function(formData) {
			return $http.post(makeDefaultFavoriteUrl, formData);
		};

		svc.delete = function(searchId) {
			return $http.get(deleteFavoriteUrl + '?id=' + searchId);
		};
	}
})();