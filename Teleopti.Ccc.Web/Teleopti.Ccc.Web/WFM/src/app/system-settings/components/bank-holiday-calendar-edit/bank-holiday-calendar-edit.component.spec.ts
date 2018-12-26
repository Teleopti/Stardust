import { DOCUMENT } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NgZorroAntdModule } from 'ng-zorro-antd';

import { configureTestSuite } from '@wfm/test';
import { UserService } from 'src/app/core/services';
import { BankHolidayCalendarEditComponent } from './bank-holiday-calendar-edit.component';

describe('BankHolidayCalendarEditComponent', () => {
	let fixture: ComponentFixture<BankHolidayCalendarEditComponent>;
	let document: Document;
	let component: BankHolidayCalendarEditComponent;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [BankHolidayCalendarEditComponent],
			imports: [
				TranslateModule.forRoot(),
				NgZorroAntdModule.forRoot(),
				FormsModule,
				ReactiveFormsModule,
				HttpClientTestingModule
			],
			providers: [TranslateService, UserService]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(BankHolidayCalendarEditComponent);
		document = TestBed.get(DOCUMENT);
		component = fixture.componentInstance;
	});

	it('should create component', () => {
		expect(component).toBeTruthy();
	});
});
