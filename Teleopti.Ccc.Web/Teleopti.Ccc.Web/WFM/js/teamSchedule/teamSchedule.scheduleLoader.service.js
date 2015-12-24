"use strict";

angular.module("wfm.teamSchedule").service("ScheduleLoader", [
	"$resource", "TeamSchedule", function($resource, teamScheduleSvc) {
		var service = this;

		service.loadSchedules = function(options, resultHandler) {
			if (options.selectedTeamId == undefined && !options.isSearchScheduleEnabled)
				return;

			if (options.isSearchScheduleEnabled) {
				teamScheduleSvc.searchSchedules.query(options.params).$promise.then(resultHandler);
			} else if (options.loadScheduelWithReadModel) {
				teamScheduleSvc.loadSchedulesFromReadModelForGroup.query(options.params).$promise.then(resultHandler);
			} else {
				teamScheduleSvc.loadSchedulesNoReadModel.query(options.params).$promise.then(resultHandler);
			}
		};
	}
]);
