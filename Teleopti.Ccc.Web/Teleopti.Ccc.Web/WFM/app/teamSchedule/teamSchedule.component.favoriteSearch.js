﻿(function () {
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

	favoriteSearchCtrl.$inject = ['$translate','$mdPanel', '$wfmModal', 'FavoriteSearchDataService'];

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
				controller: ['mdPanelRef', function(mdPanelRef) {
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
		};

		ctrl.$onInit = function () {
			FavoriteSearchDataService.getFavoriteSearchList().then(function (resp) {
				refreshList(resp.data);
			});
		}

		ctrl.save = function() {
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
		}

		ctrl.disableSave = function () {
			if (!ctrl.currentFavorite) return false;

			var currentSearch = ctrl.getSearch();
			return angular.equals(ctrl.currentFavorite.TeamIds, currentSearch.teamIds) &&
				ctrl.currentFavorite.SearchTerm == currentSearch.searchTerm;
		};

		function popDialog() {
			var title = $translate.instant('UpdateConfirm');
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
			if (index != -1) {

				if (ctrl.favoriteSearchList[index].IsDefault) {
					FavoriteSearchDataService.removeDefault(name)
						.then(function () {
							ctrl.favoriteSearchList[index].IsDefault = false;
						});
				} else {
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
		}

		ctrl.delete = function (name) {
			var index = favoriteSearchNameList.indexOf(name);
			FavoriteSearchDataService.delete(name)
				.then(function () {
					ctrl.favoriteSearchList.splice(index, 1);
					favoriteSearchNameList.splice(index, 1);
				});
		}

		ctrl.select = function (item) {
			ctrl.currentFavorite = item;
			ctrl.currentName = item.Name;
			ctrl.mdPanelRef.close();
			ctrl.applyFavorite({ teamIds: angular.copy(item.TeamIds), searchTerm: item.SearchTerm });
		}

		function refreshList(data) {
			if (!angular.isArray(data)) return;

			ctrl.favoriteSearchList = data;
			favoriteSearchNameList = ctrl.favoriteSearchList.map(function (f) {
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