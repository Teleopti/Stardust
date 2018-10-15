(function() {
	'use strict';
	angular
		.module('wfm.forecasting', [
			'gridshore.c3js.chart',
			'ngResource',
			'toggleService',
			'ui.router',
			'wfm.oldDateRangePicker',
			'wfm.workingHoursPicker',
			'pascalprecht.translate',
			'wfm.modal',
			'wfm.autofocus',
			'wfm.notice',
			'wfm.utilities'
		])
		.run(['$rootScope', '$state', '$location', onStateChangeSuccess]);

	function onStateChangeSuccess($rootScope, $state, $location) {
		$rootScope.$on('$stateChangeSuccess', function(_event, toState) {
			if ($location.url() === $state.current.url && toState.name === 'forecasting')
				$state.go('forecasting.start');
		});
	}
})();
