var LoginPage = require('./Login.page')

var HealthCheckPage = Object.create(LoginPage, {

    serviceList: { get: function () { return browser.element('.services li span'); } },
    stardustStatus: { get: function () { return browser.element('.stardust'); } },
    startCheck: { get: function () { return browser.element('#Start-Check'); } },
    busResults: { get: function () { return browser.element('#Bus-Results'); } },
	
    open: { value: function() {
        LoginPage.open.call(this, 'HealthCheck');
    } },
	
	signin: { value: function() {
		LoginPage.signin(this);
		this.serviceList.waitForExist(60 * 1000);
		this.stardustStatus.waitForExist(60 * 1000);
    } },
	
	clickStartCheck: { value: function() {
		console.log('check service bus and broker');
        this.startCheck.click();
    } },
});

module.exports = HealthCheckPage