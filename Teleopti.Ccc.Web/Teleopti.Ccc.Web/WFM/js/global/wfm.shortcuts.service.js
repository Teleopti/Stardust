
(function() {
	'use strict';

	angular.module('wfm')
		.service('WfmShortcuts', ['$state', 'ShortCuts', function($state, ShortCuts) {
			var service = {};

			service.init = function() {
				var keys = [16];

				ShortCuts.registerKeySequence(49, keys, function() {
					$state.go('forecasting')
				});
				ShortCuts.registerKeySequence(50, keys, function() {
					$state.go('resourceplanner')
				});
				ShortCuts.registerKeySequence(51, keys, function() {
					$state.go('permissions')
				});
				ShortCuts.registerKeySequence(52, keys, function() {
					$state.go('outbound')
				});
				ShortCuts.registerKeySequence(53, keys, function() {
					$state.go('people')
				});
				ShortCuts.registerKeySequence(54, keys, function() {
					$state.go('requests')
				});
				ShortCuts.registerKeySequence(55, keys, function() {
					$state.go('seatPlan')
				});
				ShortCuts.registerKeySequence(56, keys, function() {
					$state.go('seatMap')
				});
				ShortCuts.registerKeySequence(57, keys, function() {
					$state.go('rta')
				});
				ShortCuts.registerKeySequence(48, keys, function() {
					$state.go('intraday')
				});
			};

			return service;
		}]);
})();
