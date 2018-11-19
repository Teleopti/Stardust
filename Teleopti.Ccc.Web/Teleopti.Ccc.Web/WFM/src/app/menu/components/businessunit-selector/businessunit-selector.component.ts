import { DOCUMENT } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { BusinessUnit, BusinessUnitService } from '../../shared/businessunit.service';

@Component({
	selector: 'business-unit-selector',
	templateUrl: './businessunit-selector.component.html',
	styleUrls: ['./businessunit-selector.component.scss']
})
export class BusinessUnitSelectorComponent implements OnInit {
	businessUnits: BusinessUnit[] = [];

	constructor(private buService: BusinessUnitService, @Inject(DOCUMENT) private document: Document) {}

	ngOnInit() {
		this.buService.getBusinessUnits().subscribe(businessUnits => {
			this.businessUnits = businessUnits;
			if (!this.selectedBusinessUnit) {
				const primaryBusinessUnit = this.businessUnits[0];
				this.buService.selectBusinessUnit(primaryBusinessUnit.Id);
			}
		});
	}

	get selectedBusinessUnit(): BusinessUnit {
		const BuId = this.buService.getSelectedBusinessUnitId();
		return this.businessUnits.find(bu => bu.Id === BuId);
	}

	handleSelect(bu: BusinessUnit) {
		this.buService.selectBusinessUnit(bu.Id);
		this.document.location.reload();
	}
}
