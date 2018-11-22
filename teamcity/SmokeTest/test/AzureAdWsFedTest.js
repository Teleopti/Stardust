var assert = require('assert');
var MytimePage = require('./pages/Mytime.page');
var IdentityProvidersPage = require('./pages/IdentityProviders.page');
var MicrosoftLoginPage = require('./pages/MicrosoftLogin.page');

describe('Azure AD using WsFed', function() {
	this.timeout(360 * 1000); // Set global timeout for this test to 6 minutes
	this.retries(3);
	beforeEach(function () {
        browser.reload();
    });
	// ktWPzyvL67VpGwl2GcpmkyQaZ0bMgz2-rtC5fXGuFYU is the claim nameidentifier from azure ad for demo@teleopti.com, the correct claim should be used is http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name
    it('should be able to sign in with AD user using WsFed (metadata)', function () {
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
		IdentityProvidersPage.adfs3.waitForExist(10 * 1000);
		IdentityProvidersPage.adfs3.click();
		MicrosoftLoginPage.signin();
		MytimePage.usernameLabel.waitForExist(60 * 1000);
		MytimePage.signout();
    });
});