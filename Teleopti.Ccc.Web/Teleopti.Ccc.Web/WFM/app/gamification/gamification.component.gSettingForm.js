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
				data.AnsweredCallsGoldThreshold,
				'^\\d+$',
				10000,
				'desc'
			));

			measureConfigs.push(new MeasureConfig(
				true,
				data.AdherenceBadgeEnabled,
				null,
				null,
				'Adherence',
				null,
				true,
				data.AdherenceThreshold.Value,
				data.AdherenceBronzeThreshold.Value,
				data.AdherenceSilverThreshold.Value,
				data.AdherenceGoldThreshold.Value,
				'^0.\\d+$',
				20.0,
				'desc'
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
				data.AHTGoldThreshold,
				'^(0[0-1]):([0-5][0-9]):([0-5][0-9])$',
				'01:00:00',
				'asc'
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
			ctrl.ruleSet = data.GamificationSettingRuleSet;
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

		ctrl.updateRuleSet = function () {
			dataService.saveData('ModifyChangeRule', {
				GamificationSettingId: ctrl.id,
				Rule: ctrl.ruleSet
			}).then(function () {
				$log.log('updated rule set successfully');
			}, function () {
				$log.log('failed to update rule set')
			});
		};

		function MeasureConfig(builtin, enabled, id, externalId, name, dataType, largerIsBetter, threshold, bronzeThreshold, silverThreshold, goldThreshold, valueFormat, max, valueOrder) {
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
			this.valueFormat = valueFormat;
			this.max = max;
			this.valueOrder = valueOrder;
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

		MeasureConfig.prototype.setBadgeThreshold = function (badgeThreshold) {
			if (!badgeThreshold || badgeThreshold === this.badgeThreshold) return;

			$log.log(this.name + ': set badge threshold to ' + badgeThreshold);

			var previous = this.badgeThreshold;
			this.badgeThreshold = badgeThreshold;

			var self = this;

			switch (this.name) {
				case 'AnsweredCalls':
					dataService.saveData('AnsweredCallsThreshold', {
						GamificationSettingId: ctrl.id,
						Value: this.badgeThreshold
					}).then(function () {
						$log.log(self.name + ': updated badge threshold')
					}, function () {
						$log.log(self.name + ': failed to update badge threshold. restoring: ' + previous);
						self.badgeThreshold = previous;
					});

					break;

				case 'Adherence':
					dataService.saveData('AdherenceThreshold', {
						GamificationSettingId: ctrl.id,
						Value: this.badgeThreshold
					}).then(function () {
						$log.info(self.name + ': updated badge threshold')
					}, function () {
						$log.error(self.name + ': failed to update badge threshold. restoring: ' + previous);
						self.badgeThreshold = previous;
					});
					break;

				default:
					break;
			}
		};

		MeasureConfig.prototype.setBronzeBadgeThreshold = function (bronzeBadgeThreshold) {
			$log.log(this.name + ': set bronze badge threshold to ' + bronzeBadgeThreshold);

			var previous = this.bronzeBadgeThreshold;
			this.bronzeBadgeThreshold = bronzeBadgeThreshold;

			var self = this;
			dataService.saveData('AnsweredCallsThreshold', {
				GamificationSettingId: ctrl.id,
				Value: this.bronzeBadgeThreshold
			}).then(function () {
				$log.log('updated bronze badge threshold')
			}, function () {
				$log.log('failed to update bronze badge threshold. restoring: ' + previous);
				self.bronzeBadgeThreshold = previous;
			});
		};

		MeasureConfig.prototype.setSilverBadgeThreshold = function (silverBadgeThreshold) {
			$log.log(this.name + ': set silver badge threshold to ' + silverBadgeThreshold);

			var previous = this.silverBadgeThreshold;
			this.silverBadgeThreshold = silverBadgeThreshold;

			var self = this;
			dataService.saveData('AnsweredCallsThreshold', {
				GamificationSettingId: ctrl.id,
				Value: this.silverBadgeThreshold
			}).then(function () {
				$log.log('updated silver badge threshold')
			}, function () {
				$log.log('failed to update silver badge threshold. restoring: ' + previous);
				self.silverBadgeThreshold = previous;
			});
		};

		MeasureConfig.prototype.setGoldBadgeThreshold = function (goldBadgeThreshold) {
			$log.log(this.name + ': set gold badge threshold to ' + goldBadgeThreshold);

			var previous = this.goldBadgeThreshold;
			this.goldBadgeThreshold = goldBadgeThreshold;

			var self = this;

			dataService.saveData('AnsweredCallsThreshold', {
				GamificationSettingId: ctrl.id,
				Value: this.goldBadgeThreshold
			}).then(function () {
				$log.log('updated gold badge threshold')
			}, function () {
				$log.log('failed to update gold badge threshold. restoring: ' + previous);
				self.goldBadgeThreshold = previous;
			});
		};
	}

})(angular);
