import { DOCUMENT } from '@angular/common';
import { HttpClientTestingModule } from '@angular/common/http/testing';
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

describe('BankHolidayCalendarAddComponent', () => {
	let fixture: ComponentFixture<BankHolidayCalendarAddComponent>;
	let document: Document;
	let component: BankHolidayCalendarAddComponent;

	configureTestSuite();

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

	it('should create component', () => {
		expect(component).toBeTruthy();
	});

	it('should render name and year input box', () => {
		const addBankHolidayCalendarPanel = document.getElementsByClassName('add-new-bank-holiday-calendar')[0];

		expect(addBankHolidayCalendarPanel).toBeTruthy();
		expect(addBankHolidayCalendarPanel.getElementsByClassName('ant-input').length).toBe(2);
		expect(addBankHolidayCalendarPanel.getElementsByTagName('nz-year-picker').length).toBe(1);
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
		component.newYearTab(new Date('2015-01-10T00:00:00.000Z'));

		component.dateChangeCallback(new Date('2015-01-10T00:00:00.000Z'), component.newCalendarYears[0]);
		component.dateChangeCallback(new Date('2015-01-10T00:00:00.000Z'), component.newCalendarYears[0]);

		expect(component.newCalendarYears[0].Dates.length).toBe(1);
	});

	it('should focus to another year after deleting one year tab', () => {
		component.newYearTab(new Date('2015-01-10T00:00:00.000Z'));
		component.dateChangeCallback(new Date('2015-01-10T00:00:00.000Z'), component.newCalendarYears[0]);

		component.newYearTab(new Date('2016-01-10T00:00:00.000Z'));
		component.dateChangeCallback(new Date('2016-01-10T00:00:00.000Z'), component.newCalendarYears[1]);

		component.deleteYearTab(component.newCalendarYears[1]);

		expect(component.newCalendarTabIndex).toBe(0);
		expect(component.newCalendarYears.length).toBe(1);
		expect(component.newCalendarYears[0].Active).toBe(true);
	});

	it('should keep the remaining dates in selected dates array after removing a date', () => {
		component.newYearTab(new Date('2015-01-10T00:00:00.000Z'));
		component.dateChangeCallback(new Date('2015-01-10T00:00:00.000Z'), component.newCalendarYears[0]);
		component.dateChangeCallback(new Date('2015-01-11T00:00:00.000Z'), component.newCalendarYears[0]);

		component.removeDateOfYear(component.newCalendarYears[0].Dates[1], component.newCalendarYears[0]);

		expect(component.newCalendarYears[0].SelectedDates.length).toBe(1);
		expect(component.newCalendarYears[0].SelectedDates[0]).toBe(new Date('2015-01-10T00:00:00.000Z').getTime());
	});
});
