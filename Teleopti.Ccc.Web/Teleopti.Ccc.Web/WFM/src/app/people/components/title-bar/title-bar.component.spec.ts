import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TitleBarComponent } from './title-bar.component';
import { MockTranslationModule } from '../../../../mocks/translation';
import { FeedbackMessageComponent } from '../../../shared/components/feedback-message';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { configureTestSuite } from '../../../../configure-test-suit';
import { HttpClientModule } from '@angular/common/http';
import { NoopAnimationsModule } from '../../../../../node_modules/@angular/platform-browser/animations';

describe('TitleBarComponent', () => {
	let component: TitleBarComponent;
	let fixture: ComponentFixture<TitleBarComponent>;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [TitleBarComponent, FeedbackMessageComponent],
			imports: [MockTranslationModule, NgZorroAntdModule, HttpClientModule, NoopAnimationsModule]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(TitleBarComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
