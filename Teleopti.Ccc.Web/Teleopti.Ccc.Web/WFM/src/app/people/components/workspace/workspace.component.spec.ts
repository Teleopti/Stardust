import { HttpClientModule } from '@angular/common/http';
import { DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { NzButtonModule, NzTableModule } from 'ng-zorro-antd';
import { configureTestSuite } from '../../../../configure-test-suit';
import { MockTranslationModule } from '../../../../mocks/translation';
import { adina, eva, fakeBackendProvider, myles, SearchService, WorkspaceService } from '../../services';
import { WorkspaceComponent } from './workspace.component';

describe('WorkspaceComponent', () => {
	let component: WorkspaceComponent;
	let fixture: ComponentFixture<WorkspaceComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [WorkspaceComponent],
			imports: [MockTranslationModule, HttpClientModule, NzTableModule, NzButtonModule],
			providers: [fakeBackendProvider, SearchService, WorkspaceService]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(WorkspaceComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should show selected person', () => {
		component.workspaceService.selectPerson(adina);

		fixture.detectChanges();

		expect(page.getPeople.length).toEqual(1);
	});

	it('should be able to remove person', () => {
		component.workspaceService.selectPerson(adina);
		fixture.detectChanges();
		expect(page.getPeople.length).toEqual(1);

		page.getRemoveButtons[0].nativeElement.click();
		fixture.detectChanges();
		expect(page.getPeople.length).toEqual(0);
	});

	it('should be able to clear workspace', () => {
		component.workspaceService.selectPeople([adina, myles, eva]);
		fixture.detectChanges();
		expect(page.getPeople.length).toEqual(3);

		page.clearButton.nativeElement.click();
		fixture.detectChanges();
		expect(page.getPeople.length).toEqual(0);
	});
});

class Page {
	get getPeople() {
		return this.queryAll('[data-test-workspace] [data-test-person]');
	}

	get getRemoveButtons() {
		return this.queryAll('[data-test-workspace] [data-test-person] [data-test-person-remove]');
	}

	get clearButton() {
		return this.queryAll('[data-test-workspace] [data-test-clear-button]')[0];
	}

	fixture: ComponentFixture<WorkspaceComponent>;

	constructor(fixture: ComponentFixture<WorkspaceComponent>) {
		this.fixture = fixture;
	}

	private queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}
}
