﻿(function () {
	'use strict';

	angular.module("wfm.teamSchedule").service("ActivityService", ['$http', '$q', ActivityService]);

	function ActivityService($http, $q) {

		var getAllActivitiesUrl = '../api/TeamScheduleData/FetchActivities';
		var addActivityUrl = '../api/TeamScheduleCommand/AddActivity';
		var addPersonalActivityUrl = '../api/TeamScheduleCommand/AddPersonalActivity';
		var removeActivityUrl = '../api/TeamScheduleCommand/RemoveActivity';
		var moveActivityUrl = '../api/TeamScheduleCommand/MoveActivity';
		var undoScheduleChangeUrl = '../api/TeamScheduleCommand/BackoutScheduleChange';
		var moveInvalidOverlappedActivityUrl = '../api/TeamScheduleCommand/MoveNonoverwritableLayers';

		this.fetchAvailableActivities = fetchAvailableActivities;
		this.addActivity = addActivity;
		this.addPersonalActivity = addPersonalActivity;
		this.removeActivity = removeActivity;
		this.moveActivity = moveActivity;
		this.undoScheduleChange = undoScheduleChange;
		this.moveInvalidOverlappedActivity = moveInvalidOverlappedActivity;

		function moveInvalidOverlappedActivity(input) {
			return $q(function (resolve, reject) {
				$http.post(moveInvalidOverlappedActivityUrl, normalizeInput(input)).then(function (data) {
					resolve(data);
				}, function (err) {
					reject(err);
				});
			});
		}

		function fetchAvailableActivities() {
			var deferred = $q.defer();
			$http.get(getAllActivitiesUrl).success(function (data) {
				deferred.resolve(data);
			});
			return deferred.promise;
		}

		function addActivity(input) {
			var deferred = $q.defer();
			$http.post(addActivityUrl, input).then(function (data) {
				deferred.resolve(data);
			}, function (error) {
				deferred.reject(error);
			});
			return deferred.promise;
		}

		function addPersonalActivity(input) {
			var deferred = $q.defer();
			$http.post(addPersonalActivityUrl, normalizeInput(input)).then(function (data) {
				deferred.resolve(data);
			}, function (error) {
				deferred.reject(error);
			});
			return deferred.promise;
		}

		function undoScheduleChange(input) {
			var deferred = $q.defer();
			$http.post(undoScheduleChangeUrl, normalizeInput(input)).then(function (data) {
				deferred.resolve(data);
			}, function (error) {
				deferred.reject(error);
			});
			return deferred.promise;
		}

		function normalizeInput(input) {
			var normalized = angular.copy(input);
			normalized.Date = moment(input.Date).format('YYYY-MM-DD');
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
