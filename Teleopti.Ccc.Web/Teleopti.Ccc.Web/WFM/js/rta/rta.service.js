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
				return $resource('../api/BusinessUnit/Skills', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					teamIds: data,
				}).$promise;
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

			this.getAgentsForSkill = function (data) {
				return $resource('../api/Agents/ForSkill', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					skillId: data.skillId,
				}).$promise;
			};

			this.getStatesForSkill = function (data) {
				return $resource('../api/Agents/GetStatesForSkill', {}, {
					query: {
						method: 'GET'
					}
				}).query({
					skillId: data.skillId
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

			this.getAlarmStatesForSkill = function (data) {
				return $resource('../api/Agents/GetAlarmStatesForSkill', {}, {
					query: {
						method: 'GET'
					}
				}).query({
					skillId: data.skillId
				}).$promise;
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
				return $resource('../api/Skill/NameFor', {}, {
					query: {
						method: 'GET'
					}
				}).query({
					skillId: data
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
