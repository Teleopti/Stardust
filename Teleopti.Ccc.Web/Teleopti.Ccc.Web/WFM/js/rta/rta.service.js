(function() {
	'use strict';

	angular.module('wfm.rta').service('RtaService', ['RtaResourceFactory',
		function($resource) {

			this.getAgentsForSites = function(data) {
				return $resource('../api/Agents/ForSites', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					siteIds: data.siteIds,
				}).$promise;
			};

			this.getSkills = function(data) {
				return $resource('../api/Skills', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query().$promise;
			};

			this.getSkillAreas = function (data) {
				return $resource('../api/SkillAreas', {}, {
					query: {
						method: 'GET'
					}
				}).query().$promise;
			};

			this.getAgentsForTeams = function (data) {
				return $resource('../api/Agents/ForTeams',{},{
						query: {
							method: 'GET',
							isArray: true
						}
					}).query({
						teamIds: data.teamIds,
					}).$promise;
			};

			this.getAgentsForSkills = function (data) {
				return $resource('../api/Agents/ForSkills', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					skillIds: data.skillIds,
				}).$promise;
			};

			this.getStatesForSkills = function (data) {
				return $resource('../api/Agents/GetStatesForSkills', {}, {
					query: {
						method: 'GET'
					}
				}).query({
					ids: data.skillIds
				}).$promise;
			};

			this.getStatesForSites = function(data) {
				return $resource('../api/Agents/GetStatesForSites', {}, {
					query: {
						method: 'GET'
					}
				}).query({
					ids: data.siteIds
				}).$promise;
			};

			this.getAlarmStatesForSites = function (data) {
				return $resource('../api/Agents/GetAlarmStatesForSites', {}, {
					query: {
						method: 'GET'
					}
				}).query({
					ids: data.siteIds
				}).$promise;
			};

			this.getAlarmStatesForSitesExcludingStateGroups = function(data) {
				return $resource('../api/Agents/GetAlarmStatesForSitesExcludingStateGroups', {}, {
						query: {
							method: 'POST'
						}
					}).query({ ids: data.siteIds, excludedStateGroupIds: data.excludedStateGroupIds })
					.$promise;
			};

			this.getAlarmStatesForSkills = function(data) {
				return $resource('../api/Agents/GetAlarmStatesForSkills',
					{},
					{
						query: {
							method: 'GET'
						}
					})
					.query({
						ids: data.skillIds
					})
					.$promise;
			};

			this.getAlarmStatesForSkillsExcludingStateGroups = function(data) {
				return $resource('../api/Agents/GetAlarmStatesForSkillsExcludingStateGroups',
					{},
					{
						query: {
							method: 'POST'
						}
					})
					.query({
						ids: data.skillIds, excludedStateGroupIds: data.excludedStateGroupIds
					})
					.$promise;
			};

			this.getStatesForTeams = function (data) {
				return $resource('../api/Agents/GetStatesForTeams', {}, {
						query: {
							method: 'GET'
						}
					}).query({ ids: data.teamIds })
					.$promise;
			};

			this.getAlarmStatesForTeams = function(data) {
				return $resource('../api/Agents/GetAlarmStatesForTeams', {}, {
						query: {
							method: 'GET'
						}
					}).query({ ids: data.teamIds })
					.$promise;
			};

			this.getAlarmStatesForTeamsExcludingStateGroups = function(data) {
				return $resource('../api/Agents/GetAlarmStatesForTeamsExcludingStateGroups', {}, {
						query: {
							method: 'POST'
						}
					}).query({ ids: data.teamIds, excludedStateGroupIds: data.excludedStateGroupIds })
					.$promise;
			};

			this.getAdherenceForTeamsOnSite = function(data) {
				return $resource('../api/Teams/GetOutOfAdherenceForTeamsOnSite', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					siteId: data.siteId
				}).$promise;
			};

			this.getAdherenceForAllSites = function(data) {
				return $resource('../api/Sites/GetOutOfAdherenceForAllSites', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query().$promise;
			};

			this.getSites = function(data) {
				return $resource('../api/Sites', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query().$promise;
			};

			this.getTeams = function(data) {
				return $resource('../api/Teams/Build', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					siteId: data.siteId
				}).$promise;
			};

			this.getSkillName = function (data) {
				return $resource('../api/Skills/NameFor', {}, {
					query: {
						method: 'GET'
					}
				}).query({
					skillId: data
				}).$promise;
			};

			this.getSkillArea = function (data) {
				return $resource('../api/SkillArea/For', {}, {
					query: {
						method: 'GET'
					}
				}).query({
					skillAreaId: data
				}).$promise;
			};

			this.forToday = function(data) {
				return $resource('../api/Adherence/ForToday', {}, {
					query: {
						method: 'GET',
						isArray: false
					}
				}).query({
					personId: data.personId
				}).$promise;
			};

			this.getPersonDetails = function(data) {
				return $resource('../api/Agents/PersonDetails', {}, {
					query: {
						method: 'GET',
						isArray: false
					}
				}).query({
					personId: data.personId
				}).$promise;
			};

			this.getAdherenceDetails = function(data) {
				return $resource('../api/Adherence/ForDetails', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					personId: data.personId
				}).$promise;
			};
		}
	]);
})();
