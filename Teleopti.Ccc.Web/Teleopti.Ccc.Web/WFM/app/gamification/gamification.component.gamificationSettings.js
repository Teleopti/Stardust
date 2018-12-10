(function (angular) {

	'use strict';

	angular.module('wfm.gamification')
		.component('gamificationSettings', {
			templateUrl: 'app/gamification/html/g.component.gamificationSettings.tpl.html',
			controller: GamificationSettingsController
		});

	GamificationSettingsController.$inject = ['$translate', '$mdDialog', 'gamificationSettingService'];

	function GamificationSettingsController($translate, $mdDialog, gamificationSettingService) {
		var ctrl = this;

		ctrl.getGamificationSettingsDescriptors = function () {
			gamificationSettingService.getSettingsDescriptor().then(function (data) {
				ctrl.settingDescriptors = data;
				if (ctrl.settingDescriptors && ctrl.settingDescriptors.length > 0) {
					ctrl.getSettingById(ctrl.settingDescriptors[0].GamificationSettingId);
				}
				else {
					// Create a new setting if there is no setting in database.
					ctrl.addNewSetting();
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
		}

		ctrl.settingSelectionChanged = function () {
			// Common these codes to fetch setting when selection changed.
			// if (ctrl.currentSettingId) {
			// 	var setting = ctrl.allSettings.find(function (item) {
			// 		return item.Id == ctrl.currentSettingId;
			// 	});

			// 	if (!setting) {
			// 		ctrl.getSettingById(ctrl.currentSettingId);
			// 	} else {
			// 		ctrl.settingData = setting;
			// 	}
			// }

			ctrl.getSettingById(ctrl.currentSettingId);
		};

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
			});
		}

		ctrl.confirmResetBadges = function name() {
			var confirm = $mdDialog.confirm()
				.title($translate.instant('ResetBadges'))
				.textContent($translate.instant('ResetBadgesConfirm'))
				.ok($translate.instant('Ok'))
				.cancel($translate.instant('Cancel'));

			$mdDialog.show(confirm).then(function () {
				gamificationSettingService.resetBadge().then(function (response) { });
			});
		}

		ctrl.confirmDeleteSetting = function name() {

			var setting = ctrl.allSettings.find(function (item) {
			 		return item.Id == ctrl.currentSettingId;
			});

			var confirm = $mdDialog.confirm()
				.title($translate.instant('DeleteGamificationSetting'))
				.textContent($translate.instant('AreYouSureYouWantToDeleteGamificationSetting').replace('{0}', setting.Name))
				.ok($translate.instant('Ok'))
				.cancel($translate.instant('Cancel'));

			$mdDialog.show(confirm).then(function () {
				ctrl.deleteSetting();
			});
		}


	}
})(angular);