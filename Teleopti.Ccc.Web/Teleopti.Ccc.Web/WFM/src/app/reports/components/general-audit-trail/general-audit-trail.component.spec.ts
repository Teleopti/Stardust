import { HttpClientModule } from '@angular/common/http';
import { DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { NzButtonModule, NzFormModule, NzInputModule, NzTableModule, NzToolTipModule } from 'ng-zorro-antd';
import { of } from 'rxjs';
import { configureTestSuite } from '../../../../configure-test-suit';
import { MockTranslationModule } from '../../../../mocks/translation';
import { GeneralAuditTrail } from './general-audit-trail.component';

describe('SearchPageComponent', () => {
	let component: GeneralAuditTrail;
	let fixture: ComponentFixture<GeneralAuditTrail>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [GeneralAuditTrail],
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
		fixture = TestBed.createComponent(GeneralAuditTrail);
		component = fixture.componentInstance;
		page = new Page(fixture);
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});

class Page {
	fixture: ComponentFixture<GeneralAuditTrail>;

	constructor(fixture: ComponentFixture<GeneralAuditTrail>) {
		this.fixture = fixture;
	}

	private queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}
}
