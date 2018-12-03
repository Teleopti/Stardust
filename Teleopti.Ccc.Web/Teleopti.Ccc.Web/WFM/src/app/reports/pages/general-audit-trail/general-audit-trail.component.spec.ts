import { HttpClientModule } from '@angular/common/http';
import { DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { configureTestSuite } from '@wfm/test';
import {
	en_US,
	NzAutocompleteModule,
	NzButtonModule,
	NzDatePickerModule,
	NzFormModule,
	NzInputModule,
	NzTableModule,
	NzToolTipModule,
	NZ_I18N
} from 'ng-zorro-antd';
import { of } from 'rxjs';
import { UserService } from '../../../core/services';
import { AuditTrailService } from '../../services';
import { GeneralAuditTrailComponent as GeneralAuditTrail } from './general-audit-trail.component';

describe('SearchPageComponent', () => {
	let component: GeneralAuditTrail;
	let fixture: ComponentFixture<GeneralAuditTrail>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [GeneralAuditTrail],
			imports: [
				MockTranslationModule,
				HttpClientModule,
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
				{ provide: UserService, useClass: MockUserService },
				{ provide: NZ_I18N, useValue: en_US }
			]
		}).compileComponents();
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
});

class MockUserService implements Partial<UserService> {
	get preferences$() {
		return of({
			Id: '1',
			UserName: '',
			Language: 'en-US',
			IsTeleoptiApplicationLogon: true,
			DateFormatLocale: 'en-US'
		});
	}
}

class Page {
	fixture: ComponentFixture<GeneralAuditTrail>;

	constructor(fixture: ComponentFixture<GeneralAuditTrail>) {
		this.fixture = fixture;
	}

	private queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}
}
