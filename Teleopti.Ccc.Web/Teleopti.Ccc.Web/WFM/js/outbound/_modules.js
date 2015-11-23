(function() {
	'use strict';
	var outbound = angular.module('wfm.outbound', [
			'ui.router',
			'outboundServiceModule',
			'ngAnimate',
			'pascalprecht.translate',
			'angularMoment',
			'wfm.cardList',
			'wfm.daterangepicker',
			'wfm.workinghourspicker',
			'toggleService',
			'gantt',
			'gantt.table',
			'gantt.tooltips',
			'wfm.numericValue'
		]
	);
	angular.module('outboundServiceModule', [ 'ngResource', 'pascalprecht.translate']);

	outbound.run([
		'$rootScope', '$state', '$location', function($rootScope, $state, $location) {
			$rootScope.$on('$stateChangeSuccess',
				function(event, toState) {
					if ($location.url() == $state.current.url && toState.name == 'outbound') $state.go('outbound.summary');
				});
		}
	]);
})();