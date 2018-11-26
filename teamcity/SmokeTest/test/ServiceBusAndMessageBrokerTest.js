var assert = require('assert');
var HealthCheckPage = require('./pages/HealthCheck.page');
var IdentityProvidersPage = require('./pages/IdentityProviders.page');

describe('health check', function() {
	this.retries(3);

	beforeEach(function () {
        browser.reload();
    });
	
    it('starting healthcheck should show services started', function () {
		// Given that we are signed in and showing the HealthCheck page
		HealthCheckPage.open();
		if (HealthCheckPage.isCurrentPage()) {
			return; // We are somehow already signed in, move on
		}
		if (IdentityProvidersPage.isCurrentPage()) {
			IdentityProvidersPage.teleoptiProvider.click();
		}
		
		HealthCheckPage.signin();
    });
});