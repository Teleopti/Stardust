(function () {
	'use strict';
	angular.module('skillGroupService', ['ngResource'])
		.factory('SkillGroupSvc', SkillGroupSvc);

	SkillGroupSvc.$inject = ['$resource'];

	function SkillGroupSvc($resource) {
		return {
			getSkills: $resource(
				'../api/intraday/skills',
				{},
				{
					query: {
						method: 'GET',
						params: {},
						isArray: true,
                        cancellable: true
					}
				}
			),
			createSkillGroup: $resource(
				'../api/skillgroup/create',
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
				'../api/skillgroup/skillgroups',
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
				'../api/skillgroup/delete/:id',
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
