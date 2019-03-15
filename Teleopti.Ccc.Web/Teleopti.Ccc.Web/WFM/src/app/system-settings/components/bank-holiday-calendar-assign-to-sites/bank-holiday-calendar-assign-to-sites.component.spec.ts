import { DOCUMENT } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpResponse } from '@angular/common/http';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { NgZorroAntdModule } from 'ng-zorro-antd';

import { configureTestSuite, PageObject } from '@wfm/test';
import { BankHolidayCalendarAssignToSitesComponent } from './bank-holiday-calendar-assign-to-sites.component';
import { MockTranslationModule, MockTranslateService } from '@wfm/mocks/translation';
import { BankCalendarDataService } from '../../shared';
import { GroupPageService } from 'src/app/shared/services/group-page-service';
import { By } from '@angular/platform-browser';

describe('BankHolidayCalendarAssignToSitesComponent', () => {
	let fixture: ComponentFixture<BankHolidayCalendarAssignToSitesComponent>;
	let document: Document;
	let dataService: BankCalendarDataService;
	let component: BankHolidayCalendarAssignToSitesComponent;
	let httpTestingController: HttpTestingController;
	let page: Page;

	configureTestSuite();

	beforeEach(() => {
		TestBed.configureTestingModule({
			declarations: [BankHolidayCalendarAssignToSitesComponent],
			imports: [
				MockTranslationModule,
				NgZorroAntdModule,
				FormsModule,
				ReactiveFormsModule,
				HttpClientTestingModule,
				NoopAnimationsModule
			],
			providers: [MockTranslateService, GroupPageService, BankCalendarDataService]
		}).compileComponents();

		fixture = TestBed.createComponent(BankHolidayCalendarAssignToSitesComponent);
		document = TestBed.get(DOCUMENT);
		page = new Page(fixture);
		component = fixture.componentInstance;
		dataService = TestBed.get(BankCalendarDataService);
		fixture.autoDetectChanges(true);
		httpTestingController = TestBed.get(HttpTestingController);
	});

	it('should render sites list', () => {
		httpTestingController
			.match('../api/GroupPage/AvailableGroupPages?date=' + moment().format('YYYY-MM-DD'))[0]
			.flush({
				BusinessHierarchy: [
					{
						Children: [{ Name: 'BTS', Id: '9d013613-7c79-4621-b166-a39a00b9d634' }],
						Id: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3',
						Name: 'BTS'
					},
					{
						Children: [{ Name: 'Dubai 1', Id: '3f6236f8-dfb0-4b44-93ed-a7ba00f816bf' }],
						Id: '6bf99381-0110-4866-a3f6-a7ba00f7fdd4',
						Name: 'Dubai'
					}
				],
				GroupPages: []
			});

		fixture.detectChanges();

		const container = document.getElementsByClassName('bank-holiday-calendar-assign-to-sites')[0];
		const list = container.getElementsByClassName('bank-holiday-calendar-sites-list')[0];
		const listItems = list.getElementsByTagName('nz-list-item');

		expect(listItems[0].innerHTML.indexOf('BTS') > -1).toBeTruthy();
		expect(listItems[1].innerHTML.indexOf('Dubai') > -1).toBeTruthy();
	});

	it('should preset sites selected calendar', () => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
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
		]);

		httpTestingController
			.match('../api/GroupPage/AvailableGroupPages?date=' + moment().format('YYYY-MM-DD'))[0]
			.flush({
				BusinessHierarchy: [
					{
						Children: [{ Name: 'BTS', Id: '9d013613-7c79-4621-b166-a39a00b9d634' }],
						Id: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3',
						Name: 'BTS'
					},
					{
						Children: [{ Name: 'Dubai 1', Id: '3f6236f8-dfb0-4b44-93ed-a7ba00f816bf' }],
						Id: '6bf99381-0110-4866-a3f6-a7ba00f7fdd4',
						Name: 'Dubai'
					}
				],
				GroupPages: []
			});

		httpTestingController.match('../api/SiteBankHolidayCalendars')[0].flush([
			{
				Site: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3',
				Calendars: ['e0e97b97-1f4c-4834-9cc1-a9c3003b10df']
			}
		]);

		fixture.detectChanges();

		expect(component.sitesList[0].SelectedCalendarId).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
	});

	it('should be able to select a calendar for a site', () => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
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
		]);

		httpTestingController
			.match('../api/GroupPage/AvailableGroupPages?date=' + moment().format('YYYY-MM-DD'))[0]
			.flush({
				BusinessHierarchy: [
					{
						Children: [{ Name: 'BTS', Id: '9d013613-7c79-4621-b166-a39a00b9d634' }],
						Id: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3',
						Name: 'BTS'
					},
					{
						Children: [{ Name: 'Dubai 1', Id: '3f6236f8-dfb0-4b44-93ed-a7ba00f816bf' }],
						Id: '6bf99381-0110-4866-a3f6-a7ba00f7fdd4',
						Name: 'Dubai'
					}
				],
				GroupPages: []
			});

		httpTestingController
			.match('../api/SiteBankHolidayCalendars')[0]
			.flush([{ Site: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3', Calendars: [] }]);

		fixture.detectChanges();

		page.listItems[0].nativeElement.click();

		fixture.detectChanges();

		page.dropdownOptions[0].click();

		httpTestingController
			.expectOne('../api/SiteBankHolidayCalendars/Update')
			.event(new HttpResponse<boolean>({ body: true }));

		fixture.detectChanges();

		expect(component.sitesList[0].SelectedCalendarId).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
	});

	it('should clean the selected value if failed to update calendar for a site', () => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
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
		]);

		httpTestingController
			.match('../api/GroupPage/AvailableGroupPages?date=' + moment().format('YYYY-MM-DD'))[0]
			.flush({
				BusinessHierarchy: [
					{
						Children: [{ Name: 'BTS', Id: '9d013613-7c79-4621-b166-a39a00b9d634' }],
						Id: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3',
						Name: 'BTS'
					},
					{
						Children: [{ Name: 'Dubai 1', Id: '3f6236f8-dfb0-4b44-93ed-a7ba00f816bf' }],
						Id: '6bf99381-0110-4866-a3f6-a7ba00f7fdd4',
						Name: 'Dubai'
					}
				],
				GroupPages: []
			});
		httpTestingController
			.match('../api/SiteBankHolidayCalendars')[0]
			.flush([{ Site: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3', Calendars: [] }]);

		fixture.detectChanges();

		page.listItems[0].nativeElement.click();

		fixture.detectChanges();

		page.dropdownOptions[0].click();

		httpTestingController
			.expectOne('../api/SiteBankHolidayCalendars/Update')
			.event(new HttpResponse<boolean>({ body: false }));

		fixture.detectChanges();

		expect(component.sitesList[0].SelectedCalendarId).toBe(null);
	});

	it('should clean the selected value if calendar is deleted', () => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
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
		]);
		httpTestingController
			.match('../api/GroupPage/AvailableGroupPages?date=' + moment().format('YYYY-MM-DD'))[0]
			.flush({
				BusinessHierarchy: [
					{
						Children: [{ Name: 'BTS', Id: '9d013613-7c79-4621-b166-a39a00b9d634' }],
						Id: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3',
						Name: 'BTS'
					},
					{
						Children: [{ Name: 'Dubai 1', Id: '3f6236f8-dfb0-4b44-93ed-a7ba00f816bf' }],
						Id: '6bf99381-0110-4866-a3f6-a7ba00f7fdd4',
						Name: 'Dubai'
					}
				],
				GroupPages: []
			});
		httpTestingController
			.match('../api/SiteBankHolidayCalendars')[0]
			.flush([
				{ Site: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3', Calendars: ['e0e97b97-1f4c-4834-9cc1-a9c3003b10df'] }
			]);

		fixture.detectChanges();
		expect(component.sitesList[0].SelectedCalendarId).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');

		dataService.bankHolidayCalendarsList$.next([]);
		fixture.detectChanges();
		expect(component.sitesList[0].SelectedCalendarId).toBe(null);
	});

	it('should keep the selected calendar value for related sites when calendar info is updated', () => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
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
							}
						]
					}
				]
			}
		]);
		httpTestingController
			.match('../api/GroupPage/AvailableGroupPages?date=' + moment().format('YYYY-MM-DD'))[0]
			.flush({
				BusinessHierarchy: [
					{
						Children: [{ Name: 'BTS', Id: '9d013613-7c79-4621-b166-a39a00b9d634' }],
						Id: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3',
						Name: 'BTS'
					},
					{
						Children: [{ Name: 'Dubai 1', Id: '3f6236f8-dfb0-4b44-93ed-a7ba00f816bf' }],
						Id: '6bf99381-0110-4866-a3f6-a7ba00f7fdd4',
						Name: 'Dubai'
					}
				],
				GroupPages: []
			});
		httpTestingController
			.match('../api/SiteBankHolidayCalendars')[0]
			.flush([
				{ Site: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3', Calendars: ['e0e97b97-1f4c-4834-9cc1-a9c3003b10df'] },
				{ Site: '6bf99381-0110-4866-a3f6-a7ba00f7fdd4', Calendars: ['e0e97b97-1f4c-4834-9cc1-a9c3003b10df'] }
			]);
		fixture.detectChanges();

		expect(component.sitesList[0].SelectedCalendarId).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
		expect(component.sitesList[1].SelectedCalendarId).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
		expect(component.siteCalendarsList.length).toBe(2);

		page.listItems[0].nativeElement.click();

		fixture.detectChanges();

		page.dropdownOptions[0].click();

		httpTestingController
			.expectOne('../api/SiteBankHolidayCalendars/Update')
			.event(new HttpResponse<boolean>({ body: true }));
		fixture.detectChanges();

		expect(component.sitesList[0].SelectedCalendarId).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
		expect(component.sitesList[1].SelectedCalendarId).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');

		dataService.bankHolidayCalendarsList$.next([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar renamed',
				Years: [
					{
						Year: '2013',
						Dates: [
							{
								Id: '1a9e52aa-ca90-42a0-aa6d-a9c3003b10df',
								Date: '2013-01-09',
								Description: 'BankHoliday1',
								IsDeleted: false
							}
						]
					}
				],
				ActiveYearIndex: 0
			}
		]);
		fixture.detectChanges();

		expect(component.sitesList[0].SelectedCalendarId).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
		expect(component.sitesList[1].SelectedCalendarId).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
		expect(component.siteCalendarsList[0].Site).toBe('7a6c0754-4de8-48fb-8aee-a39a00b9d1c3');
		expect(component.siteCalendarsList[0].Calendars[0]).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
		expect(component.siteCalendarsList[1].Site).toBe('6bf99381-0110-4866-a3f6-a7ba00f7fdd4');
		expect(component.siteCalendarsList[1].Calendars[0]).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
	});

	it('should update the list after adding a calendar for a new site', () => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				CurrentYearIndex: 0,
				Years: []
			}
		]);
		httpTestingController
			.match('../api/GroupPage/AvailableGroupPages?date=' + moment().format('YYYY-MM-DD'))[0]
			.flush({
				BusinessHierarchy: [
					{
						Children: [{ Name: 'BTS', Id: '9d013613-7c79-4621-b166-a39a00b9d634' }],
						Id: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3',
						Name: 'BTS'
					},
					{
						Children: [{ Name: 'Dubai 1', Id: '3f6236f8-dfb0-4b44-93ed-a7ba00f816bf' }],
						Id: '6bf99381-0110-4866-a3f6-a7ba00f7fdd4',
						Name: 'Dubai'
					}
				],
				GroupPages: []
			});
		httpTestingController
			.match('../api/SiteBankHolidayCalendars')[0]
			.flush([
				{ Site: '6bf99381-0110-4866-a3f6-a7ba00f7fdd4', Calendars: ['e0e97b97-1f4c-4834-9cc1-a9c3003b10df'] }
			]);
		fixture.detectChanges();

		expect(component.siteCalendarsList.length).toBe(1);

		page.listItems[0].nativeElement.click();

		fixture.detectChanges();

		page.dropdownOptions[0].click();

		httpTestingController
			.expectOne('../api/SiteBankHolidayCalendars/Update')
			.event(new HttpResponse<boolean>({ body: true }));
		fixture.detectChanges();

		expect(component.sitesList[0].SelectedCalendarId).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
		expect(component.sitesList[1].SelectedCalendarId).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');

		expect(component.siteCalendarsList[0].Site).toBe('6bf99381-0110-4866-a3f6-a7ba00f7fdd4');
		expect(component.siteCalendarsList[0].Calendars[0]).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
		expect(component.siteCalendarsList[1].Site).toBe('7a6c0754-4de8-48fb-8aee-a39a00b9d1c3');
		expect(component.siteCalendarsList[1].Calendars[0]).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');
	});

	it('should update the siteCalendarsList after changing calendar for a site', () => {
		httpTestingController.match('../api/BankHolidayCalendars')[0].flush([
			{
				Id: 'e0e97b97-1f4c-4834-9cc1-a9c3003b10df',
				Name: 'Bank holiday calendar',
				CurrentYearIndex: 0,
				Years: []
			},
			{
				Id: 'bab44a97-2f77-419e-a5bb-cd12a8c39300',
				Name: 'Bank holiday calendar 2',
				CurrentYearIndex: 0,
				Years: []
			}
		]);
		httpTestingController
			.match('../api/GroupPage/AvailableGroupPages?date=' + moment().format('YYYY-MM-DD'))[0]
			.flush({
				BusinessHierarchy: [
					{
						Children: [{ Name: 'BTS', Id: '9d013613-7c79-4621-b166-a39a00b9d634' }],
						Id: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3',
						Name: 'BTS'
					},
					{
						Children: [{ Name: 'Dubai 1', Id: '3f6236f8-dfb0-4b44-93ed-a7ba00f816bf' }],
						Id: '6bf99381-0110-4866-a3f6-a7ba00f7fdd4',
						Name: 'Dubai'
					}
				],
				GroupPages: []
			});
		httpTestingController
			.match('../api/SiteBankHolidayCalendars')[0]
			.flush([
				{ Site: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3', Calendars: ['e0e97b97-1f4c-4834-9cc1-a9c3003b10df'] }
			]);

		fixture.detectChanges();

		expect(component.siteCalendarsList[0].Calendars[0]).toBe('e0e97b97-1f4c-4834-9cc1-a9c3003b10df');

		page.listItems[0].nativeElement.click();

		fixture.detectChanges();

		page.dropdownOptions[1].click();

		httpTestingController
			.expectOne('../api/SiteBankHolidayCalendars/Update')
			.event(new HttpResponse<boolean>({ body: true }));
		fixture.detectChanges();

		expect(component.sitesList[0].SelectedCalendarId).toBe('bab44a97-2f77-419e-a5bb-cd12a8c39300');
		expect(component.siteCalendarsList[0].Calendars[0]).toBe('bab44a97-2f77-419e-a5bb-cd12a8c39300');
	});
});

class Page extends PageObject {
	get container() {
		return this.query('.bank-holiday-calendar-assign-to-sites');
	}

	get list() {
		return this.container.query(By.css('.bank-holiday-calendar-sites-list'));
	}

	get listItems() {
		return this.list.queryAll(By.css('input'));
	}

	get dropdownOptions() {
		return this.fixture.nativeElement.ownerDocument.querySelectorAll(
			'.ant-select-dropdown-menu-item'
		) as HTMLElement[];
	}
}
