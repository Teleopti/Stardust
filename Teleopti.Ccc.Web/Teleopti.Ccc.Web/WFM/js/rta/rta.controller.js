(function() {
	'use strict';
	angular.module('wfm.rta')
		.controller('RtaCtrl', [
			'$scope', '$filter', '$state', '$stateParams', '$interval', 'RtaService', 'RtaOrganizationService', 'RtaFormatService',
			function($scope, $filter, $state, $stateParams, $interval, RtaService, RtaOrganizationService, RtaFormatService) {

				$scope.getAdherencePercent = RtaFormatService.numberToPercent;
				$scope.checkboxesChecked = 0;
				var selectedSiteIds = [];

				var polling = $interval(function() {
					RtaService.getAdherenceForAllSites().then(updateAdherence);
				}, 5000);

				RtaService.getSites().then(function(sites) {
					$scope.sites = sites;
					return RtaService.getAdherenceForAllSites();
				}).then(function(siteAdherence) {
					updateAdherence(siteAdherence);
				});

				function updateAdherence(siteAdherence) {
					siteAdherence.forEach(function(site) {
						var filteredSite = $filter('filter')($scope.sites, {
							Id: site.Id
						});
						if (filteredSite.length > 0)
							filteredSite[0].OutOfAdherence = site.OutOfAdherence ? site.OutOfAdherence : 0;
					});
				};

				$scope.toggleSelection = function(siteId) {
					var index = selectedSiteIds.indexOf(siteId);
					if (index > -1) {
						selectedSiteIds.splice(index, 1);
						$scope.checkboxesChecked--;
					} else {
						selectedSiteIds.push(siteId);
						$scope.checkboxesChecked++;
					}
				};

				$scope.onSiteSelect = function(site) {
					if (site.NumberOfAgents < 1)
						return;
					$state.go('rta-teams', {
						siteId: site.Id
					});
				};

				$scope.openSelectedSites = function() {
					if (selectedSiteIds.length === 0) return;
					$state.go('rta-agents-sites', {
						siteIds: selectedSiteIds
					});
				};

				$scope.$on('$destroy', function() {
					$interval.cancel(polling);
				});
			}
		]);
})();
