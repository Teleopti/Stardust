var assert = require('assert');
var HealthCheckPage = require('./pages/HealthCheck.page');

describe('health check', function() {

	before(function() {
		// Given that we are signed in and showing the HealthCheck page
		HealthCheckPage.open();
		HealthCheckPage.signin();
    });
	
    it('starting healthcheck should show results', function () {
		HealthCheckPage.clickStartCheck();
		HealthCheckPage.busResults.waitForExist(60 * 1000);
    });
});