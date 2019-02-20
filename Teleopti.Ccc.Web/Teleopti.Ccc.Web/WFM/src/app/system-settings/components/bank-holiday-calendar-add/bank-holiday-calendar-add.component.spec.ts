import { DOCUMENT } from '@angular/common';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import en from '@angular/common/locales/en';
import zh from '@angular/common/locales/zh';

import { async, ComponentFixture, TestBed, fakeAsync, flush } from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { configureTestSuite } from '@wfm/test';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { UserService } from 'src/app/core/services';
import { ToggleMenuService } from 'src/app/menu/shared/toggle-menu.service';
import { BankHolidayCalendarAddComponent } from './bank-holiday-calendar-add.component';
import { BankCalendarDataService } from '../../shared';
import { registerLocaleData } from '@angular/common';

fdescribe('BankHolidayCalendarAddComponent', () => {
	let fixture: ComponentFixture<BankHolidayCalendarAddComponent>;
	let document: Document;
	let component: BankHolidayCalendarAddComponent;

	configureTestSuite();

	beforeAll(() => {
		registerLocaleData(zh);
	});

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [BankHolidayCalendarAddComponent],
			imports: [
				MockTranslationModule,
				NgZorroAntdModule,
				FormsModule,
				ReactiveFormsModule,
				HttpClientTestingModule,
				NoopAnimationsModule
			],
			providers: [
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

		fixture = TestBed.createComponent(BankHolidayCalendarAddComponent);
		document = TestBed.get(DOCUMENT);
		component = fixture.componentInstance;
		fixture.autoDetectChanges(true);
	}));

	afterAll(() => {
		registerLocaleData(en);
	});

	it('should create component', () => {
		expect(component).toBeTruthy();
	});

	it('should render name input box', () => {
		const addBankHolidayCalendarHeader = document.getElementsByClassName('add-new-bank-holiday-calendar-header')[0];

		expect(addBankHolidayCalendarHeader).toBeTruthy();
		expect(addBankHolidayCalendarHeader.getElementsByClassName('ant-input').length).toBe(1);
		expect(addBankHolidayCalendarHeader.getElementsByClassName('ant-input')[0].getAttribute('placeholder')).toBe(
			'NewCalendarName'
		);
	});

	it('should render cancel and save button', () => {
		const addBankHolidayCalendarPanel = document.getElementsByClassName('add-new-bank-holiday-calendar')[0];

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

	it('should trim space before and after when checking the existing name', () => {
		component.bankHolidayCalendarsList = [
			{
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
			}
		];

		component.newCalendarName = 'Bank holiday calendar ';
		component.checkNewCalendarName();
		fixture.detectChanges();

		expect(component.nameAlreadyExisting).toBe(true);
	});

	it('should not add duplicated date', () => {
		component.dateChangeCallback(new Date('2015-01-10'));
		component.dateChangeCallback(new Date('2015-01-10'));

		expect(component.selectedDates.length).toBe(1);
	});

	it('should be able to add a date back after removing it', () => {
		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();

		const addNewBankHolidayCalendarPanel = document.getElementsByClassName('add-new-bank-holiday-calendar')[0];
		const dateRows = addNewBankHolidayCalendarPanel
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item');

		expect(dateRows.length).toBe(1);
		expect(dateRows[0].innerHTML.indexOf('2015-01-10') > -1).toBeTruthy();
		expect(dateRows[0].innerHTML.indexOf('BankHoliday') > -1).toBeTruthy();

		component.removeDate(component.selectedDates[0]);
		fixture.detectChanges();

		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();

		expect(component.selectedDates.length).toBe(1);
		expect(component.selectedDates[0].Date).toBe('2015-01-10');
		expect(component.selectedDates[0].Description).toBe('BankHoliday');
	});

	it('should categorize dates by year', () => {
		component.dateChangeCallback(new Date('2015-01-10'));
		component.dateChangeCallback(new Date('2016-01-10'));
		fixture.detectChanges();

		expect(component.newCalendarYears.length).toBe(2);

		expect(component.newCalendarYears[0].Year).toBe('2015');
		expect(component.newCalendarYears[0].Dates.length).toBe(1);
		expect(component.newCalendarYears[0].Dates[0].Date).toBe('2015-01-10');

		expect(component.newCalendarYears[1].Year).toBe('2016');
		expect(component.newCalendarYears[1].Dates.length).toBe(1);
		expect(component.newCalendarYears[1].Dates[0].Date).toBe('2016-01-10');
	});
});
