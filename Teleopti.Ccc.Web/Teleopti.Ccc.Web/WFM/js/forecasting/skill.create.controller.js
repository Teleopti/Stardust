'use strict';
angular.module('wfm.forecasting')
	.controller('ForecastingSkillCreateCtrl', [
		'SkillService', '$scope', '$filter',
		function (skillService, $scope, $filter) {
			$scope.model = {};

			$scope.activities = [];
			skillService.activities.get().$promise.then(function(result) {
				var activities = $filter('orderBy')(result, 'Name');
				$scope.activities = activities;
				$scope.model.selectedActivity = activities[0];
			});
			$scope.timezones = [];
			skillService.timezones.get().$promise.then(function(result) {
				$scope.timezones = result.Timezones;
				$scope.model.selectedTimezone = $filter('filter')(result.Timezones, function(x) { return x.Id === result.DefaultTimezone; })[0];
			});

			$scope.gridOptions = {
				enableGridMenu: false,
				enableSelectAll: true,
				columnDefs: [
					{ displayName: 'Name', field: 'Name', enableColumnMenu: false },
					{ displayName: 'Log object', field: 'LogObjectName', enableColumnMenu: false },
					{ displayName: 'Description', field: 'Description', enableColumnMenu: false }
				],
				onRegisterApi: function (gridApi) {
					$scope.gridApi = gridApi;
				}
			};
			skillService.queues.get().$promise.then(function (result) {
				$scope.gridOptions.data = result;
			});

			$scope.createSkill = function() {
				var selectedRows = $scope.gridApi.selection.getSelectedRows();
				var queues = [];
				angular.forEach(selectedRows, function(row) {
					queues.push(row.Id);
				});
				skillService.skill.create(
				{
					Name: $scope.model.name,
					ActivityId: $scope.model.selectedActivity.Id,
					TimezoneId: $scope.model.selectedTimezone.Id,
					Queues: queues
				}).$promise.then(
					function(result) {
						console.log(result);

					}
				);
			};

		}
	]);
