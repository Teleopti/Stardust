(function (angular) {

	'use strict';

	angular.module('wfm.gamification')
		.component('gamificationSettings', {
			templateUrl: 'app/gamification/html/g.component.gamificationSettings.tpl.html',
			controller: GamificationSettingsController
		});

	GamificationSettingsController.$inject = ['$mdSelect', '$element', '$scope', '$translate', 'gamificationSettingService'];

	function GamificationSettingsController($mdSelect, $element, $scope, $translate, gamificationSettingService) {
		var ctrl = this;

		ctrl.getGamificationSettingsDescriptors = function () {
			gamificationSettingService.getSettingsDescriptor().then(function (data) {
				ctrl.settingDescriptors = data;
				if (ctrl.settingDescriptors != null && ctrl.settingDescriptors.length > 0) {
					var aSetting = ctrl.getSettingById(ctrl.settingDescriptors[0].GamificationSettingId);
				}
			});
		};

		ctrl.getSettingById = function (id) {
			console.log('getting data for setting:' + id);
			gamificationSettingService.getSettingById(id).then(function (data) {
				console.log('raw setting data for' + id + ' is:');
				console.log(data);
				var settingData = ctrl.convertSettingToModel(data);
				console.log('data after convert is:');
				console.log(settingData);

				ctrl.allSettings.push(settingData);
				ctrl.settingSelectionChanged();
				console.log(ctrl.allSettings);
			});
		}


		ctrl.convertSettingToModel = function (setting) {
			var result = {};
			result.id = setting.Id;
			result.name = setting.Name;
			result.updated_by = setting.UpdatedBy;
			result.updated_on = setting.UpdatedOn;
			result.rules = [{
				id: '0',
				name: 'Use Different Throsholds'
			}, {
				id: '1',
				name: 'Use Ratio Conversion',
				items: [{
					id: 'SilverToBronzeBadgeRate',
					name: 'A silver badge equals bronze badge count',
					value: setting.SilverToBronzeBadgeRate
				}, {
					id: 'GoldToSilverBadgeRate',
					name: 'A gold badge equals sikver badge count',
					value: setting.GoldToSilverBadgeRate
				}]
			}];
			result.settings = [{
				id: 'UseBadgeForAnsweredCalls',
				name: 'Use Badge For Answered Calls',
				enable: setting.AnsweredCallsBadgeEnabled,
				is_buildin: true,
				rule_settings: [{
					rule_id: '0',
					items: [{
						id: 'AnsweredCallsGoldThreshold',
						value: setting.AnsweredCallsGoldThreshold,
						type: 'number'
					}, {
						id: 'AnsweredCallsSilverThreshold',
						value: setting.AnsweredCallsSilverThreshold,
						type: 'number'
					}, {
						id: 'AnsweredCallsBronzeThreshold',
						value: setting.AnsweredCallsBronzeThreshold,
						type: 'number'
					}]
				}, {
					rule_id: '1',
					items: [{
						id: 'AnsweredCallsThreshold',
						value: setting.AnsweredCallsThreshold,
						type: 'number'
					}]
				}]
			}, {
				id: 'UseBadgeForAdherence',
				name: 'Use Badge For Adherence',
				enable: setting.AdherenceBadgeEnabled,
				is_buildin: true,
				rule_settings: [{
					rule_id: '0',
					items: [{
						id: 'AdherenceGoldThreshold',
						value: setting.AdherenceGoldThreshold.Value,
						type: 'percent'
					}, {
						id: 'AdherenceSilverThreshold',
						value: setting.AdherenceSilverThreshold.Value,
						type: 'percent'
					}, {
						id: 'AdherenceBronzeThreshold',
						value: setting.AdherenceBronzeThreshold.Value,
						type: 'percent'
					}]
				}, {
					rule_id: '1',
					items: [{
						id: 'AdherenceThreshold',
						value: setting.AdherenceThreshold.Value,
						type: 'percent'
					}]
				}]
			}, {
				id: 'UseBadgeForAHT',
				name: 'Use Badge For AHT',
				enable: setting.AHTBadgeEnabled,
				is_buildin: true,
				rule_settings: [{
					rule_id: '0',
					items: [{
						id: 'AHTGoldThreshold',
						value: setting.AHTGoldThreshold,
						type: 'time'
					}, {
						id: 'AHTSilverThreshold',
						value: setting.AHTSilverThreshold,
						type: 'time'
					}, {
						id: 'AHTBronzeThreshold',
						value: setting.AHTBronzeThreshold,
						type: 'time'
					}]
				}, {
					rule_id: '1',
					items: [{
						id: 'AHTThreshold',
						value: setting.AHTThreshold,
						type: 'time'
					}]
				}]
			}];

			if (setting.ExternalBadgeSettings && setting.ExternalBadgeSettings.length > 0) {

				for (var index = 0; index < setting.ExternalBadgeSettings.length; index++) {
					var item = setting.ExternalBadgeSettings[index];
					var aExternalSetting = {
						id: item.Id,
						name: item.Name,
						enable: item.Enabled,
						is_buildin: false,
						rule_settings: [{
							rule_id: '0',
							items: [{
								id: 'GoldThreshold',
								value: item.GoldThreshold,
								type: 'number'
							}, {
								id: 'SilverThreshold',
								value: item.SilverThreshold,
								type: 'number'
							}, {
								id: 'BronzeThreshold',
								value: item.BronzeThreshold,
								type: 'number'
							}]
						}, {
							rule_id: '1',
							items: [{
								id: 'Threshold',
								value: item.Threshold,
								type: 'number'
							}]
						}]

					};

					result.settings.push(aExternalSetting);
				}
			}

			return result;
		}

		ctrl.$onInit = function () {
			ctrl.allSettings = [];
			ctrl.getGamificationSettingsDescriptors();
			ctrl.title = 'Gamification Settings';

			ctrl.currentRuleIndex = 0;
			//ctrl.currentRule = ctrl.rules[ctrl.currentRuleIndex];

			ctrl.selectedSettingIndex = 0;
			ctrl.currentSetting = ctrl.allSettings[ctrl.selectedSettingIndex];
		}

		ctrl.settingSelectionChanged = function () {
			ctrl.currentSetting = ctrl.allSettings[ctrl.selectedSettingIndex]
			ctrl.currentRuleIndex = 0;
			ctrl.ruleSelectionChanged();
		}

		ctrl.ruleSelectionChanged = function () {
			if (ctrl.currentSetting) {
				ctrl.currentRule = ctrl.currentSetting.rules[ctrl.currentRuleIndex];
			}

		}

		ctrl.addSetting = function () {

		}

		ctrl.deleteSetting = function () {

		}

		ctrl.resetBadges = function name() {

		}

		ctrl.itemSelected = function () {
			if (ctrl.getCurentSelectedCount() > 3) {
				return true;
			}

			return false;
		}

		ctrl.getCurentSelectedCount = function () {
			var result = 0;
			var viewedItems = ctrl.viewedSetting;
			for (var index = 0; index < viewedItems.length; index++) {
				var element = viewedItems[index];
				if (element.is_checked) {
					result++;
				}
			}

			return result;
		}

		Object.defineProperty(ctrl, 'viewedSetting', {
			get: function () {
				return ctrl.getViewSetting();
			}
		})

		ctrl.getViewSetting = function () {
			var result;
			ctrl.currentSetting.rule_settings.forEach(function (ruleSetting) {
				if (ruleSetting.rule_id == ctrl.currentRule.id) {
					result = ruleSetting.settings;
				}
			}, this);

			return result;
		}
	}

})(angular);