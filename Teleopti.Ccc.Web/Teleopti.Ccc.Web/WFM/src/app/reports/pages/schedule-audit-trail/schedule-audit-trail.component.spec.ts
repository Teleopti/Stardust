/*import { HttpClientModule } from '@angular/common/http';
import { DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { NzButtonModule, NzFormModule, NzInputModule, NzTableModule, NzToolTipModule } from 'ng-zorro-antd';
import { of } from 'rxjs';
import { configureTestSuite } from '../../../../test/configure-test-suit';
import { MockTranslationModule } from '../../../../mocks/translation';
import { ScheduleAuditTrail } from './schedule-audit-trail.component';

describe('SearchPageComponent', () => {
	let component: ScheduleAuditTrail;
	let fixture: ComponentFixture<ScheduleAuditTrail>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [ScheduleAuditTrail],
			imports: [
				MockTranslationModule,
				HttpClientModule,
				ReactiveFormsModule,
				NoopAnimationsModule,
				NzFormModule,
				NzButtonModule,
				NzTableModule,
				NzInputModule,
				NzToolTipModule
			],
			providers: []
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(ScheduleAuditTrail);
		component = fixture.componentInstance;
		page = new Page(fixture);
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});

class Page {
	fixture: ComponentFixture<ScheduleAuditTrail>;

	constructor(fixture: ComponentFixture<ScheduleAuditTrail>) {
		this.fixture = fixture;
	}

	private queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}
}
*/
