(function () {
	'use strict';
	angular.module('wfm.forecasting')
		.controller('ForecastingSkillCreateCtrl', [
			'SkillService', '$scope', '$filter', '$state', 'growl', 'workingHoursService',
			function (skillService, $scope, $filter, $state, growl, workingHoursService) {
				var open24Hours = workingHoursService.createEmptyWorkingPeriod(new Date(2000, 1, 1, 0, 0, 0, 0), new Date(2000, 1, 2, 0, 0, 0, 0));
				angular.forEach(open24Hours.WeekDaySelections, function (weekDay) {
					weekDay.Checked = true;
				});

				$scope.model = {
					serviceLevelPercent: 80,
					serviceLevelSeconds: 20,
					shrinkage: 0,
					workingHours: [open24Hours]
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
						{ displayName: 'Name', field: 'Name', enableColumnMenu: false, headerCellFilter:'translate' },
						{ displayName: 'LogObject', field: 'LogObjectName', enableColumnMenu: false, headerCellFilter:'translate' },
						{ displayName: 'Description', field: 'Description', enableColumnMenu: false, headerCellFilter:'translate' }
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

				$scope.createSkill = function (isFormValid) {
					if (!isFormValid || !$scope.queueSelected || $scope.hideQueueInvalidMessage) {
						if ($scope.hideQueueInvalidMessage)
							$scope.queueSelected = false;
						growl.warning("<i class='mdi mdi-alert'></i> {{'CouldNotApply'|translate}}", {
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

				$scope.noOpenHoursWarning = function() {
					for (var i = 0, len = $scope.model.workingHours.length; i < len; i++) {
						var workingHour = $scope.model.workingHours[i];
						if ($filter('filter')(workingHour.WeekDaySelections, function(x) { return x.Checked; }).length !== 0)
							return false;
					}
					return true;
				};
			}
		]);
})();
