(function (angular) { 'use strict';
	angular.module('wfm.gamification').component('importJob',{
		templateUrl: 'app/gamification/html/g.component.importJob.tpl.html',
		bindings: {
			name: '<'
		},
		controller: [ImportJobController]
	});

	function ImportJobController() {

	}
})(angular);