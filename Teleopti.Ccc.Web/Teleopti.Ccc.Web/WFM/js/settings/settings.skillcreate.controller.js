'use strict';
angular.module('wfm.settings')
	.controller('SettingsSkillCreateCtrl', [
		'Settings', '$scope', '$filter',
		function (settingsService, $scope, $filter) {
			$scope.model = {};

			$scope.activities = [];
			settingsService.activities.get().$promise.then(function(result) {
				$scope.activities = result;
			});
			$scope.timezones = [];
			settingsService.timezones.get().$promise.then(function(result) {
				$scope.timezones = result.Timezones;
				$scope.model.selectedTimezone = $filter('filter')(result.Timezones, function(x) { return x.Id === result.DefaultTimezone; })[0];
			});

			$scope.activityChanged = function () {

			};

			$scope.timezoneChanged = function () {
				alert($scope.model.selectedTimezone.Id + "   " + $scope.model.selectedTimezone.Name);
			};
		}
	]);
