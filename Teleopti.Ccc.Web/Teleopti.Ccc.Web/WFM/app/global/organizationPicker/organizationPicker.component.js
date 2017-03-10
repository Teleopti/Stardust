﻿(function () {
	'use strict';

	angular.module('wfm.organizationPicker')
		.component('organizationPicker',
		{
			templateUrl: [
				'$attrs', function ($attrs) {
					if (checkIsMultiple($attrs)) {
						return 'app/global/organizationPicker/organizationPicker.tpl.html';
					}
					return 'app/global/organizationPicker/organizationPicker.single.tpl.html';
				}
			],
			controller: organizationPickerCtrl,
			bindings: {
				date: '<',
				preselectedTeamIds: '<?',
				onOpen: '&',
				onPick: '&',
				onInitAsync: '<?'
			}
		});

	function checkIsMultiple(attrs) {
		return !('single' in attrs);
	}

	organizationPickerCtrl.$inject = ['$scope', '$translate', '$attrs', 'organizationPickerSvc'];

	function organizationPickerCtrl($scope, $translate, $attrs, orgPickerSvc) {
		var ctrl = this,
			currentSite,
			initialSelectedTeamIds = [],
			isMultiple = checkIsMultiple($attrs);

		ctrl.groupList = [];
		ctrl.availableTeamIds = [];
		ctrl.searchTerm = '';
		ctrl.selectedTeamIds = [];

		ctrl.$onInit = function init() {
			orgPickerSvc.getAvailableHierarchy(moment(ctrl.date).format('YYYY-MM-DD'))
				.then(function (resp) {
					populateGroupList({ sites: resp.data.Children });
					if (!ctrl.preselectedTeamIds || !ctrl.preselectedTeamIds.length) {
						ctrl.preselectedTeamIds = [];
						if (resp.data.LogonUserTeamId) {
							ctrl.preselectedTeamIds.push(resp.data.LogonUserTeamId);
						}
					}
					if (ctrl.preselectedTeamIds.length) {
						if (!isMultiple) {
							ctrl.selectedTeamIds = ctrl.preselectedTeamIds[0];
						} else {
							ctrl.selectedTeamIds = angular.copy(ctrl.preselectedTeamIds);
						}
					}
					ctrl.selectedTeamIds = angular.copy(ctrl.preselectedTeamIds);

					updateAllSiteSelection();

					if (ctrl.onInitAsync) {
						if (ctrl.selectedTeamIds.length > 0) {
							ctrl.onInitAsync.resolve(angular.copy(ctrl.selectedTeamIds));
						} else {
							ctrl.onInitAsync.resolve();
						}
					}

				});
		};


		ctrl.$onChanges = function (changesObj) {
			if (!changesObj.preselectedTeamIds) return;
			if (changesObj.preselectedTeamIds.currentValue === changesObj.preselectedTeamIds.previousValue) return;
			ctrl.selectedTeamIds = angular.copy(changesObj.preselectedTeamIds.currentValue);
			updateAllSiteSelection();
		}

		function populateGroupList(groupData) {
			var groupList = [];
			var availableTeamIds = [];

			groupData.sites.forEach(function (g) {
				var site = {
					id: g.Id,
					name: g.Name,
					teams: [],
					isChecked: false,
					expanded: false
				};
				g.Children.forEach(function (t) {
					site.teams.push({
						id: t.Id,
						name: t.Name
					});
					availableTeamIds.push(t.Id);
				});
				groupList.push(site);
			});

			ctrl.groupList = groupList;
			ctrl.availableTeamIds = availableTeamIds;
		}

		ctrl.formatSelectedDisplayName = function () {
			if (!ctrl.selectedTeamIds) return '';

			if (ctrl.selectedTeamIds.length > 1) {
				return $translate.instant("SeveralTeamsSelected").replace("{0}", ctrl.selectedTeamIds.length);
			}

			for (var i = 0; i < ctrl.groupList.length; i++) {
				var teams = ctrl.groupList[i].teams;
				for (var j = 0; j < teams.length; j++) {
					if (ctrl.selectedTeamIds[0] === teams[j].id) {
						return teams[j].name;
					}
				}
			}
			return $translate.instant("Organization");
		};

		ctrl.processSearchTermFilter = function (site) {
			if (site && ctrl.searchTerm.length > 0) {
				if (site.name.toLowerCase().indexOf(ctrl.searchTerm.toLowerCase()) > -1) {
					return '';
				}
				return ctrl.searchTerm;
			}
		};

		ctrl.showTeamListOfSite = function (site, event) {
			site.expanded = !site.expanded;
			event.stopPropagation();
		};

		ctrl.updateSiteCheck = function (site) {
			if (site)
				toggleSiteSelection(site);
		};

		function toggleSiteSelection(site) {
			var checked = site.isChecked,
				index,
				teamId;

			site.teams.forEach(function (team) {
				teamId = team.id;
				index = ctrl.selectedTeamIds.indexOf(teamId);
				if (checked && index === -1) {
					ctrl.selectedTeamIds.push(teamId);
				}
				if (!checked && index > -1)
					ctrl.selectedTeamIds.splice(index, 1);
			});

		}
		function isTeamSelected(teamId) {
			if (isMultiple) {
				return ctrl.selectedTeamIds.indexOf(teamId) > -1;
			}
			return ctrl.selectedTeamIds === teamId;
		}

		ctrl.partialTeamsSelected = function (site) {
			if (!site) return false;

			var some = false,
				all = true;

			site.teams.forEach(function (team) {
				if (isTeamSelected(team.id)) {
					some = true;
				} else {
					all = false;
				}
			});

			return some && !all;
		};

		ctrl.hasTeamsSelected = function (site) {
			if (!site) return false;
			var hasItem = false;
			site.teams.forEach(function (team) {
				if (isTeamSelected(team.id)) {
					hasItem = true;
				}
			});
			return hasItem;
		}

		ctrl.setCurrentSiteValue = function (site) {
			currentSite = site;
		};

		$scope.$watchCollection(function () {
			return ctrl.selectedTeamIds;
		},
			function (newValue) {
				if (!newValue) return;
				updateGroupSelection(currentSite);
			});


		function updateAllSiteSelection() {
			if (!ctrl.selectedTeamIds || ctrl.selectedTeamIds.length === 0 || !ctrl.groupList || !ctrl.groupList.length) return;
			ctrl.groupList.forEach(updateGroupSelection);
		}

		function updateGroupSelection(site) {
			if (!site) return;
			if (site.teams &&
				site.teams.every(function (team) {
					return isTeamSelected(team.id);
			}))
				site.isChecked = true;
			else
				site.isChecked = false;
		}

		ctrl.searchOrganization = function () {
			if (ctrl.searchTerm.length > 0) {
				ctrl.groupList.forEach(function (site) {
					site.expanded = true;
				});
			} else {
				ctrl.groupList.forEach(function (site) {
					site.expanded = false;
				});
			}
		};

		ctrl.onPickerOpen = function () {
			ctrl.onOpen();
			initialSelectedTeamIds = angular.copy(ctrl.selectedTeamIds);
		};

		ctrl.onPickerClose = function () {
			ctrl.searchTerm = '';

			if (ctrl.groupList.length > 0) {
				ctrl.groupList.forEach(function (s) {
					s.expanded = false;
				});
			}

			//load the schedule data when team ids changed
			if (isMultiple) {
				if (ctrl.selectedTeamIds.length === initialSelectedTeamIds.length &&
					ctrl.selectedTeamIds.every(function (id) {
						return initialSelectedTeamIds.indexOf(id) > -1;
				})) {
					return;
				}
			} else {
				if (initialSelectedTeamIds == ctrl.selectedTeamIds) {
					return;
				}
			}

			ctrl.onPick({
				teams: angular.copy(ctrl.selectedTeamIds)
			});

		};
	}
})();