(function () {
	"use strict";

	angular.module('wfm.teamSchedule').service('FavoriteSearchDataService', ['$http', FavoriteSearchService]);

	function FavoriteSearchService($http) {
		/*var svc = this;
		var getFavoriteSearchListUrl = 'app/teamSchedule/getFavoriteSearchList';
		var addFavoriteUrl = '';
		var updateFavoriteUrl = '';
		var makeDefaultFavoriteUrl = '';
		var deleteFavoriteUrl = '';

		svc.getFavoriteSearchList = function () {
			return $http.get(getFavoriteSearchListUrl);
		};

		svc.add = function(name, curSearch) {
			return $http.post(addFavoriteUrl, { Name: name, TeamIds: curSearch.teamIds, SearchTerm: curSearch.searchTerm });
		};

		svc.update = function(curFavorite) {
			return $http.post(updateFavoriteUrl, curFavorite);
		};

		svc.makeDefault = function(name) {
			return $http.post(makeDefaultFavoriteUrl,{Name:name});
		};

		svc.delete = function(name) {
			return $http.post(deleteFavoriteUrl, { Name: name });
		};
		*/

		var fakeData = [
			
		];

		this.getFavoriteSearchList = function () {
			return {
				then: function (cb) { cb({ data: fakeData }); }
			}
		}

		this.add = function (currentName, currentSearch) {
			var result = {
				Id: 'id',
				Name: currentName,
				TeamIds: currentSearch.teamIds,
				SearchTerm: currentSearch.searchTerm,
				IsDefault: false
			};
			return {
				then: function (cb) { cb({ data: result }); }
			}
		}

		this.update = function () {
			return { then: function (cb) { cb(); } };
		}

		this.delete = function () {
			return { then: function (cb) { cb(); } };
		}

		this.makeDefault = function () {
			return { then: function (cb) { cb(); } };
		}

		this.removeDefault = function () {
			return { then: function (cb) { cb(); } };
		}
	}
})();