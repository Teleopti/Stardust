// Protractor configuration file, see link for more information
// https://github.com/angular/protractor/blob/master/lib/config.ts

const { SpecReporter } = require('jasmine-spec-reporter');
const { join } = require('path');

const baseUrl = 'http://localhost:52858';

let config = {
	allScriptsTimeout: 11000,
	specs: ['./**/*.e2e-spec.ts'],
	capabilities: {
		browserName: 'chrome'
	},
	directConnect: true,
	// seleniumAddress: 'http://localhost:4444/wd/hub',
	seleniumServerJar: join(
		'..',
		'node_modules',
		'protractor',
		'node_modules',
		'webdriver-manager',
		'selenium',
		'selenium-server-standalone-3.13.0.jar'
	),
	seleniumPort: 4444,
	baseUrl,
	framework: 'jasmine',
	jasmineNodeOpts: {
		showColors: true,
		defaultTimeoutInterval: 30000,
		print: function() {}
	},
	onPrepare() {
		require('ts-node').register({
			project: join(__dirname, 'tsconfig.e2e.json')
		});
		jasmine.getEnv().addReporter(new SpecReporter({ spec: { displayStacktrace: true } }));

		browser.driver.get(baseUrl + '/TeleoptiWFM/AuthenticationBridge/authenticate?whr=urn:Teleopti');

		const page = new LoginPageObject();

		page.usernameInput.sendKeys('tdemo');
		page.passwordInput.sendKeys('tdemo');
		page.signInButton.click();

		let hasSelectedBusinessUnit = false;
		return browser.driver.wait(async function() {
			const currentUrl = await browser.driver.getCurrentUrl();
			const asksForBusinessunit = /businessunit/.test(currentUrl);
			if (asksForBusinessunit && !hasSelectedBusinessUnit) {
				console.info('Asking for businessunit');
				console.info('Selecting second businessunit');
				page.teleoptiBusinessUnit.click();
				hasSelectedBusinessUnit = true;
			}

			return /TeleoptiWFM\/Web\/WFM\//.test(currentUrl);
		}, 10000);
	}
};

class LoginPageObject {
	get usernameInput() {
		return browser.driver.findElement(by.id('Username-input'));
	}
	get passwordInput() {
		return browser.driver.findElement(by.id('Password-input'));
	}
	get signInButton() {
		return browser.driver.findElement(by.id('Signin-button'));
	}
	get teleoptiBusinessUnit() {
		return browser.driver.findElement(by.css('#BusinessUnits li:nth-child(2) a'));
	}
}

module.exports = { config };
