'use strict';
angular.module('wfm.forecasting')
	.controller('ForecastingSkillCreateCtrl', [
		'SkillService', '$scope', '$filter', '$state',
		function (skillService, $scope, $filter, $state) {
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

			$scope.queueSelected = true;
			$scope.hideQueueInvalidMessage = true;
			$scope.gridOptions = {
				enableGridMenu: false,
				enableSelectAll: true,
				enableFullRowSelection: true,
				columnDefs: [
					{ displayName: 'Name', field: 'Name', enableColumnMenu: false },
					{ displayName: 'Log object', field: 'LogObjectName', enableColumnMenu: false },
					{ displayName: 'Description', field: 'Description', enableColumnMenu: false }
				],
				onRegisterApi: function (gridApi) {
					$scope.gridApi = gridApi;
					gridApi.selection.on.rowSelectionChanged($scope, function (row) {
						$scope.queueSelected = row.grid.selection.selectedCount > 0;
						$scope.hideQueueInvalidMessage = false;
					});
					gridApi.selection.on.rowSelectionChangedBatch($scope, function (rows, test) {
						$scope.queueSelected = rows[0].grid.selection.selectedCount > 0;
						$scope.hideQueueInvalidMessage = false;
					});
				}
			};
			skillService.queues.get().$promise.then(function (result) {
				$scope.gridOptions.data = result;
			});

			$scope.createSkill = function (formValid) {
				if ($scope.hideQueueInvalidMessage) {
					$scope.queueSelected = false;
					return;
				}
				if (!formValid || !$scope.queueSelected) {
					return;
				}
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
						$state.go('forecasting.start');
					}
				);
			};
		}
	]);
