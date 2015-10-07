'use strict';
angular.module('wfm.forecasting')
	.controller('ForecastingDefaultCtrl', [
		'$state', '$location', function($state, $location) {
			if ($location.url() == $state.current.url)
				$state.go('forecasting.start');
		}
	]);
