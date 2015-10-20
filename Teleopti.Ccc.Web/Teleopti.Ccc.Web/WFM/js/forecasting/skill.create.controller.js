(function () {
	'use strict';
	angular.module('wfm.forecasting')
		.controller('ForecastingSkillCreateCtrl', [
			'SkillService', '$scope', '$filter', '$state', 'growl',
			function (skillService, $scope, $filter, $state, growl) {
				$scope.model = {
					serviceLevelPercent: 80,
					serviceLevelSeconds: 20,
					shrinkage: 0,
					workingHours: [
					{
						StartTime: new Date(2000, 1, 1, 0, 0, 0, 0),
						EndTime: new Date(2000, 1, 2, 0, 0, 0, 0),
						WeekDaySelections: [
							{
								Checked: true,
								WeekDay: 1
							},
							{
								Checked: true,
								WeekDay: 2
							},
							{
								Checked: true,
								WeekDay: 3
							},
							{
								Checked: true,
								WeekDay: 4
							},
							{
								Checked: true,
								WeekDay: 5
							},
							{
								Checked: true,
								WeekDay: 6
							},
							{
								Checked: true,
								WeekDay: 0
							}
						]
					}]
				};

				$scope.activities = [];
				skillService.activities.get().$promise.then(function (result) {
					var activities = $filter('orderBy')(result, 'Name');
					$scope.activities = activities;
					if ($scope.activities[0])
						$scope.model.selectedActivity = $scope.activities[0];
				});
				$scope.timezones = [];
				skillService.timezones.get().$promise.then(function (result) {
					$scope.timezones = result.Timezones;
					$scope.model.selectedTimezone = $filter('filter')(result.Timezones, function (x) { return x.Id === result.DefaultTimezone; })[0];
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
						gridApi.selection.on.rowSelectionChangedBatch($scope, function (rows) {
							$scope.queueSelected = rows[0].grid.selection.selectedCount > 0;
							$scope.hideQueueInvalidMessage = false;
						});
					}
				};
				skillService.queues.get().$promise.then(function (result) {
					$scope.gridOptions.data = result;
				});

				function formatTimespanObj(timespan) {
					var startTimeMoment = moment(timespan.StartTime),
						endTimeMoment = moment(timespan.EndTime);

					if (startTimeMoment.isSame(endTimeMoment, 'day')) {
						return {
							StartTime: startTimeMoment.format('HH:mm'),
							EndTime: endTimeMoment.format('HH:mm')
						};
					} else {
						return {
							StartTime: startTimeMoment.format('HH:mm'),
							EndTime: '1.' + endTimeMoment.format('HH:mm')
						};
					}
				}

				$scope.createSkill = function (formValid) {
					if (!formValid || !$scope.queueSelected || $scope.hideQueueInvalidMessage) {
						if ($scope.hideQueueInvalidMessage)
							$scope.queueSelected = false;
						growl.warning("<i class='mdi mdi-alert'></i> Could not apply. See issues above.", {
							ttl: 5000,
							disableCountDown: true
						});
						return;
					}
					var selectedRows = $scope.gridApi.selection.getSelectedRows();
					var queues = [];
					angular.forEach(selectedRows, function (row) {
						queues.push(row.Id);
					});
					var workingHours = [];
					angular.forEach($scope.model.workingHours, function (workingHour) {
						var period = formatTimespanObj(workingHour);
						workingHours.push({
							StartTime: period.StartTime,
							EndTime: period.EndTime,
							WeekDaySelections: workingHour.WeekDaySelections
						});
					});
					skillService.skill.create(
					{
						Name: $scope.model.name,
						ActivityId: $scope.model.selectedActivity.Id,
						TimezoneId: $scope.model.selectedTimezone.Id,
						Queues: queues,
						ServiceLevelPercent: $scope.model.serviceLevelPercent,
						ServiceLevelSeconds: $scope.model.serviceLevelSeconds,
						Shrinkage: $scope.model.shrinkage,
						OpenHours: workingHours
					}).$promise.then(
						function (result) {
							$state.go('forecasting.start', { workloadId: result.WorkloadId });
						}
					);
				};
			}
		]);
})();