(function() {
	'use strict';
	angular.module('wfm.intraday')
		.service('intradayStaffingService', [
			'$resource', 'intradayService', '$q',
			function($resource, intradayService, $q) {


				this.resourceCalculate = $resource('../ForecastAndStaffingForSkill', {
					date: '@date',
					skillId: '@skillId'
				}, {
					query: {
						method: 'GET',
						params: {
							date: name,
							skillId: name
						},
						isArray: true
					}
				});
				this.resourceCalculateForArea = $resource('../ForecastAndStaffingForSkillArea', {
					date: '@date',
					skillAreaId: '@skillAreaId'
				}, {
					query: {
						method: 'GET',
						params: {
							date: name,
							skillAreaId: name
						},
						isArray: true
					}
				});
				this.TriggerResourceCalculate = $resource('../TriggerResourceCalculate', {}, {
					query: {
						method: 'GET',
						params: {},
						isArray: false
					}
				});

				this.matchSkill = function(id) {
					var deferred = $q.defer();
					intradayService.getSkills.query().$promise.then(function(response) {
						var matched;
						response.forEach(function(skill) {
							if (skill.Id === id) {
								matched = skill
							}
						})
						deferred.resolve(matched);
					})
					return deferred.promise;
				};
			}
		]);
})();
