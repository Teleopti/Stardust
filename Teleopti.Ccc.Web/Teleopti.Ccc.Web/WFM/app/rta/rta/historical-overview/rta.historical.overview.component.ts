import {Component, Inject, OnInit} from '@angular/core'
import {RTA_DATA_SERVICE} from "./rta.data.service.provider";
import {RTA_STATE_SERVICE} from "./rta.state.service.provider";

@Component({
	selector: 'rta-historical-overview',
	templateUrl: './rta.historical.overview.html'
})

export class RtaHistoricalOverviewComponent implements OnInit {
	organizationPickerOpen: boolean;
	organizationPickerSelectionText: string;
	organizationPickerSearchTerm: string;
	organizationPickerClearEnabled: string;
	openCard: boolean;
	sites: any [] = [];
	siteModel: Object;

	constructor(
		@Inject(RTA_DATA_SERVICE) private rtaDataService: any, @Inject(RTA_STATE_SERVICE) private rtaStateService: any) {
	}

	ngOnInit(): void {

		this.rtaStateService.setCurrentState([]).then();

		this.rtaDataService.load().then(data => {
			this.buildSites(data.organization);
		});
	}


	buildSites(organization: any): void {
		let self = this;
		organization.forEach(site =>  {
			let siteModel = {
				Id: site.Id,
				Name: site.Name,
				Teams: [],
				get isChecked() {
					return self.rtaStateService.isSiteSelected(site.Id);
				},
				get isMarked() {
					return self.rtaStateService.siteHasTeamsSelected(site.Id);
				},
				toggle: function () {
					self.rtaStateService.toggleSite(site.Id);
					self.updateOrganizationPicker();
				}
			};

			site.Teams.forEach(function (team) {
				siteModel.Teams.push({
					Id: team.Id,
					Name: team.Name,
					get isChecked() {
						return self.rtaStateService.isTeamSelected(team.Id);
					},
					toggle: function () {
						self.rtaStateService.toggleTeam(team.Id);
						self.updateOrganizationPicker();
					}
				});
			});

			this.sites.push(siteModel);
		});
		this.updateOrganizationPicker();
	}

	updateOrganizationPicker() : void {
		this.organizationPickerSelectionText = this.rtaStateService.organizationSelectionText();
		// vm.organizationPickerClearEnabled = (vm.sites || []).some(function (site) {
		// 	return site.isChecked || site.isMarked;
		// });
	}

	clearOrganizationSelection() {
		console.log('hej')
	}

	closePicker(event: Object) {
		this.organizationPickerOpen = false;
	}
	

