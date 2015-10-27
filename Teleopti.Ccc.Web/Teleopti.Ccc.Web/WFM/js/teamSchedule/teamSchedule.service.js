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

		this.loadSchedules = $resource('../api/TeamSchedule/Group', {
			groupId: "@groupId",
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