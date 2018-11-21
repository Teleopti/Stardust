import { of } from 'rxjs';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ToggleMenuService } from './toggle-menu.service';

const mockWindow: Partial<Window> = {
	innerWidth: 1200
};

const mockMobileWindow: Partial<Window> = {
	innerWidth: 300
};

describe('ToggleMenuService', () => {
	let toggleMenuService: ToggleMenuService;

	beforeEach(() => {
		toggleMenuService = new ToggleMenuService(mockWindow as Window);
	});

	it('should be visible', async(() => {
		toggleMenuService.setMenuVisible(true);

		toggleMenuService.showMenu$.subscribe(isVisible => {
			expect(isVisible).toEqual(true);
		});
	}));

	it('should not be visible', async(() => {
		toggleMenuService.setMenuVisible(false);

		toggleMenuService.showMenu$.subscribe(isVisible => {
			expect(isVisible).toEqual(false);
		});
	}));

	it('should toggle', async(() => {
		toggleMenuService.setMenuVisible(true);

		toggleMenuService.toggle();

		toggleMenuService.showMenu$.subscribe(isVisible => {
			expect(isVisible).toEqual(false);
		});
	}));

	it('should hide on mobile', async(() => {
		toggleMenuService = new ToggleMenuService(mockMobileWindow as Window);

		toggleMenuService.showMenu$.subscribe(isVisible => {
			expect(isVisible).toEqual(false);
		});
	}));
});
