﻿(function () {
	var intraday = angular.module('wfm.intraday', ['gridshore.c3js.chart', 'ngResource', 'ui.router', 'wfm.notice', 'pascalprecht.translate']);

	intraday.run([
  		'$rootScope', '$state', '$location', function ($rootScope, $state, $location) {

  			$rootScope.$on('$stateChangeSuccess',
  				function (event, toState) {
  					if ($location.url() == $state.current.url && toState.name == 'intraday') $state.go('intraday.area');
  				});

  		}
	]);

})();
