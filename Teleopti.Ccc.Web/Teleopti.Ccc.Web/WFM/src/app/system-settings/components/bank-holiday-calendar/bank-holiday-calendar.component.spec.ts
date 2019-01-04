import { DOCUMENT } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { registerLocaleData } from '@angular/common';
import zh from '@angular/common/locales/zh';
import en from '@angular/common/locales/en';

import { configureTestSuite } from '@wfm/test';
import { UserService, TogglesService } from 'src/app/core/services';
import { BankHolidayCalendarComponent } from './bank-holiday-calendar.component';
import { BankHolidayCalendarAddComponent } from '../bank-holiday-calendar-add';
import { BankHolidayCalendarEditComponent } from '../bank-holiday-calendar-edit';
import { ToggleMenuService } from 'src/app/menu/shared/toggle-menu.service';

describe('BankHolidayCalendarComponent', () => {
	let fixture: ComponentFixture<BankHolidayCalendarComponent>;
	let document: Document;
	let component: BankHolidayCalendarComponent;
	let httpTestingController: HttpTestingController;

	configureTestSuite();
	beforeAll(() => {
		registerLocaleData(zh);
	});

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [
				BankHolidayCalendarComponent,
				BankHolidayCalendarAddComponent,
				BankHolidayCalendarEditComponent
			],
			imports: [
				TranslateModule.forRoot(),
				NgZorroAntdModule.forRoot(),
				FormsModule,
				ReactiveFormsModule,
				HttpClientTestingModule,
				BrowserAnimationsModule
			],
			providers: [
				TranslateService,
				UserService,
				{
					provide: ToggleMenuService,
					useValue: new ToggleMenuService({
						innerWidth: 1200
					} as Window)
				},
				TogglesService
			]
		}).compileComponents();

		fixture = TestBed.createComponent(BankHolidayCalendarComponent);
		document = TestBed.get(DOCUMENT);
		component = fixture.componentInstance;
		fixture.autoDetectChanges(true);
		httpTestingController = TestBed.get(HttpTestingController);
	}));

	afterAll(() => {
		registerLocaleData(en);
	});

	it('should create component', async(() => {
		expect(component).toBeTruthy();
	}));

	it('should render title', async(() => {
		let bankHolidayCalendarSettings = document.getElementsByClassName('bank-holiday-settings')[0];

		expect(bankHolidayCalendarSettings).toBeTruthy();
		expect(bankHolidayCalendarSettings.getElementsByTagName('h2').length).toBe(1);
		expect(
			bankHolidayCalendarSettings.getElementsByTagName('h2')[0].innerHTML.indexOf('BankHolidayCalendars') > -1
		).toBeTruthy();
	}));

	it('should show add new bank holiday calendar panel after clicking plus icon', async(() => {
		document.getElementsByClassName('add-bank-holiday-calendar-icon')[0].dispatchEvent(new Event('click'));

		let addCalendarPanel = document.getElementsByClassName('add-new-bank-holiday-calendar')[0];

		expect(addCalendarPanel).toBeTruthy();
	}));

	it('should list bank holiday calendars', async(() => {
		component.bankHolidayCalendarsList.splice(0, 0, {
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
		});

		fixture.detectChanges();

		let bankHolidayCalendarSettings = document.getElementsByClassName('bank-holiday-settings')[0];
		let list = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');

		expect(list.length).toBe(1);

		list[0].getElementsByClassName('ant-collapse-header')[0].dispatchEvent(new Event('click'));
		expect(
			list[0].getElementsByClassName('bank-holiday-calendar-date-list')[0].getElementsByTagName('nz-list-item')
				.length
		).toBe(2);

		let firstRow = list[0]
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item')[0];
		let secondRow = list[0]
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item')[1];

		expect(firstRow.innerHTML.indexOf('2013-01-09') > -1).toBeTruthy();
		expect(firstRow.innerHTML.indexOf('BankHoliday1') > -1).toBeTruthy();
		expect(secondRow.innerHTML.indexOf('2013-01-10') > -1).toBeTruthy();
		expect(secondRow.innerHTML.indexOf('BankHoliday2') > -1).toBeTruthy();
	}));

	it('should list bank holiday calendars in alphabetical order', async(() => {
		var calendars = [
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'London bank holiday calendar',
				Years: [
					{
						Year: '2013',
						Dates: [
							{
								Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
								Date: '2013-01-09',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					}
				]
			},
			{
				Id: '7bb434fb-1e00-4e9e-a427-7fb0f3100508',
				Name: 'Bank holiday calendar',
				Years: [
					{
						Year: '2013',
						Dates: [
							{
								Id: '16a8fe5e-4150-4366-8703-8a6d7f3bab45',
								Date: '2013-01-09',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					}
				]
			}
		];
		component.bankHolidayCalendarsList = calendars;

		fixture.detectChanges();

		let bankHolidayCalendarSettings = document.getElementsByClassName('bank-holiday-settings')[0];
		let list = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');

		expect(list.length).toBe(2);
		expect(
			list[0].getElementsByClassName('ant-collapse-header')[0].innerHTML.indexOf('London bank holiday calendar') >
				-1
		).toBeTruthy();
		expect(
			list[1].getElementsByClassName('ant-collapse-header')[0].innerHTML.indexOf('Bank holiday calendar') > -1
		).toBeTruthy();
	}));

	it('should show delete and edit button for each bank holiday calendar item', async(() => {
		component.bankHolidayCalendarsList.splice(0, 0, {
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
		});

		fixture.detectChanges();

		let bankHolidayCalendarSettings = document.getElementsByClassName('bank-holiday-settings')[0];
		let list = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');

		expect(list.length).toBe(1);
		expect(list[0].getElementsByClassName('ant-btn').length).toBe(2);
		expect(
			list[0]
				.getElementsByClassName('ant-btn')[0]
				.getElementsByTagName('span')[0]
				.innerText.trim()
		).toBe('Delete');
		expect(
			list[0]
				.getElementsByClassName('ant-btn')[1]
				.getElementsByTagName('span')[0]
				.innerText.trim()
		).toBe('Edit');
	}));

	it('should go to edit bank holiday calendar panel after click edit buton', async(() => {
		component.bankHolidayCalendarsList.splice(0, 0, {
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
		});

		fixture.detectChanges();

		let bankHolidayCalendarSettings = document.getElementsByClassName('bank-holiday-settings')[0];
		let list = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');

		list[0].getElementsByClassName('ant-btn')[1].dispatchEvent(new Event('click'));
		fixture.detectChanges();
		expect(document.getElementsByClassName('edit-bank-holiday-calendar')[0]).toBeTruthy();
	}));

	it('should show site tab when WFM_Setting_AssignBankHolidayCalendarsToSites_79899 is turn on', async(() => {
		let toggleReq = httpTestingController.match('../ToggleHandler/AllToggles');
		toggleReq[0].flush({
			WFM_Setting_BankHolidayCalendar_Create_79297: true,
			WFM_Setting_AssignBankHolidayCalendarsToSites_79899: true
		});

		fixture.detectChanges();

		expect(component.isAssignBankHolidayCalendarsToSitesEnabled).toBe(true);
		expect(document.getElementsByClassName('bank-holiday-calendar-site-tab').length).toBe(1);
		expect(
			document.getElementsByClassName('bank-holiday-calendar-site-tab')[0].getElementsByTagName('span')[0]
				.innerText
		).toBe('AssignCalendarsToSites');
	}));
});
