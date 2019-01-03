(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaOverviewController46933', RtaOverviewController);

	RtaOverviewController.$inject = ['rtaService', 'rtaStateService', 'rtaDataService', 'rtaPollingService', 'rtaConfigurationValidator', '$http', '$state', '$stateParams', '$scope'];

	function RtaOverviewController(rtaService, rtaStateService, rtaDataService, rtaPollingService, rtaConfigurationValidator, $http, $state, $stateParams, $scope) {
		var vm = this;
		vm.skills = [];
		vm.skillAreas = [];
		var skills, skillAreas;
 
		rtaConfigurationValidator.validate();
		
		vm.totalAgentsInAlarm = 0;

		Object.defineProperty(vm, 'skillPickerPreselectedItem', {
			get: function () {
				return rtaStateService.skillPickerPreselectedItem();
			}
		});
		
		vm.loading = function() {
			return !(vm.siteCards && skills && skillAreas);
		};

		vm.displayNoSitesMessage = function () {
			if (!vm.siteCards) 
				return false;
			return vm.siteCards.length == 0;
		};
		
		vm.highlightAgentsButton = rtaStateService.hasSelection;
		vm.goToAgents = rtaStateService.goToAgents;

		var classSuffixOnSelection = "-selected";
		var classSuffixNoSelection = "-border";
		var noBorderClass = 'rta-card-no-border';

		var poller = rtaPollingService.create(getSites);
		rtaStateService.setCurrentState($stateParams)
			.then(function () {
				vm.siteCards = [];
				poller.start();
			});
		$scope.$on('$destroy', poller.destroy);

		rtaDataService.load().then(function (data) {
			skills = data.skills;
			skillAreas = data.skillAreas; 
			vm.skills = data.skills;
			vm.skillAreas = data.skillAreas;
		});

		vm.selectTeamOrSite = function (selectable) {
			selectable.isSelected = !selectable.isSelected;
		};

		function getSites() {
			return rtaService.getOverviewModelFor(
				rtaStateService.pollParams({
					skillIds: true,
					openedSiteIds: true
				}))
				.then(function (data) {
					updateSites(data);
					vm.totalAgentsInAlarm = data.TotalAgentsInAlarm;
					vm.noSiteCards = !vm.siteCards.length;
				})
		}

		function updateSites(data) {
			data.Sites.forEach(function (site) {
				var siteCard = vm.siteCards.find(function (siteCard) {
					return siteCard.Id === site.Id;
				});

				site.Teams = site.Teams || [];
				if (!siteCard) {
					siteCard = {
						Id: site.Id,
						Name: site.Name,
						get isOpen() {
							return rtaStateService.isSiteOpen(site.Id);
						},
						set isOpen(newValue) {
							rtaStateService.openSite(site.Id, newValue);
							if (newValue) poller.force();
						},
						get isSelected() {
							return rtaStateService.isSiteSelected(site.Id);
						},
						set isSelected(newValue) {
							rtaStateService.selectSite(site.Id, newValue);
						},
						get ClassesOnSelection() {
							return this.isSelected ? (this.Color + classSuffixOnSelection + ' ' + noBorderClass) : (this.Color + classSuffixNoSelection);
						},
						teams: [],
						AgentsCount: site.AgentsCount,
						href: rtaStateService.agentsHrefForSite(site.Id)
					};

					vm.siteCards.push(siteCard);
				}
				siteCard.Color = site.Color; 	//internal
				updateTeams(siteCard, site.Teams);
				siteCard.InAlarmCount = site.InAlarmCount;
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
						get isSelected() {
							return rtaStateService.isTeamSelected(team.Id);
						},
						set isSelected(newValue) {
							rtaStateService.selectTeam(team.Id, newValue);
						},
						AgentsCount: team.AgentsCount,
                        href: rtaStateService.agentsHrefForTeam(teams.length, team.SiteId, team.Id)
					};
					siteCard.teams.push(teamCard);
				}
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
			vm.siteCards = [];
			poller.force();
		}
	}

})();

