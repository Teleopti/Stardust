(function() {
	'use strict';
	angular.module('wfm.rta')
		.controller('RtaCtrl', [
			'$scope', '$filter', '$state', '$stateParams', '$interval', 'RtaService', 'RtaOrganizationService',
			function($scope, $filter, $state, $stateParams, $interval, RtaService, RtaOrganizationService) {

				var selectedSiteIds = [];

				var updateAdherence = function(siteAdherence) {
					siteAdherence.forEach(function(site) {
						var filteredSite = $filter('filter')($scope.sites, {
							Id: site.Id
						});
						if (filteredSite.length > 0)
							filteredSite[0].OutOfAdherence = site.OutOfAdherence ? site.OutOfAdherence : 0;
					})
				};

				RtaService.getSites.query()
					.$promise.then(function(sites) {
						$scope.sites = sites;
						return RtaService.getAdherenceForAllSites.query().$promise;
					}).then(function(siteAdherence) {
						updateAdherence(siteAdherence);
						$interval(function() {
							RtaService.getAdherenceForAllSites.query()
								.$promise.then(updateAdherence);
						}, 5000);
					});

				$scope.onSiteSelect = function(site) {
					$state.go('rta-teams', {
						siteId: site.Id
					});
				};

				$scope.toggleSelection = function(siteId) {
					var index = selectedSiteIds.indexOf(siteId);
					if (index > -1) {
						selectedSiteIds.splice(index, 1);
					} else {
						selectedSiteIds.push(siteId);
					}
				};

				$scope.openSelectedSites = function() {
					$state.go('rta-agents-sites-selected', {
						siteIds: selectedSiteIds
					});
				};

			}
		]);
})();
