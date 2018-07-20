import { browser, by, element, Key } from 'protractor';

export class CommonPage {
	navigateToSearch() {
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

	get searchInput() {
		return element(by.css('[data-test-searchinput]'));
	}

	get searchResult() {
		return element.all(by.css('[data-test-search] [data-test-person]'));
	}

	async selectPerson(index: number | Array<number>) {
		let indexes = Array.isArray(index) ? index : [index];

		for (let i of indexes) {
			await this.searchResult
				.get(i)
				.element(by.tagName('input'))
				.click();
		}
	}
}
