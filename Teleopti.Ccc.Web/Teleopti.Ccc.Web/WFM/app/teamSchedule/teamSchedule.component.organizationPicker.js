(function() {
	'use strict';

	angular.module("wfm.teamSchedule")
		.component('organizationPicker', {
			templateUrl: 'app/teamSchedule/html/organizationPicker.tpl.html',
			controller: organizationPickerCtrl,
			bindings: {
				availableGroups: '<',
				selectedTeamIds: '<?',
				onPick: '&'
			}
		});

	organizationPickerCtrl.$inject = ['$scope', '$translate'];

	function organizationPickerCtrl($scope, $translate) {
		var ctrl = this,
			currentSite,
			preSelectedTeamIds = [],
			initialSelectedTeamIds = [];

		ctrl.groupList = [];
		ctrl.availableTeamIds = [];
		ctrl.selectedTeamIds = [];

		ctrl.$onInit = function() {};

		ctrl.refresh = function() {
			populateGroupList();

			if (preSelectedTeamIds != null && preSelectedTeamIds.length > 0) {
				if (preSelectedTeamIds.every(function(tId) {
						return ctrl.availableTeamIds.indexOf(tId) > -1;
					})) {
					ctrl.selectedTeamIds = ctrl.selectedTeamIds.concat(preSelectedTeamIds);
				}
			}

			updateAllSiteSelection();
			ctrl.onSelectionDone();
		};

		ctrl.$onChanges = function(changesObj) {
			if (!changesObj ||
				!changesObj.availableGroups ||
				!changesObj.availableGroups.currentValue ||
				!changesObj.availableGroups.currentValue.sites ||
				changesObj.availableGroups.currentValue.sites.length == 0) return;

			ctrl.refresh();
		};

		ctrl.onPickerOpen = function() {
			initialSelectedTeamIds = ctrl.selectedTeamIds.concat();
		};

		function populateGroupList() {
			if (!ctrl.availableGroups || !ctrl.availableGroups.sites || ctrl.availableGroups.sites.length == 0) return;
			ctrl.groupList = [];

			preSelectedTeamIds = ctrl.availableGroups.preSelectedTeamIds;

			ctrl.availableGroups.sites.forEach(function(g) {
				var site = {
					id: g.Id,
					name: g.Name,
					teams: [],
					isChecked: false,
					expanded: false
				};
				g.Children.forEach(function(t) {
					site.teams.push({
						id: t.Id,
						name: t.Name
					});
					ctrl.availableTeamIds.push(t.Id);
				});
				ctrl.groupList.push(site);
			});
		}

		ctrl.formatSelectedDisplayName = function() {
			if (!ctrl.selectedTeamIds) return '';

			if (ctrl.selectedTeamIds.length > 1)
				return $translate.instant("SeveralTeamsSelected").replace("{0}", ctrl.selectedTeamIds.length);

			for (var i = 0; i < ctrl.groupList.length; i++) {
				var teams = ctrl.groupList[i].teams
				for (var j = 0; j < teams.length; j++) {
					if (ctrl.selectedTeamIds[0] === teams[j].id) {
						return teams[j].name;
					}
				}
			}
			return $translate.instant("Organization");
		};

		ctrl.processSearchTermFilter = function(site) {
			if (site && ctrl.searchTerm.length > 0) {
				if (site.name.toLowerCase().indexOf(ctrl.searchTerm.toLowerCase()) > -1) {
					return '';
				}
				return ctrl.searchTerm;
			}
		};

		ctrl.showTeamListOfSite = function(site, event) {
			site.expanded = !site.expanded;
			event.stopPropagation();
		};

		ctrl.updateSiteCheck = function(site) {
			if (site)
				toggleSiteSelection(site);
		};

		function toggleSiteSelection(site) {
			var checked = site.isChecked,
				index, teamId;

			site.teams.forEach(function(team) {
				teamId = team.id;
				index = ctrl.selectedTeamIds.indexOf(teamId);
				if (checked && index === -1)
					ctrl.selectedTeamIds.push(teamId);
				if (!checked && index > -1)
					ctrl.selectedTeamIds.splice(index, 1);
			});
		}

		ctrl.partialTeamsSelected = function(site) {
			if (!site) return false;

			var some = false,
				all = true;

			site.teams.forEach(function(team) {
				if (ctrl.selectedTeamIds.indexOf(team.id) > -1) {
					some = true;
				} else {
					all = false;
				}
			});

			return some && !all;
		};

		ctrl.setCurrentSiteValue = function(site) {
			currentSite = site;
		};

		$scope.$watchCollection(function() {
			return ctrl.selectedTeamIds;
		}, function(newValue, oldValue, scope) {
			if (newValue)
				updateGroupSelection(currentSite);
		});

		function updateAllSiteSelection() {
			ctrl.groupList.forEach(updateGroupSelection);
		}

		function updateGroupSelection(site) {
			if (!site) return;
			if (site.teams && site.teams.every(function(team) {
					return ctrl.selectedTeamIds.indexOf(team.id) > -1;
				}))
				site.isChecked = true;
			else
				site.isChecked = false;
		}

		ctrl.onSearchOrganization = function($event) {
			$event.stopPropagation();
		};

		ctrl.onSelectionDone = function() {
			ctrl.searchTerm = '';

			if (ctrl.groupList.length > 0) {
				ctrl.groupList.forEach(function(s) {
					s.expanded = false;
				});
			}

			//load the schedule data when team ids changed
			if (ctrl.selectedTeamIds.length == initialSelectedTeamIds.length &&
				ctrl.selectedTeamIds.every(function(id) {
					return initialSelectedTeamIds.indexOf(id) > -1;
				})) {
				return;
			}

			ctrl.onPick({
				teams: angular.copy(ctrl.selectedTeamIds)
			});
		};
	}
})();