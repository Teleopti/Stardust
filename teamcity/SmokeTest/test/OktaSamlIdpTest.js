var OktaPage = require('./pages/Okta.page');
var MytimePage = require('./pages/Mytime.page');
var IdentityProvidersPage = require('./pages/IdentityProviders.page');

describe('Saml idp initiated SSO', function() {
    it('should be able to sign in with Okta user idp initiated', function () {
		browser.deleteCookie();
		OktaPage.open();
		OktaPage.signin();
		MytimePage.usernameLabel.waitForExist(60 * 1000);
		MytimePage.signout();
    }, 2);
});