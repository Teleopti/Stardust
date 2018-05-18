var assert = require('assert');
var HealthCheckPage = require('./pages/HealthCheck.page');
var IdentityProvidersPage = require('./pages/IdentityProviders.page');

describe('health check', function() {
	
    it('starting healthcheck should show services started', function () {
		browser.deleteCookie();
		// Given that we are signed in and showing the HealthCheck page
		HealthCheckPage.open();
		if (HealthCheckPage.isCurrentPage()) {
			return; // We are somehow already signed in, move on
		}
		if (IdentityProvidersPage.isCurrentPage()) {
			IdentityProvidersPage.teleoptiProvider.click();
		}
		
		HealthCheckPage.signin();
    }, 2);
});