	groupings: any [] = [
		{
			Name: 'Denver/Avalanche',
			AdherencePercentage: 74,
			Color: '#EE8F7D',
			agentsAdherence: [{
				Name: 'Andeen Ashley',
				Adherence: [
					{
						Date: '1/5',
						AdherencePercentage: 82,
						Color: '#FFC285',
						WasLateForWork: true
					},
					{
						Date: '2/5',
						AdherencePercentage: 90,
						Color: '#FFC285'
					},
					{
						Date: '3/5',
						AdherencePercentage: 85,
						Color: '#FFC285'
					},
					{
						Date: '4/5',
						AdherencePercentage: 88,
						Color: '#FFC285'
					},
					{
						Date: '5/5',
						AdherencePercentage: 50,
						Color: '#EE8F7D',
						WasLateForWork: true
					},
					{
						Date: '6/5',
						AdherencePercentage: 70,
						Color: '#EE8F7D'
					},
					{
						Date: '7/5',
						AdherencePercentage: 72,
						Color: '#EE8F7D'
					}
				],
				PeriodAdherence: {
					Color: '#EE8F7D',
					Value: 73
				},
				LateForWork:
					{
						Count: 2,
						TotalMinutes: 24
					}
			},
				{
					Name: 'Aneedn Anna',
					Adherence: [
						{
							Date: '1/5',
							AdherencePercentage: 70,
							Color: '#EE8F7D'
						},
						{
							Date: '2/5',
							AdherencePercentage: 56,
							Color: '#EE8F7D'
						},
						{
							Date: '3/5',
							AdherencePercentage: 83,
							Color: '#FFC285',
							WasLateForWork: true
						},
						{
							Date: '4/5',
							AdherencePercentage: 71,
							Color: '#EE8F7D'
						},
						{
							Date: '5/5',
							AdherencePercentage: 95,
							Color: '#C2E085'
						},
						{
							Date: '6/5',
							AdherencePercentage: 77,
							Color: '#EE8F7D'
						},
						{
							Date: '7/5',
							AdherencePercentage: 84,
							Color: '#FFC285'
						}
					],
					PeriodAdherence: {
						Color: '#EE8F7D',
						Value: 77
					},
					LateForWork:
						{
							Count: 1,
							TotalMinutes: 10
						}
				},
				{
					Name: 'Aleed Jane',
					Adherence: [
						{
							Date: '1/5',
							AdherencePercentage: 83,
							Color: '#FFC285'
						},
						{
							Date: '2/5',
							AdherencePercentage: 95,
							Color: '#C2E085',
							WasLateForWork: true
						},
						{
							Date: '3/5',
							AdherencePercentage: 78,
							Color: '#EE8F7D'
						},
						{
							Date: '4/5',
							AdherencePercentage: 78,
							Color: '#EE8F7D'
						},
						{
							Date: '5/5',
							AdherencePercentage: 98,
							Color: '#C2E085',
							WasLateForWork: true
						},
						{
							Date: '6/5',
							AdherencePercentage: 95,
							Color: '#C2E085',
							WasLateForWork: true
						},
						{
							Date: '7/5',
							AdherencePercentage: 85,
							Color: '#FFC285'
						}
					],
					PeriodAdherence: {
						Color: '#EE8F7D',
						Value: 75
					},
					LateForWork:
						{
							Count: 3,
							TotalMinutes: 42
						}
				}

			]
		},
		{
			Name: 'London/Team Preferences',
			AdherencePercentage: 84,
			Color: '#FFC285',
			agentsAdherence: [{
				Name: 'Bndeen Ashley',
				Adherence: [
					{
						Date: '1/5',
						AdherencePercentage: 84,
						Color: '#FFC285',
						WasLateForWork: true
					},
					{
						Date: '2/5',
						AdherencePercentage: 95,
						Color: '#C2E085'
					},
					{
						Date: '3/5',
						AdherencePercentage: 88,
						Color: '#FFC285'
					},
					{
						Date: '4/5',
						AdherencePercentage: 88,
						Color: '#FFC285'
					},
					{
						Date: '5/5',
						AdherencePercentage: 87,
						Color: '#FFC285',
						WasLateForWork: true
					},
					{
						Date: '6/5',
						AdherencePercentage: 72,
						Color: '#EE8F7D'
					},
					{
						Date: '7/5',
						AdherencePercentage: 79,
						Color: '#EE8F7D'
					}
				],
				PeriodAdherence: {
					Color: '#FFC285',
					Value: 82
				},
				LateForWork:
					{
						Count: 2,
						TotalMinutes: 8
					}
			},
				{
					Name: 'Bneedn Anna',
					Adherence: [
						{
							Date: '1/5',
							AdherencePercentage: 88,
							Color: '#FFC285'
						},
						{
							Date: '2/5',
							AdherencePercentage: 99,
							Color: '#C2E085'
						},
						{
							Date: '3/5',
							AdherencePercentage: 82,
							Color: '#FFC285'
						},
						{
							Date: '4/5',
							AdherencePercentage: 81,
							Color: '#FFC285'
						},
						{
							Date: '5/5',
							AdherencePercentage: 72,
							Color: '#EE8F7D'
						},
						{
							Date: '6/5',
							AdherencePercentage: 94,
							Color: '#C2E085'
						},
						{
							Date: '7/5',
							AdherencePercentage: 88,
							Color: '#FFC285',
							WasLateForWork: true
						}
					],
					PeriodAdherence: {
						Color: '#FFC285',
						Value: 82
					},
					LateForWork:
						{
							Count: 1,
							TotalMinutes: 10
						}
				},
				{
					Name: 'Blanca Jane',
					Adherence: [
						{
							Date: '1/5',
							AdherencePercentage: 82,
							Color: '#FFC285'
						},
						{
							Date: '2/5',
							AdherencePercentage: 82,
							Color: '#FFC285'
						},
						{
							Date: '3/5',
							AdherencePercentage: 70,
							Color: '#EE8F7D'
						},
						{
							Date: '4/5',
							AdherencePercentage: 87,
							Color: '#FFC285'
						},
						{
							Date: '5/5',
							AdherencePercentage: 90,
							Color: '#FFC285',
							WasLateForWork: true
						},
						{
							Date: '6/5',
							AdherencePercentage: 88,
							Color: '#FFC285'
						},
						{
							Date: '7/5',
							AdherencePercentage: 85,
							Color: '#FFC285'
						}
					],
					PeriodAdherence: {
						Color: '#FFC285',
						Value: 88
					},
					LateForWork:
						{
							Count: 1,
							TotalMinutes: 5
						}
				}

			]
		},
		{
			Name: 'Barcelona/Red',
			AdherencePercentage: 94,
			Color: '#C2E085',
			agentsAdherence: [{
				Name: 'Cndeen Ashley',
				Adherence: [
					{
						Date: '1/5',
						AdherencePercentage: 92,
						Color: '#C2E085',
						WasLateForWork: true
					},
					{
						Date: '2/5',
						AdherencePercentage: 97,
						Color: '#C2E085'
					},
					{
						Date: '3/5',
						AdherencePercentage: 94,
						Color: '#C2E085'
					},
					{
						Date: '4/5',
						AdherencePercentage: 98,
						Color: '#C2E085'
					},
					{
						Date: '5/5',
						AdherencePercentage: 99,
						Color: '#C2E085'
					},
					{
						Date: '6/5',
						AdherencePercentage: 94,
						Color: '#C2E085'
					},
					{
						Date: '7/5',
						AdherencePercentage: 99,
						Color: '#C2E085'
					}
				],
				PeriodAdherence: {
					Color: '#C2E085',
					Value: 94
				},
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

