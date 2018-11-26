var assert = require('assert');
var MytimePage = require('./pages/Mytime.page');
var IdentityProvidersPage = require('./pages/IdentityProviders.page');
var MicrosoftLoginPage = require('./pages/MicrosoftLogin.page');

describe('Azure AD using OAuth', function() {
	this.timeout(360 * 1000); // Set global timeout for this test to 6 minutes
	this.retries(3);

	beforeEach(function () {
        browser.reload();
    });
	
	it('should be able to sign in with AD user using OAuth', function () {
		// Given that we are showing the IdentityProviders selection page
		MytimePage.open();
		if (MytimePage.isCurrentPage()) {
			MytimePage.signout();
		}
		if (!IdentityProvidersPage.isCurrentPage()) {
			MytimePage.signin();
			MytimePage.signout();
		}
		IdentityProvidersPage.azureadProvider.waitForExist(10 * 1000);
		IdentityProvidersPage.azureadProvider.click();
		MicrosoftLoginPage.signin();
		MytimePage.usernameLabel.waitForExist(60 * 1000);
		MytimePage.signout();
    });
});