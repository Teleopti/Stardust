import { DOCUMENT } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { IStateService } from 'angular-ui-router';

import { configureTestSuite } from '@wfm/test';
import { UserService } from 'src/app/core/services';
import { PasswordService } from 'src/app/authentication/services/password.service';
import { SystemSettingsComponent } from './system-settings.component';

class mockStateService implements Partial<IStateService> {
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
			declarations: [SystemSettingsComponent],
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
				UserService,
				PasswordService,
				TranslateService
			]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(SystemSettingsComponent);
		document = TestBed.get(DOCUMENT);
		component = fixture.componentInstance;
	});

	it('should create component', () => {
		expect(component).toBeTruthy();
	});

	it('should show header title', () => {
		fixture.detectChanges();

		var titleEle = document.getElementsByTagName('h1')[0];
		expect(titleEle).toBeTruthy();
		expect(titleEle.getElementsByTagName('i').length).toBe(1);
		expect(titleEle.getElementsByTagName('i')[0].className).toBe('anticon anticon-setting');
	});
});
