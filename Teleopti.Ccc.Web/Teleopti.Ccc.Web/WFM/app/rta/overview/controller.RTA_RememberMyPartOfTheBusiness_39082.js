﻿(function () {
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
		vm.highlightAgentsButton = rtaStateService.hasSelection;
		vm.goToAgents = rtaStateService.goToAgents;

		var pollPromise;
		var destroyed = false;
		var classSuffixOnSelection = "-selected";
		var classSuffixNoSelection = "-border";
		var noBorderClass = 'rta-card-no-border';

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
			if (destroyed)
				return;
			pollPromise = $timeout(function () {
				getSites().then(pollNext)
			}, 5000);
		}

		function pollStop() {
			if (pollPromise)
				$timeout.cancel(pollPromise);
		}

		$scope.$on('$destroy', 
		function(){
			destroyed = true;
			pollStop()
		});

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
						siteCard.ClassesOnSelection = siteCard.isSelected ? (site.Color + classSuffixOnSelection + ' ' + noBorderClass) : (site.Color + classSuffixNoSelection);
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
				teamCard.ClassOnSelection = teamCard.isSelected ? teamCard.Color + classSuffixOnSelection : team.Color;
			})
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

