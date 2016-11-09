
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

			var teamsBySkillsStateName = "rta.teams-by-skill";

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
												doWhenSelecting(skill, $scope.selectedSkill, teamsBySkillsStateName, goToSitesBySkill);
										};

										$scope.selectedSkillAreaChange = function (skillArea) {
											if (!skillArea) return
												$scope.skillAreaId = skillArea.Id;
												doWhenSelecting(skillArea, $scope.selectedSkillArea, teamsBySkillsStateName, goToSitesBySkillArea);
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
					getAdherenceForTeamsOnSite($scope.siteId)
					.then(function (teamAdherence) {
							updateAdherence($scope.teams, teamAdherence);
					});
				} else {
					getAdherenceForAllSites()
						.then(function (siteAdherences) {
							updateAdherence($scope.sites, siteAdherences);
						});
				}
			}, 5000);

			$scope.siteId ? getTeamsInfo() : getSitesInfo();

			function getTeamsInfo() {
				getTeams($scope.siteId)
				.then(function (teams) {
					$scope.teams = teams;
					return getAdherenceForTeamsOnSite($scope.siteId);
				}).then(function (teamAdherence) {
						updateAdherence($scope.teams, teamAdherence);
				});
			};

			function getSitesInfo() {
				getSites().then(function (sites) {
					$scope.sites = sites;
					return getAdherenceForAllSites();
				}).then(function (siteAdherences) {
					updateAdherence($scope.sites, siteAdherences);
				});
			}

			function getTeams(siteId) {
				return 	RtaService.getTeams({
						siteId: siteId
					});
			};

			function getSites() {
				return RtaService.getSites();
			}

			function getAdherenceForTeamsOnSite(siteId) {
				return RtaService.getAdherenceForTeamsOnSite({
					siteId: siteId
				});
			};

			function getAdherenceForAllSites() {
					return RtaService.getAdherenceForAllSites();
			}

			function updateAdherence(level, adherence) {
					RtaAdherenceService.updateAdherence(level, adherence);
			};

			$scope.toggleSelection = function (itemId) {
				var index = $scope.selectedItemIds.indexOf(itemId);
				if (index > -1) {
					$scope.selectedItemIds.splice(index, 1);
				} else {
					$scope.selectedItemIds.push(itemId);
				}
			}

			$scope.openSelectedItems = function () {
					if ($scope.selectedItemIds.length > 0)
					goToAgents($scope.selectedItemIds);
			};

			function goToAgents(selectedItemIds) {
				$scope.siteId ? RtaRouteService.goToAgents({teamIds: selectedItemIds}) : RtaRouteService.goToAgents({siteIds : selectedItemIds});
				};

			$scope.goToSkillSelection = function () {
				RtaRouteService.goToSelectSkill();
			};

			$scope.goBackWithUrl = function () {
				return RtaRouteService.urlForSites();
			};

			function goToDashboard() {
				RtaRouteService.goToSites();
			}

			function goToDashboard() {
				RtaRouteService.goToSites();
			}

			$scope.$watch(
				function () {
					return $sessionStorage.buid;
				},
				function (newValue, oldValue) {
					if (oldValue !== undefined && newValue !== oldValue) {
						goToDashboard();
					}
				}
			);

			$scope.$on('$destroy', function () {
				$interval.cancel(polling);
			});
		}
	]);
})();
