(function () {
	'use strict';

	angular.module('wfm.rta').service('RtaService', ['RtaResourceFactory', '$q',
		function ($resource, $q) {


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

			this.getOrganization = function (data) {
				return $resource('../api/Organizations', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query().$promise;
			};


			this.agentsFor = function (data) {
				return $resource('../api/Agents/For', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query(data).$promise;
			};

			this.statesFor = function (data) {
				return $resource('../api/Agents/StatesFor', {}, {
					query: {
						method: 'GET'
					}
				}).query(data).$promise;
			};

			this.inAlarmFor = function (data) {
				return $resource('../api/Agents/InAlarmFor', {}, {
					query: {
						method: 'GET'
					}
				}).query(data).$promise;
			};


			this.inAlarmExcludingPhoneStatesFor = function (data) {
				return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
					query: {
						method: 'GET'
					}
				}).query(data)
					.$promise;
			};

			this.getAlarmStatesForSitesAndSkillsExcludingStates = function (data) {
				return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
					query: {
						method: 'GET'
					}
				}).query(data)
					.$promise;
			};


			this.getAlarmStatesForTeamsAndSkillsExcludingStates = function (data) {
				return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
					query: {
						method: 'GET'
					}
				}).query(data)
					.$promise;
			};

			this.getAlarmStatesForSkillsExcludingStates = function (data) {
				return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
					query: {
						method: 'GET'
					}
				}).query(data)
					.$promise;
			};


			this.getAlarmStatesForTeamsExcludingStates = function (data) {
				return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
					query: {
						method: 'GET'
					}
				}).query(data)
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

			this.getAdherenceForSitesBySkills = function (data) {
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

			this.getSitesForSkills = function (skillIds) {
				return $resource('../api/Sites/ForSkills', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					skillIds: skillIds
				}).$promise;
			};

			this.getAdherenceForTeamsBySkills = function (data) {
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

			this.getTeamsForSiteAndSkills = function (data) {
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
