import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite, PageObject } from '@wfm/test';
import { IStateService } from 'angular-ui-router';
import { ReplaySubject } from 'rxjs';
import { MediaQueryService } from 'src/app/browser/services/media-query.service';
import { Area, AreaService } from '../../shared/area.service';
import { ToggleMenuService } from '../../shared/toggle-menu.service';
import { SideMenuComponent } from './side-menu.component';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

const areaOne: Area = { InternalName: 'people' };
const areaTwo: Area = { InternalName: 'permissions' };

class MockToggleMenuService implements Partial<ToggleMenuService> {
	public get showMenu$() {
		return new ReplaySubject<boolean>(1);
	}
	isMobileView() {
		return false;
	}
}

class MockMediaQueryService implements Partial<MediaQueryService> {
	public get isMobileSize$() {
		return new ReplaySubject<boolean>(1);
	}
}

const mockStateService: Partial<IStateService> = {
	current: {
		name: areaOne.InternalName
	},
	href() {
		return '';
	},
	get() {
		return this.current.name;
	}
};

describe('SideMenuComponent', () => {
	let component: SideMenuComponent;
	let fixture: ComponentFixture<SideMenuComponent>;
	let page: Page;
	let httpMock: HttpTestingController;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [SideMenuComponent],
			imports: [HttpClientTestingModule],
			providers: [
				{
					provide: '$state',
					useValue: mockStateService
				},
				{ provide: ToggleMenuService, useClass: MockToggleMenuService },
				{ provide: MediaQueryService, useClass: MockMediaQueryService },
				AreaService
			]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(SideMenuComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
		httpMock = TestBed.get(HttpTestingController);
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should display areas', () => {
		const areas = httpMock.match('../api/Global/Application/WfmAreasWithPermission')[0];
		areas.flush([areaOne, areaTwo]);

		fixture.detectChanges();
		expect(page.menuItems.length).toEqual(2);
	});

	it('should hightlight active area', () => {
		const areas = httpMock.match('../api/Global/Application/WfmAreasWithPermission')[0];
		areas.flush([areaOne, areaTwo]);

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
