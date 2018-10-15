import { HttpClientModule } from '@angular/common/http';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { configureTestSuite } from '../../../../configure-test-suit';
import { FeedbackMessageComponent } from '../../../shared/components/feedback-message';
import { TitleBarComponent } from './title-bar.component';

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
