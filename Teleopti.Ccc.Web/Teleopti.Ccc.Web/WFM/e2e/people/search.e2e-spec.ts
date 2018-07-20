import { browser, by, element } from 'protractor';

describe('People Search', () => {
	let page: Page;

	beforeEach(() => {
		page = new Page();
	});

	it('should display title', () => {
		page.navigateTo();
		expect(page.title).toContain('People');
	});
});

class Page {
	navigateTo() {
		return browser.get('/TeleoptiWFM/Web/WFM/#/people/search');
	}

	get title() {
		return element(by.css('.view-title h1')).getText();
	}
}
