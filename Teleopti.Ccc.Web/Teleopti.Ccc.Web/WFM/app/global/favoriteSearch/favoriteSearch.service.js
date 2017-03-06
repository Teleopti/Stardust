(function () {
	"use strict";

	angular.module('wfm.favoriteSearch').service('FavoriteSearchDataService', ['$q','$http', FavoriteSearchService]);

	function FavoriteSearchService($q, $http) {
		var svc = this;

		var updateFavoriteUrl = '../api/FavoriteSearch/Update';
		var makeDefaultFavoriteUrl = '../api/FavoriteSearch/ChangeDefault';
		var deleteFavoriteUrl = '../api/FavoriteSearch/Delete';
		var getFavoriteSearchPermissionUrl = '../api/FavoriteSearch/GetPermission';
		var defer = $q.defer();

		var currentModule;
		var moduleMap = {
			'wfm.requests': {
				fetchAvailableFavoritesUrl: '../api/FavoriteSearch/Requests/FetchAvailableFavorites',
				addFavoriteUrl: '../api/FavoriteSearch/Requests/AddFavorite'
				
			},
			'wfm.teamSchedule' : {
				fetchAvailableFavoritesUrl: '../api/FavoriteSearch/Teams/FetchAvailableFavorites',
				addFavoriteUrl: '../api/FavoriteSearch/Teams/AddFavorite'
			}
		};

		svc.setModule = function(module) {
			currentModule = module;
		};

		svc.getPermission = function() {
			return $http.get(getFavoriteSearchPermissionUrl);
		};

		svc.hasPermission = function(){
			return defer.promise;
		};

		svc.getFavoriteSearchList = function () {
			return $http.get(moduleMap[currentModule].fetchAvailableFavoritesUrl);
		};

		svc.add = function(name, curSearch) {
			return $http.post(moduleMap[currentModule].addFavoriteUrl, { Name: name, TeamIds: curSearch.TeamIds, SearchTerm: curSearch.SearchTerm });
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

		svc.initPermission = function(){
			svc.getPermission().then(function(res){
				if(res.data != undefined){
					defer.resolve(res);
				}else{
					defer.reject();
				}
			},function(){
				defer.reject();
			});
		};
	}
})();