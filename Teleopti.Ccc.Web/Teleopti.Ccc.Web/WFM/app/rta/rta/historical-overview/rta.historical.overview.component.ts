import {Component, Inject, OnInit} from '@angular/core'
import { RTA_DATA_SERVICE } from "./rta.data.service.provider";

@Component({
	selector: 'rta-historical-overview',
	templateUrl: './rta.historical.overview.html'
})

export class RtaHistoricalOverviewComponent implements OnInit{
	organizationPickerOpen: boolean;
	organizationPickerSelectionText: string;
	organizationPickerSearchTerm: string;
	organizationPickerClearEnabled: string;
	openCard: boolean;
	sites: any [];
	
	constructor(
		@Inject(RTA_DATA_SERVICE) private rtaDataService: any) {
		console.log('in constructor6');
	}

	ngOnInit(): void {
		this.rtaDataService.load().then(data => {
			this.sites = data.organization;
		});
	}	

	clearOrganizationSelection() {
		console.log('hej')
	}

	closePicker(event: Object) {
		this.organizationPickerOpen = false;
	}
	
	groupings: any [] = [
		{
			Name: 'Denver - Team Linda',
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
						Color: '#C2E085'
					},
					{
						Date: '3/5',
						AdherencePercentage: 85,
						Color: '#C2E085'
					},
					{
						Date: '4/5',
						AdherencePercentage: 88,
						Color: '#C2E085'
					},
					{
						Date: '5/5',
						AdherencePercentage: 92,
						Color: '#C2E085',
						WasLateForWork: true
					},
					{
						Date: '6/5',
						AdherencePercentage: 95,
						Color: '#C2E085'
					},
					{
						Date: '7/5',
						AdherencePercentage: 86,
						Color: '#C2E085'
					}
				],
				PeriodAdherence: {
					Color:'#EE8F7D',
					Value:73
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
							AdherencePercentage: 92,
							Color: '#C2E085'
						},
						{
							Date: '2/5',
							AdherencePercentage: 89,
							Color: '#C2E085'
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
							AdherencePercentage: 87,
							Color: '#C2E085'
						},
						{
							Date: '7/5',
							AdherencePercentage: 84,
							Color: '#FFC285'
						}
					],
					PeriodAdherence: {
						Color:'#FFC285',
						Value:77
					},
					LateForWork:
						{
							Count: 1,
							TotalMinutes: 20
						}
				},
				{
					Name: 'Aleed Jane',
					Adherence: [
						{
							Date: '1/5',
							AdherencePercentage: 80,
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
							AdherencePercentage: 89,
							Color: '#C2E085'
						},
						{
							Date: '4/5',
							AdherencePercentage: 89,
							Color: '#C2E085'
						},
						{
							Date: '5/5',
							AdherencePercentage: 98,
							Color: '#C2E085',
							WasLateForWork: true
						},
						{
							Date: '6/5',
							AdherencePercentage: 92,
							Color: '#C2E085',
							WasLateForWork: true
						},
						{
							Date: '7/5',
							AdherencePercentage: 80,
							Color: '#FFC285'
						}
					],
					PeriodAdherence: {
						Color:'#C2E085',
						Value:91
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
			Name: 'Denver - Team Robert',
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
						Color: '#C2E085'
					},
					{
						Date: '4/5',
						AdherencePercentage: 88,
						Color: '#C2E085'
					},
					{
						Date: '5/5',
						AdherencePercentage: 90,
						Color: '#C2E085',
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
					Color:'#FFC285',
					Value:82
				},
				LateForWork:
					{
						Count: 2,
						TotalMinutes: 28
					}
			},
				{
					Name: 'Bneedn Anna',
					Adherence: [
						{
							Date: '1/5',
							AdherencePercentage: 88,
							Color: '#C2E085'
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
							Color: '#C2E085',
							WasLateForWork: true
						}
					],
					PeriodAdherence: {
						Color:'#FFC285',
						Value:80
					},
					LateForWork:
						{
							Count: 1,
							TotalMinutes: 20
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
							AdherencePercentage: 75,
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
							Color: '#C2E085'
						},
						{
							Date: '5/5',
							AdherencePercentage: 90,
							Color: '#C2E085',
							WasLateForWork: true
						},
						{
							Date: '6/5',
							AdherencePercentage: 88,
							Color: '#C2E085'
						},
						{
							Date: '7/5',
							AdherencePercentage: 85,
							Color: '#C2E085'
						}
					],
					PeriodAdherence: {
						Color:'#C2E085',
						Value:89
					},
					LateForWork:
						{
							Count: 1,
							TotalMinutes: 25
						}
				}

			]
		},
		{
			Name: 'London - Team Preferences',
			agentsAdherence: [{
				Name: 'Cndeen Ashley',
				Adherence: [
					{
						Date: '1/5',
						AdherencePercentage: 82,
						Color: '#FFC285',
						WasLateForWork: true
					},
					{
						Date: '2/5',
						AdherencePercentage: 97,
						Color: '#C2E085'
					},
					{
						Date: '3/5',
						AdherencePercentage: 84,
						Color: '#FFC285'
					},
					{
						Date: '4/5',
						AdherencePercentage: 88,
						Color: '#C2E085'
					},
					{
						Date: '5/5',
						AdherencePercentage: 90,
						Color: '#C2E085',
						WasLateForWork: true
					},
					{
						Date: '6/5',
						AdherencePercentage: 94,
						Color: '#C2E085'
					},
					{
						Date: '7/5',
						AdherencePercentage: 80,
						Color: '#FFC285'
					}
				],
				PeriodAdherence: {
					Color:'#C2E085',
					Value:88
				},
				LateForWork:
					{
						Count: 2,
						TotalMinutes: 40
					}
			}
			]
		}
	];
}

