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
				if (ctrl.settingDescriptors && ctrl.settingDescriptors.length > 0) {
					ctrl.getSettingById(ctrl.settingDescriptors[0].GamificationSettingId);
				}
			});
		};

		ctrl.getSettingById = function (id) {
			gamificationSettingService.getSettingById(id).then(function (data) {
				ctrl.settingData = data;
				ctrl.currentSettingId = data.Id;
				ctrl.allSettings.push(data);
			});
		};

		ctrl.$onInit = function () {
			ctrl.allSettings = [];
			ctrl.getGamificationSettingsDescriptors();
			ctrl.title = 'Gamification Settings';
		}

		ctrl.settingSelectionChanged = function () {
			if (ctrl.currentSettingId) {
				var setting = ctrl.findElementInArray(ctrl.allSettings, ctrl.currentSettingId);

				if (!setting) {
					ctrl.getSettingById(ctrl.currentSettingId);
				} else {
					ctrl.settingData = setting;
				}
			}
		};

		ctrl.findElementInArray = function (target, id) {
			if (target && target.length > 0) {
				for (var index = 0; index < target.length; index++) {
					var item = target[index];
					if (item.Id == id) {
						return item;
					}
				}
			}
		}

		ctrl.updateCurrentSettingNameInList = function (settingName) {
			var settingDescriptor = ctrl.settingDescriptors.find(function (item) {
				return item.GamificationSettingId == ctrl.currentSettingId;
			})

			settingDescriptor.Value.Name = settingName;
		};

		ctrl.addNewSetting = function () {
			gamificationSettingService.createNewSetting().then(function (data) {
				if (data) {

					ctrl.allSettings.push(data);

					var newSettingDescriptor = {
						GamificationSettingId: data.Id,
						Value: {
							Name: data.Name,
							ShortName: ''
						}
					}

					ctrl.settingDescriptors.push(newSettingDescriptor);
					ctrl.currentSettingId = data.Id;

					ctrl.settingData = data;
				}
			});
		};

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
					return item.Id == deletedId;
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
						return item.Id == ctrl.currentSettingId;
					})

					if (curSetting) {
						ctrl.settingData = curSetting;
					}
				}

			}, function (error) {
				console.log(error);
			})
		}

		ctrl.resetBadges = function name() {
			gamificationSettingService.resetBadge().then(function (response) {
			})
		}
	}

})(angular);