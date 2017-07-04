(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaMainController', RtaMainController);

	RtaMainController.$inject = ['rtaService', 'skills', 'skillAreas', '$state', '$stateParams', '$interval', '$scope'];

	function RtaMainController(rtaService, skills, skillAreas, $state, $stateParams, $interval, $scope) {
		var vm = this;
		vm.skillIds = $stateParams.skillIds || [];
		vm.skillAreaId = $stateParams.skillAreaId;
		$stateParams.open = ($stateParams.open || "false");
		vm.skills = skills || [];
		vm.skillAreas = skillAreas || [];
		vm.organization = [];
		vm.siteCards = [];

		(function fetchDataForFilterComponent() {
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
			var teamPolling;
			var sitePolling;
			var sitePollingWithSkills;
			var pollingIntervals = [];

			if (angular.isDefined(vm.skillAreaId)) {
				vm.skillIds = getSkillIdsFromSkillAreaId(vm.skillAreaId);
				getSiteCards(vm.skillIds);
			} else if (vm.skillIds.length) {
				getSiteCards(vm.skillIds);
			} else {
				getSiteCards();
			}

			function getSiteCards(ids) {
				if (angular.isDefined(ids)) {
					rtaService.getSiteCardsFor(ids).then(function (result) {
						vm.siteCards = buildSiteCards(result);
					});
				} else {
					rtaService.getSiteCardsFor().then(function (result) {
						vm.siteCards = buildSiteCards(result);
					});
				}
			}

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

			function fetchTeamData(card) {
				if (!card.isOpen) {
					var match = pollingIntervals.find(function (interval) {
						return interval.siteId === card.site.Id;
					});
					var index = pollingIntervals.indexOf(match);
					pollingIntervals.splice(index, 1);
					$interval.cancel(match.interval);
				} else {
					fetchTeams(card);
					teamPolling = $interval(function () {
						fetchTeams(card);
					}, 5000);
					pollingIntervals.push({
						siteId: card.site.Id,
						interval: teamPolling
					});
				}
			}

			function fetchTeams(card) {
				rtaService.getTeamCardsFor({ siteIds: card.site.Id }).then(function (teams) {
					card.teams = teams;
				});
			}

			function getSkillIdsFromSkillAreaId(id) {
				var temp = [];
				var match = vm.skillAreas.find(function (sa) {
					return sa.Id === id;
				});

				match.Skills.forEach(function (skill) {
					temp.push(skill.Id);
				});

				return temp;
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
					} else {
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

			function updateSiteCard(site) {
				var match = vm.siteCards.find(function (card) {
					return card.site.Id === site.Id;
				});
				match.site.Color = translateSiteColors(site);
				match.site.InAlarmCount = site.InAlarmCount;
			}

			$scope.$on('$destroy', function () {
				if (pollingIntervals.length)
					pollingIntervals.forEach(function (i) {
						$interval.cancel(i.interval);
					});

				$interval.cancel(sitePolling);
				$interval.cancel(sitePollingWithSkills);
			});

			vm.filterOutput = function (selectedItem) {
				if (!angular.isDefined(selectedItem)) {

					vm.skillIds = [];
					$state.go($state.current.name, { skillAreaId: undefined, skillIds: undefined }, { notify: false });
					getSiteCards();

				}	else if (selectedItem.hasOwnProperty('Skills')) {

					$state.go($state.current.name, { skillAreaId: selectedItem.Id, skillIds: undefined }, { notify: false });
					vm.skillIds = getSkillIdsFromSkillAreaId(selectedItem.Id);
					getSiteCards(vm.skillIds);

				}	else {

					vm.skillIds = [selectedItem.Id];
					$state.go($state.current.name, { skillAreaId: undefined, skillIds: vm.skillIds }, { notify: false });
					getSiteCards(vm.skillIds);
					
				}
			}

		})();
	}
})();
