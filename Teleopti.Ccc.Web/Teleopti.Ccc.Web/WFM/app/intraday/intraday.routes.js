(function () {
	'use strict';

	angular
		.module('wfm.intraday')
		.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider
			.state('intraday',
				{
					params: {
						isNewSkillArea: false,
						returnState: 'intraday'
					},
					url: '/intraday',
					templateUrl: 'app/intraday/intraday.html',
					controller: 'IntradayController as vm'
				})
			.state('intraday.area',
				{
					templateUrl: 'app/intraday/intraday-area.html',
					controller: 'IntradayAreaController as vm'
				})
			.state('intraday.skill-area-config',
				{
					url: '/skill-area-config',
					templateUrl: 'app/global/skill-group/skillgroup.html',
					controller: 'SkillGroupController as vm'
				})
	}
})();
