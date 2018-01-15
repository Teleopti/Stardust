; (function (angular) {
	'use strict';
	angular.module('wfm.gamification')
		.component('gSettingForm', {
			templateUrl: 'app/gamification/html/gSettingForm.tpl.html',
			bindings: {
				setting: '<'
			},
			controller: ['gamificationSettingService', '$log', '$scope', gSettingFormCtrl]
		});

	function gSettingFormCtrl(dataService, $log, $scope) {
		var ctrl = this;

		var addBuiltinMeasures = function (measureConfigs) {
			measureConfigs.push(new MeasureConfig(
				true
			));
			measureConfigs.push(new MeasureConfig(
				true
			));
			measureConfigs.push(new MeasureConfig(
				true
			));
		};

		var makeBuiltinMeasureConfigs = function (data) {
			var measureConfigs = [];

			measureConfigs.push(new MeasureConfig(
				true,
				data.AnsweredCallsBadgeEnabled,
				null,
				null,
				'AnsweredCalls',
				null,
				true,
				data.AnsweredCallsThreshold,
				data.AnsweredCallsBronzeThreshold,
				data.AnsweredCallsSilverThreshold,
				data.AnsweredCallsGoldThreshold
			));

			measureConfigs.push(new MeasureConfig(
				true,
				data.AdherenceBadgeEnabled,
				null,
				null,
				'Adherence',
				null,
				true,
				data.AdherenceThreshold,
				data.AdherenceBronzeThreshold,
				data.AdherenceSilverThreshold,
				data.AdherenceGoldThreshold
			));

			measureConfigs.push(new MeasureConfig(
				true,
				data.AHTBadgeEnabled,
				null,
				null,
				'AHT',
				null,
				true,
				data.AHTThreshold,
				data.AHTBronzeThreshold,
				data.AHTSilverThreshold,
				data.AHTGoldThreshold
			));

			return measureConfigs;
		};

		var makeExternalMeasureConfigs = function (data) {
			var measureConfigs = [];

			data.ExternalBadgeSettings.forEach(function (x) {
				measureConfigs.push(new MeasureConfig(
					false,
					x.Enabled,
					x.Id,
					x.QualityId,
					x.Name,
					x.DataType,
					x.LargerIsBetter,
					x.Threshold,
					x.BronzeThreshold,
					x.SilverThreshold,
					x.GoldThreshold
				));
			});

			return measureConfigs;
		};

		var updateSettingForm = function (data) {
			ctrl.id = data.Id;
			ctrl.name = data.Name;
			ctrl.changeInfo = data.UpdatedBy + ' ' + data.UpdatedOn;
			ctrl.rule = data.GamificationSettingRuleSet;
			ctrl.silverRate = data.SilverToBronzeBadgeRate;
			ctrl.goldRate = data.GoldToSilverBadgeRate;
			ctrl.builtinMeasureConfigs = makeBuiltinMeasureConfigs(data);
			ctrl.externalMeasureConfigs = makeExternalMeasureConfigs(data);
		};

		ctrl.$onChanges = function (changesObj) {
			// $log.log(changesObj);
			if (changesObj.setting && changesObj.setting.currentValue) {
				updateSettingForm(changesObj.setting.currentValue);
			}
		};

		ctrl.updateMeasureConfig = function () {
			dataService.updateMeasureConfig().then();
		};

		function MeasureConfig(builtin, enabled, id, externalId, name, dataType, largerIsBetter, threshold, bronzeThreshold, silverThreshold, goldThreshold) {
			this.builtin = builtin;
			this.enabled = enabled;
			this.id = id;
			this.externalId = externalId;
			this.name = name;
			this.dataType = dataType;
			this.largerIsBetter = largerIsBetter;
			this.badgeThreshold = threshold;
			this.bronzeBadgeThreshold = bronzeThreshold;
			this.silverBadgeThreshold = silverThreshold;
			this.goldBadgeThreshold = goldThreshold;
		}

		MeasureConfig.prototype.setName = function (name) {
			if (this.builtin)
				return;

			$log.log(this.name + ': set name to ' + name);

			var previous = this.name;
			this.name = name;

			var self = this;

			dataService.saveData('ExternalBadgeSettingDescription', {
				GamificationSettingId: ctrl.id,
				QualityId: this.externalId,
				Name: name
			}).then(function () {
				$log.log('updated name');
			}, function () {
				$log.error('failed to save name. restoring: ' + previous);
				self.name = previous;
			});
		};

		MeasureConfig.prototype.setEnabled = function (enabled) {
			$log.log(this.name + ': set enabled to ' + enabled);

			var previous = this.enabled;
			this.enabled = enabled;

			var self = this;

			dataService.saveData('ExternalBadgeSettingEnabled', {
				GamificationSettingId: ctrl.id,
				QualityId: this.externalId,
				Value: enabled
			    }).then(function () {
				$log.log('updated enabled');
			}, function () {
				$log.log('failed to update enabled. restoring: ' + previous);
				self.enabled = previous;
			});
		};
	}

})(angular);
