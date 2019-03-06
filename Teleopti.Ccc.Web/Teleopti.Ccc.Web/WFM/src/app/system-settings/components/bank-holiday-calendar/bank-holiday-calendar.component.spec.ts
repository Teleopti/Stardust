import { DOCUMENT } from '@angular/common';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { ComponentFixture, TestBed, fakeAsync, tick, async } from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { configureTestSuite } from '@wfm/test';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { TogglesService, UserService } from 'src/app/core/services';
import { ToggleMenuService } from 'src/app/menu/shared/toggle-menu.service';
import { BankHolidayCalendarEditComponent } from '../bank-holiday-calendar-edit';
import { BankHolidayCalendarAssignToSitesComponent } from '../bank-holiday-calendar-assign-to-sites';
import { BankHolidayCalendarComponent } from './bank-holiday-calendar.component';
import { MockTranslationModule, MockTranslateService } from '@wfm/mocks/translation';
import { BankCalendarDataService } from '../../shared';
import { GroupPageService } from 'src/app/shared/services/group-page-service';
import { getDebugNode, DebugElement } from '@angular/core';

describe('BankHolidayCalendarComponent', () => {
	let fixture: ComponentFixture<BankHolidayCalendarComponent>;
	let document: Document;
	let component: BankHolidayCalendarComponent;
	let httpTestingController: HttpTestingController;

	configureTestSuite();

	beforeEach(() => {
		TestBed.configureTestingModule({
			declarations: [
				BankHolidayCalendarComponent,
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

	it('should create component', doneFn => {
		expect(component).toBeTruthy();
		doneFn();
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

		const addCalendarPanel = document.getElementsByClassName('edit-bank-holiday-calendar')[0];

		expect(addCalendarPanel).toBeTruthy();
	});

	it('should list bank holiday calendars', () => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				Years: [
					{
						Year: 2013,
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
						Year: 2013,
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
						Year: 2013,
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
						Year: 2013,
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

	it('should go to edit bank holiday calendar panel and show the dates and highlight selected dates', () => {
		fixture.detectChanges();
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				Years: [
					{
						Year: 2013,
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
						Year: 2014,
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

		expect(component.isEdittingCalendar).toBe(true);

		const editComponent = getEditBankHolidayCalendarComponent();

		expect(editComponent.selectedDatesTimeList.length).toBe(4);
		expect(editComponent.selectedDatesTimeList[0]).toBe(new Date('2013-01-09').getTime());
		expect(editComponent.selectedDatesTimeList[1]).toBe(new Date('2013-01-10').getTime());
		expect(editComponent.selectedDatesTimeList[2]).toBe(new Date('2014-01-09').getTime());
		expect(editComponent.selectedDatesTimeList[3]).toBe(new Date('2014-01-10').getTime());

		expect(document.getElementsByClassName('edit-bank-holiday-calendar')[0]).toBeTruthy();
		expect(
			document
				.querySelector('.bank-holiday-calendar-date-content .ant-collapse-header')
				.getAttribute('aria-expanded')
		).toBeTruthy();

		const activePanel = document.querySelectorAll(
			'.bank-holiday-calendar-date-content nz-collapse-panel[ng-reflect-nz-active=true]'
		);
		expect(activePanel.length).toBe(1);

		const dateRows = activePanel[0].querySelectorAll('.bank-holiday-calendar-date-item');

		expect(dateRows[0].innerHTML.indexOf('2013-01-09') > -1).toBeTruthy();
		expect(dateRows[0].innerHTML.indexOf('BankHoliday') > -1).toBeTruthy();
		expect(dateRows[1].innerHTML.indexOf('2013-01-10') > -1).toBeTruthy();
		expect(dateRows[1].innerHTML.indexOf('BankHoliday') > -1).toBeTruthy();
	});

	it('should empty the selected calendar when go to add new calendar page', () => {
		fixture.detectChanges();
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				Years: [
					{
						Year: 2013,
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

		list[0].getElementsByClassName('anticon-edit')[0].parentElement.dispatchEvent(new Event('click'));
		fixture.detectChanges();

		expect(component.isEdittingCalendar).toBe(true);
		expect(document.getElementsByClassName('edit-bank-holiday-calendar')[0]).toBeTruthy();
		expect(
			document
				.querySelector('.bank-holiday-calendar-date-content .ant-collapse-header')
				.getAttribute('aria-expanded')
		).toBeTruthy();

		const activePanel = document.querySelectorAll(
			'.bank-holiday-calendar-date-content nz-collapse-panel[ng-reflect-nz-active=true]'
		);
		expect(activePanel.length).toBe(1);

		backToBankHolidayList();

		// click add button to add new bank holiday page
		document.querySelector('.add-bank-holiday-calendar-icon').dispatchEvent(new Event('click'));
		fixture.detectChanges();

		const editComponent = getEditBankHolidayCalendarComponent();

		expect(component.selectedCalendar).toBeFalsy();
		expect(component.bankHolidayCalendarsList.length).toBe(1);
		expect(component.bankHolidayCalendarsList[0].Id).toBeTruthy();

		expect(editComponent.newCalendar).toBeTruthy();
		expect(editComponent.newCalendar.ActiveYearIndex).toBe(0);
		expect(editComponent.newCalendar.Years.length === 0);
	});

	it('should go to edit bank holiday calendar panel and active the datepickers year to actived year', async(() => {
		fixture.detectChanges();
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				Years: [
					{
						Year: 2013,
						Dates: [
							{
								Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
								Date: '2013-01-09',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					},
					{
						Year: 2014,
						Dates: [
							{
								Id: '6f5fe53b-9045-4f0e-bbc6-ae0a12d00bc7',
								Date: '2014-01-09',
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

		openCalendar(list);
		activeYear(list);

		fixture.whenStable().then(() => {
			fixture.detectChanges();

			list[0].getElementsByClassName('anticon-edit')[0].parentElement.dispatchEvent(new Event('click'));
			fixture.detectChanges();

			expect(component.isEdittingCalendar).toBe(true);

			const editComponent = getEditBankHolidayCalendarComponent();

			expect(
				moment(editComponent.selectedYearDate, editComponent.dateFormat).format(editComponent.yearFormat)
			).toBe('2014');
		});
	}));

	it('should go to edit bank holiday calendar panel and active current view year tab after clicking edit buton', async(() => {
		fixture.detectChanges();
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				Years: [
					{
						Year: 2013,
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
						Year: 2014,
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

		openCalendar(list);
		activeYear(list);

		fixture.whenStable().then(() => {
			fixture.detectChanges();

			list[0].getElementsByClassName('anticon-edit')[0].parentElement.dispatchEvent(new Event('click'));
			fixture.detectChanges();

			expect(component.isEdittingCalendar).toBe(true);
			expect(document.getElementsByClassName('edit-bank-holiday-calendar')[0]).toBeTruthy();
			expect(
				document
					.querySelector('.bank-holiday-calendar-date-content .ant-collapse-header')
					.getAttribute('aria-expanded')
			).toBeTruthy();

			const activePanel = document.querySelectorAll(
				'.bank-holiday-calendar-date-content nz-collapse-panel[ng-reflect-nz-active=true]'
			);
			expect(activePanel.length).toBe(1);

			const dateRows = activePanel[0].querySelectorAll('.bank-holiday-calendar-date-item');

			expect(dateRows[0].innerHTML.indexOf('2014-01-09') > -1).toBeTruthy();
			expect(dateRows[0].innerHTML.indexOf('BankHoliday') > -1).toBeTruthy();
			expect(dateRows[1].innerHTML.indexOf('2014-01-10') > -1).toBeTruthy();
			expect(dateRows[1].innerHTML.indexOf('BankHoliday') > -1).toBeTruthy();
		});
	}));

	it('should reset the active year after saving', async(() => {
		fixture.detectChanges();
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				Years: [
					{
						Year: 2013,
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
						Year: 2014,
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
		const calendarList = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');

		// 1. Open the calendar
		openCalendar(calendarList);

		// 2. Active the year 2014 in the calendar
		activeYear(calendarList);

		fixture.whenStable().then(() => {
			fixture.detectChanges();

			// 3. Go to edit page for this calendar
			clickEditButton(calendarList);

			const editComponent = getEditBankHolidayCalendarComponent();

			expect(editComponent.newCalendar).toBeTruthy();
			expect(editComponent.newCalendar.ActiveYearIndex).toBe(1);

			// 4. Add a new date for year 2013
			editComponent.dateChangeCallback(new Date('2013-01-11'));
			fixture.detectChanges();

			httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				Years: [
					{
						Year: 2013,
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
							},
							{
								Id: '4dfd442a-0b90-46ed-bdd9-f495a478c962',
								Date: '2013-01-11',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					},
					{
						Year: 2014,
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

			// 5. Go back to calendar list after the calendar is saved
			backToBankHolidayList();

			const activeYears = document.querySelectorAll('.ant-collapse-content .ant-tabs-tab.ant-tabs-tab-active');
			expect(activeYears.length).toBe(1);
			expect(activeYears[0].innerHTML.indexOf('2013') > -1).toBeTruthy();

			// 6. Go to edit page again
			clickEditButton(calendarList);

			const activePanels = document.querySelectorAll(
				'.bank-holiday-calendar-date-content nz-collapse-panel[ng-reflect-nz-active=true]'
			);
			expect(activePanels.length).toBe(1);
			expect(activePanels[0].querySelector('.ant-collapse-header').innerHTML.indexOf('2013')).toBeTruthy();

			// There should be one year active: 2013
			const activeYearContent = activePanels[0].querySelector(
				'.ant-collapse-content.ant-collapse-content-active'
			);
			expect(activeYearContent.querySelectorAll('nz-list-item').length).toBe(3);
			expect(
				activeYearContent.querySelectorAll('nz-list-item')[0].innerHTML.indexOf('2013-01-09') > -1
			).toBeTruthy();
			expect(
				activeYearContent.querySelectorAll('nz-list-item')[1].innerHTML.indexOf('2013-01-10') > -1
			).toBeTruthy();
			expect(
				activeYearContent.querySelectorAll('nz-list-item')[2].innerHTML.indexOf('2013-01-11') > -1
			).toBeTruthy();
		});
	}));

	it('should popup a modal and let user confirm when deleting a caldendar', fakeAsync(() => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar 2013',
				Years: [
					{
						Year: 2013,
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
						Year: 2013,
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

	it('should keep actived years active when making changes to other years', async(() => {
		fixture.detectChanges();
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				Years: [
					{
						Year: 2013,
						Dates: [
							{
								Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
								Date: '2013-01-09',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					},
					{
						Year: 2014,
						Dates: [
							{
								Id: '6f5fe53b-9045-4f0e-bbc6-ae0a12d00bc7',
								Date: '2014-01-09',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					},
					{
						Year: 2015,
						Dates: [
							{
								Id: '943eacf7-d712-4e58-89fd-c5f6686e6d99',
								Date: '2015-01-09',
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
		const calendarList = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');

		// 1. Open the calendar
		openCalendar(calendarList);

		// 2. Active the year 2014 in the calendar
		activeYear(calendarList);

		fixture.whenStable().then(() => {
			fixture.detectChanges();

			// 3. Go to edit page for this calendar
			clickEditButton(calendarList);

			const editComponent = getEditBankHolidayCalendarComponent();

			expect(editComponent.newCalendar).toBeTruthy();
			expect(editComponent.newCalendar.ActiveYearIndex).toBe(1);
			let activePanels = document.querySelectorAll(
				'.bank-holiday-calendar-date-content nz-collapse-panel[ng-reflect-nz-active=true]'
			);
			expect(activePanels.length).toBe(1);
			expect(activePanels[0].querySelector('.ant-collapse-header').innerHTML.indexOf('2014')).toBeTruthy();

			// 4. Add a new date for year 2013
			editComponent.dateChangeCallback(new Date('2013-01-10'));
			fixture.detectChanges();

			httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				Years: [
					{
						Year: 2013,
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
						Year: 2014,
						Dates: [
							{
								Id: '6f5fe53b-9045-4f0e-bbc6-ae0a12d00bc7',
								Date: '2014-01-09',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					}
				]
			});
			fixture.detectChanges();

			activePanels = document.querySelectorAll(
				'.bank-holiday-calendar-date-content nz-collapse-panel[ng-reflect-nz-active=true]'
			);
			expect(activePanels.length).toBe(2);
			expect(activePanels[0].querySelector('.ant-collapse-header').innerHTML.indexOf('2013')).toBeTruthy();
			expect(activePanels[1].querySelector('.ant-collapse-header').innerHTML.indexOf('2014')).toBeTruthy();

			// There should be two years actived: 2013 and 2014
			const activeYearContent2013 = activePanels[0].querySelector(
				'.ant-collapse-content.ant-collapse-content-active'
			);
			expect(activeYearContent2013.querySelectorAll('nz-list-item').length).toBe(2);
			expect(
				activeYearContent2013.querySelectorAll('nz-list-item')[0].innerHTML.indexOf('2013-01-09') > -1
			).toBeTruthy();
			expect(
				activeYearContent2013.querySelectorAll('nz-list-item')[1].innerHTML.indexOf('2013-01-10') > -1
			).toBeTruthy();

			const activeYearContent2014 = activePanels[1].querySelector(
				'.ant-collapse-content.ant-collapse-content-active'
			);
			expect(activeYearContent2014.querySelectorAll('nz-list-item').length).toBe(1);
			expect(
				activeYearContent2014.querySelectorAll('nz-list-item')[0].innerHTML.indexOf('2014-01-09') > -1
			).toBeTruthy();
		});
	}));

	it('should keep last actived year on edit page active when go back to calendar list', async(() => {
		fixture.detectChanges();
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				Years: [
					{
						Year: 2013,
						Dates: [
							{
								Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
								Date: '2013-01-09',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					},
					{
						Year: 2014,
						Dates: [
							{
								Id: '6f5fe53b-9045-4f0e-bbc6-ae0a12d00bc7',
								Date: '2014-01-09',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					},
					{
						Year: 2015,
						Dates: [
							{
								Id: '943eacf7-d712-4e58-89fd-c5f6686e6d99',
								Date: '2015-01-09',
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
		const calendarList = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');

		// 1. Open the calendar
		openCalendar(calendarList);

		// 2. Active the year 2014 in the calendar
		activeYear(calendarList);

		fixture.whenStable().then(() => {
			fixture.detectChanges();

			// 3. Go to edit page for this calendar
			clickEditButton(calendarList);

			const editComponent = getEditBankHolidayCalendarComponent();

			expect(editComponent.newCalendar).toBeTruthy();
			expect(editComponent.newCalendar.ActiveYearIndex).toBe(1);
			const activePanels = document.querySelectorAll(
				'.bank-holiday-calendar-date-content nz-collapse-panel[ng-reflect-nz-active=true]'
			);
			expect(activePanels.length).toBe(1);
			expect(activePanels[0].querySelector('.ant-collapse-header').innerHTML.indexOf('2014')).toBeTruthy();

			// 4. Add a new date for year 2013
			editComponent.dateChangeCallback(new Date('2015-01-10'));
			fixture.detectChanges();

			httpTestingController.match('../api/BankHolidayCalendars/Save')[0].flush({
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				Years: [
					{
						Year: 2013,
						Dates: [
							{
								Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
								Date: '2013-01-09',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					},
					{
						Year: 2014,
						Dates: [
							{
								Id: '6f5fe53b-9045-4f0e-bbc6-ae0a12d00bc7',
								Date: '2014-01-09',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					},
					{
						Year: 2015,
						Dates: [
							{
								Id: '943eacf7-d712-4e58-89fd-c5f6686e6d99',
								Date: '2015-01-09',
								Description: 'BankHoliday',
								IsDeleted: false
							},
							{
								Id: '876b72ef-4238-423a-a05b-a9c3003b10df',
								Date: '2015-01-10',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					}
				]
			});
			fixture.detectChanges();

			backToBankHolidayList();

			const activeYears = document.querySelectorAll('.ant-collapse-content .ant-tabs-tab.ant-tabs-tab-active');
			expect(activeYears.length).toBe(1);
			expect(activeYears[0].innerHTML.indexOf('2015') > -1).toBeTruthy();
		});
	}));

	it('should bank holiday calendar name not be changed if edit component bank holiday calendar name is empty', async(() => {
		fixture.detectChanges();
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				Years: [
					{
						Year: 2013,
						Dates: [
							{
								Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
								Date: '2013-01-09',
								Description: 'BankHoliday',
								IsDeleted: false
							}
						]
					},
					{
						Year: 2014,
						Dates: [
							{
								Id: '6f5fe53b-9045-4f0e-bbc6-ae0a12d00bc7',
								Date: '2014-01-09',
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
		const calendarList = bankHolidayCalendarSettings.getElementsByTagName('nz-collapse-panel');

		// 1. Open the calendar
		openCalendar(calendarList);

		// 2. Active the year 2014 in the calendar
		activeYear(calendarList);

		fixture.whenStable().then(() => {
			fixture.detectChanges();

			// 3. Go to edit page for this calendar
			clickEditButton(calendarList);

			const editComponent = getEditBankHolidayCalendarComponent();

			expect(editComponent.newCalendar).toBeTruthy();
			expect(editComponent.newCalendar.ActiveYearIndex).toBe(1);

			// 4. empty calendar name
			const editComponentElement = document.querySelector('.edit-bank-holiday-calendar');
			editComponent.calendarName = '';
			editComponentElement
				.querySelector('.edit-bank-holiday-calendar-header > input')
				.dispatchEvent(new Event('change'));
			fixture.detectChanges();

			// 5. click delete button
			const panels = editComponentElement.querySelectorAll('nz-collapse-panel');
			panels[1].querySelector('nz-list-item .remove-date-icon').dispatchEvent(new Event('click'));
			fixture.detectChanges();

			const nzAlert = editComponentElement.querySelectorAll('nz-alert');
			expect(nzAlert[0].getAttribute('nzType')).toBe('error');
			expect(httpTestingController.match('../api/BankHolidayCalendars/Save').length).toBe(0);
			expect(panels[1].querySelectorAll('nz-list-item').length).toBe(1);

			// 6. Go back to calendar list after the calendar is saved
			backToBankHolidayList();

			const panelHeader = document.querySelector('nz-collapse-panel .ant-collapse-header');
			expect(panelHeader.innerHTML.indexOf('Bank holiday calendar') > -1).toBeTruthy();
		});
	}));

	function getEditBankHolidayCalendarComponent() {
		const parentElement: DocumentFragment = fixture.debugElement.nativeElement;
		const editElement = getDebugNode(parentElement.querySelector('.edit-bank-holiday-calendar')) as DebugElement;
		return editElement.componentInstance;
	}

	function clickEditButton(calendarList) {
		calendarList[0].getElementsByClassName('anticon-edit')[0].parentElement.dispatchEvent(new Event('click'));
		fixture.detectChanges();
	}

	function backToBankHolidayList() {
		document
			.querySelector('.edit-bank-holiday-calendar > .operation-buttons button')
			.dispatchEvent(new Event('click'));
		fixture.detectChanges();
	}

	function openCalendar(calendarList) {
		calendarList[0].querySelector('.ant-collapse-header').dispatchEvent(new Event('click'));
		fixture.detectChanges();
	}

	function activeYear(calendarList) {
		calendarList[0].querySelectorAll('.ant-collapse-content .ant-tabs-tab')[1].dispatchEvent(new Event('click'));
		fixture.detectChanges();
	}
});
