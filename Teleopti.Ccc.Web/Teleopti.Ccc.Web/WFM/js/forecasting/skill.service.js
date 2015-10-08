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
		}
	]);
