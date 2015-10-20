'use strict';

angular.module('teamScheduleService', ['ngResource']).service('TeamSchedule', [
	'$resource', '$http', function($resource, $http) {
		this.loadAailableGroupPages = $resource('../api/GroupPage/AllAvailable', {
			date: "@queryDate"
		}, {
			query: {
				method: 'GET',
				params: {},
				isArray: false
			}
		});
	}
]);