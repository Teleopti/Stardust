(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaOverviewController39082', RtaOverviewController);

	RtaOverviewController.$inject = ['rtaService', 'rtaStateService', 'skills', 'skillAreas', '$state', '$stateParams', '$scope', '$timeout'];

	function RtaOverviewController(rtaService, rtaStateService, skills, skillAreas, $state, $stateParams, $scope, $timeout) {
		var vm = this;
		vm.skills = skills || [];
		vm.skillAreas = skillAreas || [];
		vm.siteCards = [];
		vm.totalAgentsInAlarm = 0;

		rtaStateService.setCurrentState($stateParams)
			.then(pollInitiate);

		vm.skillPickerPreselectedItem = rtaStateService.skillPickerPreselectedItem();

		vm.displayNoSitesMessage = function () { return vm.siteCards.length == 0; };
		vm.displayNoSitesForSkillMessage = rtaStateService.hasSkillSelection;
		vm.goToAgents = rtaStateService.goToAgents;
		vm.highlightAgentsButton = {
			get hasSelection() {
				return rtaStateService.hasSelection();
			}
		}
		
		var pollPromise;

		vm.selectTeamOrSite = function (selectable) {
			selectable.isSelected = !selectable.isSelected;
		};

		function pollInitiate() {
			vm.siteCards = [];
			pollNow();
		}

		function pollNow() {
			pollStop();
			getSites()
				.then(pollNext);
		}

		function pollNext() {
			pollPromise = $timeout(function () {
				getSites().then(pollNext)
			}, 5000);
		}

		function pollStop() {
			if (pollPromise)
				$timeout.cancel(pollPromise);
		}

		$scope.$on('$destroy', pollStop);

		function getSites() {
			return rtaService.getOverviewModelFor(rtaStateService.pollParams())
				.then(function (data) {
					data.Sites.forEach(function (site) {
						var siteCard = vm.siteCards.find(function (siteCard) {
							return siteCard.Id === site.Id;
						});

						site.Teams = site.Teams || [];
						if (!siteCard) {
							siteCard = {
								Id: site.Id,
								Name: site.Name,
								get isOpen() { return rtaStateService.isSiteOpen(site.Id); },
								set isOpen(newValue) {
									rtaStateService.openSite(site.Id, newValue);
									if (newValue) pollNow();
								},
								get isSelected() { return rtaStateService.isSiteSelected(site.Id); },
								set isSelected(newValue) { rtaStateService.selectSite(site.Id, newValue); },
								teams: [],
								AgentsCount: site.AgentsCount,
								href: rtaStateService.agentsHrefForSite(site.Id)
							};

							vm.siteCards.push(siteCard);
						};

						updateTeams(siteCard, site.Teams);
						siteCard.Color = translateSiteColors(site);
						siteCard.InAlarmCount = site.InAlarmCount;

					});

					vm.totalAgentsInAlarm = data.TotalAgentsInAlarm;
					vm.noSiteCards = !vm.siteCards.length;
				});
		}

		function updateTeams(siteCard, teams) {
			teams.forEach(function (team) {
				var teamCard = siteCard.teams.find(function (t) {
					return team.Id === t.Id;
				});

				if (!teamCard) {
					teamCard = {
						Id: team.Id,
						Name: team.Name,
						SiteId: team.SiteId,
						get isSelected() { return rtaStateService.isTeamSelected(team.Id); },
						set isSelected(newValue) { rtaStateService.selectTeam(team.Id, newValue); },
						AgentsCount: team.AgentsCount,
						href: rtaStateService.agentsHrefForTeam(team.SiteId, team.Id)
					};
					siteCard.teams.push(teamCard);
				};
				teamCard.Color = team.Color;
				teamCard.InAlarmCount = team.InAlarmCount;
			})
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

		vm.selectSkillOrSkillArea = function (skillOrSkillArea) {
			if (!angular.isDefined(skillOrSkillArea))
				rtaStateService.deselectSkillAndSkillArea();
			else if (skillOrSkillArea.hasOwnProperty('Skills'))
				rtaStateService.selectSkillArea(skillOrSkillArea.Id);
			else
				rtaStateService.selectSkill(skillOrSkillArea.Id);
		}

	}
})();

