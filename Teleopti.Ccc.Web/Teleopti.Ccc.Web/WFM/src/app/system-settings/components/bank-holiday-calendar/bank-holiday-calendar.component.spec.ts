import { DOCUMENT } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NgZorroAntdModule } from 'ng-zorro-antd';

import { configureTestSuite } from '@wfm/test';
import { UserService } from 'src/app/core/services';
import { BankHolidayCalendarComponent } from './bank-holiday-calendar.component';
import { BankHolidayCalendarAddComponent } from '../bank-holiday-calendar-add';

describe('BankHolidayCalendarComponent', () => {
	let fixture: ComponentFixture<BankHolidayCalendarComponent>;
	let document: Document;
	let component: BankHolidayCalendarComponent;

	configureTestSuite();
	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [BankHolidayCalendarComponent, BankHolidayCalendarAddComponent],
			imports: [
				TranslateModule.forRoot(),
				NgZorroAntdModule.forRoot(),
				FormsModule,
				ReactiveFormsModule,
				HttpClientTestingModule
			],
			providers: [TranslateService, UserService]
		}).compileComponents();

		fixture = TestBed.createComponent(BankHolidayCalendarComponent);
		document = TestBed.get(DOCUMENT);
		component = fixture.componentInstance;
		fixture.autoDetectChanges(true);
	}));

	it('should create component', async(() => {
		expect(component).toBeTruthy();
	}));

	it('should render title', async(() => {
		var bankHolidayCalendarSettings = document.getElementsByClassName('bank-holiday-settings')[0];

		expect(bankHolidayCalendarSettings).toBeTruthy();
		expect(bankHolidayCalendarSettings.getElementsByTagName('h2').length).toBe(1);
		expect(
			bankHolidayCalendarSettings.getElementsByTagName('h2')[0].innerHTML.indexOf('BankHolidayCalendars') > -1
		).toBeTruthy();
	}));

	it('should show add new bank holiday calendar panel after clicking plus icon', async(() => {
		document.getElementsByClassName('add-bank-holiday-calendar-icon')[0].dispatchEvent(new Event('click'));

		var addCalendarPanel = document.getElementsByClassName('add-new-bank-holiday-calendar')[0];

		expect(addCalendarPanel).toBeTruthy();
	}));
});
