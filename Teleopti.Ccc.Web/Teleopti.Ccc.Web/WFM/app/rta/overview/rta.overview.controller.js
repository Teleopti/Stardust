
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
		'Toggle',
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
			NoticeService,
			toggleService
			) {

			$scope.selectedItemIds = [];
			$scope.siteId = $stateParams.siteId || null;
			$scope.getAdherencePercent = RtaFormatService.numberToPercent;

			$scope.displaySkillOrSkillAreaFilter = false;

			toggleService.togglesLoaded.then(function() {
				$scope.displaySkillOrSkillAreaFilter = toggleService.RTA_SiteAndTeamOnSkillOverview_40817;
				if($scope.displaySkillOrSkillAreaFilter) {

					RtaService.getSkills()
						.then(function (skills) {
							$scope.skillsLoaded = true;
							$scope.skills = skills;
							if ($scope.skillId !== null) {
								$scope.selectedSkill = getSelected(skills, $scope.skillId);
							}
						});

					RtaService.getSkillAreas()
						.then(function (skillAreas) {
							$scope.skillAreasLoaded = true;
							$scope.skillAreas = skillAreas.SkillAreas;
							if ($scope.skillAreaId !== null) {
								$scope.selectedSkillArea = getSelected(skillAreas.SkillAreas, $scope.skillAreaId);
							}
						});

						function getSelected(outOf, shouldMatch) {
							return outOf.find(function (o) {
								return o.Id === shouldMatch;
							})
						};

						$scope.querySearch = function (query, myArray) {
							var results = query ? myArray.filter(createFilterFor(query)) : myArray;
							return results;
						};

						function createFilterFor(query) {
							var lowercaseQuery = angular.lowercase(query);
							return function filterFn(item) {
								var lowercaseName = angular.lowercase(item.Name);
								return (lowercaseName.indexOf(lowercaseQuery) === 0);
							};
						};

						$scope.selectedSkillChange = function (skill) {
							if (!skill) return;
								$scope.skillId = skill.Id;
								doWhenSelecting(skill, $scope.selectedSkill, "rta.teams-by-skill", goToSitesBySkill);
						};

						$scope.selectedSkillAreaChange = function (skillArea) {
							if (!skillArea) return
								$scope.skillAreaId = skillArea.Id;
								doWhenSelecting(skillArea, $scope.selectedSkillArea, "rta.teams-by-skillArea", goToSitesBySkillArea);
						};

						function doWhenSelecting(item, selected, teamsStateName, goToSites) {
							selected = item;
							if ($state.current.name !== teamsStateName)
								goToSites(item.Id);
						};

						function goToSitesBySkill(skillId) {
							RtaRouteService.goToSitesBySkill(skillId);
						};

						function goToSitesBySkillArea(skillAreaId) {
							RtaRouteService.goToSitesBySkillArea(skillAreaId);
						};
				}
			});

			RtaOrganizationService.getSiteName($scope.siteId)
				.then(function(name) {
					$scope.siteName = name;
				});

			if ($scope.siteId === null) {
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
