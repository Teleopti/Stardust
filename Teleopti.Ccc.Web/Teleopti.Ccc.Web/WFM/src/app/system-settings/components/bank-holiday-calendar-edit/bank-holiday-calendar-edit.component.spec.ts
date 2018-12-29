import { DOCUMENT } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NgZorroAntdModule } from 'ng-zorro-antd';

import { configureTestSuite } from '@wfm/test';
import { UserService } from 'src/app/core/services';
import { BankHolidayCalendarEditComponent } from './bank-holiday-calendar-edit.component';

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
				HttpClientTestingModule,
				BrowserAnimationsModule
			],
			providers: [TranslateService, UserService]
		}).compileComponents();

		fixture = TestBed.createComponent(BankHolidayCalendarEditComponent);
		document = TestBed.get(DOCUMENT);
		component = fixture.componentInstance;
	}));

	it('should create component', () => {
		expect(component).toBeTruthy();
	});

	it('should render delete, cancel and save button', async(() => {
		component.edittingCalendar = {
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'Bank holiday calendar',
			Years: [
				{
					Year: '2013',
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2013-01-09',
							Description: 'BankHoliday',
							IsDeleted: false
						},
						{
							Id: '876b72ef-4238-423a-a05b-a9c3003b10df',
							Date: '2013-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		};
		fixture.detectChanges();

		let editBankHolidayCalendarPanel = document.getElementsByClassName('edit-bank-holiday-calendar')[0];
		let operationButtons = editBankHolidayCalendarPanel.getElementsByClassName('operation-buttons')[0];

		expect(operationButtons.getElementsByClassName('ant-btn').length).toBe(3);
		expect(
			operationButtons
				.getElementsByClassName('ant-btn')[0]
				.getElementsByTagName('span')[0]
				.innerText.trim()
		).toBe('Delete');
		expect(
			operationButtons
				.getElementsByClassName('ant-btn')[1]
				.getElementsByTagName('span')[0]
				.innerText.trim()
		).toBe('Cancel');
		expect(
			operationButtons
				.getElementsByClassName('ant-btn')[2]
				.getElementsByTagName('span')[0]
				.innerText.trim()
		).toBe('Save');
	}));

	it('should render existing date rows', () => {
		component.edittingCalendar = {
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'Bank holiday calendar',
			Years: [
				{
					Year: '2013',
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2013-01-09',
							Description: 'BankHoliday 1',
							IsDeleted: false
						},
						{
							Id: '876b72ef-4238-423a-a05b-a9c3003b10df',
							Date: '2013-01-10',
							Description: 'BankHoliday 2',
							IsDeleted: false
						}
					]
				}
			]
		};
		fixture.detectChanges();

		let editBankHolidayCalendarPanel = document.getElementsByClassName('edit-bank-holiday-calendar')[0];
		let dateRows = editBankHolidayCalendarPanel
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item');

		expect(dateRows.length).toBe(2);
		expect(dateRows[0].innerHTML.indexOf('2013-01-09') > -1).toBeTruthy();
		expect(dateRows[0].innerHTML.indexOf('BankHoliday 1') > -1).toBeTruthy();
		expect(dateRows[1].innerHTML.indexOf('2013-01-10') > -1).toBeTruthy();
		expect(dateRows[1].innerHTML.indexOf('BankHoliday 2') > -1).toBeTruthy();
	});

	it('should be able to add new date', () => {
		component.edittingCalendar = {
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'Bank holiday calendar',
			Years: [
				{
					Year: '2013',
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2013-01-09',
							Description: 'BankHoliday 1',
							IsDeleted: false
						}
					]
				}
			]
		};
		fixture.detectChanges();

		let editBankHolidayCalendarPanel = document.getElementsByClassName('edit-bank-holiday-calendar')[0];
		let dateRows = editBankHolidayCalendarPanel
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item');

		expect(dateRows.length).toBe(1);
		expect(component.edittingCalendarYears[0].Dates.length).toBe(1);
		expect(dateRows[0].innerHTML.indexOf('2013-01-09') > -1).toBeTruthy();
		expect(dateRows[0].innerHTML.indexOf('BankHoliday 1') > -1).toBeTruthy();

		component.addNewDateForYear(new Date('2013-01-10'), component.edittingCalendarYears[0]);
		fixture.detectChanges();

		expect(dateRows.length).toBe(2);
		expect(component.edittingCalendarYears[0].Dates.length).toBe(2);
		expect(dateRows[1].innerHTML.indexOf('2013-01-10') > -1).toBeTruthy();
		expect(dateRows[1].innerHTML.indexOf('BankHoliday') > -1).toBeTruthy();
	});

	it('should set IsDeleted to true when removing a date', () => {
		component.edittingCalendar = {
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'Bank holiday calendar',
			Years: [
				{
					Year: '2013',
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2013-01-09',
							Description: 'BankHoliday 1',
							IsDeleted: false
						},
						{
							Id: '876b72ef-4238-423a-a05b-a9c3003b10df',
							Date: '2013-01-10',
							Description: 'BankHoliday 2',
							IsDeleted: false
						}
					]
				}
			]
		};
		fixture.detectChanges();

		let editBankHolidayCalendarPanel = document.getElementsByClassName('edit-bank-holiday-calendar')[0];
		let dateRows = editBankHolidayCalendarPanel
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item');

		expect(dateRows.length).toBe(2);
		expect(component.edittingCalendarYears[0].Dates.length).toBe(2);

		dateRows[1].getElementsByClassName('remove-date-icon')[0].dispatchEvent(new Event('click'));
		fixture.detectChanges();

		expect(dateRows.length).toBe(1);
		expect(component.edittingCalendarYears[0].Dates.length).toBe(1);
		expect(component.edittingCalendarYears[0].ModifiedDates[0].IsDeleted).toBe(true);
	});

	it('should be able to change date description', () => {
		component.edittingCalendar = {
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'Bank holiday calendar',
			Years: [
				{
					Year: '2013',
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2013-01-09',
							Description: 'BankHoliday 1',
							IsDeleted: false
						},
						{
							Id: '876b72ef-4238-423a-a05b-a9c3003b10df',
							Date: '2013-01-10',
							Description: 'BankHoliday 2',
							IsDeleted: false
						}
					]
				}
			]
		};
		fixture.detectChanges();

		let editBankHolidayCalendarPanel = document.getElementsByClassName('edit-bank-holiday-calendar')[0];
		let dateRows = editBankHolidayCalendarPanel
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item');

		expect(dateRows.length).toBe(2);
		expect(component.edittingCalendarYears[0].Dates.length).toBe(2);

		component.edittingCalendarYears[0].Dates[1].Description = 'new description';
		component.updateDateDescription(
			component.edittingCalendarYears[0].Dates[1],
			component.edittingCalendarYears[0]
		);
		fixture.detectChanges();

		expect(dateRows.length).toBe(2);
		expect(component.edittingCalendarYears[0].Dates.length).toBe(2);
		expect(component.edittingCalendarYears[0].ModifiedDates[0].IsDeleted).toBe(false);
		expect(component.edittingCalendarYears[0].ModifiedDates[0].Description).toBe('new description');
	});
});
