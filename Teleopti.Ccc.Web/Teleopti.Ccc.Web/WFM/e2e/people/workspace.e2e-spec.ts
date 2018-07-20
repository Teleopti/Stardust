import { by, element } from 'protractor';
import { CommonPage } from './common';

describe('People Workspace', () => {
	let page: Page;

	beforeEach(() => {
		page = new Page();
	});

	it('should be able to manage selection', async () => {
		await page.navigateToSearch();
		await page.search('a');
		await page.selectPerson(0);
		await page.selectPerson(1);
		await page.selectPerson(2);
		expect(page.people.count()).toBe(3);
		await page.removePerson(0);
		expect(page.people.count()).toBe(2);
		await page.clearButton.click();
		expect(page.people.count()).toBe(0);
	});
});

class Page extends CommonPage {
	async removePerson(index: number) {
		await element
			.all(by.css('[data-test-workspace] [data-test-person-remove]'))
			.get(index)
			.click();
	}

	get clearButton() {
		return element(by.css('[data-test-workspace] [data-test-clear-button]'));
	}

	get people() {
		return element.all(by.css('[data-test-workspace] [data-test-person]'));
	}
}
