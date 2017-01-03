(function () {
	'use strict';

	angular.module('wfm.teamSchedule')
		.component('favoriteSearch',
		{
			templateUrl: 'app/teamSchedule/html/favoriteSearch.tpl.html',
			controller: favoriteSearchCtrl,
			bindings: {				
				applyFavorite: '&',
				getSearch: '&'
			}
		});

	favoriteSearchCtrl.$inject = ['FavoriteSearchDataService'];
	
	function favoriteSearchCtrl(FavoriteSearchDataService) {
		var ctrl = this;
		ctrl.favoriteSearchList = [];
		var favoriteSearchNameList = [];

		ctrl.$onInit = function () {
			FavoriteSearchDataService.getFavoriteSearchList().then(function(resp) {
				refreshList(resp.data);
			});
		}

		ctrl.save = function () {
			var currentSearch = ctrl.getSearch();

			if (favoriteSearchNameList.indexOf(ctrl.currentName) === -1){
				FavoriteSearchDataService.add(ctrl.currentName, currentSearch)
					.then(function (resp) {
						ctrl.favoriteSearchList.unshift(resp.data);
						favoriteSearchNameList.unshift(resp.data.Name);
					});
			} else {
				var updatedCurrent = {
					Id: ctrl.currentFavorite.Id,
					Name: ctrl.currentFavorite.Name,
					TeamIds: currentSearch.teamIds,
					SearchTerm: currentSearch.searchTerm
				}
				FavoriteSearchDataService.update(updatedCurrent)
					.then(function () {
						updateFavorite(ctrl.currentFavorite, updatedCurrent);
					});
			}			
		}

		ctrl.makeDefault = function(name) {
			var index = favoriteSearchNameList.indexOf(name);
			if (index != -1) {
				FavoriteSearchDataService.makeDefault(name)
					.then(function () {
						for (var i = 0; i < ctrl.favoriteSearchList.length; i++) {
							if (i === index) {
								ctrl.favoriteSearchList[i].IsDefault = true;
							} else {
								ctrl.favoriteSearchList[i].IsDefault = false;
							}
						}
						
					});
			}
		}

		ctrl.delete = function(name) {
			var index = favoriteSearchNameList.indexOf(name);
			FavoriteSearchDataService.delete(name)
				.then(function() {
					ctrl.favoriteSearchList.splice(index, 1);
					favoriteSearchNameList.splice(index, 1);
				});
		}

		function updateFavorite(target, value) {
			target.SearchTerm = value.SearchTerm;
			target.TeamIds = value.TeamIds;
		}

		ctrl.openMenu = function ($mdOpenMenu, ev) {
			$mdOpenMenu(ev);
		}

		ctrl.select = function(item) {
			ctrl.currentFavorite = item;
			ctrl.onPick(item);
		}

		

		function refreshList(data) {
			if (!angular.isArray(data)) return;

			ctrl.favoriteSearchList = data;
			favoriteSearchNameList = ctrl.favoriteSearchList.map(function(f) {
				return f.Name;
			});

			var defaultes = ctrl.favoriteSearchList.filter(function (f) {
				return f.IsDefault;
			});
			ctrl.currentFavorite = defaultes.length > 0 ? defaultes[0] : undefined;
			ctrl.currentName = ctrl.currentFavorite ? ctrl.currentFavorite.Name : '';
		}
	}

})();