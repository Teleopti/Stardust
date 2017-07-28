(function () {
	'use strict';

	angular.module('wfm.groupPage').service('groupPageService', GroupPageService);

	GroupPageService.$inject = ['$http'];

	function GroupPageService($http) {
		var urlForAvailableGroupPages = "../api/GroupPage/AvailableStructuredGroupPages";

		this.fetchAvailableGroupPages = function(startDateStr, endDateStr) {
			return $http.get(urlForAvailableGroupPages,
				{
					params: {
						startDate: startDateStr,
						endDate: endDateStr
					}
				});
		}
	}
})();