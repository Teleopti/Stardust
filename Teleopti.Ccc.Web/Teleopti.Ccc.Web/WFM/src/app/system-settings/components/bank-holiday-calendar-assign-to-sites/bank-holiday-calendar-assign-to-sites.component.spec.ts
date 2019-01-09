import { DOCUMENT } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NgZorroAntdModule } from 'ng-zorro-antd';

import { configureTestSuite } from '@wfm/test';
import { UserService } from 'src/app/core/services';
import { BankHolidayCalendarAssignToSitesComponent } from './bank-holiday-calendar-assign-to-sites.component';

import { GroupPageService } from 'src/app/shared/services/group-page-service';

fdescribe('BankHolidayCalendarAssignToSitesComponent', () => {
	let fixture: ComponentFixture<BankHolidayCalendarAssignToSitesComponent>;
	let document: Document;
	let component: BankHolidayCalendarAssignToSitesComponent;
	let httpTestingController: HttpTestingController;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [BankHolidayCalendarAssignToSitesComponent],
			imports: [
				TranslateModule.forRoot(),
				NgZorroAntdModule.forRoot(),
				FormsModule,
				ReactiveFormsModule,
				HttpClientTestingModule,
				NoopAnimationsModule
			],
			providers: [TranslateService, UserService, GroupPageService]
		}).compileComponents();

		fixture = TestBed.createComponent(BankHolidayCalendarAssignToSitesComponent);
		document = TestBed.get(DOCUMENT);
		component = fixture.componentInstance;
		fixture.autoDetectChanges(true);
		httpTestingController = TestBed.get(HttpTestingController);
	}));

	it('should create component', () => {
		expect(component).toBeTruthy();
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

		let container = document.getElementsByClassName('bank-holiday-calendar-assign-to-sites')[0];
		let list = container.getElementsByClassName('bank-holiday-calendar-sites-list')[0];
		let listItems = list.getElementsByTagName('nz-list-item');

		expect(listItems[0].innerHTML.indexOf('BTS') > -1).toBeTruthy();
		expect(listItems[1].innerHTML.indexOf('Dubai') > -1).toBeTruthy();
	});

	it('should be able to select a calendar for a site', () => {
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

		let container = document.getElementsByClassName('bank-holiday-calendar-assign-to-sites')[0];
		let list = container.getElementsByClassName('bank-holiday-calendar-sites-list')[0];
		let listItems = list.getElementsByTagName('nz-list-item');

		listItems[0].getElementsByTagName('input')[0].dispatchEvent(new Event('click'));

		document.getElementsByClassName('ant-select-dropdown-menu-item')[0].dispatchEvent(new Event('click'));

		httpTestingController.match('../api/SiteBankHolidayCalendars/Update')[0].flush('true');
		fixture.detectChanges();

		expect(listItems[0].innerHTML.indexOf('BTS') > -1).toBeTruthy();
	});
});
