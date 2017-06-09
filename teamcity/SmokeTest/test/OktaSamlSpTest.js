var OktaPage = require('./pages/Okta.page');
var MytimePage = require('./pages/Mytime.page');
var IdentityProvidersPage = require('./pages/IdentityProviders.page');

describe('Saml sp initiated SSO', function() {
		it('should be able to sign in with Okta user sp initiated', function () {
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
		IdentityProvidersPage.oktaspProvider.waitForExist(10 * 1000);
		IdentityProvidersPage.oktaspProvider.click();

		OktaPage.signin();
		MytimePage.usernameLabel.waitForExist(60 * 1000);
		MytimePage.signout();
    }, 2);
});