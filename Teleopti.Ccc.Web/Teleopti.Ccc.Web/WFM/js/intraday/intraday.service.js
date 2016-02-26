(function () {
	'use strict';
	angular.module('wfm.intraday')
		.service('intradayService', [
			'$resource', function ($resource) {
				this.skillList = $resource('../api/intraday/skillstatus', {}, {
					query: {
						method: 'GET',
						params: {},
						isArray: true
					}
				});

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

				this.getMonitorData = $resource('../api/intraday/monitorskillarea/:id', { id: '@id' }, {
					query: {
						method: 'GET',
						params: {},
						isArray: false
					}
				});

				this.formatDateTime = function (time) {
					if (time === null || time === undefined || time === '') return '--:--:--';
					var momentTime = moment(time);
					if (momentTime.format("YYYY") > moment("1970").format("YYYY")) {
						return momentTime.format('HH:mm:ss');
					} else {
						return '--:--:--';
					}
				};
			}
		]);
})();