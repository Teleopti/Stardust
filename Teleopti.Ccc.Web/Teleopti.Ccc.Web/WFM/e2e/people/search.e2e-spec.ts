import { browser, by, element, Key } from 'protractor';

describe('People Search', () => {
	let page: Page;

	beforeEach(() => {
		page = new Page();
	});

	it('should display title', () => {
		page.navigateTo();
		expect(page.title).toContain('People');
	});

	it('should be searchable', async () => {
		await page.navigateTo();
		await page.search('a');
		expect(page.searchResult.count()).toBeGreaterThan(0);
	});
});

class Page {
	navigateTo() {
		return browser.get('/TeleoptiWFM/Web/WFM/#/people/search');
	}

	async search(keyword: string) {
		await this.searchInput.sendKeys(keyword);
		await this.searchInput.sendKeys(Key.ENTER);
		await browser.wait(async () => {
			const resultCount = await this.searchResult.count();
			return resultCount > 0;
		}, 4 * 1000);
	}

	get title() {
		return element(by.css('.view-title h1')).getText();
	}

	get searchInput() {
		return element(by.css('[data-test-searchinput]'));
	}

	get searchResult() {
		return element.all(by.css('[data-test-person]'));
	}
}
