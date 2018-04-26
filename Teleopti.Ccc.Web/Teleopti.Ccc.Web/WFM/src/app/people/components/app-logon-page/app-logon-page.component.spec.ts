import { DebugElement } from '@angular/core';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { PeopleTestModule } from '../../people.test.module';
import { AppLogonPageComponent } from './app-logon-page.component';
import { WorkspaceService, adina } from '../../services';
import { adinaLogon } from '../../services/fake-backend/logons';

describe('AppLogonPageComponent', () => {
	let component: AppLogonPageComponent;
	let fixture: ComponentFixture<AppLogonPageComponent>;
	let workspaceService: WorkspaceService;
	let page: Page;

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			imports: [PeopleTestModule]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(AppLogonPageComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
		workspaceService = fixture.debugElement.injector.get(WorkspaceService);

		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should display person app logon', () => {
		workspaceService.selectPerson(adina);
		fixture.detectChanges();
		let input: HTMLInputElement = page.logonFields[0].nativeElement;
		expect(input.value).toEqual(adinaLogon.LogonName);
	});
});

class Page {
	get logonFields() {
		return this.queryAll('[data-test-application-logon] [data-test-person-logon]');
	}

	fixture: ComponentFixture<AppLogonPageComponent>;

	constructor(fixture: ComponentFixture<AppLogonPageComponent>) {
		this.fixture = fixture;
	}

	private queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}
}
