var assert = require('assert');
var MytimePage = require('./pages/Mytime.page');
var IdentityProvidersPage = require('./pages/IdentityProviders.page');
var MicrosoftLoginPage = require('./pages/MicrosoftLogin.page');

describe('azure ad signin should work', function() {
	
	before(function() {
		MytimePage.open();
		MytimePage.signin();
    });
	
    it('should work', function () {
		MytimePage.signout();
		IdentityProvidersPage.azureadProvider.waitForExist(10 * 1000);
		IdentityProvidersPage.azureadProvider.click();
		MicrosoftLoginPage.signin();
		MytimePage.usernameLabel.waitForExist(60 * 1000);
		
		console.log('navigate to health check');
		browser.url(UrlToTest + '/HealthCheck');
    });
});