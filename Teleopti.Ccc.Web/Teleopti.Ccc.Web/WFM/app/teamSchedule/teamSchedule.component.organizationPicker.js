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

	organizationPickerCtrl.$inject = ['$scope', '$translate'];

	function organizationPickerCtrl($scope, $translate) {
		var ctrl = this,
			currentSite,
			logonUserTeamId;

		ctrl.groupList = [];
		ctrl.selectedTeamIds = [];

		ctrl.$onInit = function() {
			populateGroupList();
			ctrl.selectedTeamIds.push(logonUserTeamId);
			ctrl.onSelectionDone();
		};

		ctrl.$onChanges = function(changesObj) {
			if (!changesObj.availableGroups || !changesObj.availableGroups.sites || changesObj.availableGroups.sites.length == 0) return;
			populateGroupList();
		};

		function populateGroupList() {
			if(!ctrl.availableGroups.sites || ctrl.availableGroups.sites.length == 0) return;
			ctrl.groupList = [];
			logonUserTeamId = ctrl.availableGroups.logonUserTeamId;

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
			return $translate.instant('Organization');
		};
		
		ctrl.processSearchTermFilter = function(site) {
			if(site && ctrl.searchTerm.length > 0) {
				if(site.name.toLowerCase().indexOf(ctrl.searchTerm.toLowerCase()) > -1) {
					return '';
				}
				return ctrl.searchTerm;
			}
		};

		ctrl.showTeamListOfSite = function(site, event){
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

		ctrl.partialTeamsSelected = function(site){
			return site && site.teams.some(function(team){
				return ctrl.selectedTeamIds.indexOf(team.id) > -1;
			});
		};

		ctrl.setCurrentSiteValue = function(site) {
			currentSite = site;
		};

		$scope.$watchCollection(function() {
			return ctrl.selectedTeamIds;
		}, function(newValue, oldValue, scope) {
			if (newValue)
				updateGroupSelection();
		});

		function updateGroupSelection() {
			if (!currentSite) return;
			if (currentSite.teams.every(function(team) {
					return ctrl.selectedTeamIds.indexOf(team.id) > -1;
				}))
				currentSite.isChecked = true;
			else
				currentSite.isChecked = false;
		}

		ctrl.onSearchOrganization = function($event){
			$event.stopPropagation();
		};

		ctrl.onSelectionDone = function() {
			ctrl.searchTerm = '';

			if(ctrl.groupList.length > 0){
				ctrl.groupList.forEach(function(s){
					s.expanded = false;
				});
			}

			//load the schedule data
			ctrl.onPick({
				groups: ctrl.selectedTeamIds
			});
		};
	}
})();