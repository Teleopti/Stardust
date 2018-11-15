import { HttpClientModule } from '@angular/common/http';
import { DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
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
import { of } from 'rxjs';
import { configureTestSuite } from '@wfm/test';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { GeneralAuditTrailComponent as GeneralAuditTrail } from './general-audit-trail.component';
import { AuditTrailService } from '../../services';
import { UserService, UserPreferences } from '../../../core/services';

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
	getPreferences() {
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
