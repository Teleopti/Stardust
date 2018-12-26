import { DOCUMENT } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NgZorroAntdModule } from 'ng-zorro-antd';

import { configureTestSuite } from '@wfm/test';
import { UserService } from 'src/app/core/services';
import { BankHolidayCalendarAddComponent } from './bank-holiday-calendar-add.component';

describe('BankHolidayCalendarAddComponent', () => {
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
			providers: [TranslateService, UserService]
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
			addBankHolidayCalendarPanel
				.getElementsByClassName('ant-btn')[0]
				.getElementsByTagName('span')[0]
				.innerText.trim()
		).toBe('Cancel');
		expect(
			addBankHolidayCalendarPanel
				.getElementsByClassName('ant-btn')[1]
				.getElementsByTagName('span')[0]
				.innerText.trim()
		).toBe('Save');
	});

	it('should not add duplicated date', () => {
		component.newYearTab(new Date('2015-01-10T00:00:00.000Z'));

		component.addNewDateForYear(new Date('2015-01-10T00:00:00.000Z'), component.newCalendarYears[0]);
		component.addNewDateForYear(new Date('2015-01-10T00:00:00.000Z'), component.newCalendarYears[0]);

		expect(component.newCalendarYears[0].Dates.length).toBe(1);
	});

	it('should focus to another year after deleting one year tab', () => {
		component.newYearTab(new Date('2015-01-10T00:00:00.000Z'));
		component.addNewDateForYear(new Date('2015-01-10T00:00:00.000Z'), component.newCalendarYears[0]);

		component.newYearTab(new Date('2016-01-10T00:00:00.000Z'));
		component.addNewDateForYear(new Date('2016-01-10T00:00:00.000Z'), component.newCalendarYears[1]);

		component.deleteYearTab(component.newCalendarYears[1]);

		expect(component.newCalendarTabIndex).toBe(0);
		expect(component.newCalendarYears.length).toBe(1);
		expect(component.newCalendarYears[0].Active).toBe(true);
	});
});
