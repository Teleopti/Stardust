(function() {
	'use strict';

	angular.module('wfm.intraday').config(stateConfig);

	// if (toggles.WFM_Intraday_Redesign_77214) {
	// 	$state.go('intraday.modern');
	// } else {

	function stateConfig($stateProvider, ToggleProvider) {
		var toggles = ToggleProvider;

		const intraday = {
			abstract: true,
			name: 'intraday',
			template: '<div><section ui-view></section></div>'
		};
		const intradayArea = {
			name: 'intraday.intradayArea',
			url: '/intraday',
			parent: intraday,
			params: {
				isNewSkillArea: false
			},
			templateUrl: function() {
				return toggles.WFM_Intraday_Redesign_77214
					? 'app/intraday/intraday-modern.html'
					: 'app/intraday/intraday-area.html';
			},
			controller: 'IntradayAreaController as vm'
		};
		const groupManager = {
			name: 'intraday.skill-group-manager',
			parent: intraday,
			params: {
				returnState: 'intraday.intradayArea',
				isNewSkillArea: false,
				selectedGroup: {}
			},
			url: '/intraday/skill-group-manager',
			templateUrl: 'app/global/skill-group/skill-group-manager.html',
			controller: 'SkillGroupManagerController as vm'
		};

		$stateProvider
			.state(intraday)
			.state(intradayArea)
			.state(groupManager);
	}
})();
