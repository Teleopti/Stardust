import { DOCUMENT } from '@angular/common';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MockTranslationModule, MockTranslateService } from '@wfm/mocks/translation';
import { configureTestSuite } from '@wfm/test';
import { IStateService } from 'angular-ui-router';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { PasswordService } from 'src/app/authentication/services/password.service';
import { UserService } from 'src/app/core/services';
import { BankHolidayCalendarComponent } from '../../components/bank-holiday-calendar';
import { BankHolidayCalendarAddComponent } from '../../components/bank-holiday-calendar-add';
import { BankHolidayCalendarAssignToSitesComponent } from '../../components/bank-holiday-calendar-assign-to-sites';
import { BankHolidayCalendarEditComponent } from '../../components/bank-holiday-calendar-edit';
import { SystemSettingsComponent } from './system-settings.component';

class MockStateService implements Partial<IStateService> {
	public current: {
		name: 'systemSettings';
	};

	public href() {
		return '';
	}
}

describe('SystemSettings page', () => {
	let fixture: ComponentFixture<SystemSettingsComponent>;
	let document: Document;
	let component: SystemSettingsComponent;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [
				SystemSettingsComponent,
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
				HttpClientTestingModule
			],
			providers: [
				{
					provide: '$state',
					useClass: MockStateService
				},
				UserService,
				PasswordService,
				MockTranslateService
			]
		}).compileComponents();

		fixture = TestBed.createComponent(SystemSettingsComponent);
		document = TestBed.get(DOCUMENT);
		component = fixture.componentInstance;
		fixture.autoDetectChanges(true);
	}));

	it('should create component', () => {
		expect(component).toBeTruthy();
	});

	it('should show header title', () => {
		const titleEle = document.getElementsByTagName('h1')[0];
		expect(titleEle).toBeTruthy();
		expect(titleEle.getElementsByTagName('i').length).toBe(1);
		expect(titleEle.getElementsByTagName('i')[0].className).toBe('anticon anticon-setting');
	});
});
