"use strict";

angular.module("wfm.teamSchedule").service("ScheduleLoader", [
	"$resource", "Toggle", "TeamSchedule", function($resource, toggleSvc, teamScheduleSvc) {
		var service = this;

		service.loadSchedules = function (params, resultHandler) {
			var searchScheduleEnabled = toggleSvc.WfmTeamSchedule_FindScheduleEasily_35611;
			var loadScheduleWithReadModel = !toggleSvc.WfmTeamSchedule_NoReadModel_35609;

			if (searchScheduleEnabled) {
				teamScheduleSvc.searchSchedules.query(params).$promise.then(resultHandler);
			} else if (loadScheduleWithReadModel) {
				teamScheduleSvc.loadSchedulesFromReadModelForGroup.query(params).$promise.then(resultHandler);
			} else {
				teamScheduleSvc.loadSchedulesNoReadModel.query(params).$promise.then(resultHandler);
			}
		};
	}
]);
