(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaHistoricalOverviewController', RtaHistoricalOverviewController);

	RtaHistoricalOverviewController.$inject = ['$filter', '$stateParams', '$http', '$translate', 'rtaStateService', 'rtaDataService'];

	function RtaHistoricalOverviewController($filter, $stateParams, $http, $translate, rtaStateService, rtaDataService) {

		var vm = this;
		vm.clearEnabled = false;
		rtaStateService.setCurrentState($stateParams);

		rtaDataService.load().then(function (data) {
			buildSites(data.organization);
		});

		function buildSites(organization) {
			vm.sites = [];

			organization.forEach(function (site) {
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

				vm.sites.push(siteModel);
			});

			updateOrganizationPicker();
		}

		function updateOrganizationPicker() {
			vm.organizationPickerSelectionText = rtaStateService.organizationSelectionText();
			vm.clearEnabled = (vm.sites || []).some(function (site) {
				return site.isChecked || site.isMarked;
			});
		}

		vm.toneAdherence = function (percentage) {
			var light = 40 + ((percentage / 100) * 60);
			return 'hsl(0,0%,' + light + '%)';
		};

		vm.applyOrganizationSelection = function () {
			vm.organizationPickerOpen = false;
			var params = rtaStateService.historicalOverviewParams();
			var noParams = !(params.siteIds.length || params.teamIds.length);
			if (noParams) {
				vm.cards = [];
				return;
			}
			$http.get('../api/HistoricalOverview', {
				params: params
			}).then(function (response) {
				vm.cards = response.data;
				vm.cards.forEach(function (card) {
					card.toggle = function () {
						card.isOpen = !card.isOpen;
					}
				});
			});

			// vm.cards = [
			// 	{
			// 		Name: 'Denver/Avalanche',
			// 		TeamAdherence: 74,
			// 		Agents: [{
			// 			Name: 'Andeen Ashley',
			// 			IntervalAdherence: 73,
			// 			Days: [
			// 				{
			// 					Date: '27/12',
			// 					Adherence: 100,
			// 					WasLateForWork: true
			// 				},
			// 				{
			// 					Date: '2/5',
			// 					Adherence: 90
			// 				},
			// 				{
			// 					Date: '3/5',
			// 					Adherence: 85
			// 				},
			// 				{
			// 					Date: '4/5',
			// 					Adherence: 88
			// 				},
			// 				{
			// 					Date: '5/5',
			// 					Adherence: 30,
			// 					WasLateForWork: true
			// 				},
			// 				{
			// 					Date: '6/5',
			// 					Adherence: 70
			// 				},
			// 				{
			// 					Date: '7/5',
			// 					Adherence: 72
			// 				}
			// 			],
			// 			LateForWork:
			// 				{
			// 					Count: 2,
			// 					TotalMinutes: 24
			// 				}
			// 		},
			// 			{
			// 				Name: 'Aneedn Anna',
			// 				IntervalAdherence: 77,
			// 				Days: [
			// 					{
			// 						Date: '1/5',
			// 						Adherence: 70,
			// 					},
			// 					{
			// 						Date: '2/5',
			// 						Adherence: 56,
			// 						WasLateForWork: true
			// 					},
			// 					{
			// 						Date: '3/5',
			// 						Adherence: 83
			// 					},
			// 					{
			// 						Date: '4/5',
			// 						Adherence: 71
			// 					},
			// 					{
			// 						Date: '5/5',
			// 						Adherence: 95
			// 					},
			// 					{
			// 						Date: '6/5',
			// 						Adherence: 77
			// 					},
			// 					{
			// 						Date: '7/5',
			// 						Adherence: 84
			// 					}
			// 				],
			// 				LateForWork:
			// 					{
			// 						Count: 1,
			// 						TotalMinutes: 10
			// 					}
			// 			},
			// 			{
			// 				Name: 'Aleed Jane',
			// 				IntervalAdherence: 75,
			// 				Days: [
			// 					{
			// 						Date: '1/5',
			// 						Adherence: 83,
			// 					},
			// 					{
			// 						Date: '2/5',
			// 						Adherence: 95,
			// 						WasLateForWork: true
			// 					},
			// 					{
			// 						Date: '3/5',
			// 						Adherence: 78,
			// 					},
			// 					{
			// 						Date: '4/5',
			// 						Adherence: 78,
			// 					},
			// 					{
			// 						Date: '5/5',
			// 						Adherence: 98,
			// 						WasLateForWork: true
			// 					},
			// 					{
			// 						Date: '6/5',
			// 						Adherence: 95,
			// 						WasLateForWork: true
			// 					},
			// 					{
			// 						Date: '7/5',
			// 						Adherence: 85,
			// 					}
			// 				],
			// 				LateForWork:
			// 					{
			// 						Count: 3,
			// 						TotalMinutes: 42
			// 					}
			// 			}
			//
			// 		]
			// 	},
			// 	{
			// 		Name: 'Barcelona/Red',
			// 		TeamAdherence: 94,
			// 		Agents: [{
			// 			Name: 'Cndeen Ashley',
			// 			IntervalAdherence: 94,
			// 			Days: [
			// 				{
			// 					Date: '1/5',
			// 					Adherence: 92,
			// 					WasLateForWork: true
			// 				},
			// 				{
			// 					Date: '2/5',
			// 					Adherence: 97,
			// 				},
			// 				{
			// 					Date: '3/5',
			// 					Adherence: 94,
			// 				},
			// 				{
			// 					Date: '4/5',
			// 					Adherence: 98,
			// 				},
			// 				{
			// 					Date: '5/5',
			// 					Adherence: 99,
			// 				},
			// 				{
			// 					Date: '6/5',
			// 					Adherence: 94,
			// 				},
			// 				{
			// 					Date: '7/5',
			// 					Adherence: 99,
			// 				}
			// 			],
			// 			LateForWork:
			// 				{
			// 					Count: 1,
			// 					TotalMinutes: 3
			// 				}
			// 		}
			// 		]
			// 	}
			// ];
		};

		vm.clearOrganizationSelection = function () {
			rtaStateService.deselectOrganization();
			updateOrganizationPicker();
		};

		vm.goToAgents = rtaStateService.goToAgents;
		vm.goToOverview = rtaStateService.goToOverview;
	}
})();
