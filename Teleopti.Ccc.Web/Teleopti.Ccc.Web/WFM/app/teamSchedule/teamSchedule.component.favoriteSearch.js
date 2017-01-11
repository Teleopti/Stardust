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

	favoriteSearchCtrl.$inject = ['$translate', '$mdPanel', '$wfmModal', 'FavoriteSearchDataService'];

	function favoriteSearchCtrl($translate, $mdPanel, $wfmModal, FavoriteSearchDataService) {
		var ctrl = this;
		ctrl.favoriteSearchList = [];
		ctrl.isTest = false;
		var favoriteSearchNameList = [];

		ctrl.openPanel = function (ev) {
			var position = $mdPanel.newPanelPosition()
				.relativeTo('.fav-search-open-btn')
				.addPanelPosition($mdPanel.xPosition.ALIGN_START, $mdPanel.yPosition.BELOW);
			var config = {
				attachTo: angular.element(document.body),
				controller: ['mdPanelRef', function (mdPanelRef) {
					if (mdPanelRef) ctrl.mdPanelRef = mdPanelRef;
					return ctrl;
				}
				],
				controllerAs: '$ctrl',
				templateUrl: 'app/teamSchedule/html/favoriteSearchPanel.tpl.html',
				panelClass: 'fav-search-panel',
				position: position,
				openFrom: ev,
				clickOutsideToClose: true,
				escapeToClose: true,
				focusOnOpen: false,
				zIndex: 150
			};
			$mdPanel.open(config);

			ctrl.favoriteSearchList.sort(reorderListAccordingToIsDefault);
		};

		ctrl.$onInit = function () {
			FavoriteSearchDataService.getFavoriteSearchList().then(function (resp) {
				refreshList(resp.data);
			});
		};

		ctrl.save = function () {
			if(!ctrl.enableSave()) return;
			var currentSearch = angular.copy(ctrl.getSearch());
			if (favoriteSearchNameList.indexOf(ctrl.currentName) === -1) {
				FavoriteSearchDataService.add(ctrl.currentName, currentSearch)
					.then(function (resp) {
						ctrl.favoriteSearchList.unshift(resp.data);
						favoriteSearchNameList.unshift(resp.data.Name);
						ctrl.currentFavorite = resp.data;
					});
			} else {
				ctrl.isTest ? updateFavorite() : popDialog();
			}
		};

		ctrl.enableSave = function () {
			var currentSearch = ctrl.getSearch();

			var isNameValid = ctrl.currentName && ctrl.currentName !== '' && ctrl.currentName.length <= 50;
			var hasTeamIds = currentSearch.teamIds.length > 0;

			if (!ctrl.currentFavorite) return isNameValid && hasTeamIds;

			var nameChanged = ctrl.currentName != ctrl.currentFavorite.Name;
			var notSameTeamIdsAndSearchTerm = !angular.equals(ctrl.currentFavorite.TeamIds, currentSearch.teamIds) ||
				ctrl.currentFavorite.SearchTerm != currentSearch.searchTerm;

			return (nameChanged || notSameTeamIdsAndSearchTerm) && isNameValid && hasTeamIds;
		};

		function popDialog() {
			var title = $translate.instant('UpdateConfirmation');
			var message = $translate.instant('AreYouSureToUpdateCurrentFavoriteSearch').replace('{0}', ctrl.currentName);
			$wfmModal.confirm(message, title).then(function (resp) {
				if (resp) {
					updateFavorite();
				}
			});
		}

		function updateFavorite() {
			var currentSearch = angular.copy(ctrl.getSearch());

			var updatedCurrent = {
				Id: ctrl.currentFavorite.Id,
				Name: ctrl.currentFavorite.Name,
				TeamIds: currentSearch.teamIds,
				SearchTerm: currentSearch.searchTerm
			};
			FavoriteSearchDataService.update(updatedCurrent)
				.then(function () {
					ctrl.currentFavorite.SearchTerm = updatedCurrent.SearchTerm;
					ctrl.currentFavorite.TeamIds = updatedCurrent.TeamIds;
				});
		}

		ctrl.toggleDefault = function (name) {
			var index = favoriteSearchNameList.indexOf(name);
			if (index !== -1) {
				//already default => not default
				if (ctrl.favoriteSearchList[index].IsDefault) {
					var clonedSearchItem = angular.copy(ctrl.favoriteSearchList[index]);
					clonedSearchItem.IsDefault = false;

					FavoriteSearchDataService.update(clonedSearchItem)
						.then(function() {
							ctrl.favoriteSearchList[index].IsDefault = false;
						});
				} else { //not default => default
					var currentDefault = ctrl.favoriteSearchList.filter(function(f) {
						return f.IsDefault;
					})[0];

					FavoriteSearchDataService.changeDefault({
							CurrentDefaultId: ctrl.favoriteSearchList[index].Id,
							PreDefaultId: currentDefault? currentDefault.Id:null
						})
						.then(
							function () {
								if (currentDefault) {
									currentDefault.IsDefault = false;
								}
								
								ctrl.favoriteSearchList[index].IsDefault = true;
							});
				}
			}
		};

		ctrl.delete = function (name) {
			var index = favoriteSearchNameList.indexOf(name);
			FavoriteSearchDataService.delete(ctrl.favoriteSearchList[index].Id)
				.then(function () {
					ctrl.favoriteSearchList.splice(index, 1);
					favoriteSearchNameList.splice(index, 1);
				});
		};

		ctrl.select = function (item) {
			ctrl.currentFavorite = item;
			ctrl.currentName = item.Name;
			ctrl.mdPanelRef.close();
			ctrl.applyFavorite({ teamIds: angular.copy(item.TeamIds), searchTerm: item.SearchTerm });
		};

		function refreshList(data) {
			if (!angular.isArray(data)) return;

			ctrl.favoriteSearchList = data.sort(reorderListAccordingToIsDefault);
			favoriteSearchNameList = ctrl.favoriteSearchList.map(function (f) {
				return f.Name;
			});

			var defaultes = ctrl.favoriteSearchList.filter(function (f) {
				return f.IsDefault;
			});
			ctrl.currentFavorite = defaultes.length > 0 ? defaultes[0] : undefined;
			ctrl.currentName = ctrl.currentFavorite ? ctrl.currentFavorite.Name : '';
		}

		function reorderListAccordingToIsDefault(item1, item2){
			if(item1.IsDefault)
				return -1;
			if(item2.IsDefault)
				return 1;
			return 0;
		}
	}

})();