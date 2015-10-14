(function() {
	'use strict';
	angular.module('wfm.outbound', [
			'outboundServiceModule',
			'ngAnimate',
			'pascalprecht.translate',
			'angularMoment',
			'wfm.cardList',
			'wfm.daterangepicker',
			'wfm.timerangepicker',
			'toggleService',
			'gantt',
			'gantt.table',
			'gantt.tooltips'
		]
	);
	angular.module('outboundServiceModule', ['ngResource', 'pascalprecht.translate']).run([
		'$rootScope', '$state', '$location', function($rootScope, $state, $location) {

			$rootScope.$on('$stateChangeSuccess',
				function(event, toState) {
					if ($location.url() == $state.current.url && toState.name == 'outbound') $state.go('outbound.summary');
				});

		}
	]);
})();