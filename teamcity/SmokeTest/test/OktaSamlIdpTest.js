var OktaPage = require('./pages/Okta.page');
var MytimePage = require('./pages/Mytime.page');
var IdentityProvidersPage = require('./pages/IdentityProviders.page');

describe('Saml idp initiated SSO', function () {
	this.retries(3);
	beforeEach(function () {
		browser.reload();
	});
	it('should be able to sign in with Okta user idp initiated', function () {
		OktaPage.open();
		OktaPage.signin();
		MytimePage.usernameLabel.waitForExist(60 * 1000);
		MytimePage.signout();
	});
});