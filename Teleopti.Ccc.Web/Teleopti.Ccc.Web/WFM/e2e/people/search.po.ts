import { browser, by, element } from 'protractor';

export class PeopleSearch {
	navigateTo() {
		return browser.get('/TeleoptiWFM/Web/WFM/#/people/search');
	}

	getTitle() {
		return element(by.css('.view-title h1')).getText();
	}
}
