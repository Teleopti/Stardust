(function () {
	"use strict";
	angular
		.module("wfm.forecasting")
		.controller("ForecastingSkillCreateController", [
			"$scope",
			"$filter",
			"$state",
			"$translate",
			"NoticeService",
			"SkillService",
			"workingHoursService",
			"uiGridConstants",
			forecastingSkillCreateController
		]);

	function forecastingSkillCreateController(
		$scope,
		$filter,
		$state,
		$translate,
		noticeService,
		skillService,
		workingHoursService,
		uiGridConstants
	) {
		var vm = this;

		var open24Hours = workingHoursService.createEmptyWorkingPeriod(
			new Date(2000, 1, 1, 0, 0, 0, 0),
			new Date(2000, 1, 2, 0, 0, 0, 0)
		);

		angular.forEach(
			open24Hours.WeekDaySelections,

			function (weekDay) {
				weekDay.Checked = true;
			}
		);

		vm.model = {
			serviceLevelPercent: 80,
			serviceLevelSeconds: 20,
			shrinkage: 0,
			workingHours: [open24Hours]
		};

		vm.activities = [];
		skillService.activities.get().$promise.then(function (result) {
			var activities = $filter("orderBy")(result, "Name");
			vm.activities = activities;

			if (vm.activities[0]) vm.model.selectedActivity = vm.activities[0];
		});

		vm.timezones = [];

		skillService.timezones.get().$promise.then(function (result) {
			vm.timezones = result.Timezones;
			vm.model.selectedTimezone = $filter("filter")(result.Timezones, function (
				x
			) {
				return x.Id === result.DefaultTimezone;
			})[0];
		});

		vm.queueSelected = true;

		vm.hideQueueInvalidMessage = true;

		vm.gridOptions = {
			//enableGridMenu: true,
			enableSelectAll: true,
			enableFullRowSelection: true,
			enableSorting: true,
			columnDefs: [
				{
					displayName: "Name",
					field: "Name",
					enableColumnMenu: false,
					headerCellFilter: "translate",
					sort: {
						direction: uiGridConstants.ASC,
					}
				},

				{
					displayName: "LogObject",
					field: "LogObjectName",
					enableColumnMenu: false,
					headerCellFilter: "translate",
					sort: {
						direction: uiGridConstants.ASC,
					}
				},

				{
					displayName: "Description",
					field: "Description",
					enableColumnMenu: false,
					headerCellFilter: "translate",
					sort: {
						direction: uiGridConstants.ASC,
					}
				}
			],

			onRegisterApi: function (gridApi) {
				vm.gridApi = gridApi;
				gridApi.selection.on.rowSelectionChanged($scope, function (row) {
					vm.queueSelected = row.grid.selection.selectedCount > 0;
					vm.hideQueueInvalidMessage = false;
				});

				gridApi.selection.on.rowSelectionChangedBatch($scope, function (rows) {
					vm.queueSelected = rows[0].grid.selection.selectedCount > 0;
					vm.hideQueueInvalidMessage = false;
				});
			}
		};

		skillService.queues.get().$promise.then(function (result) {
			vm.gridOptions.data = result;
		});

		function formatTimespanObj(timespan) {
			var startTimeMoment = moment(timespan.StartTime),
				endTimeMoment = moment(timespan.EndTime);

			if (startTimeMoment.isSame(endTimeMoment, "day")) {
				return {
					StartTime: startTimeMoment.format("HH:mm"),
					EndTime: endTimeMoment.format("HH:mm")
				};
			} else {
				return {
					StartTime: startTimeMoment.format("HH:mm"),
					EndTime: "1." + endTimeMoment.format("HH:mm")
				};
			}
		}

		vm.createSkill = function (isFormValid) {
			if (!isFormValid || !vm.queueSelected || vm.hideQueueInvalidMessage) {
				if (vm.hideQueueInvalidMessage) vm.queueSelected = false;
				noticeService.warning($translate.instant("CouldNotApply"), 5000, true);
				return;
			}

			var selectedRows = vm.gridApi.selection.getSelectedRows();

			var queues = [];

			angular.forEach(selectedRows, function (row) {
				queues.push(row.Id);
			});

			var workingHours = [];

			angular.forEach(vm.model.workingHours, function (workingHour) {
				var period = formatTimespanObj(workingHour);
				workingHours.push({
					StartTime: period.StartTime,
					EndTime: period.EndTime,
					WeekDaySelections: workingHour.WeekDaySelections
				});
			});

			skillService.skill
				.create({
					Name: vm.model.name,
					ActivityId: vm.model.selectedActivity.Id,
					TimezoneId: vm.model.selectedTimezone.Id,
					Queues: queues,
					ServiceLevelPercent: vm.model.serviceLevelPercent,
					ServiceLevelSeconds: vm.model.serviceLevelSeconds,
					Shrinkage: vm.model.shrinkage,
					OpenHours: workingHours
				})
				.$promise.then(function (result) {
					$state.go("forecast", { workloadId: result.WorkloadId });
				});
		};

		vm.noOpenHoursWarning = function () {
			for (var i = 0, len = vm.model.workingHours.length; i < len; i++) {
				var workingHour = vm.model.workingHours[i];

				if (
					$filter("filter")(workingHour.WeekDaySelections, function (x) {
						return x.Checked;
					}).length !== 0
				)
					return false;
			}
			return true;
		};
	}
})();