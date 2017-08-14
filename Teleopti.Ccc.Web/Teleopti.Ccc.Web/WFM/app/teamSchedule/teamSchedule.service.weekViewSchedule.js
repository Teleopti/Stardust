(function() {
	'use strict';
	angular.module('wfm.teamSchedule').factory('weekViewScheduleSvc', weekViewScheduleSvc);

	weekViewScheduleSvc.$inject = ['$http', '$q'];

	function weekViewScheduleSvc($http, $q) {
		var apiEndpoint = '../api/TeamSchedule/SearchWeekSchedules';
		var apiEndpointByGroup = '../api/TeamSchedule/SearchWeekSchedulesByGroups';
		function getSchedules(query, isGroupPages) {
			var deferred = $q.defer();
			var url = isGroupPages ? apiEndpointByGroup : apiEndpoint;
			$http.post(url, query).then(function (resp) {
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