import { HttpClientModule } from '@angular/common/http';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { configureTestSuite } from '../../../../configure-test-suit';
import { FeedbackMessageComponent } from './feedback-message.component';

describe('FeedbackMessageComponent', () => {
	let component: FeedbackMessageComponent;
	let fixture: ComponentFixture<FeedbackMessageComponent>;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [FeedbackMessageComponent],
			imports: [MockTranslationModule, HttpClientModule, NgZorroAntdModule, NoopAnimationsModule]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(FeedbackMessageComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
