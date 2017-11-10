(function () {
	'use strict';
	angular.module('wfm.gamification').service('gamificationSettingService', GamificationSettingService);
	GamificationSettingService.$inject = ['$http', '$q'];

	var gamificationUrlMapping = [{
		id: 'ModifyDescription',
		url: '../api/Gamification/ModifyDescription',
		method: 'post'
	}, {
		id: 'UseBadgeForAnsweredCalls',
		url: '../api/Gamification/ModifyAnsweredCallsEnabled',
		method: 'post'
	}, {
		id: 'AnsweredCallsGoldThreshold',
		url: '../api/Gamification/ModifyAnsweredCallsForGold',
		method: 'post'
	}, {
		id: 'AnsweredCallsSilverThreshold',
		url: '../api/Gamification/ModifyAnsweredCallsForSilver',
		method: 'post'
	}, {
		id: 'AnsweredCallsBronzeThreshold',
		url: '../api/Gamification/ModifyAnsweredCallsForBronze',
		method: 'post'
	}, {
		id: 'UseBadgeForAHT',
		url: '../api/Gamification/ModifyAHTEnabled',
		method: 'post'
	}, {
		id: 'AHTGoldThreshold',
		url: '../api/Gamification/ModifyAHTForGold',
		method: 'posst'
	}, {
		id: 'AHTSilverThreshold',
		url: '../api/Gamification/ModifyAHTForSilver',
		method: 'post'
	}, {
		id: 'AHTBronzeThreshold',
		url: '../api/Gamification/ModifyAHTForBronze',
		method: 'post'
	}, {
		id: 'UseBadgeForAdherence',
		url: '../api/Gamification/ModifyAdherenceEnabled',
		method: 'post'
	}, {
		id: 'AdherenceGoldThreshold',
		url: '../api/Gamification/ModifyAdherenceForGold',
		method: 'post'
	}, {
		id: 'AdherenceSilverThreshold',
		url: '../api/Gamification/ModifyAdherenceForSilver',
		method: 'post',
	}, {
		id: 'AdherenceBronzeThreshold',
		url: '../api/Gamification/ModifyAdherenceForBronze',
		method: 'post'
	}, {
		id: '',
		url: '../api/Gamification/Reset',
		method: 'post'
	}, {
		id: 'ModifyChangeRule',
		url: '../api/Gamification/ModifyChangeRule',
		method: 'post'
	}, {
		id: 'AnsweredCallsThreshold',
		url: '../api/Gamification/ModifyAnsweredCalls',
		method: 'post'
	}, {
		id: 'AHTThreshold',
		url: '../api/Gamification/ModifyAHTThreshold',
		method: 'post'
	}, {
		id: 'AdherenceThreshold',
		url: '../api/Gamification/ModifyAdherence',
		method: 'post'
	}, {
		id: 'GoldToSilverBadgeRate',
		url: '../api/Gamification/ModifyGoldToSilverRate',
		method: 'post'
	}, {
		id: 'SilverToBronzeBadgeRate',
		url: '../api/Gamification/ModifySilverToBronzeRate',
		method: 'post'
	}];


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

		this.saveData = function name(apikey, data) {
			var api = gamificationUrlMapping.find(function (element) {
				return element.id == apikey;
			})
			return $http.post(api.url, data, {});
		}
	}
})();