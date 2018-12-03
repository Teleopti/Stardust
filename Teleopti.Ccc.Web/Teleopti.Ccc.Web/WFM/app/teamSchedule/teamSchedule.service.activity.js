(function () {
	'use strict';

	angular.module("wfm.teamSchedule").service("ActivityService", ['$http', '$q', 'serviceDateFormatHelper', ActivityService]);

	function ActivityService($http, $q, serviceDateFormatHelper) {
		var activities = [];

		var getAllActivitiesUrl = '../api/TeamScheduleData/FetchActivities';
		var addActivityUrl = '../api/TeamScheduleCommand/AddActivity';
		var addPersonalActivityUrl = '../api/TeamScheduleCommand/AddPersonalActivity';
		var addOvertimeActivityUrl = '../api/TeamSchedule/AddOvertimeActivity';
		var removeActivityUrl = '../api/TeamScheduleCommand/RemoveActivity';
		var moveActivityUrl = '../api/TeamScheduleCommand/MoveActivity';
		var moveShiftUrl = '../api/TeamSchedule/MoveShift';
		var removeShiftUrl = '../api/TeamScheduleCommand/RemoveShift';
		var undoScheduleChangeUrl = '../api/TeamScheduleCommand/BackoutScheduleChange';
		var moveInvalidOverlappedActivityUrl = '../api/TeamScheduleCommand/MoveNonoverwritableLayers';
		var getAllMultiplicatorDefinitionSetsUrl = '../api/MultiplicatorDefinitionSet/Overtime';



		this.fetchAvailableActivities = fetchAvailableActivities;
		this.fetchAvailableDefinitionSets = fetchAvailableDefinitionSets;
		this.addActivity = addActivity;
		this.addOvertimeActivity = addOvertimeActivity;
		this.removeActivity = removeActivity;
		this.moveActivity = moveActivity;
		this.moveShift = moveShift;
		this.removeShift = removeShift;
		this.undoScheduleChange = undoScheduleChange;
		this.moveInvalidOverlappedActivity = moveInvalidOverlappedActivity;

		function addOvertimeActivity(input) {
			return $q(function (resolve, reject) {
				$http.post(addOvertimeActivityUrl, input).then(function (data) {
					resolve(data);
				}, function (err) {
					reject(err);
				});
			});
		}

		function moveShift(input) {
			return $q(function (resolve, reject) {
				$http.post(moveShiftUrl, input).then(function (data) {
					resolve(data);
				}, function (err) {
					reject(err);
				});
			});
		}

		function removeShift(input) {
			return $q(function (resolve, reject) {
				$http.post(removeShiftUrl, input).then(function (data) {
					resolve(data);
				}, function (err) {
					reject(err);
				});
			});
		}

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
			if (activities.length > 0) {
				deferred.resolve(activities);
				return deferred.promise;
			}

			$http.get(getAllActivitiesUrl).success(function (data) {
				activities = data;
				deferred.resolve(data);
			});
			return deferred.promise;
		}

		function addActivity(input) {
			var url = '';
			switch (input.ActivityType) {
				case 1:
					url = addActivityUrl;
					break;
				case 2:
					url = addPersonalActivityUrl;
					break;
			}
			var deferred = $q.defer();
			$http.post(url, input).then(function (data) {
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
			if (!!input.Date)
				normalized.Date = serviceDateFormatHelper.getDateOnly(input.Date);
			return normalized;
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

		function moveActivity(moveActivityForm) {
			var deffered = $q.defer();
			$http.post(moveActivityUrl, normalizeInput(moveActivityForm)).then(function (result) {
				deffered.resolve(result);
			}, function (error) {
				deffered.reject(error);
			});
			return deffered.promise;
		}

		var definitionSets = [];
		function fetchAvailableDefinitionSets() {
			return $q(function (resolve, reject) {
				if (definitionSets.length) {
					resolve(definitionSets);
					return;
				}
				$http.get(getAllMultiplicatorDefinitionSetsUrl).then
					(function (data) {
						definitionSets = data.data;
						resolve(definitionSets);
					},
					function (err) {
						reject(err);
					});
			});
		}
	}

})();
