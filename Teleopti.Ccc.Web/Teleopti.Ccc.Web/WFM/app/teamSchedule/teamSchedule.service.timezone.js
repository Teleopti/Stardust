(function () {
	'use strict';
	angular.module("wfm.teamSchedule").service("TimezoneDataService", TimezoneDataService);

	TimezoneDataService.$inject = ["$http", "$q"];

	function TimezoneDataService($http, $q) {
		var self = this;
		var timezoneUrl = '../api/Global/TimeZone';

		self.getAll = getAll;

		function getAll() {
			return $q(function (resolve, reject) {
				$http.get(timezoneUrl).then(function (response) {
					resolve(response.data);
				}, function (err) {
					reject(err);
				});
			});
		}
	}
})();