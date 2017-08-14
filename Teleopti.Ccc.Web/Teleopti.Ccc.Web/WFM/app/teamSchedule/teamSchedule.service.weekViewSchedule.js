(function() {
	'use strict';
	angular.module('wfm.teamSchedule').factory('weekViewScheduleSvc', weekViewScheduleSvc);

	weekViewScheduleSvc.$inject = ['$http', '$q'];

	function weekViewScheduleSvc($http, $q) {
		var apiEndpoint = '../api/TeamSchedule/SearchWeekSchedules';
		function getSchedules(query) {
			var deferred = $q.defer();
			$http.post(apiEndpoint, query).then(function (resp) {
				deferred.resolve(resp.data);
			}, function(error) {
				deferred.reject(error);
			});
			return deferred.promise;
		}		

		return {
			getSchedules: getSchedules
		};
	}
})();