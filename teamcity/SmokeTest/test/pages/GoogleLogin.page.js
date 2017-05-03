var Page = require('./Page.page')

var GoogleLoginPage = Object.create(Page, {

    username: { get: function () { return browser.element('input[type=email]'); } },
    next: { get: function () { return browser.element('#identifierNext > content'); } },
    password: { get: function () { return browser.element('input[type=password]'); } },
    signinButton: { get: function () { return browser.element('#passwordNext'); } },
	
    clickSignin: { value: function() {
        this.signinButton.click();
    } },
	
	signin: { value: function() {
		console.log('Signing in using Google');
		if (!browser.isExisting('input[type=password]')) {
			this.username.waitForVisible(60 * 1000);
			console.log('Username input exists, entering username');
			this.username.setValue('demo.teleopti@gmail.com');
			console.log('Clicking next button');
			this.next.click();
		}		
		
		this.password.waitForVisible(60 * 1000);
		console.log('Password input exists, entering password');
		this.password.setValue('m8kemew0rk');
		console.log('Clicking next button');
        this.signinButton.click();
    } }
});

module.exports = GoogleLoginPage