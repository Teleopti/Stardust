import { HttpClientModule } from '@angular/common/http';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { configureTestSuite, PageObject } from '@wfm/test';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { FeedbackMessageComponent } from './feedback-message.component';

describe('FeedbackMessageComponent', () => {
	let component: FeedbackMessageComponent;
	let fixture: ComponentFixture<FeedbackMessageComponent>;
	let page: Page;

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
		page = new Page(fixture);
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
	it('should show feedback message', () => {
		component.feedbackType = 'newRelease';
		fixture.detectChanges();
		fixture.whenStable().then(() => {
			expect(page.feedbackMessage.nativeElement.innerText.length).toBeGreaterThan(0);
		});
	});
});

class Page extends PageObject {
	get feedbackMessage() {
		return this.queryAll('[data-test-feedbackmessage]')[0];
	}
}
