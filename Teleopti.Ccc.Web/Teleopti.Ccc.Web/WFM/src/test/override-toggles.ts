import { Provider } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Toggles, TogglesService } from '../app/core/services';

class MockTogglesService implements Partial<TogglesService> {
	_toggles$: Observable<Toggles>;
	get toggles$() {
		return this._toggles$;
	}

	constructor(toggles: Toggles) {
		this._toggles$ = of(toggles);
	}
}

/**
 * Use this to override the toggles provided to a specific test
 */
export function overrideToggles(togglesService: TogglesService, toggles: Toggles) {
	return spyOnProperty(togglesService, 'toggles$', 'get').and.returnValue(of(toggles));
}

/**
 * Use this to tell a test module which toggles are activated
 * Add this as a Provider
 * @param toggles the toggles to mock
 */
export function MockToggles(toggles: Toggles = {}): Provider {
	return {
		provide: TogglesService,
		useValue: new MockTogglesService(toggles)
	};
}
