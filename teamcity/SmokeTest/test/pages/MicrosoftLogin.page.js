var Page = require('./Page.page')

var MicrosoftLoginPage = Object.create(Page, {

    username: { get: function () { return browser.element("input[type='email']"); } },
    nextbutton: { get: function () { return browser.element("input[type='submit']"); } },
    password: { get: function () { return browser.element('#cred_password_inputtext'); } },
	password2: { get: function () { return browser.element('#passwordInput'); } },
    signinButton:     { get: function () { return browser.element('#submitButton'); } },
    keepSignedInButton:     { get: function () { return browser.element('#idSIButton9'); } },
	
    clickSignin: { value: function() {
        this.signinButton.click();
    } },
	
	signin: { value: function() {
		console.log('Signing in using Teleopti Azure AD');
		this.username.waitForVisible(60 * 1000);
		this.username.setValue('demo@teleopti.com');
        this.nextbutton.click();
		this.password2.waitForVisible(60 * 1000);
		this.password2.setValue('teleoptidemo');
        this.signinButton.click();
        this.keepSignedInButton.waitForVisible(60 * 1000);
        this.keepSignedInButton.click();
    } }
});

module.exports = MicrosoftLoginPage