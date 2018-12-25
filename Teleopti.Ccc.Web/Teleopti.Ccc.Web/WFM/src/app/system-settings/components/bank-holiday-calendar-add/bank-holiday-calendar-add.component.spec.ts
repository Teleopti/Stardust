import { DOCUMENT } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NgZorroAntdModule } from 'ng-zorro-antd';

import { configureTestSuite } from '@wfm/test';
import { BankHolidayCalendarAddComponent } from './bank-holiday-calendar-add.component';

fdescribe('BankHolidayCalendarAddComponent', () => {
	let fixture: ComponentFixture<BankHolidayCalendarAddComponent>;
	let document: Document;
	let component: BankHolidayCalendarAddComponent;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [BankHolidayCalendarAddComponent],
			imports: [
				TranslateModule.forRoot(),
				NgZorroAntdModule.forRoot(),
				FormsModule,
				ReactiveFormsModule,
				HttpClientTestingModule
			],
			providers: [TranslateService]
		}).compileComponents();

		fixture = TestBed.createComponent(BankHolidayCalendarAddComponent);
		document = TestBed.get(DOCUMENT);
		component = fixture.componentInstance;
		fixture.autoDetectChanges(true);
	}));

	it('should create component', () => {
		expect(component).toBeTruthy();
	});

	it('should render name and year input box', () => {
		var addBankHolidayCalendarPanel = document.getElementsByClassName('add-new-bank-holiday-calendar')[0];

		expect(addBankHolidayCalendarPanel).toBeTruthy();
		expect(addBankHolidayCalendarPanel.getElementsByClassName('ant-input').length).toBe(2);
		expect(addBankHolidayCalendarPanel.getElementsByTagName('nz-year-picker').length).toBe(1);
	});

	it('should render cancel and save button', () => {
		var addBankHolidayCalendarPanel = document.getElementsByClassName('add-new-bank-holiday-calendar')[0];

		expect(addBankHolidayCalendarPanel.getElementsByClassName('ant-btn').length).toBe(2);
		expect(
			addBankHolidayCalendarPanel.getElementsByClassName('ant-btn')[0].getElementsByTagName('span')[0].innerText
		).toBe('Cancel');
		expect(
			addBankHolidayCalendarPanel.getElementsByClassName('ant-btn')[1].getElementsByTagName('span')[0].innerText
		).toBe('Save');
	});
});
