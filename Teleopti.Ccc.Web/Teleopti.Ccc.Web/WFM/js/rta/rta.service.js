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

			this.getAgentsForTeams = function(data) {
				return $resource('../api/Agents/ForTeams', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					teamIds: data.teamIds,
				}).$promise;
			};

			this.getStatesForSites = function(data) {
				return $resource('../api/Agents/GetStatesForSites', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					ids: data.siteIds,
					inAlarmOnly: data.inAlarmOnly
				}).$promise;
			};

			this.getStatesForTeams = function(data) {
				return $resource('../api/Agents/GetStatesForTeams', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					ids: data.teamIds,
					inAlarmOnly: data.inAlarmOnly
				}).$promise;
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
				return $resource('../api/Teams/ForSite', {}, {
					query: {
						method: 'GET',
						isArray: true
					}
				}).query({
					siteId: data.siteId
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
