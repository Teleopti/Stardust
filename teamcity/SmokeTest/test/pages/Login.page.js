var Page = require('./Page.page')

var LoginPage = Object.create(Page, {
	title: { get: function () { return 'Teleopti WFM'; } },
	
    username: { get: function () { return browser.element('#Username-input'); } },
    password: { get: function () { return browser.element('#Password-input'); } },
    signinButton:     { get: function () { return browser.element('#Signin-button'); } },
	
    clickSignin: { value: function() {
        this.signinButton.click();
    } },
	
	signin: { value: function() {
		console.log('Signing in with demo/demo');
		this.username.waitForExist(60 * 1000);
		this.username.setValue('demo');
		this.password.setValue('demo');
        this.signinButton.click();
    } }
});

module.exports = LoginPage