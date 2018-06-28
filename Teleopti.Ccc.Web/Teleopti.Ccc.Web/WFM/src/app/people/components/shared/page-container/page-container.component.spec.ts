import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PageContainerComponent } from './page-container.component';
import { PeopleModule } from '../../../people.module';
import { WorkspaceComponent } from '../..';
import { MockTranslationModule } from '../../../../../mocks/translation';
import { WorkspaceService, SearchService, fakeBackendProvider } from '../../../services';
import { HttpClientModule } from '@angular/common/http';

describe('PageContainerComponent', () => {
	let component: PageContainerComponent;
	let fixture: ComponentFixture<PageContainerComponent>;

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [PageContainerComponent, WorkspaceComponent],
			imports: [MockTranslationModule, HttpClientModule],
			providers: [WorkspaceService, SearchService, fakeBackendProvider]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(PageContainerComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
