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
				controller: 'PeopleStart'
		})
			//.state('peopleold.start', {
			//	templateUrl: 'app/peopleold/html/people-list.html',
			//	controller: 'PeopleStartCtrl'
			//})
	}
})();
