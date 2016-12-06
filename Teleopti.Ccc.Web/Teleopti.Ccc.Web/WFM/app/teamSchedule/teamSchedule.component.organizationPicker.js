﻿(function () {
	'use strict';

	angular.module("wfm.teamSchedule")
		.component('organizationPicker',
		{
			templateUrl: 'app/teamSchedule/html/organizationPicker.tpl.html',
			controller: organizationPickerCtrl,
			bindings: {
				availableGroups: '<',
				onPick: '&'
			}
		});

	organizationPickerCtrl.$inject = [];

	function organizationPickerCtrl() {
		var ctrl = this;

		ctrl.$onInit = $onInit;
		ctrl.$onChanges = $onChanges;
		ctrl.onSelectionDone = onSelectionDone;
		ctrl.shortDisplayNameOfTheSelected = shortDisplayNameOfTheSelected;
		ctrl.isSiteChecked = isSiteChecked;
		ctrl.toggleSite = toggleSite;

		function $onInit() {
			populateGroupList();
			ctrl.selectedGroups = [];
			onSelectionDone();
		}

		function $onChanges(changesObj) {
			if (!changesObj.availableGroups || changesObj.availableGroups.length === 0) return;
			populateGroupList();

		}


		function populateGroupList() {
			ctrl.groupList = [];
			ctrl.availableGroups.forEach(function (g) {
				var site = { id: g.Id, name: g.Name, teams: [], isChecked: false};
				g.Children.forEach(function(t) {
					site.teams.push({
						id: t.Id,
						name: t.Name
					});
				});
				ctrl.groupList.push(site);
			});
		}

		function shortDisplayNameOfTheSelected() {
			if (!ctrl.selectedGroups) return '';
			if (ctrl.selectedGroups.length > 1)
				return ctrl.selectedGroups.length + " teams selected";
			for (var i = 0; i < ctrl.groupList.length; i++) {
				var teams = ctrl.groupList[i].teams
				for (var j = 0; j < teams.length; j++) {
					if (ctrl.selectedGroups[0] === teams[j].id) {
						return teams[j].name;
					}
				}
			}
			return '';
		}

		function toggleSite(teams) {
			if (!teams) return;
			var previousSiteStatus = isSiteChecked(teams);
			var currentStatus = !previousSiteStatus;
			if (currentStatus) {
				teams.forEach(function(t) {
					if (ctrl.selectedGroups.indexOf(t.id) < 0) {
						ctrl.selectedGroups.push(t.id);
					}
				});
			} else {
				teams.forEach(function (t) {
					if (ctrl.selectedGroups.indexOf(t.id) != -1) {
						ctrl.selectedGroups.splice(ctrl.selectedGroups.indexOf(t.id), 1);
					}
				});
			}
		}

		function isSiteChecked(teams) {
			if (!teams) return false;
			var isChecked = true;
			for (var i = 0; i < teams.length; i++) {
				if (ctrl.selectedGroups.indexOf(teams[i].id) < 0) {
					isChecked = false;
					break;
				}
			}
			return isChecked;
		}

		function onSelectionDone() {
			ctrl.onPick({ groups: ctrl.selectedGroups });
		}
	}

})();