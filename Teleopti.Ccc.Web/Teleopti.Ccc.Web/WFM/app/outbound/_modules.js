﻿(function() {
	'use strict';

	angular.module('outboundServiceModule', ['ngResource', 'pascalprecht.translate']);

	var outbound = angular.module('wfm.outbound', [
		'ui.router',
		'outboundServiceModule',
		'ngAnimate',
		'pascalprecht.translate',
		'angularMoment',
		'wfm.cardList',
		'wfm.daterangepicker',
		'wfm.workingHoursPicker',
		'toggleService',
		'gantt',
		'gantt.table',
		'gantt.tooltips',
		'wfm.notice',
		'wfm.numericValue',
		'ngStorage'
	]);

	outbound.run([
		'$rootScope',
		'$state',
		'$location',
		function($rootScope, $state, $location) {
			$rootScope.$on('$stateChangeSuccess', function(event, toState) {
				if ($location.url() == $state.current.url && toState.name == 'outbound') $state.go('outbound.summary');
			});
		}
	]);
})();
