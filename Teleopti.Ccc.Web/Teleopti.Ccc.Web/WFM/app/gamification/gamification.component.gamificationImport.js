(function (angular) { 'use strict';
	angular.module('wfm.gamification')
		.component('gamificationImport', {
			templateUrl: 'app/gamification/html/g.component.gamificationImport.tpl.html',
			controller: ['GamificationDataService',GamificationImportController]
		});

	function GamificationImportController(dataService) {
		var ctrl = this;

		ctrl.$onInit = function () {
			dataService.fetchJobs().then(function (data) {
				ctrl.jobs = data;
			});
		};
	}

})(angular);
