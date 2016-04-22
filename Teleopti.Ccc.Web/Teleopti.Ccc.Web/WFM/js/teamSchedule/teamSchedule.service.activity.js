(function () {
	'use strict';

	angular.module("wfm.teamSchedule").service("ActivityService", ['$http', '$q', ActivityService]);

	function ActivityService($http, $q) {

		var getAllActivitiesUrl = '../api/TeamScheduleData/FetchActivities';
		var addActivityUrl = '../api/TeamScheduleCommand/AddActivity';
		var removeActivityUrl = '../api/TeamScheduleCommand/RemoveActivity';

		this.fetchAvailableActivities = fetchAvailableActivities;
		this.addActivity = addActivity;
		this.removeActivity = removeActivity;

		function fetchAvailableActivities() {
			var deferred = $q.defer();
			$http.get(getAllActivitiesUrl).success(function (data) {
				deferred.resolve(data);
			});
			return deferred.promise;
		}

		function addActivity(activity) {
			var deferred = $q.defer();
			$http.post(addActivityUrl, normalizeActivity(activity)).then(function (data) {
				deferred.resolve(data);
			}, function (error) {
				deferred.reject(error);
			});
			return deferred.promise;
		}

		function normalizeActivity(activity) {
			return activity;
		}

		function removeActivity(removeActivityForm) {
			var deffered = $q.defer();
			$http.post(removeActivityUrl, removeActivityForm).then(function (result) {
				deffered.resolve(result);
			}, function (error) {
				deffered.reject(error);
			});
			return deffered.promise;
		}
	}

})();
