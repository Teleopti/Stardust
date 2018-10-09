import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NzFormModule } from '../../../../../node_modules/ng-zorro-antd';
import { configureTestSuite } from '../../../../configure-test-suit';
import { ExternalApplicationService, NavigationService } from '../../services';
import { AddAppPageComponent } from './add-app-page.component';

describe('AddAppPageComponent', () => {
	let component: AddAppPageComponent;
	let fixture: ComponentFixture<AddAppPageComponent>;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [AddAppPageComponent],
			imports: [ReactiveFormsModule, NzFormModule],
			providers: [
				{ provide: ExternalApplicationService, useValue: {} },
				{ provide: NavigationService, useValue: {} }
			]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(AddAppPageComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
