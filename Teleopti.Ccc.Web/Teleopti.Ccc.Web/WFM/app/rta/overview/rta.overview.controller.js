
(function () {
	'use strict';

	angular.module('wfm.rta').controller('RtaOverviewCtrl', [
		'$scope',
		'$stateParams',
		'$interval',
		'$filter',
		'$sessionStorage',
		'$state',
		'$translate',
		'RtaOrganizationService',
		'RtaService',
		'RtaRouteService',
		'RtaFormatService',
		'RtaAdherenceService',
		'NoticeService',		
		function (
			$scope,
			$stateParams,
			$interval,
			$filter,
			$sessionStorage,
			$state,
			$translate,
			RtaOrganizationService,
			RtaService,
			RtaRouteService,
			RtaFormatService,
			RtaAdherenceService,
			NoticeService
			) {

			$scope.selectedItemIds = [];
			$scope.siteId = $stateParams.siteId || null;
			$scope.getAdherencePercent = RtaFormatService.numberToPercent;

			RtaOrganizationService.getSiteName($scope.siteId)
				.then(function(name) {
					$scope.siteName = name;
				});

			if ($state.siteId !== null) {
				var message = $translate.instant('WFMReleaseNotification')
					.replace('{0}', 'RTA')
					.replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx'>")
					.replace('{2}', '</a>')
					.replace('{3}', "<a href='../Anywhere#realtimeadherencesites'>RTA</a>");
				NoticeService.info(message, null, true);
			}

			var polling = $interval(function () {
				if ($scope.siteId) {
					RtaService.getAdherenceForTeamsOnSite({
						siteId: $scope.siteId
					}).then(function (teamAdherence) {
						RtaAdherenceService.updateAdherence($scope.teams, teamAdherence);
					});
				} else {
					RtaService.getAdherenceForAllSites()
						.then(function (siteAdherences) {
							RtaAdherenceService.updateAdherence($scope.sites, siteAdherences);
						});
				}
			}, 5000);

			if ($scope.siteId) {
				RtaService.getTeams({
					siteId: $scope.siteId
				}).then(function (teams) {
					$scope.teams = teams;
					return RtaService.getAdherenceForTeamsOnSite({
						siteId: $scope.siteId
					});
				}).then(function (teamAdherence) {
					RtaAdherenceService.updateAdherence($scope.teams, teamAdherence);
				});
			} else {
				RtaService.getSites().then(function (sites) {
					$scope.sites = sites;
					return RtaService.getAdherenceForAllSites();
				}).then(function (siteAdherences) {
					RtaAdherenceService.updateAdherence($scope.sites, siteAdherences);
				});
			}

			$scope.toggleSelection = function (itemId) {
				var index = $scope.selectedItemIds.indexOf(itemId);
				if (index > -1) {
					$scope.selectedItemIds.splice(index, 1);
				} else {
					$scope.selectedItemIds.push(itemId);
				}
			}

			$scope.openSelectedTeams = function () {
				if ($scope.selectedItemIds.length > 0)
					$state.go('rta.agents-teams', { teamIds: $scope.selectedItemIds });
			}

			$scope.openSelectedSites = function () {
				if ($scope.selectedItemIds.length > 0)
					$state.go('rta.agents-sites', { siteIds: $scope.selectedItemIds });
			};

			$scope.goToSkillSelection = function () {
				$state.go('rta.select-skill');
			}

			$scope.goBackWithUrl = function () {
				return RtaRouteService.urlForSites();
			};

			$scope.$watch(
				function () {
					return $sessionStorage.buid;
				},
				function (newValue, oldValue) {
					if (oldValue !== undefined && newValue !== oldValue) {
						RtaRouteService.goToSites();
					}
				}
			);

			$scope.$on('$destroy', function () {
				$interval.cancel(polling);
			});
		}
	]);
})();
