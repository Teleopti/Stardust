import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PageContainerComponent } from './page-container.component';
import { ApiAccessModule } from '../../../api-access.module';

describe('PageContainerComponent', () => {
	let component: PageContainerComponent;
	let fixture: ComponentFixture<PageContainerComponent>;

	beforeEach(
		async(() => {
			TestBed.configureTestingModule({
				imports: [ApiAccessModule]
			}).compileComponents();
		})
	);

	beforeEach(() => {
		fixture = TestBed.createComponent(PageContainerComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
