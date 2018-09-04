(function() {
	'use strict';

	angular.module('wfm.forecasting').config(stateConfig);

	function stateConfig($stateProvider, $urlRouterProvider) {
		$stateProvider
			.state('forecasting', {
				url: '/forecasting',
				template: '<div><section ui-view></section></div>',
				controllerAs: 'vm',
				controller: function($state) {
					// toggles.WFM_Forecaster_Refact_44480
					$state.go('forecast');
				}
			})
			.state('forecasting.start', {});

		$stateProvider
			.state('forecast', {
				url: '/forecast',
				templateUrl: 'app/forecasting/html/r2forecast.html',
				controller: 'r2ForecastRefactController',
				controllerAs: 'vm'
			})
			.state('forecast-modify', {
				url: '/forecast/modify/:workloadId',
				templateUrl: 'app/forecasting/html/modify.html',
				controller: 'ForecastModifyController',
				controllerAs: 'vm',
				params: {
					workloadId: undefined
				}
			})
			.state('forecast-history', {
				url: '/forecast/history/:workloadId',
				templateUrl: 'app/forecasting/html/history.html',
				controller: 'ForecastHistoryController',
				controllerAs: 'vm',
				params: {
					workloadId: undefined
				}
			})
			.state('forecast-create-skill', {
				url: '/forecast/create',
				templateUrl: 'app/forecasting/html/skillCreate.html',
				controller: 'ForecastingSkillCreateController',
				controllerAs: 'vm'
			})
			.state('forecast-no-skills', {
				url: '/forecast/noskills',
				templateUrl: 'app/forecasting/html/noSkills.html'
			});
	}
})();
