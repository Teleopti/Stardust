"use strict";

angular.module("wfm.teamSchedule").service("TeamSchedule", [
	"$resource", "Toggle","$q", function ($resource, toggleSvc, $q) {

		var service = this;

		service.loadAllTeams = $resource("../api/GroupPage/AllTeams", {
			date: "@queryDate"
		}, {
			query: {
				method: "GET",
				params: {},
				isArray: true
			}
		});

		service.loadSchedulesFromReadModelForGroup = $resource("../api/TeamSchedule/Group", {
			groupId: "@groupId",
			date: "@queryDate",
			pageSize: "@pageSize",
			currentPageIndex: "@currentPageIndex"
		}, {
			query: {
				method: "GET",
				params: {},
				isArray: false
			}
		});

		service.loadSchedulesNoReadModel = $resource("../api/TeamSchedule/GroupNoReadModel", {
			groupId: "@groupId",
			date: "@queryDate"
		}, {
			query: {
				method: "GET",
				params: {},
				isArray: false
			}
		});

		service.searchSchedules = $resource("../api/TeamSchedule/SearchSchedules", {
			keyword: "@keyword",
			date: "@queryDate",
			pageSize: "@pageSize",
			currentPageIndex: "@currentPageIndex"
		}, {
			query: {
				method: "GET",
				params: {},
				isArray: false
			}
		});

		service.getPermissions = $resource("../api/TeamSchedule/GetPermissions", {
		}, {
			query: {
				method: "GET",
				params: {},
				isArray: false
			}
		});

		service.loadAbsences = $resource("../api/Absence/GetAvailableAbsences", {}, {
			query: {
				method: "GET",
				params: {},
				isArray: true
			}
		});

		service.applyFullDayAbsence = $resource("../api/TeamSchedule/AddFullDayAbsence", {}, {
			post: {
				method: "POST",
				params: {},
				isArray: true
			}
		});

		service.applyIntradayAbsence = $resource("../api/TeamSchedule/AddIntradayAbsence", {}, {
			post: {
				method: "POST",
				params: {},
				isArray: true
			}
		});

		service.swapShifts = $resource("../api/TeamSchedule/SwapShifts", {}, {
			post: {
				method: "POST",
				params: {},
				isArray: true
			}
		});

		service.getScheduleForPeople = $resource("../api/TeamSchedule/GetScheduleForPeople", {}, {
			post: {
				method: "POST",
				params: {},
				isArray: false
			}
		});

		service.PromiseForGetAgentsPerPageSetting = function(callback) {
			return $q(function (resolve) {
				service.getAgentsPerPageSetting.post().$promise.then(function (result) {
					callback(result);
					resolve();
				});
			});
		};

		service.getAgentsPerPageSetting = $resource("../api/TeamSchedule/GetAgentsPerPage", {}, {
			post: {
				method: "POST",
				params: {},
				isArray: false
			}
		});

		service.updateAgentsPerPageSetting = $resource("../api/TeamSchedule/UpdateAgentsPerPage", {
			agents: "@agents",
		}, {
			post: {
				method: "POST",
				params: {},
				isArray: true
			}
		});

		service.PromiseForloadedAvailableAbsenceTypes = function(callback) {
			return $q(function (resolve) {
				service.loadAbsences.query().$promise.then(function(result) {
					callback(result);
					resolve();
				});
			});
		};

		service.PromiseForloadedPermissions = function (callback) {
			return $q(function(resolve) {
				service.getPermissions.query().$promise.then(function(result) {
					callback(result);
					resolve();
				});
			});
		};

		service.PromiseForloadedAllTeamsForTeamPicker = function (queryDate, callback) {

			var dateString = moment(queryDate).format("YYYY-MM-DD");

			return $q(function(resolve) {
				service.loadAllTeams.query({ date: dateString }).$promise.then(function(result) {
					callback(result);
					resolve();
				});
			});
		};
	}
]);