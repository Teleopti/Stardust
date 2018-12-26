import { DOCUMENT } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { IStateService } from 'angular-ui-router';

import { configureTestSuite } from '@wfm/test';
import { UserService } from 'src/app/core/services';
import { PasswordService } from 'src/app/authentication/services/password.service';
import { BankHolidayCalendarEditComponent } from './bank-holiday-calendar-edit.component';

class mockStateService implements Partial<IStateService> {
	public current: {
		name: 'systemSettings';
	};

	public href() {
		return '';
	}
}

describe('BankHolidayCalendarEditComponent', () => {
	let fixture: ComponentFixture<BankHolidayCalendarEditComponent>;
	let document: Document;
	let component: BankHolidayCalendarEditComponent;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [BankHolidayCalendarEditComponent],
			imports: [
				TranslateModule.forRoot(),
				NgZorroAntdModule.forRoot(),
				FormsModule,
				ReactiveFormsModule,
				HttpClientTestingModule
			],
			providers: [
				{
					provide: '$state',
					useClass: mockStateService
				},
				UserService,
				PasswordService,
				TranslateService
			]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(BankHolidayCalendarEditComponent);
		document = TestBed.get(DOCUMENT);
		component = fixture.componentInstance;
	});

	it('should create component', () => {
		expect(component).toBeTruthy();
	});
});
