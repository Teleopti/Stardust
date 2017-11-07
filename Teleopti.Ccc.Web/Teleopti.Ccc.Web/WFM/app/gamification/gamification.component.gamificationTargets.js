(function (angular) {

	'use strict';

	angular.module('wfm.gamification')
		.component('gamificationTargets', {
			templateUrl: 'app/gamification/html/g.component.gamificationTargets.tpl.html',
			controller: ['$translate', '$timeout', 'GamificationDataService', GamificationTargetsController]
		});

	function GamificationTargetsController($translate, $timeout, dataService) {
		var ctrl = this;

		var selectedText = '';

		ctrl.settings = [
			{ value: 'default', name: 'Default' },
			{ value: 'setting1', name: 'Setting 1' },
			{ value: 'setting2', name: 'Setting 2' }
		];

		ctrl.selectClosed = function () {
			var newText = ctrl.sitePickerSelectedText();
			if (newText !== selectedText) {
				selectedText = newText;
				loadTeams();
			}
		};

		ctrl.sitePickerSelectedText = function () {
			if (ctrl.selectedSites.length === 0) {
				return $translate.instant('SelectOneOrMoreSites');
			}
			return ctrl.selectedSites.sort(function (a, b) {
				if (a.position < b.position) {
					return -1;
				}
				return 1;
			})
			.map(function (site) { return site.name; })
			.join(', ');
		};

		ctrl.onAppliedSettingChange = function (teamIds, newValue) {
			console.log(teamIds, newValue);
		}

		ctrl.selectedSites = [];

		function loadTeams() {
			ctrl.isBusy = true;
			var siteIds = ctrl.selectedSites.map(function (site) { return site.id; });
			dataService.fetchTeams(siteIds).then(function (teams) {
				ctrl.teams = teams;
				ctrl.isBusy = false;
			});
		}

		dataService.fetchSites().then(function (sites) {
			ctrl.sites = sites;
		});
	}

})(angular);