'use strict';

angular.module('teamScheduleService', ['ngResource']).service('TeamSchedule', [
	'$resource', '$http', function($resource, $http) {
		this.loadAllTeams = $resource('../api/GroupPage/AllTeams', {
			date: "@queryDate"
		}, {
			query: {
				method: 'GET',
				params: {},
				isArray: true
			}
		});
	}
]);