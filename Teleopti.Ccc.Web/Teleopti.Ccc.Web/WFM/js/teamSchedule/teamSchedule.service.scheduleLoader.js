"use strict";

angular.module("wfm.teamSchedule").service("ScheduleLoader", [
	"$resource", "Toggle", "TeamSchedule", function($resource, toggleSvc, teamScheduleSvc) {
		var service = this;

		service.loadSchedules = function (params, resultHandler) {
			var loadScheduleWithoutReadModel = toggleSvc.WfmTeamSchedule_NoReadModel_35609;

			if (loadScheduleWithoutReadModel) {
				teamScheduleSvc.searchSchedules.query(params).$promise.then(resultHandler);
			}
		};
	}
]);
