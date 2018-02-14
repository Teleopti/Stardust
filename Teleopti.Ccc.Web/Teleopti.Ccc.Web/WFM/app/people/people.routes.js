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
				templateUrl: 'app/people/html/people.start.html',
				controller: 'PeopleDefault'
		})
		.state('people.mock', {
			templateUrl: 'app/people/html/people.html',
			controller: 'PeopleStart'
		})
		.state('people.new', {
			url: '/new',
			template: '<ng2-people></ng2-people>'
		})
	}
})();
