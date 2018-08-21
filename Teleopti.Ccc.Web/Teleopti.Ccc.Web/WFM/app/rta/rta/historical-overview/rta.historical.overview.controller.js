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


		function getDummyData() {
			return [
				{
					Name: 'Denver/Avalanche',
					Agents: [{
						Id: '1234',
						Name: 'Andeen Ashley',
						IntervalAdherence: 73,

						Days: [
							{
								Date: '20180801',
								DisplayDate: '1/8',
								Adherence: 100,
								WasLateForWork: true
							},
							{
								Date: '20180802',
								DisplayDate: '1/8',
								Adherence: 90
							},
							{
								Date: '20180803',
								DisplayDate: '1/8',
								Adherence: 85
							},
							{
								Date: '20180804',
								DisplayDate: '1/8',
								Adherence: 88
							},
							{
								Date: '20180805',
								DisplayDate: '1/8',
								Adherence: 30,
								WasLateForWork: true
							},
							{
								Date: '20180806',
								DisplayDate: '1/8',
								Adherence: 70
							},
							{
								Date: '20180807',
								DisplayDate: '1/8',
								Adherence: 72
							}
						],
						LateForWork:
							{
								Count: 2,
								TotalMinutes: 24
							}
					},
						{
							Id: '1234',
							Name: 'Aneedn Anna',
							IntervalAdherence: 77,
							Days: [
								{
									Date: '20180801',
									DisplayDate: '1/8',
									Adherence: 70,
								},
								{
									Date: '20180802',
									DisplayDate: '1/8',
									Adherence: 56,
									WasLateForWork: true
								},
								{
									Date: '20180803',
									DisplayDate: '1/8',
									Adherence: 83
								},
								{
									Date: '20180804',
									DisplayDate: '1/8',
									Adherence: 71
								},
								{
									Date: '20180805',
									DisplayDate: '1/8',
									Adherence: 95
								},
								{
									Date: '20180806',
									DisplayDate: '1/8',
									Adherence: 77
								},
								{
									Date: '20180807',
									DisplayDate: '1/8',
									Adherence: 84
								}
							],
							LateForWork:
								{
									Count: 1,
									TotalMinutes: 10
								}
						},
						{
							Id: '1234',
							Name: 'Aleed Jane',
							IntervalAdherence: 75,
							Days: [
								{
									Date: '20180801',
									DisplayDate: '1/8',
									Adherence: 83,
								},
								{
									Date: '20180802',
									DisplayDate: '1/8',
									Adherence: 95,
									WasLateForWork: true
								},
								{
									Date: '20180803',
									DisplayDate: '1/8',
									Adherence: 78,
								},
								{
									Date: '20180804',
									DisplayDate: '1/8',
									Adherence: 78,
								},
								{
									Date: '20180805',
									DisplayDate: '1/8',
									Adherence: 98,
									WasLateForWork: true
								},
								{
									Date: '20180806',
									DisplayDate: '1/8',
									Adherence: 95,
									WasLateForWork: true
								},
								{
									Date: '20180807',
									DisplayDate: '1/8',
									Adherence: 85,
								}
							],
							LateForWork:
								{
									Count: 3,
									TotalMinutes: 42
								}
						}

					]
				},
				{
					Id: '1234',
					Name: 'Barcelona/Red',
					Agents: [{
						Name: 'Cndeen Ashley',
						IntervalAdherence: 94,
						Days: [
							{
								Date: '20180801',
								DisplayDate: '1/8',
								Adherence: 92,
								WasLateForWork: true
							},
							{
								Date: '20180802',
								DisplayDate: '1/8',
								Adherence: 97,
							},
							{
								Date: '20180803',
								DisplayDate: '1/8',
								Adherence: 94,
							},
							{
								Date: '20180804',
								DisplayDate: '1/8',
								Adherence: 98,
							},
							{
								Date: '20180806',
								DisplayDate: '1/8',
								Adherence: 99,
							},
							{
								Date: '20180807',
								DisplayDate: '1/8',
								Adherence: 94,
							},
							{
								Date: '20180808',
								DisplayDate: '1/8',
								Adherence: 99,
							}
						],
						LateForWork:
							{
								Count: 1,
								TotalMinutes: 3
							}
					}
					]
				}
			];

		}
	}
})();
