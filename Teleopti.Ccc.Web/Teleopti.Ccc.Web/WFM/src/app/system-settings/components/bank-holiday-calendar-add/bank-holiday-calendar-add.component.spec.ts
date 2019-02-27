import { DOCUMENT } from '@angular/common';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import en from '@angular/common/locales/en';
import zh from '@angular/common/locales/zh';

import { async, ComponentFixture, TestBed } from '@angular/core/testing';
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

describe('BankHolidayCalendarAddComponent', () => {
	let fixture: ComponentFixture<BankHolidayCalendarAddComponent>;
	let document: Document;
	let component: BankHolidayCalendarAddComponent;
	let httpTestingController: HttpTestingController;

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
		httpTestingController = TestBed.get(HttpTestingController);
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

	it('should save calendar name after it is changed', () => {
		component.newCalendarName = 'I am a new calendar';
		document.querySelector('.add-new-bank-holiday-calendar-header input').dispatchEvent(new Event('change'));
		fixture.detectChanges();

		const request = httpTestingController.match('../api/BankHolidayCalendars/Save')[0];
		expect(request.request.body.Id).toBe(null);
		expect(request.request.body.Name).toBe('I am a new calendar');
		expect(request.request.body.Years.length).toBe(0);

		request.flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar - name from backend which should match the cal name from frontend',
			Years: []
		});
		fixture.detectChanges();
		expect(component.newCalendarName).toBe(
			'I am a new calendar - name from backend which should match the cal name from frontend'
		);
	});

	it('should render Back button', () => {
		const addBankHolidayCalendarPanel = document.getElementsByClassName('add-new-bank-holiday-calendar')[0];

		expect(addBankHolidayCalendarPanel.getElementsByClassName('ant-btn').length).toBe(1);
		expect(
			addBankHolidayCalendarPanel
				.getElementsByClassName('ant-btn')[0]
				.getElementsByTagName('span')[0]
				.innerText.trim()
		).toBe('Back');
	});

	it('should trim space before and after when checking the existing name', () => {
		component.bankHolidayCalendarsList = [
			{
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
			}
		];

		component.newCalendarName = 'Bank holiday calendar ';
		component.saveNewCalendarName();
		fixture.detectChanges();

		expect(component.nameAlreadyExisting).toBe(true);
	});

	it('should be able to add dates', () => {
		component.newCalendarName = 'New Bank Holiday Calendar';
		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '86784a24-ea35-466a-a535-839a22d06bf4',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		component.dateChangeCallback(new Date('2015-01-11'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '86784a24-ea35-466a-a535-839a22d06bf4',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						},
						{
							Id: '06980577-cc30-434e-88d3-e23505671a38',
							Date: '2015-01-11',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		const dateContent = document.getElementsByClassName('bank-holiday-calendar-date-content')[0];
		const dateRows = dateContent
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item');

		expect(dateContent.getElementsByTagName('nz-collapse-panel').length).toBe(1);
		expect(dateRows.length).toBe(2);
		expect(dateRows[0].innerHTML.indexOf('2015-01-10') > -1).toBeTruthy();
		expect(dateRows[0].innerHTML.indexOf('BankHoliday') > -1).toBeTruthy();
		expect(dateRows[1].innerHTML.indexOf('2015-01-11') > -1).toBeTruthy();
		expect(dateRows[1].innerHTML.indexOf('BankHoliday') > -1).toBeTruthy();

		expect(component.newCalendarYears.length).toBe(1);
		expect(component.newCalendarYears[0].Dates.length).toBe(2);
		expect(component.selectedDatesTimeList.length).toBe(2);
	});

	it("should be able to change bank holiday's description", () => {
		component.newCalendarName = 'New Bank Holiday Calendar';
		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '86784a24-ea35-466a-a535-839a22d06bf4',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		const dateContent = document.getElementsByClassName('bank-holiday-calendar-date-content')[0];
		let dateRows = dateContent
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item');

		expect(dateContent.getElementsByTagName('nz-collapse-panel').length).toBe(1);
		expect(dateRows.length).toBe(1);
		expect(dateRows[0].innerHTML.indexOf('2015-01-10') > -1).toBeTruthy();
		expect(dateRows[0].innerHTML.indexOf('BankHoliday') > -1).toBeTruthy();

		dateRows[0].querySelector('input').value = 'BankHoliday New Description';
		dateRows[0].querySelector('input').dispatchEvent(new Event('change'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '86784a24-ea35-466a-a535-839a22d06bf4',
							Date: '2015-01-10',
							Description: 'BankHoliday New Description',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		dateRows = dateContent
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item');

		expect(dateRows[0].innerHTML.indexOf('2015-01-10') > -1).toBeTruthy();
		expect(dateRows[0].innerHTML.indexOf('BankHoliday New Description') > -1).toBeTruthy();
	});

	it('should set is last added after adding dates', () => {
		component.newCalendarName = 'New Bank Holiday Calendar';
		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();
		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '86784a24-ea35-466a-a535-839a22d06bf4',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		component.dateChangeCallback(new Date('2015-01-11'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '86784a24-ea35-466a-a535-839a22d06bf4',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						},
						{
							Id: '06980577-cc30-434e-88d3-e23505671a38',
							Date: '2015-01-11',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		const dateContent = document.getElementsByClassName('bank-holiday-calendar-date-content')[0];
		const dateRows = dateContent
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item');

		expect(dateContent.getElementsByTagName('nz-collapse-panel').length).toBe(1);
		expect(dateRows.length).toBe(2);

		expect(dateRows[1].getElementsByClassName('selected-date-indicator').length).toBe(1);
	});

	it('should not add duplicated dates', () => {
		component.newCalendarName = 'New Bank Holiday Calendar';
		component.dateChangeCallback(new Date('2015-01-10'));
		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		expect(component.selectedDatesTimeList.length).toBe(1);
		expect(component.selectedDatesTimeList[0]).toBe(new Date('2015-01-10').getTime());
	});

	it('should highlight datepicker after post succeed', () => {
		component.newCalendarName = 'New Bank Holiday Calendar';
		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();

		expect(component.selectedDatesTimeList.length).toBe(0);

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});

		expect(component.selectedDatesTimeList.length).toBe(1);
		expect(component.selectedDatesTimeList[0]).toBe(new Date('2015-01-10').getTime());
	});

	it('should be able remove a date', () => {
		component.newCalendarName = 'New Bank Holiday Calendar';
		component.dateChangeCallback(new Date('2015-01-10 10:00:00'));
		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '86784a24-ea35-466a-a535-839a22d06bf4',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		component.dateChangeCallback(new Date('2015-01-11 10:00:00'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						},
						{
							Id: '86784a24-ea35-466a-a535-839a22d06bf4',
							Date: '2015-01-11',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		const addNewBankHolidayCalendarPanel = document.getElementsByClassName('add-new-bank-holiday-calendar')[0];
		const dateRows = addNewBankHolidayCalendarPanel
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item');

		expect(dateRows.length).toBe(2);
		expect(dateRows[0].innerHTML.indexOf('2015-01-10') > -1).toBeTruthy();
		expect(dateRows[0].innerHTML.indexOf('BankHoliday') > -1).toBeTruthy();

		expect(dateRows[1].innerHTML.indexOf('2015-01-11') > -1).toBeTruthy();
		expect(dateRows[1].innerHTML.indexOf('BankHoliday') > -1).toBeTruthy();

		dateRows[0].getElementsByClassName('remove-date-icon')[0].dispatchEvent(new Event('click'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '86784a24-ea35-466a-a535-839a22d06bf4',
							Date: '2015-01-11',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		const dateContent = document.getElementsByClassName('bank-holiday-calendar-date-content')[0];

		expect(dateContent.getElementsByTagName('nz-collapse-panel').length).toBe(1);
		expect(dateContent.getElementsByTagName('nz-list-item').length).toBe(1);

		expect(component.newCalendarYearsForDisplay.length).toBe(1);
		expect(component.newCalendarYearsForDisplay[0].Dates.length).toBe(1);

		expect(component.newCalendarYears.length).toBe(1);
		expect(component.newCalendarYears[0].Dates.length).toBe(1);

		expect(component.selectedDatesTimeList.length).toBe(1);
		expect(component.selectedDatesTimeList[0]).toBe(new Date('2015-01-11').getTime());
	});

	it('should be able to add a date back after removing it', () => {
		component.newCalendarName = 'New Bank Holiday Calendar';
		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		const dateContent = document.getElementsByClassName('bank-holiday-calendar-date-content')[0];
		let dateRows = dateContent
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item');

		expect(dateRows.length).toBe(1);
		expect(dateRows[0].innerHTML.indexOf('2015-01-10') > -1).toBeTruthy();
		expect(dateRows[0].innerHTML.indexOf('BankHoliday') > -1).toBeTruthy();

		dateRows[0].getElementsByClassName('remove-date-icon')[0].dispatchEvent(new Event('click'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: []
				}
			]
		});
		fixture.detectChanges();

		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		dateRows = dateContent
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item');

		expect(dateRows.length).toBe(1);
		expect(dateRows[0].innerHTML.indexOf('2015-01-10') > -1).toBeTruthy();
		expect(dateRows[0].innerHTML.indexOf('BankHoliday') > -1).toBeTruthy();

		expect(component.selectedDatesTimeList.length).toBe(1);
		expect(component.selectedDatesTimeList[0]).toBe(new Date('2015-01-10').getTime());
		expect(component.newCalendarYears.length).toBe(1);
		expect(component.newCalendarYears[0].Dates.length).toBe(1);
		expect(component.newCalendarYears[0].Dates[0].Date).toBe('2015-01-10');
		expect(component.newCalendarYears[0].Dates[0].Description).toBe('BankHoliday');
	});

	it('should categorize dates by year', () => {
		component.newCalendarName = 'New Bank Holiday Calendar';
		component.dateChangeCallback(new Date('2015-01-10'));
		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		component.dateChangeCallback(new Date('2016-01-10'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				},
				{
					Year: 2016,
					Dates: [
						{
							Id: '8c0eb95d-33cc-4136-a800-ba85bb38ca83',
							Date: '2016-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		expect(component.newCalendarYears.length).toBe(2);

		expect(component.newCalendarYears[0].Year).toBe('2015');
		expect(component.newCalendarYears[0].Dates.length).toBe(1);
		expect(component.newCalendarYears[0].Dates[0].Date).toBe('2015-01-10');

		expect(component.newCalendarYears[1].Year).toBe('2016');
		expect(component.newCalendarYears[1].Dates.length).toBe(1);
		expect(component.newCalendarYears[1].Dates[0].Date).toBe('2016-01-10');
	});

	it('should show a hint message off adding date when there is no date', () => {
		const hintElement = document.getElementsByClassName('bank-holiday-no-date-tip');

		expect(hintElement.length).toBe(1);
		expect(
			hintElement[0].innerHTML.indexOf(
				'ThereAreNoBankHolidaysDefinedForThisYearSelectADayInTheCalendarToCreateANewHoliday'
			) > -1
		).toBeTruthy();
	});

	it('should save after user input calendar name and select a date', () => {
		component.newCalendarName = 'I am a new calendar';
		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();

		expect(component.newCalendarYearsForDisplay.length).toBe(0);
		expect(component.newCalendarYears.length).toBe(1);
		expect(component.newCalendarYears[0].Dates.length).toBe(1);

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		expect(component.newCalendarYearsForDisplay.length).toBe(1);
		expect(component.newCalendarYearsForDisplay[0].Dates.length).toBe(1);
		expect(component.newCalendarYears.length).toBe(1);
		expect(component.newCalendarYears[0].Dates.length).toBe(1);
		expect(component.selectedDatesTimeList.length).toBe(1);
	});

	it('should calendar name not be empty before saving', () => {
		component.newCalendarName = '';
		document.querySelector('.add-new-bank-holiday-calendar-header input').dispatchEvent(new Event('blur'));

		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();

		expect(
			document
				.querySelector('.add-new-bank-holiday-calendar-header nz-alert')
				.innerHTML.indexOf('PleaseInputCalendarNameFirst') > -1
		).toBeTruthy();
		expect(httpTestingController.match('../api/BankHolidayCalendars/Save').length).toBe(0);
		expect(component.newCalendarYearsForDisplay.length).toBe(0);
		expect(component.selectedDatesTimeList.length).toBe(0);
		expect(component.newCalendarYears.length).toBe(0);
	});

	it('should store calendar id after adding first date', () => {
		component.newCalendarName = 'I am a new calendar';
		document.querySelector('.add-new-bank-holiday-calendar-header input').dispatchEvent(new Event('blur'));

		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();

		expect(component.newCalendarYearsForDisplay.length).toBe(0);
		expect(component.newCalendarYears.length).toBe(1);
		expect(component.newCalendarYears[0].Dates.length).toBe(1);

		let request = httpTestingController.match('../api/BankHolidayCalendars/Save')[0];
		request.flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();
		expect(component.newCalendarId).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');

		component.dateChangeCallback(new Date('2015-01-11'));
		fixture.detectChanges();

		request = httpTestingController.match('../api/BankHolidayCalendars/Save')[0];
		request.flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						},
						{
							Id: '19eed98f-d48b-4974-9b1f-fd0ad05719d2',
							Date: '2015-01-11',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});

		expect(request.request.body.Id).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
		expect(request.request.body.Name).toBe('I am a new calendar');
		expect(request.request.body.Years.length).toBe(1);
	});

	it('should update the bank holiday calendar list after saving', () => {
		component.newCalendarName = 'I am a new calendar';
		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		expect(component.bankHolidayCalendarsList.length).toBe(1);

		const calendar = component.bankCalendarDataService.bankHolidayCalendarsList$.getValue();
		expect(calendar.length).toBe(1);
		expect(calendar[0].Id).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
		expect(calendar[0].Name).toBe('I am a new calendar');
		expect(calendar[0].Years.length).toBe(1);
		expect(calendar[0].Years[0].Year).toBe('2015');
		expect(calendar[0].Years[0].Dates.length).toBe(1);
		expect(calendar[0].Years[0].Dates[0].Date).toBe('2015-01-10');
	});

	it('should not update the bank holiday calendar list when error happens', () => {
		component.newCalendarName = 'I am a new calendar';
		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].error(new ErrorEvent('error'));
		fixture.detectChanges();

		expect(component.newCalendarYears.length).toBe(1);
		expect(component.newCalendarYearsForDisplay.length).toBe(0);
		expect(component.selectedDatesTimeList.length).toBe(0);
	});

	it('should not update the newCalendarYearsForDisplay list when error happens', () => {
		component.newCalendarName = 'I am a new calendar';
		component.dateChangeCallback(new Date('2015-01-10'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		expect(component.newCalendarYearsForDisplay.length).toBe(1);

		component.dateChangeCallback(new Date('2015-01-11'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].error(new ErrorEvent('error'));
		fixture.detectChanges();

		expect(component.newCalendarYearsForDisplay.length).toBe(1);
	});

	it('should only send modifed date to backend', () => {
		component.newCalendarName = 'I am a new calendar';
		component.dateChangeCallback(new Date('2015-01-10'));

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		component.dateChangeCallback(new Date('2016-01-10'));
		fixture.detectChanges();

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
			Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
			Name: 'I am a new calendar',
			Years: [
				{
					Year: 2015,
					Dates: [
						{
							Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
							Date: '2015-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				},
				{
					Year: 2016,
					Dates: [
						{
							Id: '91d195c7-ea55-42f8-ab0c-9ff8b22dbbe8',
							Date: '2016-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		component.dateChangeCallback(new Date('2015-01-11'));
		fixture.detectChanges();

		const request = httpTestingController.match('../api/BankHolidayCalendars/Save')[0];

		expect(request.request.body.Id).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
		expect(request.request.body.Name).toBe('I am a new calendar');
		expect(request.request.body.Years.length).toBe(1);
		expect(request.request.body.Years[0].Year).toBe('2015');
		expect(request.request.body.Years[0].Dates.length).toBe(1);
		expect(request.request.body.Years[0].Dates[0].Date).toBe('2015-01-11');
	});
});
