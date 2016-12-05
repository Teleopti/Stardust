(function () {
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
		ctrl.onSelectionChanged = onSelectionChanged;
		ctrl.shortDisplayNameOfTheSelected = shortDisplayNameOfTheSelected;
		ctrl.isSiteChecked = isSiteChecked;
		ctrl.toggleSite = toggleSite;

		function $onInit() {
			populateGroupList();
			ctrl.selectedGroups = [];
		    onSelectionChanged();
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
			if (ctrl.selectedGroups && ctrl.selectedGroups.length > 1)
				return ctrl.selectedGroups.length + "groups selected";
			return "to do";
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
			
			ctrl.onPick({ groups: ctrl.selectedGroups });
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

		function onSelectionChanged() {
			console.log("2222", ctrl.selectedGroups);
		}
	}

})();