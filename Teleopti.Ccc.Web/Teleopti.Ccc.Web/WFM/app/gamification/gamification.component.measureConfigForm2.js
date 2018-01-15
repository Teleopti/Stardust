;(function (angular) { 'use strict';
	angular.module('wfm.gamification')
		.component('measureConfigForm2', {
			templateUrl: 'app/gamification/html/measureConfigForm2.tpl.html',
			bindings: {
				bronzeBadgeThreshold: '<',
				silverBadgeThreshold: '<',
				goldBadgeThreshold: '<'
			},
			controller: [function measureConfigForm2Ctrl() {

			}]
		});
})(angular);