import { DOCUMENT } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { NgZorroAntdModule } from 'ng-zorro-antd';

import { configureTestSuite } from '@wfm/test';
import { MockTranslationModule, MockTranslateService } from '@wfm/mocks/translation';
import { UserService } from 'src/app/core/services';
import { BankHolidayCalendarEditComponent } from './bank-holiday-calendar-edit.component';
import { ToggleMenuService } from 'src/app/menu/shared/toggle-menu.service';
import { BankCalendarDataService } from '../../shared';
import { BankHolidayCalendarItem } from '../../interface';

describe('BankHolidayCalendarEditComponent', () => {
	let fixture: ComponentFixture<BankHolidayCalendarEditComponent>;
	let document: Document;
	let component: BankHolidayCalendarEditComponent;
	let httpTestingController: HttpTestingController;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [BankHolidayCalendarEditComponent],
			imports: [
				MockTranslationModule,
				NgZorroAntdModule,
				FormsModule,
				HttpClientTestingModule,
				NoopAnimationsModule
			],
			providers: [
				MockTranslateService,
				UserService,
				{
					provide: ToggleMenuService,
					useValue: new ToggleMenuService({
						innerWidth: 1200
					} as Window)
				},
				BankCalendarDataService
			]
		}).compileComponents();

		fixture = TestBed.createComponent(BankHolidayCalendarEditComponent);
		document = TestBed.get(DOCUMENT);
		component = fixture.componentInstance;
		httpTestingController = TestBed.get(HttpTestingController);
	}));

	it('should create component', () => {
		expect(component).toBeTruthy();
	});

	it('should render cancel and save button', async(() => {
		component.edittingCalendar = {
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'Bank holiday calendar',
			CurrentYearIndex: 0,
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

		const editBankHolidayCalendarPanel = document.getElementsByClassName('edit-bank-holiday-calendar')[0];
		const operationButtons = editBankHolidayCalendarPanel.getElementsByClassName('operation-buttons')[0];

		expect(operationButtons.getElementsByClassName('ant-btn').length).toBe(2);

		expect(
			operationButtons
				.getElementsByClassName('ant-btn')[0]
				.getElementsByTagName('span')[0]
				.innerText.trim()
		).toBe('Cancel');
		expect(
			operationButtons
				.getElementsByClassName('ant-btn')[1]
				.getElementsByTagName('span')[0]
				.innerText.trim()
		).toBe('Save');
	}));

	it('should trim space before and after when checking the existing name', () => {
		const calendar = {
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'Bank holiday calendar',
			Years: [
				{
					Year: '2013',
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2013-01-09',
							Description: 'BankHoliday1',
							IsDeleted: false
						},
						{
							Id: '876b72ef-4238-423a-a05b-a9c3003b10df',
							Date: '2013-01-10',
							Description: 'BankHoliday2',
							IsDeleted: false
						}
					]
				}
			]
		};
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([calendar]);
		component.edittingCalendar = calendar as BankHolidayCalendarItem;
		component.edittingCalendar.CurrentYearIndex = 0;
		fixture.detectChanges();

		component.edittingCalendarName = 'Bank holiday calendar ';
		component.checkNewCalendarName();
		fixture.detectChanges();

		expect(component.nameAlreadyExisting).toBe(true);
	});

	it('should render existing date rows', () => {
		component.edittingCalendar = {
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'Bank holiday calendar',
			CurrentYearIndex: 0,
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

		const editBankHolidayCalendarPanel = document.getElementsByClassName('edit-bank-holiday-calendar')[0];
		const dateRows = editBankHolidayCalendarPanel
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
			CurrentYearIndex: 0,
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

		const editBankHolidayCalendarPanel = document.getElementsByClassName('edit-bank-holiday-calendar')[0];
		const dateRows = editBankHolidayCalendarPanel
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item');

		expect(dateRows.length).toBe(1);
		expect(component.edittingCalendarYears[0].Dates.length).toBe(1);
		expect(dateRows[0].innerHTML.indexOf('2013-01-09') > -1).toBeTruthy();
		expect(dateRows[0].innerHTML.indexOf('BankHoliday 1') > -1).toBeTruthy();

		component.dateChangeCallback(new Date('2013-01-10'), component.edittingCalendarYears[0]);
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
			CurrentYearIndex: 0,
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

		const editBankHolidayCalendarPanel = document.getElementsByClassName('edit-bank-holiday-calendar')[0];
		const dateRows = editBankHolidayCalendarPanel
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
			CurrentYearIndex: 0,
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

		const editBankHolidayCalendarPanel = document.getElementsByClassName('edit-bank-holiday-calendar')[0];
		const dateRows = editBankHolidayCalendarPanel
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

	it('should change the binding date of datepicker to last active date or first date in the list after removing a date', () => {
		component.newYearTab(new Date('2015-01-10T00:00:00.000Z'));
		component.dateChangeCallback(new Date('2015-01-10T00:00:00.000Z'), component.edittingCalendarYears[0]);
		component.dateChangeCallback(new Date('2015-01-11T00:00:00.000Z'), component.edittingCalendarYears[0]);

		component.removeDateOfYear(component.edittingCalendarYears[0].Dates[1], component.edittingCalendarYears[0]);

		expect(moment(component.edittingCalendarYears[0].YearDate).format('YYYY-MM-DD')).toBe(
			moment(new Date(component.edittingCalendarYears[0].Dates[0].Date)).format('YYYY-MM-DD')
		);
	});
});
