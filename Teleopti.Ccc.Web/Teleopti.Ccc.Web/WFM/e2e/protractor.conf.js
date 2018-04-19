// Protractor configuration file, see link for more information
// https://github.com/angular/protractor/blob/master/lib/config.ts

const { SpecReporter } = require('jasmine-spec-reporter');
const { join } = require('path');

const baseUrl = 'http://localhost:52858';

exports.config = {
	allScriptsTimeout: 11000,
	specs: ['./**/*.e2e-spec.ts'],
	capabilities: {
		browserName: 'chrome'
	},
	directConnect: true,
	seleniumAddress: 'http://localhost:4444/wd/hub',
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

		browser.driver.findElement(by.id('Username-input')).sendKeys('tdemo');
		browser.driver.findElement(by.id('Password-input')).sendKeys('tdemo');
		browser.driver.findElement(by.id('Signin-button')).click();

		return browser.driver.wait(function() {
			return browser.driver.getCurrentUrl().then(function(url) {
				return /TeleoptiWFM\/Web\/WFM\//.test(url);
			});
		}, 10000);
	}
};
