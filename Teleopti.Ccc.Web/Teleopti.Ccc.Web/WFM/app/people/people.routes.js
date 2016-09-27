(function () {
	'use strict';

	angular
	.module('wfm.people')
	.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('people', {
			url: '/people',
			params: {
				selectedPeopleIds: [],
				currentKeyword: '',
				paginationOptions: {}
			},
			templateUrl: 'app/people/html/people.html',
			controller: 'PeopleDefaultCtrl'
		}).state('people.start', {
			templateUrl: 'app/people/html/people-list.html',
			controller: 'PeopleStartCtrl'
		}).state('people.selection', {
			params: {
				selectedPeopleIds: [],
				commandTag: {},
				currentKeyword: '',
				paginationOptions: {}
			},
			templateUrl: 'app/people/html/people-selection-cart.html',
			controller: 'PeopleCartCtrl as vm'
		})
	}
})();
