(function() {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('RtaService', RtaService);

	RtaService.$inject = ['RtaResourceFactory', '$q'];

	function RtaService($resource, $q) {

		var service = {
			getSkills: getSkills,
			getSkillAreas: getSkillAreas,
			getOrganization: getOrganization,
			agentsFor: agentsFor,
			statesFor: statesFor,
			inAlarmFor: inAlarmFor,
			inAlarmExcludingPhoneStatesFor: inAlarmExcludingPhoneStatesFor,
			getAlarmStatesForSitesAndSkillsExcludingStates: getAlarmStatesForSitesAndSkillsExcludingStates,
			getAlarmStatesForTeamsAndSkillsExcludingStates: getAlarmStatesForTeamsAndSkillsExcludingStates,
			getAlarmStatesForSkillsExcludingStates: getAlarmStatesForSkillsExcludingStates,
			getAlarmStatesForTeamsExcludingStates: getAlarmStatesForTeamsExcludingStates,
			getAdherenceForTeamsOnSite: getAdherenceForTeamsOnSite,
			getAdherenceForAllSites: getAdherenceForAllSites,
			getAdherenceForSitesBySkills: getAdherenceForSitesBySkills,
			getSites: getSites,
			getSitesForSkills: getSitesForSkills,
			getAdherenceForTeamsBySkills: getAdherenceForTeamsBySkills,
			getTeams: getTeams,
			getTeamsForSiteAndSkills: getTeamsForSiteAndSkills,
			getSkillName: getSkillName,
			getSkillArea: getSkillArea,
			getPhoneStates: getPhoneStates,
			forToday: forToday,
			getPersonDetails: getPersonDetails,
			getAdherenceDetails: getAdherenceDetails,
			getAgentHistoricalData: getAgentHistoricalData
		}

		return service;

		function getSkills(data) {
			return $resource('../api/Skills', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			}).query().$promise;
		};

		function getSkillAreas(data) {
			return $resource('../api/SkillAreas', {}, {
				query: {
					method: 'GET'
				}
			}).query().$promise;
		};

		function getOrganization(data) {
			return $resource('../api/Sites/Organization', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			}).query().$promise;
		};


		function agentsFor(data) {
			return $resource('../api/Agents/For', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			}).query(data).$promise;
		};

		function statesFor(data) {
			return $resource('../api/Agents/StatesFor', {}, {
				query: {
					method: 'GET'
				}
			}).query(data).$promise;
		};

		function inAlarmFor(data) {
			return $resource('../api/Agents/InAlarmFor', {}, {
				query: {
					method: 'GET'
				}
			}).query(data).$promise;
		};

		function inAlarmExcludingPhoneStatesFor(data) {
			return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
					query: {
						method: 'GET'
					}
				}).query(data)
				.$promise;
		};

		function getAlarmStatesForSitesAndSkillsExcludingStates(data) {
			return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
					query: {
						method: 'GET'
					}
				}).query(data)
				.$promise;
		};

		function getAlarmStatesForTeamsAndSkillsExcludingStates(data) {
			return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
					query: {
						method: 'GET'
					}
				}).query(data)
				.$promise;
		};

		function getAlarmStatesForSkillsExcludingStates(data) {
			return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
					query: {
						method: 'GET'
					}
				}).query(data)
				.$promise;
		};

		function getAlarmStatesForTeamsExcludingStates(data) {
			return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
					query: {
						method: 'GET'
					}
				}).query(data)
				.$promise;
		};

		function getAdherenceForTeamsOnSite(data) {
			return $resource('../api/Teams/GetOutOfAdherenceForTeamsOnSite', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			}).query({
				siteId: data.siteId
			}).$promise;
		};

		function getAdherenceForAllSites(data) {
			return $resource('../api/Sites/GetOutOfAdherenceForAllSites', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			}).query().$promise;
		};

		function getAdherenceForSitesBySkills(data) {
			return $resource('../api/Sites/InAlarmCountForSkills', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			}).query({
				skillIds: data
			}).$promise;
		};

		function getSites(data) {
			return $resource('../api/Sites', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			}).query().$promise;
		};

		function getSitesForSkills(skillIds) {
			return $resource('../api/Sites/ForSkills', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			}).query({
				skillIds: skillIds
			}).$promise;
		};

		function getAdherenceForTeamsBySkills(data) {
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

		function getTeams(data) {
			return $resource('../api/Teams/Build', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			}).query({
				siteId: data.siteId
			}).$promise;
		};

		function getTeamsForSiteAndSkills(data) {
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

		function getSkillName(data) {
			return $resource('../api/Skills/NameFor', {}, {
				query: {
					method: 'GET'
				}
			}).query({
				skillId: data
			}).$promise;
		};

		function getSkillArea(data) {
			return $resource('../api/SkillArea/For', {}, {
				query: {
					method: 'GET'
				}
			}).query({
				skillAreaId: data
			}).$promise;
		};

		function getPhoneStates(data) {
			return $resource('../api/PhoneState/InfoFor', {}, {
				query: {
					method: 'GET'
				}
			}).query({
				ids: data
			}).$promise;
		};

		function forToday(data) {
			return $resource('../api/Adherence/ForToday', {}, {
				query: {
					method: 'GET',
					isArray: false
				}
			}).query({
				personId: data.personId
			}).$promise;
		};

		function getPersonDetails(data) {
			return $resource('../api/Agents/PersonDetails', {}, {
				query: {
					method: 'GET',
					isArray: false
				}
			}).query({
				personId: data.personId
			}).$promise;
		};

		function getAdherenceDetails(data) {
			return $resource('../api/Adherence/ForDetails', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			}).query({
				personId: data.personId
			}).$promise;
		};

		function getAgentHistoricalData(id) {
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
	};
})();
