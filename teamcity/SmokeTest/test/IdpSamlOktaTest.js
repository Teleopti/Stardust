var OktaPage = require('./pages/Okta.page');
var MytimePage = require('./pages/Mytime.page');

describe('IDP initiated SSO', function() {
	
    it('should be able to sign in with Okta user', function () {
		OktaPage.open();
		OktaPage.signin();
		MytimePage.usernameLabel.waitForExist(60 * 1000);
    }, 2);
});