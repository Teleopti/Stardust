(function () {
	'use strict';

	angular.module('wfm.favoriteSearch')
		.component('favoriteSearch',
		{
			templateUrl: 'app/global/favoriteSearch/favoriteSearch.tpl.html',
			controller: favoriteSearchCtrl,
			bindings: {
				currentFavorite: '<?', //  if set to false, the default favorite is disabled
				onInitAsync: '<?',
				applyFavorite: '&?',
				getSearch: '&'
			}
		});

	favoriteSearchCtrl.$inject = ['$translate', '$mdPanel', '$wfmConfirmModal', 'FavoriteSearchDataService'];

	function favoriteSearchCtrl($translate, $mdPanel, $wfmModal, FavoriteSearchDataService) {
		var ctrl = this;
		ctrl.favoriteSearchList = [];
		ctrl.isTest = false;
		ctrl.searchItemSaved = false;
		var favoriteSearchNameList = [];

		ctrl.openPanel = function (ev) {
			var currentSearch = ctrl.getSearch();

			if (ctrl.currentFavorite && !isSameSearch(currentSearch, ctrl.currentFavorite)) {
				ctrl.currentFavorite = false;
				ctrl.currentName = '';
			}

			var position = $mdPanel.newPanelPosition()
				.relativeTo('.fav-search-open-btn')
				.addPanelPosition($mdPanel.xPosition.ALIGN_START, $mdPanel.yPosition.BELOW);
			var config = {
				attachTo: angular.element(document.body),
				controller: [
					'mdPanelRef', function(mdPanelRef) {
						if (mdPanelRef) ctrl.mdPanelRef = mdPanelRef;
						return ctrl;
					}
				],
				controllerAs: '$ctrl',
				templateUrl: 'app/global/favoriteSearch/favoriteSearchPanel.tpl.html',
				panelClass: 'fav-search-panel',
				position: position,
				openFrom: ev,
				clickOutsideToClose: true,
				escapeToClose: true,
				focusOnOpen: false,
				zIndex: 40, // keep this below the z-index of modals defined in the styleguide
				onRemoving: function() {
					ctrl.currentName = ctrl.currentFavorite ? ctrl.currentFavorite.Name : '';
				}
			};
			$mdPanel.open(config);
		};

		function isSameSearch(s1, s2) {
			if (!s1 || !s2) return false;
			if (s1.SearchTerm !== s2.SearchTerm || s1.TeamIds.length !== s2.TeamIds.length) return false;

			for (var i = 0; i < s1.TeamIds.length; i++) {
				if (s1.TeamIds[i] !== s2.TeamIds[i]) return false;
			}
			return true;
		}

		ctrl.$onInit = function () {
			FavoriteSearchDataService.initPermission();

			FavoriteSearchDataService.hasPermission()
				.then(function(response) {
					ctrl.enabled = response.data;
					if (!ctrl.enabled) {
						ctrl.onInitAsync.resolve();
						return;
					}

					FavoriteSearchDataService.getFavoriteSearchList().then(function (resp) {
						initializeFavoriteList(resp.data);
						if (ctrl.onInitAsync) {
							ctrl.onInitAsync.resolve(ctrl.currentFavorite);
						}
					});
				});
		};

		ctrl.$onChanges = function (changObj) {			
			if (!changObj || !changObj.currentFavorite ) return;
			updateCurrentFavorite(changObj.currentFavorite.currentValue);
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

			var isNameValid = !!(ctrl.currentName) && ctrl.currentName !== '' && ctrl.currentName.length <= 50;
			var hasTeamIds = angular.isArray(currentSearch.TeamIds) && currentSearch.TeamIds.length > 0;
			if (!ctrl.currentFavorite) return isNameValid && hasTeamIds;

			var nameChanged = ctrl.currentName != ctrl.currentFavorite.Name;
			var notSameTeamIdsAndSearchTerm = !angular.equals(ctrl.currentFavorite.TeamIds, currentSearch.TeamIds) ||
				ctrl.currentFavorite.SearchTerm != currentSearch.SearchTerm;

			if(ctrl.searchItemSaved) return false;

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
			var index = favoriteSearchNameList.indexOf(ctrl.currentName);
			var currentFavorite = angular.copy(ctrl.currentFavorite || ctrl.favoriteSearchList[index]);

			var updatedCurrent = {
				Id: currentFavorite.Id,
				Name: currentFavorite.Name,
				TeamIds: currentSearch.TeamIds,
				SearchTerm: currentSearch.SearchTerm,
				IsDefault: currentFavorite.IsDefault
			};
			FavoriteSearchDataService.update(updatedCurrent)
				.then(function () {
					if(ctrl.currentFavorite){
						ctrl.currentFavorite.SearchTerm = updatedCurrent.SearchTerm;
						ctrl.currentFavorite.TeamIds = updatedCurrent.TeamIds;
					}
					ctrl.favoriteSearchList[index] = angular.copy(updatedCurrent);
					ctrl.searchItemSaved = true;
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

					if (ctrl.currentFavorite.Name === name) {
						ctrl.currentName = '';
						ctrl.currentFavorite = false;
					}
				});
		};

		ctrl.select = function (item) {
			ctrl.currentFavorite = item;
			ctrl.currentName = item.Name;
			if (ctrl.mdPanelRef)
				ctrl.mdPanelRef.close();
			ctrl.applyFavorite({
				currentFavorite: {
					TeamIds: angular.copy(ctrl.currentFavorite.TeamIds),
					SearchTerm: angular.copy(ctrl.currentFavorite.SearchTerm),
					Name: ctrl.currentName
				}
			});
		};

		function initializeFavoriteList(data) {
			if (!angular.isArray(data)) return;

			ctrl.favoriteSearchList = data.sort(reorderListAccordingToIsDefault);
			favoriteSearchNameList = ctrl.favoriteSearchList.map(function (f) {
				return f.Name;
			});

			var defaults = ctrl.favoriteSearchList.filter(function (f) {
				return f.IsDefault;
			});

			if(!ctrl.currentFavorite && ctrl.currentFavorite !== false){
				updateCurrentFavorite(defaults[0]);
			}
		}

		function updateCurrentFavorite(curFavorite) {
			ctrl.currentFavorite = curFavorite;
			ctrl.currentName = curFavorite ? curFavorite.Name : '';			
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