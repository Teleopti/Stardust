(function() {
	'use strict';

	angular.module('wfm.intraday').config(stateConfig);

	//stateConfig.$inject = ['wfm.skillGroup'];

	function stateConfig($stateProvider) {
		$stateProvider
			.state('intraday', {
				url: '/intraday',
				templateUrl: 'app/intraday/intraday.html',
				resolve: {
					toggles: function(Toggle) {
						return Toggle;
					}
				},
				controller: function($state, toggles) {
					if (toggles.WFM_Intraday_Redesign_77214) {
						$state.go('intraday.modern');
					} else {
						$state.go('intraday.legacy');
					}
				},
				constrollerAs: 'vm'
			})

			.state('intraday.modern', { url: '/modern', template: '<ng2-intraday></ng2-intraday>' })
			.state('intraday.legacy', {
				url: '/legacy',
				templateUrl: 'app/intraday/intraday-area.html',
				controller: 'IntradayAreaController as vm'
			})
			// .state('intraday.skill-area-config', {
			// 	url: '/skill-area-config',
			// 	templateUrl: 'app/global/skill-group/skillgroup.html',
			// 	controller: 'SkillGroupController as vm'
			// })
			.state('intraday.skill-group-manager', {
				url: '/skill-group-manager',
				resolve: {
					toggles: function(Toggle) {
						return Toggle;
					}
				},
				params: {
					selectedGroup: {},
					returnState: 'intraday'
				},
				templateUrl: 'app/global/skill-group/skill-group-manager.html',
				controller: 'SkillGroupManagerController as vm'
			});
	}
})();
