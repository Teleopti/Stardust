import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FeedbackMessageComponent } from './feedback-message.component';
import { NzAlertModule } from 'ng-zorro-antd';
import { HttpClientModule } from '@angular/common/http';
import { configureTestSuite } from '../../../../configure-test-suit';
import { MockTranslationModule } from '../../../../mocks/translation';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('FeedbackMessageComponent', () => {
	let component: FeedbackMessageComponent;
	let fixture: ComponentFixture<FeedbackMessageComponent>;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [FeedbackMessageComponent],
			imports: [MockTranslationModule, HttpClientModule, NzAlertModule, NoopAnimationsModule]
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
