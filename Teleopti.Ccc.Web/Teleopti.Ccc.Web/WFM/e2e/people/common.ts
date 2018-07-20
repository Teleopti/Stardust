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

	async selectPerson(index: number) {
		await this.searchResult
			.get(index)
			.element(by.tagName('input'))
			.click();
	}
}
