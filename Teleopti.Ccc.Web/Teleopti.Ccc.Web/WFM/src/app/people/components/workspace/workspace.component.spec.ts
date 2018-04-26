import { DebugElement } from '@angular/core';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { PeopleTestModule } from '../../people.test.module';
import { adina } from '../../services';
import { WorkspaceComponent } from './workspace.component';

describe('WorkspaceComponent', () => {
	let component: WorkspaceComponent;
	let fixture: ComponentFixture<WorkspaceComponent>;
	let page: Page;

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			imports: [PeopleTestModule]
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
});

class Page {
	get getPeople() {
		return this.queryAll('[data-test-workspace] [data-test-person]');
	}

	get getRemoveButtons() {
		return this.queryAll('[data-test-workspace] [data-test-person] [data-test-person-remove]');
	}

	fixture: ComponentFixture<WorkspaceComponent>;

	constructor(fixture: ComponentFixture<WorkspaceComponent>) {
		this.fixture = fixture;
	}

	private queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}
}
