(function() {
	'use strict';
	angular.module('wfm.rta')
		.controller('RtaSitesCtrl', [
			'$scope', '$filter', '$state', '$stateParams', '$interval', 'RtaService', 'RtaOrganizationService', 'RtaFormatService', 'NoticeService', 'Toggle',
			function($scope, $filter, $state, $stateParams, $interval, RtaService, RtaOrganizationService, RtaFormatService, NoticeService, toggleService) {

				$scope.underToggle = toggleService.RTA_MonitorBySkills_39081;

				$scope.rightPanelOptions = {
					panelState: false,
					panelTitle: "Show agents by skill",
					showCloseButton: true,
					showBackdrop: true,
					showResizer: true,
					showPopupButton: true
				};
				$scope.ifVisible = function() {
					return toggleService.RTA_MonitorBySkills_39081;
				}();

				$scope.getAdherencePercent = RtaFormatService.numberToPercent;
				$scope.checkboxesChecked = 0;
				var selectedSiteIds = [];
				var message = "RTA has been improved! We appreciate your <a href=' http://www.teleopti.com/wfm/customer-feedback.aspx'>feedback.</a>\
				Old <a href='../Anywhere#realtimeadherencesites'>RTA</a> can be accessed for a limited time.";

				NoticeService.info(message, null, true);

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
					$state.go('rta.teams', {
						siteId: site.Id
					});
				};

				$scope.openSelectedSites = function() {
					if (selectedSiteIds.length === 0) return;
					$state.go('rta.agents-sites', {
						siteIds: selectedSiteIds
					});
				};

				$scope.goToAgents = function() {
					$state.go('rta.agents-view');
					console.log($state);
				}
				$scope.$on('$destroy', function() {
					$interval.cancel(polling);
				});
			}
		]);
})();
