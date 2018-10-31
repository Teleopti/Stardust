import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite } from '@wfm/test';
import { NzButtonModule, NzDropDownModule } from 'ng-zorro-antd';
import { of } from 'rxjs';
import { BusinessUnitService } from '../../shared/businessunit.service';
import { BusinessUnitSelectorComponent } from './businessunit-selector.component';

class MockBusinessUnitSelectorService implements Partial<BusinessUnitService> {
	getBusinessUnits() {
		return of([{ Id: '1', Name: 'First' }, { Id: '2', Name: 'Second' }]);
	}
	getSelectedBusinessUnitId() {
		return '1';
	}
}

describe('BusinessUnitSelector', () => {
	let component: BusinessUnitSelectorComponent;
	let fixture: ComponentFixture<BusinessUnitSelectorComponent>;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [BusinessUnitSelectorComponent],
			imports: [NzDropDownModule, NzButtonModule],
			providers: [
				{
					provide: BusinessUnitService,
					useClass: MockBusinessUnitSelectorService
				}
			]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(BusinessUnitSelectorComponent);
		component = fixture.componentInstance;
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should get business units', () => {
		fixture.detectChanges();
		expect(fixture.componentInstance.businessUnits.length).toEqual(2);
	});

	it('should store selected business unit', () => {
		fixture.detectChanges();
		expect(component.selectedBusinessUnit).toBeTruthy();
	});
});
