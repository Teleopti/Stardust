(function (angular) {
	'use strict';
	angular.module('wfm.gamification')
		.service('GamificationDataService', [
			'$q',
			'$http',
			'$log',
			'$timeout',
			GamificationDataService
		]);

	function GamificationDataService($q, $http, $log, $timeout) {
		var svc = this;
		svc.fetchSites = function () {
			return $q(function (resolve, reject) {
				$http({
					method: 'POST',
					url: '../api/Gamification/LoadSites'
				}).then(function (response) {
					var sites = [];
					if (!response.data.length) resolve(sites);
					response.data.forEach(function (site_) { sites.push(new Site(site_.id, site_.text)); });
					resolve(sites);
				}, function (response) { $log.error('Gamification: failed to fetch sites'); });
			});
		};

		svc.fetchTeams = function (siteIds) {
			return $q(function (resolve, reject) {
				$http({
					method: 'POST',
					url: '../api/Gamification/LoadTeamGamification',
					data: siteIds
				}).then(function (response) {
					var teams = [];
					if (!response.data.length) resolve(teams);
					response.data.forEach(function (data) { teams.push(new Team(data.Team.id, data.Team.text, data.GamificationSettingId)); });
					resolve(teams);
				}, function (response) { $log.error('Gamification: failed to fetch teams and their settings'); });
			});
		};

		svc.fetchSettingList = function () {
			return $q(function (resolve, reject) {
				$http({
					method: 'GET',
					url: '../api/Gamification/LoadGamificationList'
				}).then(function (response) {
					var list = [];
					if (!response.data.length) resolve(list);
					response.data.forEach(function (item) { list.push(new Setting(item.GamificationSettingId, item.Value.Name)); });
					resolve(list);
				}, function (response) { $log.error('Gamification: failted to fetch setting list'); });
			});
		};

		svc.updateAppliedSetting = function (teamIds, settingId) {
			return $q(function (resolve, reject) {
				$http({
					method: 'POST',
					url: '../api/Gamification/SetTeamGamification',
					data: {
						TeamIds: teamIds,
						GamificationSettingId: settingId
					}
				}).then(function (response) {
					resolve(response.data);
				}, function (response) {
					$log.error('Gamification: failed to update applied setting');
					reject(response);
				});
			});
		};

		var counter = 5;
		svc.fetchJobs = function () {
			var url = "../api/gamification/import-jobs";
			var config = {
				url: url,
				method: 'GET',
			};

			return $q(function (resolve, reject) {
				$http(config).then(function (response) {
					var jobs = [];
					if (response.data) {
						for (var index = 0; index < response.data.length; index++) {
							var job = response.data[index];
							jobs.push(new Job(job.Id, job.Name, job.Owner, job.CreateDateTime, job.Status, job.Category));
						}
					}
					resolve(jobs);
				}, function (response) {
					reject(response);
				});
				// var jobs = [];
				// for (var i = 0; i < counter; i++) {
				// 	jobs.push(new Job(i, i + '.csv', 'Teleopti Demo', '2017-11-26T07:07:27.007Z', false));
				// }
				// counter++;

				// resolve(jobs);
			});
		};

		svc.uploadCsv = function (file) {
			var url = "../api/Gamification/NewImportExternalPerformanceInfoJob"
			var config = {
				url: url,
				method: 'POST',
				file: file,
				headers: {
					'Accept': 'application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
				}
			};

			config = overload(config);


			return $q(function (resolve, reject) {
				return $http(config).then(function (response) {
					resolve(response);
				}, function (response) {
					reject(response);
				});
			});
		};
	}

	function overload(config) {
		config.headers = config.headers || {};
		config.headers['Content-Type'] = undefined;
		config.transformRequest = config.transformRequest ?
			(angular.isArray(config.transformRequest) ?
				config.transformRequest : [config.transformRequest]) : [];
		config.transformRequest.push(function (data) {
			var formData = new FormData();

			if (config.file != null) {
				var fileFormName = config.fileFormDataName || 'file';

				if (angular.isArray(config.file)) {
					var isFileFormNameString = angular.isString(fileFormName);
					for (var i = 0; i < config.file.length; i++) {
						formData.append(isFileFormNameString ? fileFormName : fileFormName[i], config.file[i],
							(config.fileName && config.fileName[i]) || config.file[i].name);
					}
				} else {
					formData.append(fileFormName, config.file, config.fileName || config.file.name);
				}
			}
			return formData;
		});

		return config;
	}


	function Site(id, name) {
		this.id = id;
		this.name = name;
	}

	function Team(id, name, appliedSettingId) {
		this.id = id;
		this.name = name;
		this.appliedSettingId = appliedSettingId;
	}

	function Setting(id, name) {
		this.id = id;
		this.name = name;
	}

	function Job(id, name, owner, startingTime, status, category) {
		this.id = id;
		this.name = name;
		this.owner = owner;
		this.startingTime = startingTime;
		this.status = status;
		this.category = category;
	}
})(angular);
