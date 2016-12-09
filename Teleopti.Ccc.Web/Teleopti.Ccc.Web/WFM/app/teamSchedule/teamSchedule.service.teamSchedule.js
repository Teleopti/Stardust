(function() {
	"use strict";

	angular.module("wfm.teamSchedule").service("TeamSchedule", [
		"$resource", "$q", '$http', function($resource, $q, $http) {

			var service = this;
			var getTeamsHierachyUrl = '../api/TeamScheduleData/FetchPermittedTeamHierachy';
			var searchDayViewScheduleUrl = '../api/TeamSchedule/SearchSchedules';

			service.getAvalableHierachy = getAvalableHierachy;

			service.searchSchedules = searchSchedule;

			function searchSchedule(input) {
				return $q(function(resolve, reject) {
					$http.post(searchDayViewScheduleUrl, input)
						.then(function(data) {
								resolve(data);
							},
							function(err) {
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

			function getAvalableHierachy(dateStr) {
				var input = getTeamsHierachyUrl + "?date=" + dateStr;
				return $q(function(resolve, reject) {
					$http.get(input)
						.then(function(data) {
								resolve(data);
							},
							function(err) {
								reject(err);
							});
				});
			}
		}
	]);
})();