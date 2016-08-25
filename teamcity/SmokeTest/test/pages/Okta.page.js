var Page = require('./Page.page')

var OktaPage = Object.create(Page, {

    username: { get: function () { return browser.element('#user-signin'); } },
    password: { get: function () { return browser.element('#pass-signin'); } },
    signinButton:     { get: function () { return browser.element('#signin-button'); } },
	
	open: { value: function() {
		var webUrl = 'https://teleopti.oktapreview.com/home/teleoptidev811147_teleoptirnd_1/0oa802gjr9ldcyH5M0h7/aln8049ac0l5gX1JO0h7?fromHome=true';
        console.log('navigate to url ' + webUrl);
		browser.url(webUrl)
    } },
	
    clickSignin: { value: function() {
        this.signinButton.click();
    } },
	
	signin: { value: function() {
		console.log('Signing in using Okta');
		this.username.waitForExist(60 * 1000);
		this.username.setValue('demo@teleopti.com');
		this.password.setValue('M8kemew0rk');
        this.signinButton.click();
    } }
});

module.exports = OktaPage