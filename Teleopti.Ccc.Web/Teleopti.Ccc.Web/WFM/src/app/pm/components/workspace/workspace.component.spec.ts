import { HttpClientModule } from '@angular/common/http';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite, PageObject } from '@wfm/test';
import { NzButtonModule, NzTableModule, NzToolTipModule } from 'ng-zorro-antd';
import { MockTranslationModule } from '../../../../mocks/translation';
import { WorkspaceComponent } from './workspace.component';
import { PowerBIService } from '../../core/powerbi.service';
// import { adina, eva, fakeBackendProvider, myles, SearchService, WorkspaceService } from '../../services';

describe('WorkspaceComponent', () => {
	let component: WorkspaceComponent;
	let fixture: ComponentFixture<WorkspaceComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [WorkspaceComponent],
			imports: [MockTranslationModule, HttpClientModule, NzTableModule, NzButtonModule, NzToolTipModule],
			providers: [PowerBIService]
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
});

class Page extends PageObject {
	// get getPeople() {
	//     return this.queryAll('[data-test-workspace] [data-test-person]');
	// }
}
