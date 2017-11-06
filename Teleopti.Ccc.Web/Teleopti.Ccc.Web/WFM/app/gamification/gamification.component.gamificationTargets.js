(function (angular) {

	'use strict';

	angular.module('wfm.gamification')
		.component('gamificationTargets', {
			templateUrl: 'app/gamification/html/g.component.gamificationTargets.tpl.html',
			controller: ['$translate', '$timeout', GamificationTargetsController]
		});

	function GamificationTargetsController($translate, $timeout) {
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
			$timeout(function () {
				var teams = [];
				ctrl.selectedSites.forEach(function (site) {
					for (var i = 0; i < site.position; i++) {
						teams.push({
							teamId: site.id * 10 + i,
							teamName: 'Site ' + (site.position + 1) + '/Team ' + (i + 1),
							appliedSettingValue: i === 0 ? ctrl.settings[0].value : ctrl.settings[1].value
						});
					}
				});
				ctrl.teams = teams;
				ctrl.isBusy = false;
			}, 3000);
		}

		function sites(n) {
			var sites = [];
			for (var i = 0; i < n; i++) {
				sites.push({ position: i, id: i, name: 'Site ' + i });
			}
			return sites;
		}

		ctrl.sites = sites(50);
	}

})(angular);