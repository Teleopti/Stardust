(function () {
	'use strict';

	angular.module('wfm.groupPage').service('groupPageService', GroupPageService);

	GroupPageService.$inject = ['$http', '$q'];

	function GroupPageService($http, $q) {
		var urlForAvailableGroupPages = "../api/GroupPage/AvailableStructuredGroupPages";

		this.fetchAvailableGroupPages = function(startDateStr, endDateStr) {
			return $q(function(resolve, reject) {
				$http.get(urlForAvailableGroupPages,
				{
					params: {
						startDate: startDateStr,
						endDate: endDateStr
					}
				}).then(function(response) {
					resolve(response.data);
				});
			});
		}
	}
})();