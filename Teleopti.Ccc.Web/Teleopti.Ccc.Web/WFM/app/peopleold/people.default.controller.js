(function() {
	'use strict';
	angular.module('wfm.peopleold')
		.controller('PeopleDefaultCtrl', [
		'$state', '$location', function ($state, $location) {		
			if ($location.url() == $state.current.url )			
				$state.go('peopleold.start');
		}
	]);
})();