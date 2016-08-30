var assert = require('assert');
var MytimePage = require('./pages/Mytime.page');
var IdentityProvidersPage = require('./pages/IdentityProviders.page');
var MicrosoftLoginPage = require('./pages/MicrosoftLogin.page');

describe('Azure AD', function() {
	this.timeout(360 * 1000); // Set global timeout for this test to 5 minutes
	
	before(function() {
		// Given that we are showing the IdentityProviders selection page
		MytimePage.open();
		MytimePage.signin();
		MytimePage.signout();
    }, 2);
	
    it('should be able to sign in with AD user', function () {
		IdentityProvidersPage.azureadProvider.waitForExist(10 * 1000);
		IdentityProvidersPage.azureadProvider.click();
		MicrosoftLoginPage.signin();
		MytimePage.usernameLabel.waitForExist(60 * 1000);
    }, 2);
});