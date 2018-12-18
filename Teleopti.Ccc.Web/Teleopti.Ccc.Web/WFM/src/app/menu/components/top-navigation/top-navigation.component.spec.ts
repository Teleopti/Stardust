import { DOCUMENT } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TranslateModule } from '@ngx-translate/core';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { IStateService } from 'angular-ui-router';
import { ReplaySubject } from 'rxjs';

import { configureTestSuite } from '@wfm/test';
import { ThemeService, NavigationService, TogglesService, UserService } from 'src/app/core/services';
import { PasswordService } from 'src/app/authentication/services/password.service';
import { ToggleMenuService } from '../../shared/toggle-menu.service';
import { TopNavigationComponent } from './top-navigation.component';
import { BusinessUnitSelectorComponent } from '../businessunit-selector';
import { BusinessUnitService } from '../../shared/businessunit.service';
import { FeedbackComponent } from '../feedback';
import { SettingsMenuComponent } from '../settings-menu';
import { ChangePasswordComponent } from 'src/app/authentication/components/change-password/change-password.component';

class mockStateService implements Partial<IStateService> {
	public current: {
		name: 'generalSettings';
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
				BusinessUnitSelectorComponent,
				FeedbackComponent,
				SettingsMenuComponent,
				ChangePasswordComponent
			],
			imports: [
				TranslateModule.forRoot(),
				NgZorroAntdModule.forRoot(),
				FormsModule,
				ReactiveFormsModule,
				HttpClientTestingModule
			],
			providers: [
				{
					provide: '$state',
					useClass: mockStateService
				},
				{ provide: ToggleMenuService, useClass: MockToggleMenuService },
				TogglesService,
				NavigationService,
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

	it('should show general settings icon when toggle WFM_Setting_BankHolidayCalendar_Create_79297 is on', () => {
		let toggleRequest = httpMock.expectOne('../ToggleHandler/AllToggles');
		toggleRequest.flush({ WFM_Setting_BankHolidayCalendar_Create_79297: true });
		fixture.detectChanges();

		var systemSettingsIconElement = document.getElementsByClassName('system-settings-icon')[0];
		expect(systemSettingsIconElement).toBeTruthy();
		expect(systemSettingsIconElement.getElementsByTagName('i').length).toBe(1);
	});

	it('should not show general settings icon when toggle WFM_Setting_BankHolidayCalendar_Create_79297 is off', () => {
		let toggleRequest = httpMock.expectOne('../ToggleHandler/AllToggles');
		toggleRequest.flush({ WFM_Setting_BankHolidayCalendar_Create_79297: false });
		fixture.detectChanges();

		var systemSettingsIconElement = document.getElementsByClassName('system-settings-icon')[0];
		expect(systemSettingsIconElement).toBeFalsy();
	});
});
