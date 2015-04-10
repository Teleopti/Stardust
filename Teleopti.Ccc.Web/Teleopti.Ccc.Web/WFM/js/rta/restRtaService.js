(function () {
	angular.module('restRtaService', ['ngResource']).service('RtaSvrc', [
		'$resource', function ($resource) {
			this.getSites = $resource('../Sites', {}, {
				query: { method: 'GET', params: {}, isArray: true }
			});
			this.getAdherenceForAllSites = $resource('../Sites/GetOutOfAdherenceForAllSites', {}, {
				query: {
					method: 'GET',
					headers: { 'X-Business-Unit-Filter': '928dd0bc-bf40-412e-b970-9b5e015aadea' },
					params: {}, isArray: true
				}
			});
		}
	]);
})();