import { PeopleSearch } from './search.po';

describe('People Search', () => {
	let page: PeopleSearch;

	beforeEach(() => {
		page = new PeopleSearch();
	});

	it('should display title', () => {
		page.navigateTo();
		expect(page.getTitle()).toEqual('People');
	});
});
