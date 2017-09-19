(function () {
	'use strict';
	angular.module('skillGroupService', ['ngResource'])
		.factory('SkillGroupSvc', SkillGroupSvc);

	SkillGroupSvc.$inject = ['$resource'];

	function SkillGroupSvc($resource) {
		return {
			createSkillGroup: $resource(
				'../api/intraday/skillarea',
				{},
				{
					query: {
						method: 'POST',
						params: {},
						isArray: false
					}
				}
			),

			getSkillGroups: $resource(
				'../api/intraday/skillarea',
				{},
				{
					query: {
						method: 'GET',
						params: {},
						isArray: false,
						cancellable: true
					}
				}
			),

			deleteSkillGroup: $resource(
				'../api/intraday/skillarea/:id',
				{id: '@id'},
				{
					remove: {
						method: 'DELETE',
						params: {},
						isArray: false
					}
				}
			),

			getSkillGroupMonitorStatistics: $resource(
				'../api/intraday/monitorskillareastatistics/:id',
				{id: '@id'},
				{
					query: {
						method: 'GET',
						params: {},
						isArray: false,
						cancellable: true
					}
				}
			)
		}
	}
})();
