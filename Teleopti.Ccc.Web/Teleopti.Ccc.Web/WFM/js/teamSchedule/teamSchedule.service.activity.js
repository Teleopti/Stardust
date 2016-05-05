(function () {
	'use strict';

	angular.module("wfm.teamSchedule").service("ActivityService", ['$http', '$q', ActivityService]);

	function ActivityService($http, $q) {

		var getAllActivitiesUrl = '../api/TeamScheduleData/FetchActivities';
		var addActivityUrl = '../api/TeamScheduleCommand/AddActivity';
		var removeActivityUrl = '../api/TeamScheduleCommand/RemoveActivity';
		var moveActivityUrl = '../api/TeamScheduleCommand/MoveActivity';

		this.fetchAvailableActivities = fetchAvailableActivities;
		this.addActivity = addActivity;
		this.removeActivity = removeActivity;
		this.moveActivity = moveActivity;

		function fetchAvailableActivities() {
			var deferred = $q.defer();
			$http.get(getAllActivitiesUrl).success(function (data) {
				deferred.resolve(data);
			});
			return deferred.promise;
		}

		function addActivity(activity) {
			var deferred = $q.defer();
			$http.post(addActivityUrl, normalizeInput(activity)).then(function (data) {
				deferred.resolve(data);
			}, function (error) {
				deferred.reject(error);
			});
			return deferred.promise;
		}

		function normalizeInput(activity) {
			var normalized = angular.copy(activity);
			normalized.Date = moment(activity.Date).format('YYYY-MM-DD');			
			return normalized;
		}

		function removeActivity(removeActivityForm) {			
			var deffered = $q.defer();
			$http.post(removeActivityUrl, normalizeInput(removeActivityForm)).then(function (result) {
				deffered.resolve(result);
			}, function (error) {
				deffered.reject(error);
			});
			return deffered.promise;
		}

		function moveActivity(moveActivityForm) {
			var deffered = $q.defer();
			$http.post(moveActivityUrl, normalizeInput(moveActivityForm)).then(function (result) {
				deffered.resolve(result);
			}, function (error) {
				deffered.reject(error);
			});
			return deffered.promise;
		}
	}

})();
