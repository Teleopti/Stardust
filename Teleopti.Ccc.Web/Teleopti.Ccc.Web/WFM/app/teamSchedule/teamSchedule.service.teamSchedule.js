(function() {
	"use strict";

	angular.module("wfm.teamSchedule").service("TeamSchedule", [
		"$resource", "$q", '$http', 'Toggle', TeamScheduleService]);

	function TeamScheduleService($resource, $q, $http, toggles) {

		var service = this;	
		var searchDayViewScheduleUrl = '../api/TeamSchedule/SearchSchedules';
		var hierarchyUrl = '../api/TeamScheduleData/GetOrganizationWithPeriod';
		
		var settingForScheduleAuditTrailUrl = '../api/TeamSchedule/ScheduleAuditTrailSetting';

		service.searchSchedules = searchSchedule;

		function searchSchedule(input) {
			return $q(function (resolve, reject) {
				$http.post(searchDayViewScheduleUrl, input)
					.then(function (data) {
						resolve(data);
					},
					function (err) {
						reject(err);
					});
			});
		}

		// gradually replace with $http.post
		service.getSchedules = function(date, personIds) {
			var deferred = $q.defer();
			$http.post("../api/TeamSchedule/GetSchedules", {
				PersonIds: personIds,
				Date: date
			}).success(function(data) {
				deferred.resolve(data);
			}).error(function(e) {
				deferred.reject(e);
			});
			return deferred.promise;
		}

		service.getScheduleAuditTrailSetting = function() {
			return $q(function(resolve, reject) {
				$http.get(settingForScheduleAuditTrailUrl).then(function(response) {
						resolve(response.data);
					},
				function(error) {
					reject(error);
				});
			});
		}

		service.getPermissions = $resource("../api/TeamSchedule/GetPermissions", {

		}, {
			query: {
				method: "GET",
				params: {},
				isArray: false
			}
		});

		service.PromiseForGetAgentsPerPageSetting = function () {
			return service.getAgentsPerPageSetting.post().$promise;
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

		service.PromiseForloadedPermissions = function () {
			return service.getPermissions.query().$promise;
		};

		service.hierarchy = function (dateStr) {
			return getOrganizations({ date: dateStr });
		};

		service.hierarchyOverPeriod = function (startDate, endDate) {
			return getOrganizations({
				startDate: startDate,
				endDate: endDate
			});
		};

		
		function getOrganizations(params) {
			return $q(function (resolve, reject) {
				$http.get(hierarchyUrl, { params: params })
					.then(function (response) {
						resolve(response.data);
					}, function (response) {
						reject(response.data);
					});
			});
		}
	}
})();