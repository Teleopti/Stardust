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
						 	WeekDay:1
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
			skillService.activities.get().$promise.then(function(result) {
				var activities = $filter('orderBy')(result, 'Name');
				$scope.activities = activities;
				$scope.activities.unshift({ Name: 'Pick an activity...', Disabled: true });
				$scope.model.selectedActivity = $scope.activities[1];

			});
			$scope.timezones = [];
			skillService.timezones.get().$promise.then(function(result) {
				$scope.timezones = result.Timezones;
				$scope.timezones.unshift( {Name: 'Pick a timezone...', Disabled: true} );
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
				angular.forEach(selectedRows, function(row) {
					queues.push(row.Id);
				});
				var workingHours = [];
				angular.forEach($scope.model.workingHours, function(workingHour) {
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
					function(result) {
						$state.go('forecasting.start', { workloadId: result.WorkloadId });
					}
				);
			};

			$scope.skillNameValidation = {
				isValid: function(form) {
					return ((form.$submitted || form.skillName.$touched) && (form.skillName.$error.required || form.skillName.$error.maxlength));
				},
				errorMessage: function(form) {
					if (form.skillName.$error) {
						if (form.skillName.$error.required)
							return "The name is required.";
						else if (form.skillName.$error.maxlength)
							return "The name must be less than 50 characters long.";
					}
					return "";
				}
			};

			$scope.skillServiceLevelPercentValidation = {
				isValid: function(form) {
					return ((form.$submitted || form.skillServiceLevelPercent.$touched) && (form.skillServiceLevelPercent.$error.required || form.skillServiceLevelPercent.$error.maxlength));
				},
				errorMessage: function(form) {
					if (form.skillServiceLevelPercent.$error) {
						if (form.skillServiceLevelPercent.$error.required)
							return "Percentage is required.";
						else if (form.skillServiceLevelPercent.$error.maxlength)
							return "The name must be less than 50 characters long.";
					}
					return "error";
				}
			};

			$scope.skillServiceLevelSecondsValidation = {
				isValid: function(form) {
					return ((form.$submitted || form.skillServiceLevelSeconds.$touched) && (form.skillServiceLevelSeconds.$error.required || form.skillServiceLevelSeconds.$error.maxlength));
				},
				errorMessage: function(form) {
					if (form.skillServiceLevelSeconds.$error) {
						if (form.skillServiceLevelSeconds.$error.required)
							return "Seconds are required.";
						else if (form.skillServiceLevelSeconds.$error.maxlength)
							return "The name must be less than 50 characters long.";
					}
					return "error";
				}
			};

			$scope.skillShrinkageValidation = {
				isValid: function(form) {
					return ((form.$submitted || form.skillShrinkage.$touched) && (form.skillShrinkage.$error.required || form.skillShrinkage.$error.maxlength));
				},
				errorMessage: function(form) {
					if (form.skillShrinkage.$error) {
						if (form.skillShrinkage.$error.required)
							return "Shrinkage is required.";
						else if (form.skillShrinkage.$error.maxlength)
							return "The name must be less than 50 characters long.";
					}
					return "error";
				}
			};
		}
	]);
