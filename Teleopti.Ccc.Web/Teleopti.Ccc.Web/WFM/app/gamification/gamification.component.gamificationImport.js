(function (angular) {
	'use strict';
	angular.module('wfm.gamification')
		.component('gamificationImport', {
			templateUrl: 'app/gamification/html/g.component.gamificationImport.tpl.html',
			controller: ['GamificationDataService', GamificationImportController]
		});

	function GamificationImportController(dataService) {
		var ctrl = this;

		ctrl.fileSizeLimit = 5242880;

		ctrl.$onInit = function () {
			dataService.fetchJobs().then(function (data) {
				ctrl.jobs = data;
			});
		};

		ctrl.fileIsInvalid = function () {
			return !(!!ctrl.file) || ctrl.file.size > ctrl.fileSizeLimit;
		};
	}

})(angular);