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
	}, {
		id: 'ExternalBadgeSettingDescription',
		url: '../api/Gamification/Update/ExternalBadgeSettingDescription',
		method: 'post'
	}, {
		id: 'ExternalBadgeSettingThreshold',
		url: '../api/Gamification/Update/ExternalBadgeSettingThreshold',
		method: 'post'
	}, {
		id: 'ExternalBadgeSettingGoldThreshold',
		url: '../api/Gamification/Update/ExternalBadgeSettingGoldThreshold',
		method: 'post'
	}, {
		id: 'ExternalBadgeSettingSilverThreshold',
		url: '../api/Gamification/Update/ExternalBadgeSettingSilverThreshold',
		method: 'post'
	}, {
		id: 'ExternalBadgeSettingBronzeThreshold',
		url: '../api/Gamification/Update/ExternalBadgeSettingBronzeThreshold',
		method: 'post'
	}, {
		id: 'ExternalBadgeSettingEnabled',
		url: '../api/Gamification/Update/ExternalBadgeSettingEnabled',
		method: 'post'
	}];

	function GamificationSettingService($http, $q) {
		var getSettingsUrl = '../api/Gamification/LoadGamificationList';

		this.getSettingsDescriptor = function () {

			return $q(function (resolve, reject) {
				$http.get(getSettingsUrl).then(function (response) {
					resolve(response.data);
				});
			});
		}

		var loadSettingUrl = '../api/Gamification/Load/';
		this.getSettingById = function (id) {
			return $q(function (resolve, reject) {
				$http.get(loadSettingUrl + id).then(function (response) {
					resolve(response.data);
				});
			});
		}

		var createNewSettingUrl = '../api/Gamification/Create';
		this.createNewSetting = function () {
			return $q(function (resolve, reject) {
				$http.post(createNewSettingUrl, {}, {}).then(function (response) {
					resolve(response.data);
				});
			});
		}

		var deleteSettingUlr = '../api/Gamification/Delete/';
		this.deleteSetting = function (id) {
			return $http.delete(deleteSettingUlr + id);
		}

		this.saveData = function (apikey, data) {
			var api = gamificationUrlMapping.find(function (element) {
				return element.id == apikey;
			})
			return $http.post(api.url, data, {});
		}

		var resetbadgeUrl = '../api/Gamification/Reset';
		this.resetBadge = function () {
			return $http.post(resetbadgeUrl, {}, {});
		}
	}
})();