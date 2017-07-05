var LoginPage = require('./Login.page')

var HealthCheckPage = Object.create(LoginPage, {
	title: { get: function () { return 'Teleopti WFM Health Check Tool'; } },

    serviceList: { get: function () { return browser.element('.services li span'); } },
    stardustStatus: { get: function () { return browser.element('.stardust'); } },
	
    open: { value: function() {
        LoginPage.open.call(this, 'HealthCheck');
    } },
	
	signin: { value: function() {
		LoginPage.signin(this);
		this.serviceList.waitForExist(60 * 1000);
		this.stardustStatus.waitForExist(60 * 1000);
    } }
});

module.exports = HealthCheckPage