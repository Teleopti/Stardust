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
			fetchJobs();
		};

		ctrl.fileIsInvalid = function () {
			return !(!!ctrl.file) || ctrl.file.size > ctrl.fileSizeLimit || !ctrl.isCsvFile(ctrl.file);
		};

		ctrl.isCsvFile = function (file) {
			if (!file) {
				return false;
			}

			var name = file.name;
			if (!name) {
				return false;
			}

			var index = name.toLowerCase().lastIndexOf('.csv');
			if (index > -1) {
				return true;
			}

			return false;
		};

		ctrl.upload = function () {
			startSpinner();
			setUploadingError(false);
			dataService.uploadCsv().then(function () {
				stopSpinner();
				clearFile();
				fetchJobs();
			}, function(){
				setUploadingError(true);
				stopSpinner();
			});
		};

		function setUploadingError(isError){
			ctrl.file.uploadingError = isError;
		}

		function startSpinner() {
			ctrl.uploading = true;
		}

		function stopSpinner() {
			ctrl.uploading = false;
		}

		function clearFile() {
			ctrl.file = null;
		}

		function fetchJobs() {
			dataService.fetchJobs().then(function (data) {
				var lastFetch = ctrl.jobs;
				ctrl.jobs = data;
				if (lastFetch && lastFetch.length) highlightNewJobs(lastFetch, ctrl.jobs);
			});
		}

		function highlightNewJobs(prev, curr) {
			var n = curr.length - prev.length;
			for (var i = 0; i < n; i++) {
				curr[i].isNew = true;
			}
		}
	}

})(angular);