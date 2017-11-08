(function (angular) {

	'use strict';

	angular.module('wfm.gamification')
		.component('gamificationSettings', {
			templateUrl: 'app/gamification/html/g.component.gamificationSettings.tpl.html',
			controller: GamificationSettingsController
		});

	GamificationSettingsController.$inject = ['$mdSelect', '$element', '$scope', '$translate'];

	function GamificationSettingsController($mdSelect, $element, $scope, $translate) {
		var ctrl = this;

		ctrl.$onInit = function () {
			ctrl.title = 'Gamification Settings';
				console.log(data);

			ctrl.rules = [{
				id: 'rule-id0',
				name: 'Use Different Thresholds'
			result.name = setting.Name;
			}, {
				id: 'rule-id1',
				name: 'Use Ratio Conversion',
				items: [{
					id: 'rule_item_id1',
					name: 'A silver badge eaquals bronze badage count',
					value: 5
				}, {
					id: 'rule_item_id2',
					name: 'A gold badge eaquals silver badage count',
					value: 5
				}]

			}];

			ctrl.allSettings = [
				{
					setting_id: 'setting_id_0',
					name: 'Default',
					info: 'Last updated by Admin at xxxx-xx-xx xx:xx:xx',
					rule_settings: [
						{
							rule_id: 'rule-id0',
							rule_name: 'Use Different Thresholds',
							settings: [
								{
									id: 'fgdfgdfg-dfgdfgdfg-dfgdfgdf-dfgdfgdfgdf',
									name: 'Use badge for Answered Calls',
									is_checked: true,
									is_external: false,
									items: [
										{
											id: 'thresholdforgold-item1',
											name: 'threshold for gold',
											value: '160',
											value_type: 'num'
										},
										{
											id: 'thresholdforsilver-item1',
											name: 'threshold for silver',
											value: '120',
											value_type: 'num'
										},
										{
											id: 'thresholdforbronze-item1',
											name: 'threshold for bronze',
											value: '100',
											value_type: 'num'
										}
									]
								},
								{
									id: 'fgdfgdfg-dfgdfgdfg-dfgdfgdf',
									name: 'Use badge for Adherence',
									is_checked: false,
									is_external: true,
									items: [
										{
											id: 'thresholdforgold-item2',
											name: 'threshold for gold',
											value: '80',
											value_type: 'num'
										},
										{
											id: 'thresholdforsilver-item2',
											name: 'threshold for silver',
											value: '70',
											value_type: 'num'
										},
										{
											id: 'thresholdforbronze-item2',
											name: 'threshold for bronze',
											value: '60',
											value_type: 'num'
										}
									]
								},
								{
									id: 'fgdfgdfg-dfgdffdgdfggdfg-sdfsdfdsfsdff',
									name: 'Use badge for AHT',
									is_checked: false,
									is_external: true,
									items: [
										{
											id: 'thresholdforgold-item3',
											name: 'threshold for gold',
											value: '60',
											value_type: 'num'
										},
										{
											id: 'thresholdforsilver-item3',
											name: 'threshold for silver',
											value: '50',
											value_type: 'num'
										},
										{
											id: 'thresholdforbronze-item3',
											name: 'threshold for bronze',
											value: '30',
											value_type: 'num'
										}
									]
								}
							]
						},
						{
							rule_id: 'rule-id1',
							rule_name: 'Use Ratio Conversion',
							settings: [
								{
									id: 'fgdfgdfg-dfgdfgdfg-ddfgsdfgsdfgfgdfgdf-dfgdfgdfgdf',
									name: 'Use badge for Answered Calls',
									is_checked: true,
									is_external: false,
									items: [
										{
											id: 'thresholdforgold-item1',
											name: 'Use badge for Answered Calls',
											value: '80',
											value_type: 'num'
										}
									]
								},
								{
									id: 'fgdfgsdfgsdfgdfg-dfgdfgdfg-dfgdfgdf',
									name: 'Use badge for Adherence',
									is_checked: false,
									is_external: true,
									items: [
										{
											id: 'thresholdforgold-item2',
											name: 'Use badge for Adherence',
											value: '80',
											value_type: 'num'
										}
									]
								},
								{
									id: 'fgdfgdfg-dsdfgsdfgsdfgsdfgfgdfgdfg-sdfsdfdsfsdff',
									name: 'Use badge for AHT',
									is_checked: false,
									is_external: true,
									items: [
										{
											id: 'thresholdforgold-item3',
											name: 'Use badge for AHT',
											value: '60',
											value_type: 'num'
										}
									]
								}
							]
						}
					]
				}, {
					setting_id: 'setting_id_1',
					name: 'Setting 1',
					info: 'Last updated by Admin at 2017-11-03 xx:xx:xx',
					rule_settings: [
						{
							rule_id: 'rule-id0',
							rule_name: 'Use Different Thresholds',
							settings: [
								{
									id: 'fgdfgddfgdfgdffg-dfgdfgdfg-dfgdfgdf-dfgdfgdfgdf',
									name: 'Use badge for Answered Calls',
									is_checked: true,
									is_external: false,
									items: [
										{
											id: 'thresholdforgold-item1',
											name: 'threshold for gold',
											value: '300',
											value_type: 'num'
										},
										{
											id: 'thresholdforsilver-item1',
											name: 'threshold for silver',
											value: '200',
											value_type: 'num'
										},
										{
											id: 'thresholdforbronze-item1',
											name: 'threshold for bronze',
											value: '100',
											value_type: 'num'
										}
									]
								},
								{
									id: 'fgdfgdertwerttrefg-dfgdfgdfg-dfgdfgdf',
									name: 'Use badge for Adherence',
									is_checked: false,
									is_external: true,
									items: [
										{
											id: 'thresholdforgold-item2',
											name: 'threshold for gold',
											value: '80',
											value_type: 'num'
										},
										{
											id: 'thresholdforsilver-item2',
											name: 'threshold for silver',
											value: '70',
											value_type: 'num'
										},
										{
											id: 'thresholdforbronze-item2',
											name: 'threshold for bronze',
											value: '60',
											value_type: 'num'
										}
									]
								},
								{
									id: 'fgdfgdfg-drtyrtyrtyrtfgdfgdfg-sdfsdfdsfsdff',
									name: 'Use badge for AHT',
									is_checked: false,
									is_external: true,
									items: [
										{
											id: 'thresholdforgold-item3',
											name: 'threshold for gold',
											value: '60',
											value_type: 'num'
										},
										{
											id: 'thresholdforsilver-item3',
											name: 'threshold for silver',
											value: '50',
											value_type: 'num'
										},
										{
											id: 'thresholdforbronze-item3',
											name: 'threshold for bronze',
											value: '30',
											value_type: 'num'
										}
									]
								}
							]
						},
						{
							rule_id: 'rule-id1',
							rule_name: 'Use Ratio Conversion',
							settings: [
								{
									id: 'fgdfgdfg-d456456456fgdfgdfg-dfgdfgdf-dfgdfgdfgdf',
									name: 'Use badge for Answered Calls',
									is_checked: true,
									is_external: false,
									items: [
										{
											id: 'thresholdforgold-item1',
											name: 'Use badge for Answered Calls',
											value: '80',
											value_type: 'num'
										}
									]
								},
								{
									id: 'fgdfgdfg-d678678678fgdfgdfg-dfgdfgdf',
									name: 'Use badge for Adherence',
									is_checked: false,
									is_external: true,
									items: [
										{
											id: 'thresholdforgold-item2',
											name: 'Use badge for Adherence',
											value: '80',
											value_type: 'num'
										}
									]
								},
								{
									id: 'fgdfgdfg-dfgdfgd78978978978fg-sdfsdfdsfsdff',
									name: 'Use badge for AHT',
									is_checked: false,
									is_external: true,
									items: [
										{
											id: 'thresholdforgold-item3',
											name: 'Use badge for AHT',
											value: '60',
											value_type: 'num'
										}
									]
								}
							]
						}
					]
				}
			];

			ctrl.currentRuleIndex = 0;
			ctrl.currentRule = ctrl.rules[ctrl.currentRuleIndex];

			ctrl.selectedSettingIndex = 0;
			ctrl.currentSetting = ctrl.allSettings[ctrl.selectedSettingIndex];
		}

		ctrl.settingSelectionChanged = function () {
			ctrl.currentSetting = ctrl.allSettings[ctrl.selectedSettingIndex]
			ctrl.currentRuleIndex = 0;
			ctrl.ruleSelectionChanged();
		}

		ctrl.ruleSelectionChanged = function () {
			ctrl.currentRule = ctrl.rules[ctrl.currentRuleIndex];
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