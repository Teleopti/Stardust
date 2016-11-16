var assert = require('assert');
var MytimePage = require('./pages/Mytime.page');
var IdentityProvidersPage = require('./pages/IdentityProviders.page');
var MicrosoftLoginPage = require('./pages/MicrosoftLogin.page');

describe('Azure AD', function() {
	this.timeout(360 * 1000); // Set global timeout for this test to 6 minutes
	
	// ignore this one for now, need to change APP ID URI in Teleopti Azure AD first, and the claim input in claim policy need to change to name instead of nameidentifier
    xit('should be able to sign in with AD user using WsFed', function () {
		// Given that we are showing the IdentityProviders selection page
		MytimePage.open();
		if (MytimePage.isCurrentPage()) {
			MytimePage.signout();
		}
		if (!IdentityProvidersPage.isCurrentPage()) {
			MytimePage.signin();
			MytimePage.signout();
		}
		IdentityProvidersPage.IdentityServerProvider.waitForExist(10 * 1000);
		IdentityProvidersPage.IdentityServerProvider.click();
		MicrosoftLoginPage.signin();
		MytimePage.usernameLabel.waitForExist(60 * 1000);
    }, 2);
	
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
    }, 2);
});