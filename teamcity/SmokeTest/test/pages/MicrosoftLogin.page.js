var Page = require('./Page.page')

var MicrosoftLoginPage = Object.create(Page, {

    username: { get: function () { return browser.element('#cred_userid_inputtext'); } },
    password: { get: function () { return browser.element('#cred_password_inputtext'); } },
    signinButton:     { get: function () { return browser.element('#submitButton'); } },
	
    clickSignin: { value: function() {
        this.signinButton.click();
    } },
	
	signin: { value: function() {
		console.log('Signing in using Teleopti Azure AD');
		this.username.waitForExist(60 * 1000);
		this.username.setValue('demo@teleopti.com');
		this.password.setValue('teleoptidemo');
        this.signinButton.click();
    } }
});

module.exports = MicrosoftLoginPage