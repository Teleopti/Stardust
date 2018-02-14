(function() {
	'use strict';
	angular.module('wfm.people')
		.controller('PeopleDefault', [
		'$state', '$location', function ($state, $location) {		
			if ($location.url() == $state.current.url )			
				$state.go('people.mock');
		}
	]);
})();