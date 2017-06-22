(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaMainController', RtaMainController);

	RtaMainController.$inject = ['rtaService', '$state', '$stateParams', '$interval'];

	function RtaMainController(rtaService, $state, $stateParams, $interval) {
		var vm = this;
		vm.skillIds = $stateParams.skillIds || [];
		$stateParams.open = ($stateParams.open || "false");
		vm.skills = [];
		vm.skillAreas = [];
		vm.organization = [];
		vm.siteCards = [];
		(function fetchDataForFilterComponent() {
			rtaService.getSkills().then(function (result) {
				vm.skills = result;
			});

			rtaService.getSkillAreas().then(function (result) {
				vm.skillAreas = result.SkillAreas;
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

		(function fetchDataForOverviewComponent() {
			rtaService.getSiteCardsFor().then(function (result) {
				vm.siteCards = buildSiteCards(result);
			});

			$interval(function () {
				rtaService.getSiteCardsFor().then(function (result) {
					result.forEach(function (r) {
						updateSiteCard(r);
					})
				});
			}, 5000)


			function fetchTeamData(card) {
				if (!card.isOpen) return;

				rtaService.getTeamCardsFor({ siteIds: card.site.Id }).then(function (teams) {
					var match = vm.siteCards.find(function (c) {
						return c.site.Id === card.site.Id;
					})
					match.teams = teams;
				});

				$interval(function () {
					rtaService.getTeamCardsFor({ siteIds: card.site.Id }).then(function (teams) {
						var match = vm.siteCards.find(function (c) {
							return c.site.Id === card.site.Id;
						})
						match.teams = teams;
					});
				}, 5000)

			}

			function updateSiteCard(site) {
				var match = vm.siteCards.find(function (card) {
					return card.site.Id === site.Id;
				});
				match.site.Color = site.Color;
				match.site.InAlarmCount = site.InAlarmCount;
			}

			function buildSiteCards(sites) {
				return sites.map(function (site) {
					return {
						site: site,
						isOpen: $stateParams.open != "false",
						fetchTeamData: fetchTeamData
					}
				});
			}
		})();



	}
})();
