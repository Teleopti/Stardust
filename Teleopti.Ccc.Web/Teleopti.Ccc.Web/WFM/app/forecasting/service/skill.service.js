(function () {
	'use strict';
	angular.module('wfm.forecasting')
		.service('SkillService', ['$resource', skillService]);

	function skillService($resource) {
		this.activities = $resource('../api/Skill/Activities', {}, {
			get: { method: 'GET', params: {}, isArray: true }
		});

		this.timezones = $resource('../api/Global/TimeZone', {}, {
			get: { method: 'GET', params: {}, isArray: false }
		});

		this.queues = $resource('../api/Skill/Queues', {}, {
			get: { method: 'GET', params: {}, isArray: true }
		});

		this.skill = $resource('../api/Skill/Create', {}, {
			create: { method: 'POST', params: {}, isArray: false }
		});
	}
})();