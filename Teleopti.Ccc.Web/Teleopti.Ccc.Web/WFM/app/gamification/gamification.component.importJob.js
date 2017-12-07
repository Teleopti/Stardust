(function (angular) {
	'use strict';
	angular.module('wfm.gamification').component('importJob', {
		templateUrl: 'app/gamification/html/g.component.importJob.tpl.html',
		bindings: {
			name: '<',
			owner: '<',
			jobId: '<',
			startingTime: '<',
			status: '<',
			category: '<'

		},
		controller: [ImportJobController]
	});

	function ImportJobController() {

	}
})(angular);