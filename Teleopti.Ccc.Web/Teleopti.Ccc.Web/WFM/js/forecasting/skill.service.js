'use strict';
angular.module('wfm.forecasting')
	.service('SkillService', [
		'$resource', function ($resource) {
			this.activities = $resource('../api/Skill/Activities', {}, {
				get: { method: 'GET', params: {}, isArray: true }
			});

			this.timezones = $resource('../api/Skill/Timezones', {}, {
				get: { method: 'GET', params: {}, isArray: false }
			});

			this.queues = $resource('../api/Skill/Queues', {}, {
				get: { method: 'GET', params: {}, isArray: true }
			});

			this.skill = $resource('../api/Skill/Create', {}, {
				create: { method: 'POST', params: {}, isArray: false }
			});
		}
	]);
