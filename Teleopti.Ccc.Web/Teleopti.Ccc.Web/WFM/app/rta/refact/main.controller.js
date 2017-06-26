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

		var pollForTeams;

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

		(function OverviewComponentHandler() {
			rtaService.getSiteCardsFor().then(function (result) {
				vm.siteCards = buildSiteCards(result);
			});

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

			$interval(function () {
				rtaService.getSiteCardsFor().then(function (result) {
					result.forEach(function (r) {
						updateSiteCard(r);
					})
				});
			}, 5000);

			function updateSiteCard(site) {
				var match = vm.siteCards.find(function (card) {
					return card.site.Id === site.Id;
				});
				match.site.Color = translateSiteColors(site);
				match.site.InAlarmCount = site.InAlarmCount;
			}

			function fetchTeamData(card) {
				if (!card.isOpen) {
					$interval.cancel(pollForTeams);
					return;
				}
				else {
					rtaService.getTeamCardsFor({ siteIds: card.site.Id }).then(function (teams) {
						var match = vm.siteCards.find(function (c) {
							return c.site.Id === card.site.Id;
						})
						match.teams = teams;
					});

					pollForTeams = $interval(function () {
						rtaService.getTeamCardsFor({ siteIds: card.site.Id }).then(function (teams) {
							var match = vm.siteCards.find(function (c) {
								return c.site.Id === card.site.Id;
							})
							match.teams = teams;
						});
					}, 5000)

				}
			}


		})();



	}
})();
