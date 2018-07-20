import { by, element } from 'protractor';
import { CommonPage } from './common';

describe('People Search', () => {
	let page: Page;

	beforeEach(() => {
		page = new Page();
	});

	it('should display title', () => {
		page.navigateToSearch();
		expect(page.title).toContain('People');
	});

	it('should be searchable', async () => {
		await page.navigateToSearch();
		await page.search('a');
		expect(page.searchResult.count()).toBeGreaterThan(0);
	});
});

class Page extends CommonPage {
	get title() {
		return element(by.css('.view-title h1')).getText();
	}
}
