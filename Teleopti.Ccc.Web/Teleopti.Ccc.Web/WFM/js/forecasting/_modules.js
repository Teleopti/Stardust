﻿(function () {
	'use strict';
	var forecaster = angular.module('wfm.forecasting', ['gridshore.c3js.chart', 'ngResource', 'toggleService', 'ui.router', 'wfm.workinghourspicker', 'pascalprecht.translate','wfm.modal', 'wfm.autofocus']);


	forecaster.run([
		'$rootScope', '$state', '$location', function ($rootScope, $state, $location) {

			$rootScope.$on('$stateChangeSuccess',
				function (event, toState) {
					if ($location.url() == $state.current.url && toState.name == 'forecasting') $state.go('forecasting.start');
				});

		}
	]);

})();
