(function() {
	'use strict';

	angular.module('wfm.rta').service('RtaService', ['$resource',
		function($resource) {

			this.getAgentsForSites = function(data) {
				return $resource('../Agents/ForSites', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					siteIds: data.siteIds,
				}).$promise;
			};

			this.getAgentsForTeams = function(data) {
				return $resource('../Agents/ForTeams', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					teamIds: data.teamIds,
				}).$promise;
			};

			this.getStatesForSites = function(data) {
				return $resource('../Agents/GetStatesForSites', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					siteIds: data.siteIds,
				}).$promise;
			};

			this.getStatesForTeams = function(data) {
				return $resource('../Agents/GetStatesForTeams', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					teamIds: data.teamIds,
				}).$promise;
			};

			this.getSites = $resource('../Sites', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			});

			this.getTeams = $resource('../Teams/ForSite', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			});

			this.getAdherenceForTeamsOnSite = $resource('../Teams/GetOutOfAdherenceForTeamsOnSite', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			});

			this.getAdherenceForAllSites = $resource('../Sites/GetOutOfAdherenceForAllSites', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			});

			this.forToday = $resource('../Adherence/ForToday', {}, {
				query: {
					method: 'GET',
					isArray: false
				}
			});

			this.getPersonDetails = $resource('../Agents/PersonDetails', {}, {
				query: {
					method: 'GET',
					isArray: false
				}
			});

			this.getAdherenceDetails = $resource('../Adherence/ForDetails', {}, {
				query: {
					method: 'GET',
					isArray: true
				}
			});
		}
	]);
})();
