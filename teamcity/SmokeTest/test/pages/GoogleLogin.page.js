var Page = require('./Page.page')

var GoogleLoginPage = Object.create(Page, {

    username: { get: function () { return browser.element('input[type=email]'); } },
    next: { get: function () { return browser.element('#identifierNext'); } },
    password: { get: function () { return browser.element('input[type=password]'); } },
    signinButton: { get: function () { return browser.element('#passwordNext'); } },
	
	signin: { value: function() {
		console.log('Signing in using Google');
		console.log('Waiting for username input');
		this.username.waitForVisible(60 * 1000);
		console.log('Username input exists, entering username');
		this.username.setValue('demo.teleopti@gmail.com');
		console.log('Clicking next button');
		this.next.click();

		browser.pause(1000);
		console.log('Waiting for password input');
		this.password.waitForVisible(60 * 1000);
		console.log('Password input exists, entering password');
		this.password.setValue('m8kemew0rk');
		console.log('Clicking next button');
        this.signinButton.click();
    } }
});

module.exports = GoogleLoginPage