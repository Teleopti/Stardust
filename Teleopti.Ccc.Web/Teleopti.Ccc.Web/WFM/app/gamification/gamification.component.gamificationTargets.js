(function (angular) {

	'use strict';

	angular.module('wfm.gamification')
		.component('gamificationTargets', {
			templateUrl: 'app/gamification/html/g.component.gamificationTargets.tpl.html',
			controller: ['$translate', GamificationTargetsController]
		});

	function GamificationTargetsController($translate) {
		var ctrl = this;

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

		ctrl.selectedSites = [];

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