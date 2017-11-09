(function (angular) { 'use strict';
	angular.module('wfm.gamification')
		.service('GamificationDataService', ['$q', GamificationDataService]);

	function GamificationDataService($q) {
		var svc = this;
		svc.fetchSites = function () {
			return $q(function (resolve, reject) {
				var n = 10;
				var sites = [];
				for (var i = 0; i < n; i++) {
					sites.push({ position: i, id: 'site'+(i+1), name: 'Site '+(i+1) });
				}
				resolve(sites);
			});
		};

		svc.fetchTeams = function (siteIds) {
			return $q(function (resolve, reject) {
				var teams = [];
				siteIds.forEach(function (siteId) {
					var n = parseInt(siteId[siteId.length - 1]);
					for (var i = 0; i < n + 1; i++) {
						teams.push({
							teamId: siteId + 'team' + (i + 1),
							teamName: 'Site ' + n + '/Team ' + (i + 1)
						});
					}
				});
				resolve(teams);
			});
		};

		svc.fetchSettingList = function () {
			return $q(function (resolve, reject) {
				var list = [
					{ id: 'default', name: 'Default' },
					{ id: 'setting1', name: 'Setting 1' },
					{ id: 'setting2', name: 'Setting 2' },
					{ id: 'setting3', name: 'Setting 3' }
				];
				resolve(list);
			});
		};
	}
})(angular);
