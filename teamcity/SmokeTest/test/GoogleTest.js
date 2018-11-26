var assert = require('assert');
var MytimePage = require('./pages/Mytime.page');
var IdentityProvidersPage = require('./pages/IdentityProviders.page');
var GoogleLoginPage = require('./pages/GoogleLogin.page');

describe('Google', function() {
	this.timeout(360 * 1000); // Set global timeout for this test to 6 minutes
	this.retries(3);
	beforeEach(function () {
        browser.reload();
    });
    it('should be able to sign in with Google user', function () {
		browser.deleteCookie();
		// Given that we are showing the IdentityProviders selection page
		MytimePage.open();
		if (MytimePage.isCurrentPage()) {
			MytimePage.signout();
		}
		if (!IdentityProvidersPage.isCurrentPage()) {
			MytimePage.signin();
			MytimePage.signout();
		}
		IdentityProvidersPage.googleProvider.waitForExist(10 * 1000);
		IdentityProvidersPage.googleProvider.click();
		GoogleLoginPage.signin();
		MytimePage.usernameLabel.waitForExist(60 * 1000);
		MytimePage.signout();
    });
});