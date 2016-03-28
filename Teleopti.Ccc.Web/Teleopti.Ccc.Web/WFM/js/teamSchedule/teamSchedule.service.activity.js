(function () {
	'use strict';

	angular.module("wfm.teamSchedule").service("ActivityService", ['$http', '$q', ActivityService]);

	function ActivityService($http, $q) {

		var getAllActivitiesUrl = '../api/TeamScheduleData/FetchActivities';
		var addActivityUrl = '../api/TeamScheduleCommand/AddActivity';

		this.fetchAvailableActivities = function () {
			var deferred = $q.defer();
			$http.get(getAllActivitiesUrl).success(function (data) {
				deferred.resolve(data);
			});
			return deferred.promise;
		};

		this.addActivity = function(activity) {
			var deferred = $q.defer();
			$http.post(addActivityUrl, normalizeActivity(activity)).success(function(data) {
				deferred.resolve(data);
			});
			return deferred.promise;
		}

		function normalizeActivity(activity) {
			return activity;
		}
	}

})();
