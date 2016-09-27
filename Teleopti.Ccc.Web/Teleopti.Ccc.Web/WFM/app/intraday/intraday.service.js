(function () {
	'use strict';
	angular.module('wfm.intraday')
		.service('intradayService', [
			'$resource', function ($resource) {

				this.getSkills = $resource('../api/intraday/skills', {}, {
					query: {
						method: 'GET',
						params: {},
						isArray: true
					}
				});

				this.createSkillArea = $resource('../api/intraday/skillarea', {}, {
					query: {
						method: 'POST',
						params: {},
						isArray: false
					}
				});

				this.getSkillAreas = $resource('../api/intraday/skillarea', {}, {
					query: {
						method: 'GET',
						params: {},
						isArray: false
					}
				});

				this.deleteSkillArea = $resource('../api/intraday/skillarea/:id', {id:'@id'}, {
					remove: {
						method: 'DELETE',
						params: {},
						isArray: false
					}
				});

				this.getSkillAreaMonitorStatistics = $resource('../api/intraday/monitorskillareastatistics/:id', { id: '@id' }, {
					query: {
						method: 'GET',
						params: {},
						isArray: false
					}
				});

				this.getSkillMonitorStatistics = $resource('../api/intraday/monitorskillstatistics/:id', { id: '@id' }, {
					query: {
						method: 'GET',
						params: {},
						isArray: false
					}
				});

				this.getSkillAreaStaffingData = $resource('../api/intraday/monitorskillareastaffing/:id', { id: '@id' }, {
					query: {
						method: 'GET',
						params: {},
						isArray: false
					}
				});

				this.getSkillStaffingData = $resource('../api/intraday/monitorskillstaffing/:id', { id: '@id' }, {
					query: {
						method: 'GET',
						params: {},
						isArray: false
					}
				});

				this.getLatestStatisticsTimeForSkillArea = $resource('../api/intraday/lateststatisticstimeforskillarea/:id', { id: '@id' }, {
					query: {
						method: 'GET',
						params: {},
						isArray: false
					}
				});

				this.getLatestStatisticsTimeForSkill = $resource('../api/intraday/lateststatisticstimeforskill/:id', { id: '@id' }, {
					query: {
						method: 'GET',
						params: {},
						isArray: false
					}
				});

			}
		]);
})();
