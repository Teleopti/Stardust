(function () {
	'use strict';
	angular.module('wfm.gamification').service('gamificationSettingService', GamificationSettingService);
	GamificationSettingService.$inject = ['$http', '$q'];

	function GamificationSettingService($http, $q) {
		var getSettingsUrlApi = { url: '../api/Gamification/LoadGamificationList', method: 'POST' };

		this.getSettingsDescriptor = function () {

			return $q(function (resolve, reject) {
				$http.get(getSettingsUrlApi.url).then(function (response) {
					resolve(response.data);
				});
			});
		}

		var settingApi = { url: '../api/Gamification/Load/', method: 'get' };
		this.getSettingById = function (id) {
			return $q(function (resolve, reject) {
				$http.get(settingApi.url + id).then(function (response) {
					resolve(response.data);
				});
			});
		}
	}
})();