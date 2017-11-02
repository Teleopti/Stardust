(function (angular) {

	'use strict';

	angular.module('wfm.gamification')
		.component('gamificationSettings', {
			templateUrl: 'app/gamification/html/g.component.gamificationSettings.tpl.html',
			controller: GamificationSettingsController
		});

	GamificationSettingsController.$inject = ['$mdSelect', '$element', '$scope', '$translate',];

	function GamificationSettingsController($mdSelect, $element, $scope, $translate, ) {
		var ctrl = this;
		ctrl.title = 'Gamification Settings';
		ctrl.settings = [{
			id: '0',
			name: 'Default'
		}, {
			id: '1',
			name: 'Custom name1'
		},
		{
			id: '2',
			name: 'Custom name2'
		}
		];

		ctrl.rules = [{
			id: '0',
			name: 'Use Different Thresholds'
		}, {
			id: '1',
			name: 'Use Ratio Conversion'
		}]

		ctrl.settingInfos = [{
			id: 'fgdfgdfg-dfgdfgdfg-dfgdfgdf-dfgdfgdfgdf',
			name: 'Use badge for Answered Calls',
			rule_id: 'rule-id1',
			is_checked: true,
			is_external: false,
			items: [{
				id: 'thresholdforgold-item1',
				name: 'threshold for gold',
				value: '160',
				value_type: 'num'
			}, {
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
			}]
		},
		{
			id: 'fgdfgdfg-dfgdfgdfg-dfgdfgdf',
			name: 'Use badge for Adherence',
			rule_id: 'rule-id1',
			is_checked: false,
			is_external: true,
			items: [{
				id: 'thresholdforgold-item2',
				name: 'threshold for gold',
				value: '80',
				value_type: 'num'
			}, {
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
			}]
		}];

		ctrl.currentRuleId = ctrl.rules[0].id;

		ctrl.currentSettingId = ctrl.settings[0].id;

	}

})(angular);