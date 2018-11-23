import { IState } from 'angular-ui-router';
import { Areas, internalNameOf, isPermittedArea, nameOfArea, urlOfState } from './main.initializer';

describe('main initializer tests', () => {
	let areas: Areas;

	beforeEach(() => {
		areas = {
			available: [{ InternalName: 'internal1', Name: 'name1' }, { InternalName: 'internal2', Name: 'name2' }],
			permitted: [{ InternalName: 'justForMe' }],
			alwaysPermitted: ['public']
		};
	});

	it('returns name of area', () => {
		const name = nameOfArea(areas, 'internal1');

		expect(name).toEqual('name1');
	});

	it('returns url of state', () => {
		const state: IState = { url: '/myUrl' };

		expect(urlOfState(state)).toEqual('myUrl');
	});

	it('returns internal name of state', () => {
		const state: IState = { name: 'name1' };

		expect(internalNameOf(state)).toEqual('name1');
	});

	it('should permit always permitted areas', () => {
		expect(isPermittedArea(areas, 'public')).toBe(true);
		expect(isPermittedArea(areas, 'secret')).toBe(false);
	});

	it('should permit otherwise permitted areas', () => {
		expect(isPermittedArea(areas, 'justForMe')).toBe(true);
		expect(isPermittedArea(areas, 'justForMe', '/justforme')).toBe(true);
		expect(isPermittedArea(areas, '', 'just')).toBe(true);
		expect(isPermittedArea(areas, '', '--justForMe--')).toBe(true);
		expect(isPermittedArea(areas, '', '/ohnopls')).toBe(false);
	});
});
