(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaHistoricalOverviewController', RtaHistoricalOverviewController);

	RtaHistoricalOverviewController.$inject = ['$filter', '$stateParams', '$state', '$http', '$translate', 'rtaStateService', 'rtaDataService'];

	function RtaHistoricalOverviewController($filter, $stateParams, $state, $http, $translate, rtaStateService, rtaDataService) {

		var vm = this;

		rtaStateService.setCurrentState($stateParams);

		rtaDataService.load().then(function (data) {
			buildSites(data.organization);
			loadCards();
		});

		function buildSites(organization) {
			vm.sites = organization.map(function (site) {
				var siteModel = {
					Id: site.Id,
					Name: site.Name,
					FullPermission: site.FullPermission,
					Teams: [],
					get isChecked() {
						return rtaStateService.isSiteSelected(site.Id);
					},
					get isMarked() {
						return rtaStateService.siteHasTeamsSelected(site.Id);
					},
					toggle: function () {
						rtaStateService.toggleSite(site.Id);
						updateOrganizationPicker();
					}
				};

				site.Teams.forEach(function (team) {
					siteModel.Teams.push({
						Id: team.Id,
						Name: team.Name,
						get isChecked() {
							return rtaStateService.isTeamSelected(team.Id);
						},
						toggle: function () {
							rtaStateService.toggleTeam(team.Id);
							updateOrganizationPicker();
						}
					});
				});

				return siteModel;
			});

			updateOrganizationPicker();
		}

		function updateOrganizationPicker() {
			vm.organizationPickerSelectionText = rtaStateService.organizationSelectionText();
			vm.clearEnabled = (vm.sites || []).some(function (site) {
				return site.isChecked || site.isMarked;
			});
		}

		vm.closeOrganizationPicker = function () {
			if (!vm.organizationPickerOpen)
				return;
			vm.organizationPickerOpen = false;
			loadCards();
		};

		function loadCards() {

			var params = rtaStateService.historicalOverviewParams();
			var noParams = !(params.siteIds.length || params.teamIds.length);
			if (noParams) {
				vm.cards = [];
				return;
			}
			
			$http.get('../api/HistoricalOverview/Load', {
				params: params
			}).then(function (response) {
				buildCards(response.data);
			});
		}

		function buildCards(data) {
			vm.cards = data.map(function (card) {
				return {
					Name: card.Name,
					toggle: function () {
						this.isOpen = !this.isOpen;
					},
					Agents: buildAgents(card.Agents || [])
				};
			});
		}

		function buildAgents(agents) {
			return agents.map(function (agent) {
				return {
					Name: agent.Name,
					IntervalAdherence: agent.IntervalAdherence,
					Days: buildDays(agent.Days, agent.Id),
					LateForWork: {
						Count: agent.LateForWork.Count,
						TotalMinutes: agent.LateForWork.TotalMinutes
					}
				};
			});
		}

		function buildDays(days, personId) {
			return days.map(function (day) {
				return {
					DisplayDate: day.DisplayDate,
					Adherence: day.Adherence,
					Color: colorAdherence(day.Adherence),
					HistoricalAdherenceUrl: $state.href('rta-historical', {personId: personId, date: day.Date}),
					WasLateForWork: day.WasLateForWork
				};
			});
		}

		function colorAdherence(percentage) {
			var light = 40 + ((percentage / 100) * 60);
			return 'hsl(0,0%,' + light + '%)';
		}

		vm.toggleOrganizationPicker = function () {
			vm.organizationPickerOpen = !vm.organizationPickerOpen;
		};

		vm.clearOrganizationSelection = function () {
			rtaStateService.deselectOrganization();
			updateOrganizationPicker();
		};

		vm.goToAgents = rtaStateService.goToAgents;
		vm.goToOverview = rtaStateService.goToOverview;
		}
})();
