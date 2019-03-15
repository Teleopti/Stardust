import { DOCUMENT } from '@angular/common';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MockTranslationModule, MockTranslateService } from '@wfm/mocks/translation';
import { configureTestSuite } from '@wfm/test';
import { IStateService } from 'angular-ui-router';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { TogglesService } from 'src/app/core/services';
import { BankHolidayCalendarComponent } from '../../components/bank-holiday-calendar';
import { BankHolidayCalendarEditComponent } from '../../components/bank-holiday-calendar-edit';
import { BankHolidayCalendarAssignToSitesComponent } from '../../components/bank-holiday-calendar-assign-to-sites';
import { SystemSettingsComponent } from './system-settings.component';
import { BankCalendarDataService } from '../../shared';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

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

	beforeEach(() => {
		TestBed.configureTestingModule({
			declarations: [
				SystemSettingsComponent,
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
				{
					provide: '$state',
					useClass: MockStateService
				},
				MockTranslateService,
				TogglesService,
				BankCalendarDataService
			]
		}).compileComponents();

		fixture = TestBed.createComponent(SystemSettingsComponent);
		document = TestBed.get(DOCUMENT);
		component = fixture.componentInstance;
		fixture.autoDetectChanges(true);
	});

	it('should show header title', async(() => {
		fixture.whenStable().then(() => {
			const titleEle = document.getElementsByTagName('h1')[0];
			expect(titleEle).toBeTruthy();
			expect(titleEle.getElementsByTagName('i').length).toBe(1);
			expect(titleEle.getElementsByTagName('i')[0].className).toBe('anticon anticon-setting');
		});
	}));
});
