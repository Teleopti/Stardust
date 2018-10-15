import { HttpClientModule } from '@angular/common/http';
import { DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { NzButtonModule, NzTableModule, NzToolTipModule } from 'ng-zorro-antd';
import { configureTestSuite } from '../../../../configure-test-suit';
import { MockTranslationModule } from '../../../../mocks/translation';
import { WorkspaceComponent } from './workspace.component';
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
            providers: []
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

class Page {
    // get getPeople() {
    //     return this.queryAll('[data-test-workspace] [data-test-person]');
    // }

    fixture: ComponentFixture<WorkspaceComponent>;

    constructor(fixture: ComponentFixture<WorkspaceComponent>) {
        this.fixture = fixture;
    }

    private queryAll(selector: string): DebugElement[] {
        return this.fixture.debugElement.queryAll(By.css(selector));
    }
}
