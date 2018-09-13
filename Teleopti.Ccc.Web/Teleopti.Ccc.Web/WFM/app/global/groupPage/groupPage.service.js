(function () {
	'use strict';

	angular.module('wfm.groupPage').service('groupPageService', GroupPageService);

	GroupPageService.$inject = ['$http', '$q'];

	function GroupPageService($http, $q) {
		var curModule = '';
		var moduleMap = {
			'wfm.teamSchedule.dayView': "../api/GroupPage/AvailableStructuredGroupPages",
			'wfm.teamSchedule.weekView': "../api/GroupPage/AvailableStructuredGroupPagesForDate",
			'wfm.requests': "../api/GroupPage/AvailableStructuredGroupPagesForRequests"
		};

		this.fetchAvailableGroupPages = function (startDateStr, endDateStr) {
			if (!curModule)
				throw 'please set module first.';

			return $q(function (resolve, reject) {
				$http.get(moduleMap[curModule],
					{
						params: {
							startDate: startDateStr,
							endDate: endDateStr
						}
					}).then(function (response) {
						resolve(response.data);
					});
			});
		}

		this.fetchAvailableGroupPagesForDate = function (scheduleDate) {
			if (!curModule)
				throw 'please set module first.';

			return $q(function (resolve, reject) {
				$http.get(moduleMap[curModule],
					{
						params: {
							date: scheduleDate
						}
					}).then(function (response) {
						resolve(response.data);
					});
			});
		}
		this.setModule = function (module) {
			curModule = module;
		}
	}
})();