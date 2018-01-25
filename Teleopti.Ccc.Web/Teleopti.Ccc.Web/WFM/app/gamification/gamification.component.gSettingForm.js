; (function (angular) {
	'use strict';
	angular.module('wfm.gamification')
		.component('gSettingForm', {
			templateUrl: 'app/gamification/html/gSettingForm.tpl.html',
			bindings: {
				onNameUpdate: '&',
				setting: '<',
				measureNameUpdateCallback: '&'
			},
			controller: ['gamificationSettingService', '$log', '$scope', '$translate', gSettingFormCtrl]
		});

	function gSettingFormCtrl(dataService, $log, $scope, $translate) {
		var ctrl = this;
		var enabled = [];
		var numOfEnabledMeasures = 0;
		var originalName = '';

		var makeBuiltinMeasureConfigs = function (data) {
			var measureConfigs = [];

			var answeredCalls = new MeasureConfig(
				true,
				data.AnsweredCallsBadgeEnabled,
				null,
				null,
				'AnsweredCalls',
				0,
				true,
				data.AnsweredCallsThreshold,
				data.AnsweredCallsBronzeThreshold,
				data.AnsweredCallsSilverThreshold,
				data.AnsweredCallsGoldThreshold,
				'^\\d+$',
				10000,
				'desc'
			);
			measureConfigs.push(answeredCalls);
			if (answeredCalls.enabled) enabled.push(answeredCalls);

			var adherence = new MeasureConfig(
				true,
				data.AdherenceBadgeEnabled,
				null,
				null,
				'Adherence',
				1,
				true,
				data.AdherenceThreshold.Value * 100,
				data.AdherenceBronzeThreshold.Value * 100,
				data.AdherenceSilverThreshold.Value * 100,
				data.AdherenceGoldThreshold.Value * 100,
				'\\d+(\.\d+)?$',
				100.0,
				'desc'
			);
			measureConfigs.push(adherence);
			if (adherence.enabled) enabled.push(adherence);

			var aht = new MeasureConfig(
				true,
				data.AHTBadgeEnabled,
				null,
				null,
				'AverageHandlingTime',
				2,
				true,
				data.AHTThreshold,
				data.AHTBronzeThreshold,
				data.AHTSilverThreshold,
				data.AHTGoldThreshold,
				'^(0[0-1]):([0-5][0-9]):([0-5][0-9])$',
				'01:00:00',
				'asc'
			);
			measureConfigs.push(aht);
			if (aht.enabled) enabled.push(aht);

			return measureConfigs;
		};

		var makeExternalMeasureConfigs = function (data) {
			var measureConfigs = [];

			data.ExternalBadgeSettings.forEach(function (x) {
				var config = new MeasureConfig(
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
					x.GoldThreshold,
					'\\d+(\.\d+)?$'
				);

				if (config.dataType == 0) {
					config.valueFormat = '^\\d+$';
				} else {
					config.valueFormat = '\\d+(\.\d+)?$';
					config.badgeThreshold = config.badgeThreshold * 100;
					config.goldBadgeThreshold = config.goldBadgeThreshold * 100;
					config.silverBadgeThreshold = config.silverBadgeThreshold * 100;
					config.bronzeBadgeThreshold = config.bronzeBadgeThreshold * 100;
				}

				measureConfigs.push(config);
				if (config.enabled) enabled.push(config);
			});

			return measureConfigs;
		};

		var updateSettingForm = function (data) {
			enabled = [];

			ctrl.id = data.Id;
			ctrl.name = data.Name;
			originalName = ctrl.name;
			// ctrl.changeInfo = data.UpdatedBy + ' ' + data.UpdatedOn;
			ctrl.ruleSet = data.GamificationSettingRuleSet;
			ctrl.silverRate = data.SilverToBronzeBadgeRate;
			ctrl.goldRate = data.GoldToSilverBadgeRate;
			ctrl.builtinMeasureConfigs = makeBuiltinMeasureConfigs(data);
			ctrl.externalMeasureConfigs = makeExternalMeasureConfigs(data);

			//if (enabled.length > 3) $log.error('more than 3 measures are enabled!');
			numOfEnabledMeasures = enabled.length;
		};

		ctrl.$onInit = function () {
			ctrl.errors = {};
		};

		ctrl.$onChanges = function (changesObj) {
			if (changesObj.setting && changesObj.setting.currentValue) {
				updateSettingForm(changesObj.setting.currentValue);
			}
		};

		ctrl.enableConfig = function (config, enable) {
			var maxEnabledMeasures = 3;
			if (!enable) {
				config.setEnabled(false).then(function () { numOfEnabledMeasures--; });
				if (numOfEnabledMeasures <= maxEnabledMeasures)
					ctrl.errors.hasReachedLimitOfEnabledMeasures = false;
				return;
			}
			if (numOfEnabledMeasures + 1 > maxEnabledMeasures) {
				ctrl.errors.hasReachedLimitOfEnabledMeasures = true;
				return;
			}
			config.setEnabled(true).then(function () {
				numOfEnabledMeasures++;
			});
		}

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

		ctrl.updateSilverRate = function () {
			if (!ctrl.silverRate) return;
			dataService.saveData('SilverToBronzeBadgeRate', {
				GamificationSettingId: ctrl.id,
				Rate: ctrl.silverRate
			}).then(function () {
				$log.log('updated bronze/silver rate successfully');
			}, function () {
				$log.error('failed to update bronze/silver rate')
			});
		};

		ctrl.updateGoldRate = function () {
			if (!ctrl.goldRate) return;
			dataService.saveData('GoldToSilverBadgeRate', {
				GamificationSettingId: ctrl.id,
				Rate: ctrl.goldRate
			}).then(function () {
				$log.log('updated silver/gold rate successfully');
			}, function () {
				$log.error('failed to update silver/gold rate');
			});
		};

		ctrl.onNameChange = function () {
			var newNameIsValid = ctrl.name && ctrl.name.length;
			if (!newNameIsValid) return;

			dataService.saveData('ModifyDescription', {
				Name: ctrl.name,
				GamificationSettingId: ctrl.id
			}).then(function () {
				$log.log('updated setting name successfully');
				originalName = ctrl.name;
				if (ctrl.onNameUpdate)
					ctrl.onNameUpdate({ name: ctrl.name });
			}, function () {
				$log.error('failed to update setting name');
				ctrl.name = originalName;
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

			if (this.builtin) this.translatedName = $translate.instant(this.name);
		}

		MeasureConfig.prototype.setName = function (name) {
			if (this.builtin)
				return;

			$log.log(this.name + ': set name to ' + name);

			var previous = this.name;
			this.name = name;

			var self = this;

			dataService.saveData('UpdateExternalMeasureName', {
				MeasureId: self.externalId,
				DataType: this.dataType,
				Name: name
			}).then(function () {
				$log.log('updated name');
				if (ctrl.measureNameUpdateCallback) {
					$log.log('async name to other settings');
					ctrl.measureNameUpdateCallback({ measure: { externalId: self.externalId, dataType: self.dataType, name: name } });
				}
			}, function () {
				$log.error('failed to save name. restoring: ' + previous);
				self.name = previous;
			});
		};

		MeasureConfig.prototype.setEnabled = function (enable) {
			$log.log(this.name + ': set enabled to ' + enable);

			var previous = this.enabled;
			this.enabled = enable;

			var self = this;
			var promise;

			if (!this.builtin) {
				promise = dataService.saveData('ExternalBadgeSettingEnabled', {
					GamificationSettingId: ctrl.id,
					QualityId: this.externalId,
					Value: enable
				}).then(function () {
					$log.log('updated enabled');
				}, function () {
					$log.log('failed to update enabled. restoring: ' + previous);
					self.enabled = previous;
				});
				return promise;
			}

			switch (this.name) {
				case 'AnsweredCalls':
					promise = dataService.saveData('UseBadgeForAnsweredCalls', {
						GamificationSettingId: ctrl.id,
						Value: enable
					}).then(function () {
						$log.log('updated enabled');
					}, function () {
						$log.log('failed to update enabled. restoring: ' + previous);
						self.enabled = previous;
					});
					break;

				case 'Adherence':
					promise = dataService.saveData('UseBadgeForAdherence', {
						GamificationSettingId: ctrl.id,
						Value: enable
					}).then(function () {
						$log.log('updated enabled');
					}, function () {
						$log.log('failed to update enabled. restoring: ' + previous);
						self.enabled = previous;
					});
					break;

				case 'AverageHandlingTime':
					promise = dataService.saveData('UseBadgeForAHT', {
						GamificationSettingId: ctrl.id,
						Value: enable
					}).then(function () {
						$log.log('updated enabled');
					}, function () {
						$log.log('failed to update enabled. restoring: ' + previous);
						self.enabled = previous;
					});
					break;

				default:
					break;
			}

			return promise;
		};

		MeasureConfig.prototype.setBadgeThreshold = function (badgeThreshold) {
			if (!badgeThreshold || badgeThreshold === this.badgeThreshold) return;

			$log.log(this.name + ': set badge threshold to ' + badgeThreshold);

			var previous = this.badgeThreshold;
			this.badgeThreshold = badgeThreshold;

			var self = this;

			if (!this.builtin) {
				dataService.saveData('ExternalBadgeSettingThreshold', {
					GamificationSettingId: ctrl.id,
					QualityId: this.externalId,
					DataType: this.dataType,
					ThresholdValue: this.dataType === 1 ? this.badgeThreshold / 100 : this.badgeThreshold
				}).then(function () {
					$log.log('updated badge threshold')
				}, function () {
					$log.error('failed to update badge threshold. restoring: ' + previous);
					self.bronzeBadgeThreshold = previous;
				});
				return;
			}

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
						Value: this.badgeThreshold / 100
					}).then(function () {
						$log.info(self.name + ': updated badge threshold')
					}, function () {
						$log.error(self.name + ': failed to update badge threshold. restoring: ' + previous);
						self.badgeThreshold = previous;
					});
					break;

				case 'AverageHandlingTime':
					dataService.saveData('AHTThreshold', {
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
			if (angular.isUndefined(bronzeBadgeThreshold) || bronzeBadgeThreshold == this.bronzeBadgeThreshold) return;

			$log.log(this.name + ': set bronze badge threshold to ' + bronzeBadgeThreshold);

			var previous = this.bronzeBadgeThreshold;
			this.bronzeBadgeThreshold = bronzeBadgeThreshold;

			var self = this;

			if (!this.builtin) {
				dataService.saveData('ExternalBadgeSettingBronzeThreshold', {
					GamificationSettingId: ctrl.id,
					QualityId: this.externalId,
					DataType: this.dataType,
					ThresholdValue: this.dataType === 1 ? this.bronzeBadgeThreshold / 100 : this.bronzeBadgeThreshold
				}).then(function () {
					$log.log('updated bronze badge threshold')
				}, function () {
					$log.error('failed to update bronze badge threshold. restoring: ' + previous);
					self.bronzeBadgeThreshold = previous;
				});
				return;
			}

			switch (this.name) {
				case 'AnsweredCalls':
					dataService.saveData('AnsweredCallsBronzeThreshold', {
						GamificationSettingId: ctrl.id,
						Value: this.bronzeBadgeThreshold
					}).then(function () {
						$log.log('updated bronze badge threshold')
					}, function () {
						$log.log('failed to update bronze badge threshold. restoring: ' + previous);
						self.bronzeBadgeThreshold = previous;
					});
					break;

				case 'Adherence':
					dataService.saveData('AdherenceBronzeThreshold', {
						GamificationSettingId: ctrl.id,
						Value: this.bronzeBadgeThreshold / 100
					}).then(function () {
						$log.log('updated bronze badge threshold')
					}, function () {
						$log.log('failed to update bronze badge threshold. restoring: ' + previous);
						self.bronzeBadgeThreshold = previous;
					});
					break;

				case 'AverageHandlingTime':
					dataService.saveData('AHTBronzeThreshold', {
						GamificationSettingId: ctrl.id,
						Value: this.bronzeBadgeThreshold
					}).then(function () {
						$log.log('updated bronze badge threshold')
					}, function () {
						$log.log('failed to update bronze badge threshold. restoring: ' + previous);
						self.bronzeBadgeThreshold = previous;
					});
					break;

				default:
					break;
			}
		};

		MeasureConfig.prototype.setSilverBadgeThreshold = function (silverBadgeThreshold) {
			if (angular.isUndefined(silverBadgeThreshold) || silverBadgeThreshold == this.silverBadgeThreshold) return;

			$log.log(this.name + ': set silver badge threshold to ' + silverBadgeThreshold);

			var previous = this.silverBadgeThreshold;
			this.silverBadgeThreshold = silverBadgeThreshold;

			var self = this;

			if (!this.builtin) {
				dataService.saveData('ExternalBadgeSettingSilverThreshold', {
					GamificationSettingId: ctrl.id,
					QualityId: this.externalId,
					DataType: this.dataType,
					ThresholdValue: this.dataType === 1 ? this.silverBadgeThreshold / 100 : this.silverBadgeThreshold
				}).then(function () {
					$log.log('updated silver badge threshold')
				}, function () {
					$log.error('failed to update silver badge threshold. restoring: ' + previous);
					self.silverBadgeThreshold = previous;
				});
				return;
			}

			switch (this.name) {
				case 'AnsweredCalls':
					dataService.saveData('AnsweredCallsSilverThreshold', {
						GamificationSettingId: ctrl.id,
						Value: this.silverBadgeThreshold
					}).then(function () {
						$log.log('updated silver badge threshold')
					}, function () {
						$log.log('failed to update silver badge threshold. restoring: ' + previous);
						self.silverBadgeThreshold = previous;
					});
					break;

				case 'Adherence':
					dataService.saveData('AdherenceSilverThreshold', {
						GamificationSettingId: ctrl.id,
						Value: this.silverBadgeThreshold / 100
					}).then(function () {
						$log.log('updated silver badge threshold')
					}, function () {
						$log.log('failed to update silver badge threshold. restoring: ' + previous);
						self.silverBadgeThreshold = previous;
					});
					break;

				case 'AverageHandlingTime':
					dataService.saveData('AHTSilverThreshold', {
						GamificationSettingId: ctrl.id,
						Value: this.silverBadgeThreshold
					}).then(function () {
						$log.log('updated silver badge threshold')
					}, function () {
						$log.log('failed to update silver badge threshold. restoring: ' + previous);
						self.silverBadgeThreshold = previous;
					});
					break;

				default:
					break;
			}
		};

		MeasureConfig.prototype.setGoldBadgeThreshold = function (goldBadgeThreshold) {
			if (angular.isUndefined(goldBadgeThreshold) || goldBadgeThreshold == this.goldBadgeThreshold) return;

			$log.log(this.name + ': set gold badge threshold to ' + goldBadgeThreshold);

			var previous = this.goldBadgeThreshold;
			this.goldBadgeThreshold = goldBadgeThreshold;

			var self = this;

			if (!this.builtin) {
				dataService.saveData('ExternalBadgeSettingGoldThreshold', {
					GamificationSettingId: ctrl.id,
					QualityId: this.externalId,
					DataType: this.dataType,
					ThresholdValue: this.dataType === 1 ? this.goldBadgeThreshold / 100 : this.goldBadgeThreshold
				}).then(function () {
					$log.log('updated gold badge threshold')
				}, function () {
					$log.log('failed to update gold badge threshold. restoring: ' + previous);
					self.goldBadgeThreshold = previous;
				});
				return;
			}

			switch (this.name) {
				case 'AnsweredCalls':
					dataService.saveData('AnsweredCallsGoldThreshold', {
						GamificationSettingId: ctrl.id,
						Value: this.goldBadgeThreshold
					}).then(function () {
						$log.log('updated gold badge threshold')
					}, function () {
						$log.log('failed to update gold badge threshold. restoring: ' + previous);
						self.goldBadgeThreshold = previous;
					});
					break;

				case 'Adherence':
					dataService.saveData('AdherenceGoldThreshold', {
						GamificationSettingId: ctrl.id,
						Value: this.goldBadgeThreshold / 100
					}).then(function () {
						$log.log('updated gold badge threshold')
					}, function () {
						$log.log('failed to update gold badge threshold. restoring: ' + previous);
						self.goldBadgeThreshold = previous;
					});
					break;

				case 'AverageHandlingTime':
					dataService.saveData('AHTGoldThreshold', {
						GamificationSettingId: ctrl.id,
						Value: this.goldBadgeThreshold
					}).then(function () {
						$log.log('updated gold badge threshold')
					}, function () {
						$log.log('failed to update gold badge threshold. restoring: ' + previous);
						self.goldBadgeThreshold = previous;
					});
					break;

				default:
					break;
			}


		};
	}

})(angular);
