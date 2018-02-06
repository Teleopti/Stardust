(function (angular, moment) {
	'use strict';
	angular.module('wfm.gamification')
		.component('gamificationImport', {
			templateUrl: 'app/gamification/html/g.component.gamificationImport.tpl.html',
			controller: [
				'GamificationDataService',
				'$locale',
				'$state',
				'CurrentUserInfo',
				GamificationImportController
			]
		});

	function GamificationImportController(dataService, locale, state, CurrentUserInfo) {
		var currentTimezone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
		var ctrl = this;

		ctrl.fileSizeLimit = 2097152;
		ctrl.dateTimeFormat = locale.DATETIME_FORMATS.medium;
		ctrl.fileFormatWikiLink = 'https://wiki.teleopti.com/TeleoptiWFM/Gamification.import';

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
			dataService.uploadCsv(ctrl.file).then(function () {
				stopSpinner();
				clearFile();
				fetchJobs();
			}, function () {
				setUploadingError(true);
				stopSpinner();
			});
		};

		function setUploadingError(isError) {
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
			dataService.fetchJobs().then(function (jobs) {
				if (!ctrl.jobs) {
					ctrl.jobs = jobs;
					ctrl.jobs.forEach(convertStartingTimeToCurrentTimezone);
				} else {
					insertNewJobsFrom(jobs);
				}

				function convertStartingTimeToCurrentTimezone(job) {
					job.startingTime = utcToTimezone(job.startingTime, currentTimezone);

					function utcToTimezone(dateStr, timezone) {
						var f = 'YYYY-MM-DDTHH:mm:ss';
						var adjusted = moment.tz(dateStr, 'UTC').tz(timezone).format(f);
						return adjusted;
					}
				}
			});
		}

		function insertNewJobsFrom(jobs) {
			var n = jobs.length - ctrl.jobs.length;
			for (var i = 0; i < n; i++) {
				ctrl.jobs.unshift(jobs[i]);
			}
		}
	}

})(angular, moment);