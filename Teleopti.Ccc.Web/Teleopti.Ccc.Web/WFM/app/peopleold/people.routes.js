(function () {
	'use strict';

	angular
		.module('wfm.peopleold')
	.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('peopleold', {
			url: '/peopleold',
			params: {
				selectedPeopleIds: [],
				currentKeyword: '',
				paginationOptions: {}
			},
			templateUrl: 'app/peopleold/html/people.html',
			controller: 'PeopleDefaultCtrl'
		}).state('peopleold.start', {
			templateUrl: 'app/peopleold/html/people-list.html',
			controller: 'PeopleStartCtrl'
		}).state('peopleold.selection', {
			params: {
				selectedPeopleIds: [],
				commandTag: {},
				currentKeyword: '',
				paginationOptions: {}
			},
			templateUrl: 'app/peopleold/html/people-selection-cart.html',
			controller: 'PeopleCartCtrl as vm'
		})
		.state('peopleold.importagents', {
			url: '/importagents',
			template: '<wfm-import-agents></wfm-import-agents>'
		})
	}
})();
