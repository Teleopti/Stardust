(function() {
	'use strict';

	angular.module("wfm.teamSchedule")
		.component('organizationPicker', {
			templateUrl: 'app/teamSchedule/html/organizationPicker.tpl.html',
			controller: organizationPickerCtrl,
			bindings: {
				availableGroups: '<',
				onPick: '&'
			}
		});

	organizationPickerCtrl.$inject = ['$scope'];

	function organizationPickerCtrl($scope) {
		var ctrl = this;
		ctrl.currentSite = undefined;

		ctrl.$onInit = function() {
			populateGroupList();
			ctrl.selectedTeamIds = [];
			ctrl.onSelectionDone();
		};

		ctrl.$onChanges = function(changesObj) {
			if (!changesObj.availableGroups || changesObj.availableGroups.length === 0) return;
			populateGroupList();
		};

		function populateGroupList() {
			ctrl.groupList = [];
			ctrl.availableGroups.forEach(function(g) {
				var site = {
					id: g.Id,
					name: g.Name,
					teams: [],
					isChecked: false
				};
				g.Children.forEach(function(t) {
					site.teams.push({
						id: t.Id,
						name: t.Name
					});
				});
				ctrl.groupList.push(site);
			});
		}

		ctrl.formatSelectedDisplayName = function() {
			if (!ctrl.selectedTeamIds) return '';
			if (ctrl.selectedTeamIds.length > 1)
				return ctrl.selectedTeamIds.length + " teams selected";
			for (var i = 0; i < ctrl.groupList.length; i++) {
				var teams = ctrl.groupList[i].teams
				for (var j = 0; j < teams.length; j++) {
					if (ctrl.selectedTeamIds[0] === teams[j].id) {
						return teams[j].name;
					}
				}
			}
			return '';
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

		ctrl.setCurrentSiteValue = function(site) {
			ctrl.currentSite = site;
		};

		$scope.$watchCollection(function() {
			return ctrl.selectedTeamIds;
		}, function(newValue, oldValue, scope) {
			if (newValue)
				updateGroupSelection();
		});

		function updateGroupSelection() {
			if (!ctrl.currentSite) return;
			if (ctrl.currentSite.teams.every(function(team) {
					return ctrl.selectedTeamIds.indexOf(team.id) > -1;
				}))
				ctrl.currentSite.isChecked = true;
			else
				ctrl.currentSite.isChecked = false;
		}


		ctrl.onSelectionDone = function() {
			ctrl.searchTerm = '';

			//load the schedule data
		};
	}
})();