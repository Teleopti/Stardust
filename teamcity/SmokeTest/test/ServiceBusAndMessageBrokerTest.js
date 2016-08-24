var assert = require('assert');
var HealthCheckPage = require('./pages/HealthCheck.page');

describe('service bus and message broker should work', function() {

	before(function() {
		HealthCheckPage.open();
		HealthCheckPage.signin();
    });
	
    it('should work', function () {
		HealthCheckPage.clickStartCheck();
		HealthCheckPage.busResults.waitForExist(60 * 1000);
    });
});