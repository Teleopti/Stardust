var assert = require('assert');
var ASMWidgetPage = require('./pages/ASMWidget.page');
var IdentityProvidersPage = require('./pages/IdentityProviders.page');

describe('Iframe ground all', function() {
	this.timeout(360 * 1000); // Set global timeout for this test to 6 minutes
	
	
	it('should be able to iframe asmwidget', function () {
		browser.deleteCookie();
		browser.url('http://tritonweb/iframeGroundTest.html');
		browser.waitForExist('iframe');
		var my_frame = browser.element('iframe').value;
		browser.frame(my_frame);
		
		if (ASMWidgetPage.isCurrentPage()) {
			return;
		}
		if (IdentityProvidersPage.isCurrentPage()) {
			IdentityProvidersPage.teleoptiProvider.click();
		}
		ASMWidgetPage.signin();		
    }, 2);
});