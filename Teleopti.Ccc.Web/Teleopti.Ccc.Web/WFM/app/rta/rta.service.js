(function () {
	'use strict';

	angular.module('wfm.rta').service('RtaService', ['RtaResourceFactory', '$q',
		function ($resource, $q) {

			this.getAgentsForSites = function (data) {
				return $resource('../api/Agents/ForSites', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					siteIds: data.siteIds,
				}).$promise;
			};

			this.getSkills = function (data) {
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
				return $resource('../api/Agents/ForTeams', {}, {
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

			this.getStatesForSites = function (data) {
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

			this.getAlarmStatesForSitesExcludingStates = function (data) {
				return $resource('../api/Agents/GetAlarmStatesForSitesExcludingStates', {}, {
					query: {
						method: 'GET'
					}
				}).query({ ids: data.siteIds, excludedStateIds: data.excludedStateIds })
					.$promise;
			};

			this.getAlarmStatesForSkills = function (data) {
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

			this.getAlarmStatesForSkillsExcludingStates = function (data) {
				return $resource('../api/Agents/GetAlarmStatesForSkillsExcludingStates',
					{},
					{
						query: {
							method: 'GET'
						}
					})
					.query({
						ids: data.skillIds, excludedStateIds: data.excludedStateIds
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

			this.getAlarmStatesForTeams = function (data) {
				return $resource('../api/Agents/GetAlarmStatesForTeams', {}, {
					query: {
						method: 'GET'
					}
				}).query({ ids: data.teamIds })
					.$promise;
			};

			this.getAlarmStatesForTeamsExcludingStates = function (data) {
				return $resource('../api/Agents/GetAlarmStatesForTeamsExcludingStates', {}, {
					query: {
						method: 'GET'
					}
				}).query({ ids: data.teamIds, excludedStateIds: data.excludedStateIds })
					.$promise;
			};

			this.getAdherenceForTeamsOnSite = function (data) {
				return $resource('../api/Teams/GetOutOfAdherenceForTeamsOnSite', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					siteId: data.siteId
				}).$promise;
			};

			this.getAdherenceForAllSites = function (data) {
				return $resource('../api/Sites/GetOutOfAdherenceForAllSites', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query().$promise;
			};

			this.getAdherenceForAllSitesBySkill = function (data) {
				return $resource('../api/Sites/InAlarmCountForSkills', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					skillIds: data
				}).$promise;
			};

			this.getSites = function (data) {
				return $resource('../api/Sites', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query().$promise;
			};

			this.getSitesForSkill = function (skillIds) {
				return $resource('../api/Sites/ForSkills', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					skillIds: skillIds
				}).$promise;
			};

			this.getAdherenceForAllTeamsOnSitesBySkill = function (data) {
				return $resource('../api/Teams/InAlarmCountForSkills', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					skillIds: data.skillIds,
					siteId: data.siteIds
				}).$promise;
			};

			this.getTeams = function (data) {
				return $resource('../api/Teams/Build', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					siteId: data.siteId
				}).$promise;
			};

			this.getTeamsForSitesAndSkill = function (data) {
				return $resource('../api/Teams/ForSkills', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					skillIds: data.skillIds,
					siteId: data.siteIds
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

			this.getPhoneStates = function (data) {
				return $resource('../api/PhoneState/InfoFor', {}, {
					query: {
						method: 'GET'
					}
				}).query({
					ids: data
				}).$promise;
			};

			this.forToday = function (data) {
				return $resource('../api/Adherence/ForToday', {}, {
					query: {
						method: 'GET',
						isArray: false
					}
				}).query({
					personId: data.personId
				}).$promise;
			};

			this.getPersonDetails = function (data) {
				return $resource('../api/Agents/PersonDetails', {}, {
					query: {
						method: 'GET',
						isArray: false
					}
				}).query({
					personId: data.personId
				}).$promise;
			};

			this.getAdherenceDetails = function (data) {
				return $resource('../api/Adherence/ForDetails', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					personId: data.personId
				}).$promise;
			};

			this.getAgentHistoricalData = function (id) {
				return $resource('../api/HistoricalAdherence/For', {}, {
					query: {
						method: 'GET',
						isArray: false
					}
				}).query({
					personId: id
				}).$promise;
			};
			//
			// this.getAgentHistoricalData = function (id) {
			// 	return $q(function(resolve) {
			// 		resolve({
			// 			Name: 'Mikkey Dee',
			// 			Schedule: [{
			// 				Color: 'lightgreen',
			// 				StartTime: '2016-10-10 08:00:00',
			// 				EndTime: '2016-10-10 09:00:00'
			// 			}, {
			// 				Color: 'red',
			// 				StartTime: '2016-10-10 09:00:00',
			// 				EndTime: '2016-10-10 09:30:00'
			// 			}, {
			// 				Color: 'lightgreen',
			// 				StartTime: '2016-10-10 09:30:00',
			// 				EndTime: '2016-10-10 11:00:00'
			// 			}, {
			// 				Color: 'yellow',
			// 				StartTime: '2016-10-10 11:00:00',
			// 				EndTime: '2016-10-10 12:00:00'
			// 			}, {
			// 				Color: 'lightgreen',
			// 				StartTime: '2016-10-10 12:00:00',
			// 				EndTime: '2016-10-10 14:00:00'
			// 			}, {
			// 				Color: 'red',
			// 				StartTime: '2016-10-10 14:00:00',
			// 				EndTime: '2016-10-10 15:00:00'
			// 			}, {
			// 				Color: 'lightgreen',
			// 				StartTime: '2016-10-10 15:00:00',
			// 				EndTime: '2016-10-10 17:00:00'
			// 			}],
			// 			OutOfAdherences: [{
			// 				StartTime: '2016-10-10 08:00:00',
			// 				EndTime: '2016-10-10 08:03:00'
			// 			}, {
			// 				StartTime: '2016-10-10 08:10:00',
			// 				EndTime: '2016-10-10 08:15:00'
			// 			}, {
			// 				StartTime: '2016-10-10 11:58:00',
			// 				EndTime: '2016-10-10 12:00:00'
			// 			}, {
			// 				StartTime: '2016-10-10 12:00:00',
			// 				EndTime: '2016-10-10 12:05:00'
			// 			}, {
			// 				StartTime: '2016-10-10 16:00:00',
			// 				EndTime: '2016-10-10 16:05:00'
			// 			}]
			// 		})
			// 	});
			// };
		}
	]);
})();
