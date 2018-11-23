import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { configureTestSuite, PageObject } from '@wfm/test';
import { NzAlertModule, NzButtonModule, NzFormModule, NzInputModule } from 'ng-zorro-antd';
import { ResetPasswordService } from '../../shared/reset-password.service';
import { ResetPasswordFormComponent } from './reset-password-form.component';

const resetPasswordStateService: Partial<ResetPasswordService> = {};

describe('ResetPasswordFormComponent', () => {
	let component: ResetPasswordFormComponent;
	let fixture: ComponentFixture<ResetPasswordFormComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [ResetPasswordFormComponent],
			imports: [
				MockTranslationModule,
				ReactiveFormsModule,
				NzFormModule,
				NzAlertModule,
				NzButtonModule,
				NzInputModule
			],
			providers: [{ provide: ResetPasswordService, useValue: resetPasswordStateService }]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(ResetPasswordFormComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});

class Page extends PageObject {
	get menuItems() {
		return this.queryAll('[data-test-menu-item]');
	}
}
