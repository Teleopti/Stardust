(function (angular) {
	'use strict';
	angular.module('wfm.gamification')
		.service('GamificationDataService', [
			'$q',
			'$http',
			'$log',
			GamificationDataService
		]);

	function GamificationDataService($q, $http, $log) {
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

		svc.fetchJobs = function () {
			return $q(function (resolve, reject) {
				resolve([
					new Job(4, '4.csv', 'Teleopti Demo', '2017-11-27T07:07:27.007Z', true),
					new Job(3, '3.csv', 'Teleopti Demo', '2017-11-26T07:07:27.007Z', false),
					new Job(2, '2.csv', 'Teleopti Demo', '2017-11-25T07:07:27.007Z', false),
					new Job(1, '1.csv', 'Teleopti Demo', '2017-11-24T07:07:27.007Z', false),
					new Job(0, '0.csv', 'Teleopti Demo', '2017-11-23T07:07:27.007Z', false)
				]);
			});
		}
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

	function Job(id, name, owner, startingTime, status) {
		this.id = id;
		this.name = name;
		this.owner = owner;
		this.startingTime = startingTime;
		this.status = status;
	}
})(angular);
