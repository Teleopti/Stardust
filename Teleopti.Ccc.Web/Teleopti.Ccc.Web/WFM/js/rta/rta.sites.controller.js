(function() {
	'use strict';
	angular.module('wfm.rta')
		.controller('RtaSitesCtrl', [
			'$scope',
			'$filter',
			'$state',
			'$interval',
			'$translate',
			'RtaService',
			'RtaFormatService',
			'NoticeService',
			'RtaSelectionService',
			'Toggle',
			function (
				$scope,
				$filter,
				$state,
				$interval,
				$translate,
				RtaService,
				RtaFormatService,
				NoticeService,
				RtaSelectionService,
				toggleService
				) {

				$scope.monitorBySkill = toggleService.RTA_MonitorBySkills_39081;
				$scope.getAdherencePercent = RtaFormatService.numberToPercent;
				$scope.selectedSiteIds = [];
				var message = $translate.instant('WFMReleaseNotification')
					.replace('{0}', 'RTA')
					.replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx'>")
					.replace('{2}', '</a>')
					.replace('{3}', "<a href='../Anywhere#realtimeadherencesites'>RTA</a>");

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

				$scope.toggleSelection = function (siteId) {
					$scope.selectedSiteIds = RtaSelectionService.toggleSelection(siteId, $scope.selectedSiteIds);
				};

				$scope.openSelectedSites = function () {
					if ($scope.selectedSiteIds.length > 0)
						RtaSelectionService.openSelection('rta.agents-sites', {siteIds: $scope.selectedSiteIds});
				};

				$scope.goToSkillSelection = function () {
					$state.go('rta.select-skill');
				}
				$scope.$on('$destroy', function() {
					$interval.cancel(polling);
				});
			}
		]);
})();
