import {Component, Inject, OnInit} from '@angular/core'
import { RTA_SERVICE } from "./rtaService.provider";

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
		@Inject(RTA_SERVICE) private rtaService: any) {
		console.log('in constructor');
	}

	ngOnInit(): void {
	  this.rtaService.getOrganization().then(
	  	(s)=> {
			this.sites = s;
		})
	  //console.log(foo);
	}	

	clearOrganizationSelection() {
		console.log('hej')
	}

	closePicker(event: Object) {
		this.organizationPickerOpen = false;
	}

	// sites: any [] = [{
	// 	"Teams": [{
	// 		"Id": "94230b19-c23e-46c2-a500-a69600b7c715",
	// 		"Name": "Team Linda"
	// 	}, {"Id": "8aabe24f-cb06-40cf-9acc-a69600b70691", "Name": "Team Robert"}],
	// 	"Id": "d341c1ba-7079-46bb-a47c-a69600b5b585",
	// 	"Name": "Denver"
	// }, {
	// 	"Teams": [{"Id": "34590a63-6331-4921-bc9f-9b5e015ab495", "Name": "Team Preferences"}],
	// 	"Id": "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
	// 	"Name": "London"
	// }, {
	// 	"Teams": [{
	// 		"Id": "fa169ee6-2d71-4d6c-bb6d-a69600b5c0bd",
	// 		"Name": "Team Angelica"
	// 	}, {"Id": "0f6d7644-1134-4561-97ca-a69600b6021d", "Name": "Team Joshua"}],
	// 	"Id": "6c48e01f-87a8-47c6-b27f-a69600b5982f",
	// 	"Name": "Manila"
	// }, {
	// 	"Teams": [{"Id": "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495", "Name": "Team 1"}],
	// 	"Id": "6a21c802-7a34-4917-8dfd-9b5e015ab461",
	// 	"Name": "Paris"
	// }, {
	// 	"Teams": [{
	// 		"Id": "04d552aa-b39e-4afc-81c2-a69600f2a59e",
	// 		"Name": "Team Gunnar"
	// 	}, {"Id": "589b1752-d391-476f-87f6-a69600f2ad11", "Name": "Team Therese"}],
	// 	"Id": "4400c969-a6f7-44f2-8200-a69600ec3310",
	// 	"Name": "Stockholm"
	// }];
    //


	

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

