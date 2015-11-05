(function() {

	'use strict';

	angular.module('wfm.rta').service('RtaService', ['$resource',
		function($resource) {

			this.getAgents = $resource('../Agents/ForTeam?teamId=:teamId', {
				teamId: '@teamId'
			}, {
				query: {
					method: 'GET',
					params: {},
					isArray: true
				}
			});

			this.getAgentsForSites = $resource('../Agents/ForSites', {}, {
				query: {
					method: 'GET',
					params: {
						siteIds: []
					},
					isArray: true
				}
			});

			this.getStates = $resource('../Agents/GetStates?teamId=:teamId', {
				teamId: '@teamId'
			}, {
				query: {
					method: 'GET',
					params: {},
					isArray: true
				}
			});

			this.getStatesForSites = $resource('../Agents/GetStatesForSites', {}, {
				query: {
					method: 'GET',
					params: {
						siteIds: []
					},
					isArray: true
				}
			});

			this.getStatesForTeams = $resource('../Agents/GetStatesForTeams', {}, {
				query: {
					method: 'GET',
					params: {
						teamIds: []
					},
					isArray: true
				}
			});

			this.getAgentsForTeams = $resource('../Agents/ForTeams', {}, {
				query: {
					method: 'GET',
					params: {
						teamIds: []
					},
					isArray: true
				}
			});

			this.getSites = $resource('../Sites', {}, {
				query: {
					method: 'GET',
					params: {},
					isArray: true
				}
			});

			this.getTeams = $resource('../Teams/ForSite?siteId=:siteId', {
				siteId: '@siteId'
			}, {
				query: {
					method: 'GET',
					params: {},
					isArray: true
				}
			});

			this.getAdherenceForTeamsOnSite = $resource('../Teams/GetOutOfAdherenceForTeamsOnSite?siteId=:siteId', {
				siteId: '@siteId'
			}, {
				query: {
					method: 'GET',
					params: {},
					isArray: true
				}
			});

			this.getAdherenceForAllSites = $resource('../Sites/GetOutOfAdherenceForAllSites', {}, {
				query: {
					method: 'GET',
					params: {},
					isArray: true
				}
			});

			this.forToday = $resource('../Adherence/ForToday?personId=:personId', {
				personId: '@personId'
			}, {
				query: {
					method: 'GET',
					params: {},
					isArray: false
				}
			});

			this.getPersonDetails = $resource('../Agents/PersonDetails?personId=:personId', {
				personId: '@personId'
			}, {
				query: {
					method: 'GET',
					params: {},
					isArray: false
				}
			});

			this.getAdherenceDetails = $resource('../Adherence/ForDetails?personId=:personId', {
				personId: '@personId'
			}, {
				query: {
					method: 'GET',
					params: {},
					isArray: true
				}
			});
		}
	]);
})();
