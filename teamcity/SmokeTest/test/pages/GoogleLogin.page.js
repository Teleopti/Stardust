var Page = require('./Page.page')

var GoogleLoginPage = Object.create(Page, {

    username: { get: function () { return browser.element('#Email'); } },
    next: { get: function () { return browser.element('#next'); } },
    password: { get: function () { return browser.element('#Passwd'); } },
    signinButton:     { get: function () { return browser.element('#signIn'); } },
	
    clickSignin: { value: function() {
        this.signinButton.click();
    } },
	
	signin: { value: function() {
		console.log('Signing in using Google');
		this.username.waitForVisible(60 * 1000);
		this.username.setValue('demo.teleopti@gmail.com');
		this.next.click();
		this.password.waitForVisible(60 * 1000);
		this.password.setValue('m8kemew0rk');
        this.signinButton.click();
    } }
});

module.exports = GoogleLoginPage