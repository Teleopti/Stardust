import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite, PageObject } from '@wfm/test';
import { IStateService } from 'angular-ui-router';
import { Observable, of, ReplaySubject } from 'rxjs';
import { Area, AreaService } from '../../shared/area.service';
import { SideMenuComponent } from './side-menu.component';
import { ToggleMenuService } from '../../shared/toggle-menu.service';

const areaOne: Area = { InternalName: 'people' };
const areaTwo: Area = { InternalName: 'permissions' };

class MockAreaService implements Partial<AreaService> {
	getAreas(): Observable<Area[]> {
		return of([areaOne, areaTwo]);
	}
}

class MockToggleMenuService implements Partial<ToggleMenuService> {
	public get showMenu$() {
		return new ReplaySubject<boolean>(1);
	}
	isMobileView() {
		return false;
	}
}

const mockStateService: Partial<IStateService> = {
	current: {
		name: areaOne.InternalName
	}
};

describe('SideMenuComponent', () => {
	let component: SideMenuComponent;
	let fixture: ComponentFixture<SideMenuComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [SideMenuComponent],
			imports: [],
			providers: [
				{
					provide: '$state',
					useValue: mockStateService
				},
				{ provide: AreaService, useClass: MockAreaService },
				{ provide: ToggleMenuService, useClass: MockToggleMenuService }
			]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(SideMenuComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should display areas', () => {
		fixture.detectChanges();
		expect(page.menuItems.length).toEqual(2);
	});

	it('should hightlight active area', () => {
		fixture.detectChanges();
		expect(page.menuItems[0].classes['main-menu-link-active']).toBeTruthy();
	});

	it('should be able to be hidden', async(() => {
		const service = TestBed.get(ToggleMenuService);
		service.showMenu$.next(false);
		fixture.detectChanges();
		const hidden = page.menuWrapper.nativeElement.hasAttribute('hidden');
		expect(hidden).toBeTruthy();
	}));
});

class Page extends PageObject {
	get menuItems() {
		return this.queryAll('[data-test-menu-item]');
	}

	get menuWrapper() {
		return this.queryAll('[data-test-main-menu-wrapper]')[0];
	}
}
