var assert = require('assert');
var ASMWidgetPage = require('./pages/ASMWidget.page');
var IdentityProvidersPage = require('./pages/IdentityProviders.page');

describe('Iframe ground all', function() {
	this.timeout(360 * 1000); // Set global timeout for this test to 6 minutes
	this.retries(3);
	beforeEach(function () {
        browser.reload();
    });
	it('should be able to iframe asmwidget', function () {
		browser.url(process.env.UrlToTest);
		browser.deleteCookie();
		browser.url('http://tcbuildmonitor:8000/iframeGroundTest.html');
		browser.waitForExist('iframe');
		var my_frame = browser.element('iframe').value;
		browser.frame(my_frame);
		ASMWidgetPage.signin();		
    });
});