import { DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { configureTestSuite } from '../../../../configure-test-suit';
import { ApiAccessTestModule } from '../../api-access.test.module';
import { AddAppPageComponent } from './add-app-page.component';

describe('AddAppPageComponent', () => {
	let component: AddAppPageComponent;
	let fixture: ComponentFixture<AddAppPageComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			imports: [ApiAccessTestModule]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(AddAppPageComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);

		//fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should display person app logon', () => {
		//fixture.detectChanges();
		//let input: HTMLInputElement = page.logonFields[0].nativeElement;
		//expect(input.value).toEqual('');
	});
});

class Page {
	get logonFields() {
		return this.queryAll('[data-test-application-logon] [data-test-person-logon]');
	}

	fixture: ComponentFixture<AddAppPageComponent>;

	constructor(fixture: ComponentFixture<AddAppPageComponent>) {
		this.fixture = fixture;
	}

	private queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}
}
