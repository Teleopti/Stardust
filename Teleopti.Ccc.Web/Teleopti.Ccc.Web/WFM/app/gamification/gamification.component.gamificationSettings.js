(function (angular) {

	'use strict';

	angular.module('wfm.gamification')
		.component('gamificationSettings', {
			templateUrl: 'app/gamification/html/g.component.gamificationSettings.tpl.html',
			controller: GamificationSettingsController
		});

	GamificationSettingsController.$inject = ['$mdSelect', '$element', '$scope', '$translate', '$q', 'gamificationSettingService'];

	function GamificationSettingsController($mdSelect, $element, $scope, $translate, $q, gamificationSettingService) {
		var ctrl = this;

		ctrl.getGamificationSettingsDescriptors = function () {
			gamificationSettingService.getSettingsDescriptor().then(function (data) {
				ctrl.settingDescriptors = data;
				console.log(data);
				if (ctrl.settingDescriptors && ctrl.settingDescriptors.length > 0) {
					ctrl.getSettingById(ctrl.settingDescriptors[0].GamificationSettingId);
				}
			});
		};

		ctrl.getSettingById = function (id) {
			gamificationSettingService.getSettingById(id).then(function (data) {
				var settingData = ctrl.convertSettingToModel(data);
				console.log(settingData);
				if (!ctrl.findElementInArray(ctrl.allSettings, settingData.id)) {
					ctrl.allSettings.push(settingData);
				}

				ctrl.currentSetting = settingData;
				ctrl.currentSettingId = settingData.id;
				ctrl.resetRuleSelection();
			});
		}


		ctrl.saveValue = function (apiKey, value) {
			var data = { GamificationSettingId: ctrl.currentSettingId, Value: value };
			gamificationSettingService.saveData(apiKey, data).then(function (response) {
				//show message that says saved successfully
				console.log('Save data successfully', apiKey);
			}, function (error) {
				// show message that says save failed
				console.log('Fail to save data', error);
			});
		}

		ctrl.saveData = function (apiKey, data) {
			gamificationSettingService.saveData(apiKey, data).then(function (response) {
				console.log('Save data successfully', apiKey);
			}, function (error) {
				// show message that says save failed
				console.log('Fail to save data', error);
			});
		}

		ctrl.convertSettingToModel = function (setting) {
			var result = {};
			result.id = setting.Id;
			result.name = setting.Name;
			result.updated_by = setting.UpdatedBy;
			result.updated_on = setting.UpdatedOn;
			result.rules = [{
				id: 0,
				name: 'Use Different Throsholds'
			}, {
				id: 1,
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
			result.rule_id = setting.GamificationSettingRuleSet;
			result.settings = [{
				id: 'UseBadgeForAnsweredCalls',
				name: 'Use Badge For Answered Calls',
				enable: setting.AnsweredCallsBadgeEnabled,
				is_buildin: true,
				rule_settings: [{
					rule_id: 0,
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
					rule_id: 1,
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
					rule_id: 0,
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
					rule_id: 1,
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
					rule_id: 0,
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
					rule_id: 1,
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
						id: item.QualityId,
						name: item.Name,
						enable: item.Enabled,
						is_buildin: false,
						larger_is_better: item.LargerIsBetter,
						rule_settings: [{
							rule_id: 0,
							items: [{
								id: 'ExternalBadgeSettingGoldThreshold',
								value: item.GoldThreshold,
								type: 'number',
								unit_type: item.UnitType
							}, {
								id: 'ExternalBadgeSettingSilverThreshold',
								value: item.SilverThreshold,
								type: 'number',
								unit_type: item.UnitType
							}, {
								id: 'ExternalBadgeSettingBronzeThreshold',
								value: item.BronzeThreshold,
								type: 'number',
								unit_type: item.UnitType
							}]
						}, {
							rule_id: 1,
							items: [{
								id: 'ExternalBadgeSettingThreshold',
								value: item.Threshold,
								type: 'number',
								unit_type: item.UnitType
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


			ctrl.currentSetting = ctrl.allSettings[ctrl.selectedSettingIndex];
			ctrl
		}

		ctrl.settingSelectionChanged = function () {
			if (ctrl.currentSettingId) {
				var setting = ctrl.findElementInArray(ctrl.allSettings, ctrl.currentSettingId);

				if (!setting) {
					ctrl.getSettingById(ctrl.currentSettingId);
				} else {
					ctrl.currentSetting = setting;
					ctrl.resetRuleSelection();
				}
			}

		}

		ctrl.resetRuleSelection = function () {
			ctrl.currentRule = ctrl.currentSetting.rules.find(function (element) {
				return element.id == ctrl.currentSetting.rule_id;
			});
		}

		ctrl.ruleSelectionChanged = function () {
			if (ctrl.currentSetting) {
				ctrl.currentRule = ctrl.findElementInArray(ctrl.currentSetting.rules, ctrl.currentSetting.rule_id);
				var data = {
					GamificationSettingId: ctrl.currentSetting.id,
					Rule: ctrl.currentSetting.rule_id
				}

				ctrl.saveData('ModifyChangeRule', data);
			}
		}

		ctrl.findElementInArray = function (target, id) {
			if (target && target.length > 0) {
				for (var index = 0; index < target.length; index++) {
					var item = target[index];
					if (item.id == id) {
						return item;
					}
				}
			}
		}

		ctrl.changeSettingDescription = function () {
			var data = {
				GamificationSettingId: ctrl.currentSetting.id,
				Name: ctrl.currentSetting.name
			};


			ctrl.updateName();
			ctrl.saveData('ModifyDescription', data);
		}

		ctrl.updateName = function () {
			var settingDescriptor = ctrl.settingDescriptors.find(function (item) {
				return item.GamificationSettingId == ctrl.currentSetting.id;
			})

			settingDescriptor.Value.Name = ctrl.currentSetting.name;
		}

		ctrl.changeRuleValue = function (item) {
			var data = {
				GamificationSettingId: ctrl.currentSetting.id,
				Rate: item.value
			};

			ctrl.saveData(item.id, data);
		}

		ctrl.addNewSetting = function () {
			gamificationSettingService.createNewSetting().then(function (data) {
				if (data) {
					var newSetting = ctrl.convertSettingToModel(data);
					ctrl.allSettings.push(newSetting);
					ctrl.currentSetting = newSetting;

					var newSettingDescriptor = {
						GamificationSettingId: ctrl.currentSetting.id,
						Value: {
							Name: ctrl.currentSetting.name,
							ShortName: ''
						}
					}

					ctrl.settingDescriptors.push(newSettingDescriptor);
					ctrl.currentSettingId = newSetting.id;
				}
			});
		}

		ctrl.deleteSetting = function () {
			var deletedId = ctrl.currentSettingId;
			gamificationSettingService.deleteSetting(deletedId).then(function (response) {
				var settingDescriptor = ctrl.settingDescriptors.find(function (item) {
					return item.GamificationSettingId == deletedId;
				});

				if (settingDescriptor) {
					var index = ctrl.settingDescriptors.indexOf(settingDescriptor);
					if (index > -1) {
						ctrl.settingDescriptors.splice(index, 1);
					}
				}

				var setting = ctrl.allSettings.find(function (item) {
					return item.id == deletedId;
				});

				if (setting) {
					var index = ctrl.allSettings.indexOf(setting);
					if (index > -1) {
						ctrl.allSettings.splice(index, 1);
					}
				}

				if (ctrl.settingDescriptors.length > 0) {
					ctrl.currentSettingId = ctrl.settingDescriptors[0].GamificationSettingId;
					var curSetting = ctrl.allSettings.find(function (item) {
						return item.id == ctrl.currentSettingId;
					})

					if (curSetting) {
						ctrl.currentSetting = curSetting;
					}
				}

				console.log('delete data successfully.');

			}, function (error) {
				console.log(error);
			})
		}

		ctrl.resetBadges = function name() {
			gamificationSettingService.resetBadge().then(function (response) {
				console.log('reset badges successfully');
			})
		}

		ctrl.reachedMaxSelectedItems = function () {
			if (ctrl.getCurentSelectedCount() > 3) {
				return true;
			}

			return false;
		}

		ctrl.getCurentSelectedCount = function () {
			var result = 0;
			var items = ctrl.currentSetting.settings;
			for (var index = 0; index < items.length; index++) {
				var element = items[index];
				if (element.enable) {
					result++;
				}
			}

			return result;
		}

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