import { browser, by, element } from 'protractor';

/**
 * This is a PageObject file for abstracting view interaction
 * https://martinfowler.com/bliki/PageObject.html
 */
export class PeopleSearch {
	navigateTo() {
		return browser.get('/TeleoptiWFM/Web/WFM/#/people/search');
	}

	getTitle() {
		return element(by.css('.view-title h1')).getText();
	}
}
