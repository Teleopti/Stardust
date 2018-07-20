import { browser, by, element, ExpectedConditions } from 'protractor';
import { CommonPage } from './common';

describe('People Search', () => {
	let page: Page;

	beforeEach(async () => {
		page = new Page();
		await page.navigateToSearch();
		await page.search('a');
		await page.selectPerson([0, 1, 2]);
	});

	it('should be able to grant roles', async () => {
		await page.grantButton.click();
		expect(page.availableRoles.count()).toBeGreaterThan(2);
		const roleToGrant = await page.availableRoles.get(0).getText();

		await page.availableRoles.get(0).click();
		await page.saveButton.click();

		await browser.wait(ExpectedConditions.visibilityOf(page.grantButton), 2000);
		await page.grantButton.click();

		const roleElements = await page.currentGrantRoles.all(by.cssContainingText('*', roleToGrant));
		expect(roleElements.length).toBe(1);
	});

	it('should be able to revoke roles', async () => {
		await page.revokeButton.click();
		expect(page.currentRevokeRoles.count()).toBeGreaterThan(0);
		const roleToRevoke = await page.currentRevokeRoles.get(0).getText();

		await page.currentRevokeRoles.get(0).click();
		await page.saveButton.click();

		await browser.wait(ExpectedConditions.visibilityOf(page.revokeButton), 2000);
		await page.revokeButton.click();

		const roleElements = await page.currentRevokeRoles.all(by.cssContainingText('*', roleToRevoke));
		expect(roleElements.length).toBe(0);
	});
});

class Page extends CommonPage {
	get currentGrantRoles() {
		return element.all(by.css('[data-test-grant-current] [data-test-chip]'));
	}
	get currentRevokeRoles() {
		return element.all(by.css('[data-test-revoke-current] [data-test-chip]'));
	}

	get availableRoles() {
		return element.all(by.css('[data-test-grant-available] [data-test-chip]'));
	}

	get grantButton() {
		return element(by.css('[data-test-grant-button]'));
	}
	get revokeButton() {
		return element(by.css('[data-test-revoke-button]'));
	}

	get saveButton() {
		return element(by.css('[data-test-save]'));
	}
}
