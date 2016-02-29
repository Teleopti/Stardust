"use strict";

angular.module("wfm.teamSchedule").service("TeamSchedule", [
	"$resource", "$q", function($resource, $q) {

		var service = this;

		service.searchSchedules = $resource("../api/TeamSchedule/SearchSchedules", {
			keyword: "@keyword",
			date: "@queryDate",
			pageSize: "@pageSize",
			currentPageIndex: "@currentPageIndex",
			isOnlyAbsences: "@isOnlyAbsences"
		}, {
			query: {
				method: "GET",
				params: {},
				isArray: false
			}
		});

		service.getSchedules = $resource("../api/TeamSchedule/GetSchedules", {}, {
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

		service.PromiseForGetAgentsPerPageSetting = function(callback) {
			return $q(function(resolve) {
				service.getAgentsPerPageSetting.post().$promise.then(function(result) {
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
			agents: "@agents"
		}, {
			post: {
				method: "POST",
				params: {},
				isArray: true
			}
		});

		service.PromiseForloadedPermissions = function(callback) {
			return $q(function(resolve) {
				service.getPermissions.query().$promise.then(function(result) {
					callback(result);
					resolve();
				});
			});
		};
	}
]);
