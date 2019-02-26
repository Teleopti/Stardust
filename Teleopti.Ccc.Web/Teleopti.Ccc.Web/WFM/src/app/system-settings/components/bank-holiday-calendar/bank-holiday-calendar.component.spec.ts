import { DOCUMENT, registerLocaleData } from '@angular/common';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import en from '@angular/common/locales/en';
import zh from '@angular/common/locales/zh';

import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { configureTestSuite } from '@wfm/test';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { TogglesService, UserService } from 'src/app/core/services';
import { ToggleMenuService } from 'src/app/menu/shared/toggle-menu.service';
import { BankHolidayCalendarAddComponent } from '../bank-holiday-calendar-add';
import { BankHolidayCalendarAssignToSitesComponent } from '../bank-holiday-calendar-assign-to-sites';
import { BankHolidayCalendarEditComponent } from '../bank-holiday-calendar-edit';
import { BankHolidayCalendarComponent } from './bank-holiday-calendar.component';
import { MockTranslationModule, MockTranslateService } from '@wfm/mocks/translation';
import { BankCalendarDataService } from '../../shared';
import { GroupPageService } from 'src/app/shared/services/group-page-service';

describe('BankHolidayCalendarComponent', () => {
	let fixture: ComponentFixture<BankHolidayCalendarComponent>;
	let document: Document;
	let component: BankHolidayCalendarComponent;
	let httpTestingController: HttpTestingController;

	configureTestSuite();
	beforeAll(() => {
		registerLocaleData(zh);
	});

	beforeEach(() => {
		TestBed.configureTestingModule({
			declarations: [
				BankHolidayCalendarComponent,
				BankHolidayCalendarAddComponent,
				BankHolidayCalendarEditComponent,
				BankHolidayCalendarAssignToSitesComponent
			],
			imports: [
				MockTranslationModule,
				NgZorroAntdModule,
				FormsModule,
				ReactiveFormsModule,
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
				TogglesService,
				GroupPageService,
				BankCalendarDataService
			]
		}).compileComponents();

		fixture = TestBed.createComponent(BankHolidayCalendarComponent);
		document = TestBed.get(DOCUMENT);
		component = fixture.componentInstance;
		httpTestingController = TestBed.get(HttpTestingController);
	});

	afterAll(() => {
		registerLocaleData(en);
	});

	it('should create component', () => {
		fixture.detectChanges();
		expect(component).toBeTruthy();
	});

	it('should render title', () => {
		fixture.detectChanges();
		const bankHolidayCalendarSettings = document.getElementsByClassName('bank-holiday-settings')[0];

		expect(bankHolidayCalendarSettings).toBeTruthy();
		expect(bankHolidayCalendarSettings.getElementsByTagName('h2').length).toBe(1);
		expect(
			bankHolidayCalendarSettings.getElementsByTagName('h2')[0].innerHTML.indexOf('BankHolidayCalendars') > -1
		).toBeTruthy();
	});

	it('should show add new bank holiday calendar panel after clicking plus icon', () => {
		fixture.detectChanges();

		document.getElementsByClassName('add-bank-holiday-calendar-icon')[0].dispatchEvent(new Event('click'));
		fixture.detectChanges();

		const addCalendarPanel = document.getElementsByClassName('add-new-bank-holiday-calendar')[0];

		expect(addCalendarPanel).toBeTruthy();
	});

	it('should list bank holiday calendars', () => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
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
		]);

		fixture.detectChanges();

		const bankHolidayCalendarSettings = document.getElementsByClassName('bank-holiday-settings')[0];
		const list = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');

		expect(list.length).toBe(1);

		list[0].getElementsByClassName('ant-collapse-header')[0].dispatchEvent(new Event('click'));
		expect(
			list[0].getElementsByClassName('bank-holiday-calendar-date-list')[0].getElementsByTagName('nz-list-item')
				.length
		).toBe(2);

		const firstRow = list[0]
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item')[0];
		const secondRow = list[0]
			.getElementsByClassName('bank-holiday-calendar-date-list')[0]
			.getElementsByTagName('nz-list-item')[1];

		expect(firstRow.innerHTML.indexOf('2013-01-09') > -1).toBeTruthy();
		expect(firstRow.innerHTML.indexOf('BankHoliday1') > -1).toBeTruthy();
		expect(secondRow.innerHTML.indexOf('2013-01-10') > -1).toBeTruthy();
		expect(secondRow.innerHTML.indexOf('BankHoliday2') > -1).toBeTruthy();
	});

	it('should list bank holiday calendars in alphabetical order', () => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
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
		]);

		fixture.detectChanges();

		const bankHolidayCalendarSettings = document.getElementsByClassName('bank-holiday-settings')[0];
		const list = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');

		expect(list.length).toBe(2);
		expect(
			list[0].getElementsByClassName('ant-collapse-header')[0].innerHTML.indexOf('Bank holiday calendar') > -1
		).toBeTruthy();
		expect(
			list[1].getElementsByClassName('ant-collapse-header')[0].innerHTML.indexOf('London bank holiday calendar') >
				-1
		).toBeTruthy();
	});

	it('should show delete and edit icon for each bank holiday calendar item', () => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
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
			}
		]);

		fixture.detectChanges();

		const bankHolidayCalendarSettings = document.getElementsByClassName('bank-holiday-settings')[0];
		const list = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');

		expect(list.length).toBe(1);
		expect(list[0].getElementsByClassName('anticon-delete').length).toBe(1);
		expect(list[0].getElementsByClassName('anticon-edit').length).toBe(1);
	});

	xit('should go to edit bank holiday calendar panel and active current view year tab after clicking edit buton', () => {
		fixture.detectChanges();
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
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
					},
					{
						Year: '2014',
						Dates: [
							{
								Id: '6f5fe53b-9045-4f0e-bbc6-ae0a12d00bc7',
								Date: '2014-01-09',
								Description: 'BankHoliday',
								IsDeleted: false
							},
							{
								Id: 'bcb33f86-e9a7-4b07-a4c1-22a1418cfb5f',
								Date: '2014-01-10',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					}
				]
			}
		]);

		fixture.detectChanges();

		const bankHolidayCalendarSettings = document.getElementsByClassName('bank-holiday-settings')[0];
		const list = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');

		list[0].getElementsByClassName('anticon-edit')[0].parentElement.dispatchEvent(new Event('click'));
		fixture.detectChanges();
		expect(document.getElementsByClassName('edit-bank-holiday-calendar')[0]).toBeTruthy();
		expect(document.getElementsByClassName('ant-tabs-tab-active')[0].innerHTML.indexOf('2014') > -1).toBeTruthy();
	});

	it('should reset current year index after saving', () => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				CurrentYearIndex: 1,
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
					},
					{
						Year: '2014',
						Dates: [
							{
								Id: '6f5fe53b-9045-4f0e-bbc6-ae0a12d00bc7',
								Date: '2014-01-09',
								Description: 'BankHoliday',
								IsDeleted: false
							},
							{
								Id: 'bcb33f86-e9a7-4b07-a4c1-22a1418cfb5f',
								Date: '2014-01-10',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					}
				]
			}
		]);

		fixture.detectChanges();

		const bankHolidayCalendarSettings = document.getElementsByClassName('bank-holiday-settings')[0];
		const list = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');
		list[0].getElementsByClassName('anticon-edit')[0].parentElement.dispatchEvent(new Event('click'));
		fixture.detectChanges();

		const editBankHolidayCalendarPanel = document.getElementsByClassName('edit-bank-holiday-calendar')[0];
		editBankHolidayCalendarPanel
			.getElementsByClassName('operation-buttons')[0]
			.childNodes[1].dispatchEvent(new Event('click'));

		httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
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
				},
				{
					Year: '2014',
					Dates: [
						{
							Id: '6f5fe53b-9045-4f0e-bbc6-ae0a12d00bc7',
							Date: '2014-01-09',
							Description: 'BankHoliday',
							IsDeleted: false
						},
						{
							Id: 'bcb33f86-e9a7-4b07-a4c1-22a1418cfb5f',
							Date: '2014-01-10',
							Description: 'BankHoliday',
							IsDeleted: false
						}
					]
				}
			]
		});
		fixture.detectChanges();

		expect(component.bankHolidayCalendarsList.length).toBe(1);
	});

	it('should popup a modal and let user confirm when deleting a caldendar', fakeAsync(() => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar 2013',
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
			}
		]);

		fixture.detectChanges();

		const bankHolidayCalendarSettings = document.getElementsByClassName('bank-holiday-settings')[0];
		const list = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');

		list[0].getElementsByClassName('anticon-delete')[0].parentElement.dispatchEvent(new Event('click'));
		httpTestingController.match('../ToggleHandler/AllToggles')[0].flush({
			WFM_Setting_BankHolidayCalendar_Create_79297: true,
			WFM_Setting_AssignBankHolidayCalendarsToSites_79899: true
		});
		httpTestingController
			.match('../api/SitesByCalendar/' + 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df')[0]
			.flush(['4aae67fd-af87-4377-b9d3-3c82c9f68f2f']);
		fixture.detectChanges();
		tick(500);
		fixture.detectChanges();

		expect(
			document.getElementsByClassName('ant-modal-confirm-body-wrapper')[0].innerHTML.indexOf('2013') > -1
		).toBeTruthy();
	}));

	it('should notice user of how many sites are using the calendar when deleting it', fakeAsync(() => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar 2013',
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
			}
		]);

		fixture.detectChanges();

		const bankHolidayCalendarSettings = document.getElementsByClassName('bank-holiday-settings')[0];
		const list = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');

		list[0].getElementsByClassName('anticon-delete')[0].parentElement.dispatchEvent(new Event('click'));

		httpTestingController.match('../ToggleHandler/AllToggles')[0].flush({
			WFM_Setting_BankHolidayCalendar_Create_79297: true,
			WFM_Setting_AssignBankHolidayCalendarsToSites_79899: true
		});

		httpTestingController
			.match('../api/SitesByCalendar/' + 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df')[0]
			.flush(['4aae67fd-af87-4377-b9d3-3c82c9f68f2f']);

		fixture.detectChanges();
		tick(500);
		fixture.detectChanges();

		expect(
			document.getElementsByClassName('ant-modal-confirm-body-wrapper')[0].innerHTML.indexOf('2013') > -1
		).toBeTruthy();
		expect(
			document
				.getElementsByClassName('ant-modal-confirm-body-wrapper')[0]
				.innerHTML.indexOf('XSitesUseThisCalendar') > -1
		).toBeTruthy();
	}));

	it('should show site tab when WFM_Setting_AssignBankHolidayCalendarsToSites_79899 is turn on', () => {
		const toggleReq = httpTestingController.match('../ToggleHandler/AllToggles');
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
	});
});
