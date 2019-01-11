import { DOCUMENT } from '@angular/common';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite, MockComponent } from '@wfm/test';
import { IStateService } from 'angular-ui-router';
import { NzToolTipModule } from 'ng-zorro-antd';
import { ReplaySubject } from 'rxjs';
import { PasswordService } from 'src/app/authentication/services/password.service';
import { NavigationService, ThemeService, TogglesService, UserService } from 'src/app/core/services';
import { AreaService } from '../../shared/area.service';
import { BusinessUnitService } from '../../shared/businessunit.service';
import { ToggleMenuService } from '../../shared/toggle-menu.service';
import { FeedbackComponent } from '../feedback';
import { TopNavigationComponent } from './top-navigation.component';
import { MockTranslationModule } from '@wfm/mocks/translation';

class MockStateService implements Partial<IStateService> {
	public current: {
		name: 'systemSettings';
	};

	public href() {
		return '';
	}
}

class MockToggleMenuService implements Partial<ToggleMenuService> {
	public get showMenu$() {
		return new ReplaySubject<boolean>(1);
	}
	isMobileView() {
		return false;
	}
}

describe('TopNavigation', () => {
	let component: TopNavigationComponent;
	let fixture: ComponentFixture<TopNavigationComponent>;
	let httpMock: HttpTestingController;
	let document: Document;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [
				TopNavigationComponent,
				FeedbackComponent,
				MockComponent({ selector: 'settings-menu' }),
				MockComponent({ selector: 'business-unit-selector' })
			],
			imports: [MockTranslationModule, NzToolTipModule, HttpClientTestingModule],
			providers: [
				{
					provide: '$state',
					useClass: MockStateService
				},
				{ provide: ToggleMenuService, useClass: MockToggleMenuService },
				TogglesService,
				NavigationService,
				AreaService,
				BusinessUnitService,
				ThemeService,
				UserService,
				PasswordService
			]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(TopNavigationComponent);
		httpMock = TestBed.get(HttpTestingController);
		document = TestBed.get(DOCUMENT);
		component = fixture.componentInstance;
	});

	it('should create component', () => {
		expect(component).toBeTruthy();
	});

	it('should show system settings icon when toggle WFM_Setting_BankHolidayCalendar_Create_79297 is on', () => {
		const toggleRequest = httpMock.expectOne('../ToggleHandler/AllToggles');
		toggleRequest.flush({ WFM_Setting_BankHolidayCalendar_Create_79297: true });
		fixture.detectChanges();

		const systemSettingsIconElement = document.getElementsByClassName('system-settings-icon')[0];
		expect(systemSettingsIconElement).toBeTruthy();
		expect(systemSettingsIconElement.getElementsByTagName('i').length).toBe(1);
	});

	it('should not show system settings icon when toggle WFM_Setting_BankHolidayCalendar_Create_79297 is off', () => {
		const toggleRequest = httpMock.expectOne('../ToggleHandler/AllToggles');
		toggleRequest.flush({ WFM_Setting_BankHolidayCalendar_Create_79297: false });
		fixture.detectChanges();

		const systemSettingsIconElement = document.getElementsByClassName('system-settings-icon')[0];
		expect(systemSettingsIconElement).toBeFalsy();
	});
});
