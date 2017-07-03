(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaMainController', RtaMainController);

	RtaMainController.$inject = ['rtaService', '$state', '$stateParams', '$interval', '$scope', '$location'];

	function RtaMainController(rtaService, $state, $stateParams, $interval, $scope, $location) {
		var vm = this;
		vm.skillIds = $stateParams.skillIds || [];
		$stateParams.open = ($stateParams.open || "false");
		vm.skills = [];
		vm.skillAreas = [];
		vm.organization = [];
		vm.siteCards = [];
		vm.options= {skillsLoaded: false, skillAreasLoaded: false};

		(function fetchDataForFilterComponent() {

			rtaService.getSkills().then(function (result) {
				vm.skills = result;
				vm.options.skillsLoaded = true;
			});

			rtaService.getSkillAreas().then(function (result) {
				vm.skillAreas = result.SkillAreas;
				vm.options.skillAreasLoaded = true;
			});

			if (vm.skillIds.length > 0) {
				rtaService.getOrganizationForSkills({ skillIds: vm.skillIds }).then(function (result) {
					vm.organization = result;
				});

			} else {
				rtaService.getOrganization().then(function (result) {
					vm.organization = result;
				});
			}

		})();

		(function OverviewComponentHandler() {
			var sitePolling;
			var sitePollingWithSkills;

			var getSiteCards = function (ids) {
				if (angular.isDefined(ids)) {
					rtaService.getSiteCardsFor(ids).then(function (result) {
						vm.siteCards = buildSiteCards(result);
					});
				}
				else {
					rtaService.getSiteCardsFor().then(function (result) {
						vm.siteCards = buildSiteCards(result);
					});
				}
			}

			getSiteCards();

			function buildSiteCards(sites) {
				return sites.map(function (site) {
					site.Color = translateSiteColors(site);
					return {
						site: site,
						isOpen: $stateParams.open != "false",
						fetchTeamData: fetchTeamData
					}
				});
			}

			function translateSiteColors(site) {
				if (site.Color === 'good') {
					return '#C2E085';
				} else if (site.Color === 'warning') {
					return '#FFC285';
				} else if (site.Color === 'danger') {
					return '#EE8F7D';
				} else {
					return '#fff';
				}
			}

			function updateSiteCard(site) {
				var match = vm.siteCards.find(function (card) {
					return card.site.Id === site.Id;
				});
				match.site.Color = translateSiteColors(site);
				match.site.InAlarmCount = site.InAlarmCount;
			}

			var polling;
			var intervals = [];

			function fetchTeamData(card) {
				if (!card.isOpen) {
					if (intervals.length > 1) {
						var match = intervals.find(function (interval) {
							return interval.siteId === card.site.Id;
						});
						var index = intervals.indexOf(match);
						intervals.splice(index, 1);
						$interval.cancel(match.interval);
					} else {
						$interval.cancel(intervals[0].interval);
						intervals = [];
					}
				} else {
					fetchTeams(card);
					polling = $interval(function () {
						fetchTeams(card);
					}, 5000);
					intervals.push({
						siteId: card.site.Id,
						interval: polling
					});
				}
			}

			var fetchTeams = function (card) {
				rtaService.getTeamCardsFor({ siteIds: card.site.Id }).then(function (teams) {
					card.teams = teams;
				});
			}

			vm.filterOutput = function (selectedItem) {
				if (!angular.isDefined(selectedItem)) {
					vm.skillIds = [];
					$state.go($state.current.name, { skillAreaId: undefined, skillIds: undefined }, { notify: false });
					getSiteCards();
				}
				else if (selectedItem.hasOwnProperty('Skills')) {
					var skillIds = [];
					selectedItem.Skills.forEach(function (skill) {
						skillIds.push(skill.Id);
					});
					vm.skillIds = skillIds;

					$state.go($state.current.name, { skillAreaId: selectedItem.Id, skillIds: undefined }, { notify: false });
					getSiteCards(vm.skillIds);
				}
				else {
					vm.skillIds = [selectedItem.Id];
					$state.go($state.current.name, { skillAreaId: undefined, skillIds: vm.skillIds }, { notify: false });
					getSiteCards(vm.skillIds);
				}
			}

			$scope.$on('$destroy', function () {
				if (intervals.length)
					intervals.forEach(function (i) {
						$interval.cancel(i.interval);
					});

				$interval.cancel(sitePolling);
				$interval.cancel(sitePollingWithSkills);
			});

			$scope.$watch(function () { return vm.skillIds; },
				function (newValue, oldValue) {
					if (angular.isDefined(sitePollingWithSkills)) {
						$interval.cancel(sitePollingWithSkills);
					}
					if (newValue.length) {
						sitePollingWithSkills = $interval(function () {
							rtaService.getSiteCardsFor(vm.skillIds).then(function (result) {
								result.forEach(function (r) {
									updateSiteCard(r);
								})
							});
						}, 5000);
						$interval.cancel(sitePolling);
					}
					else {
						sitePolling = $interval(function () {
							rtaService.getSiteCardsFor().then(function (result) {
								result.forEach(function (r) {
									updateSiteCard(r);
								})
							});
						}, 5000);
						$interval.cancel(sitePollingWithSkills);
					}
				});

		})();
	}
})();
