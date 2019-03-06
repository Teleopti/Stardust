import { async, ComponentFixture, TestBed, tick, fakeAsync } from '@angular/core/testing';
import { ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import {
	NzButtonModule,
	NzFormModule,
	NzInputModule,
	NzTableModule,
	NzToolTipModule,
	NzAutocompleteModule,
	NzDatePickerModule,
	NZ_I18N,
	en_US
} from 'ng-zorro-antd';
import { of, Observable } from 'rxjs';
import { configureTestSuite, PageObject } from '@wfm/test';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { GeneralAuditTrailComponent as GeneralAuditTrail } from './general-audit-trail.component';
import { AuditTrailService, AuditTrailsResponse } from '../../services';
import { UserService } from '../../../core/services';
import { Person, AuditEntry } from 'src/app/shared/types';
import * as _moment_ from 'moment-timezone';
import { HttpClient } from '@angular/common/http';

class MockUserService implements Partial<UserService> {
	public get preferences$() {
		return of({
			Id: '1',
			UserName: '',
			Language: 'en-US',
			IsTeleoptiApplicationLogon: true,
			DateFormatLocale: 'en-US'
		});
	}
}

class MockAuditTrailService implements Partial<AuditTrailService> {
	getStaffingAuditTrail(personId: string, startDate: string, endDate: string): Observable<AuditTrailsResponse> {
		return of({
			AuditEntries: []
		});
	}

	getPersonByKeyword(keyword: string) {
		return of({
			Persons: []
		});
	}
}

describe('GeneralAuditTrailComponent', () => {
	let component: GeneralAuditTrail;
	let fixture: ComponentFixture<GeneralAuditTrail>;
	let page: Page;
	let auditTrailService: AuditTrailService;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [GeneralAuditTrail],
			imports: [
				MockTranslationModule,
				ReactiveFormsModule,
				NoopAnimationsModule,
				NzFormModule,
				NzButtonModule,
				NzTableModule,
				NzInputModule,
				NzToolTipModule,
				NzAutocompleteModule,
				NzDatePickerModule
			],
			providers: [
				AuditTrailService,
				{ provide: HttpClient, useValue: {} },
				{ provide: UserService, useClass: MockUserService },
				{ provide: NZ_I18N, useValue: en_US }
			]
		}).compileComponents();
		auditTrailService = TestBed.get(AuditTrailService);
		(window as any).moment = _moment_;
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(GeneralAuditTrail);
		component = fixture.componentInstance;
		page = new Page(fixture);
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should get list of persons by keyword', fakeAsync(() => {
		const spyAuditTrailService = spyOn(auditTrailService, 'getPersonByKeyword');
		const mockPerson: Person = { Id: '', FirstName: 'Foo', LastName: 'Bar' };
		spyAuditTrailService.and.returnValue(of({ Persons: [mockPerson] }));
		component.personPickerControl.setValue('Foo');
		tick(700);
		expect(spyAuditTrailService.calls.count()).toBe(1);
		expect(component.personList.length).toBe(1);
	}));

	it('should populate table with staffingAuditTrailData', () => {
		const spyAuditTrailService = spyOn(auditTrailService, 'getStaffingAuditTrail');
		const mockAuditEntry: AuditEntry = {
			Action: 'Importera bemanning',
			ActionPerformedBy: 'Ashley Andeen',
			Context: 'Staffing',
			Data: 'File name: template.csv',
			TimeStamp: '2018-11-27T17:41:19.95'
		};
		const mockStaffingAuditTrailData: AuditTrailsResponse = { AuditEntries: [mockAuditEntry] };
		spyAuditTrailService.and.returnValue(of(mockStaffingAuditTrailData));

		component.submitForm();

		expect(spyAuditTrailService.calls.count()).toBe(1);
		expect(component.auditTrailData.length).toBe(1);
	});

	it('should show error message when date range is bigger than 7 days', () => {
		const searchForm: AbstractControl = component.searchForm;
		const dateFormat = 'YYYY-MM-DD';

		searchForm.value.dateRange = [moment('2018-12-01', dateFormat), moment('2018-12-30', dateFormat)];
		expect(searchForm.get('dateRange').hasError('INVALID_DATE_RANGE_ERROR'));
	});

	it('should not be able to search without a person selected', () => {
		expect(component.notValidFields()).toBeTruthy();
	});
});

class Page extends PageObject {}
