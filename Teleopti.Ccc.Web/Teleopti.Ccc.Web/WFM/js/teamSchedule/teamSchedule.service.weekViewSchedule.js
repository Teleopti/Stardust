(function() {
	'use strict';
	angular.module('wfm.teamSchedule').factory('weekViewScheduleSvc', weekViewScheduleSvc);

	weekViewScheduleSvc.$inject = ['$http', '$q', '$timeout'];

	function weekViewScheduleSvc($http, $q, $timeout) {
		var apiEndpoint = '../api/TeamSchedule';

		function getSchedules(query) {
			var deferred = $q.defer();
			$http.post(apiEndpoint, query).then(function(data) {
				deferred.resolve(data);
			}, function(error) {
				deferred.reject(error);
			});
			return deferred.promise;
		}


		function getFakeSchedules(query) {
			var fakeData = [
				{
					PersonId: "ashley",
					Name: "ashley",
					Days: [
						{
							Date: "2016-08-08",
							SummeryTitle: "Early",
							SummeryTimeSpan: "08:00-17:00",
							Color: 'grey'
						}, {
							Date: "2016-08-09",
							SummeryTitle: "Early",
							SummeryTimeSpan: "08:00-17:00",
							Color: '#ffffff'
						}, {
							Date: "2016-08-10",
							SummeryTitle: "Early",
							SummeryTimeSpan: "08:00-17:00",
							Color: 'grey'
						}, {
							Date: "2016-08-11",
							SummeryTitle: "Early",
							SummeryTimeSpan: "08:00-17:00",
							Color: '#FFFFFF'
						}, {
							Date: "2016-08-12",
							SummeryTitle: "Early",
							SummeryTimeSpan: "08:00-17:00",
							Color: '#FFFFFF'
						}, {
							Date: "2016-08-13",
							SummeryTitle: "Early",
							SummeryTimeSpan: "08:00-17:00",
							Color: '#FFFFFF'
						}, {
							Date: "2016-08-14",
							SummeryTitle: "Early",
							SummeryTimeSpan: "08:00-17:00",
							Color: '#FFFFFF'
						}
					]
				}
			];

			var deferred = $q.defer();
			$timeout(function() {
				deferred.resolve(fakeData);
			}, 1000);
		
			return deferred.promise;
		}

		return {
			getSchedules: getSchedules,
			getFakeSchedules: getFakeSchedules
		};
	}
})();