import {Component} from '@angular/core'

@Component({
	selector: 'rta-historical-overview',
	templateUrl: './rta.historical.overview.html'
})

export class RtaHistoricalOverviewComponent {
	organizationPickerOpen: boolean;
	clearOrganizationSelection() {
		console.log('hej')
	}
	
	closePicker(event:Object) {
		this.organizationPickerOpen = false;
	}

	sites: any [] = [{
		"Teams": [{
			"Id": "94230b19-c23e-46c2-a500-a69600b7c715",
			"Name": "Team Linda"
		}, {"Id": "8aabe24f-cb06-40cf-9acc-a69600b70691", "Name": "Team Robert"}],
		"Id": "d341c1ba-7079-46bb-a47c-a69600b5b585",
		"Name": "Denver"
	}, {
		"Teams": [{"Id": "34590a63-6331-4921-bc9f-9b5e015ab495", "Name": "Team Preferences"}],
		"Id": "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
		"Name": "London"
	}, {
		"Teams": [{
			"Id": "fa169ee6-2d71-4d6c-bb6d-a69600b5c0bd",
			"Name": "Team Angelica"
		}, {"Id": "0f6d7644-1134-4561-97ca-a69600b6021d", "Name": "Team Joshua"}],
		"Id": "6c48e01f-87a8-47c6-b27f-a69600b5982f",
		"Name": "Manila"
	}, {
		"Teams": [{"Id": "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495", "Name": "Team 1"}],
		"Id": "6a21c802-7a34-4917-8dfd-9b5e015ab461",
		"Name": "Paris"
	}, {
		"Teams": [{
			"Id": "04d552aa-b39e-4afc-81c2-a69600f2a59e",
			"Name": "Team Gunnar"
		}, {"Id": "589b1752-d391-476f-87f6-a69600f2ad11", "Name": "Team Therese"}],
		"Id": "4400c969-a6f7-44f2-8200-a69600ec3310",
		"Name": "Stockholm"
	}];
}

