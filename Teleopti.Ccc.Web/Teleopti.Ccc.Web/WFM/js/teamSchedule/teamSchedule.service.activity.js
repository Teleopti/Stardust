(function () {
	'use strict';

	angular.module("wfm.teamSchedule").service("ActivityService", ['$http', '$q', ActivityService]);

	function ActivityService($http, $q) {

		this.fetchAvailableActivities = function () {
			var deferred = $q.defer();
			$http.get('../api/TeamScheduleData/FetchActivities').success(function (data) {
				deferred.resolve(data);
			});
			return deferred.promise;
		};
	}

})();
