import { HttpClient, HttpClientModule } from '@angular/common/http';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite, PageObject } from '@wfm/test';
import { NzButtonModule, NzTableModule, NzToolTipModule } from 'ng-zorro-antd';
import { of } from 'rxjs';
import { MockTranslationModule } from '../../../../mocks/translation';
import { WorkspaceComponent } from './workspace.component';
import { ReportService } from '../../core/report.service';

describe('WorkspaceComponent', () => {
	let component: WorkspaceComponent;
	let fixture: ComponentFixture<WorkspaceComponent>;

	// let page: Page;
	let reportSvc: ReportService;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [WorkspaceComponent],
			imports: [MockTranslationModule, HttpClientModule, NzTableModule, NzButtonModule, NzToolTipModule],
			providers: [
				ReportService,
				{
					provide: HttpClient,
					useValue: {}
				}
			]
		}).compileComponents();
		reportSvc = TestBed.get(ReportService);
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(WorkspaceComponent);
		component = fixture.componentInstance;
		// page = new Page(fixture);
		fixture.detectChanges();
	});

	it('should create Insights WorkspaceComponent', () => {
		const reportConfigResponse = {
			TokenType: 'Embed',
			AccessToken: 'DummyAccessToken',
			ReportUrl: 'https://app.powerbi.com/reportEmbed?reportId=TestReportId&groupId=TestGroupId',
			ReportId: 'TestReportId'
		};
		spyOn(reportSvc, 'getReportConfig').and.returnValue(of(reportConfigResponse));

		expect(component).toBeTruthy();
	});
});

// class Page extends PageObject {
// 	// get getPeople() {
// 	//     return this.queryAll('[data-test-workspace] [data-test-person]');
// 	// }
// }
