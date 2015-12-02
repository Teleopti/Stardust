"use strict";

angular.module("teamScheduleService", ["ngResource"]).service("TeamSchedule", [
	"$resource", function($resource) {
		this.loadAllTeams = $resource("../api/GroupPage/AllTeams", {
			date: "@queryDate"
		}, {
			query: {
				method: "GET",
				params: {},
				isArray: true
			}
		});

		this.loadSchedules = $resource("../api/TeamSchedule/Group", {
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

		this.loadSchedulesNoReadModel = $resource("../api/TeamSchedule/GroupNoReadModel", {
			groupId: "@groupId",
			date: "@queryDate"
		}, {
			query: {
				method: "GET",
				params: {},
				isArray: true
			}
		});

		this.loadAbsences = $resource("../api/TeamSchedule/Absences", {}, {
			query: {
				method: "GET",
				params: {},
				isArray: true
			}
		});

		this.searchSchedules = $resource("../api/TeamSchedule/SearchSchedules", {
			keyword: "@keyword",
			date: "@queryDate"
		}, {
			query: {
				method: "GET",
				params: {},
				isArray: false
			}
		});

		this.getPermissions = $resource("../api/TeamSchedule/GetPermissions", {
		},{
			query: {
				method: "GET",
				params: {},
				isArray: false
			}
		});
	}
]);