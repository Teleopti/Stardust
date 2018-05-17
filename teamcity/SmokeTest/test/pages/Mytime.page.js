var LoginPage = require('./Login.page')

var MytimePage = Object.create(LoginPage, {
	title: { get: function () { return 'Teleopti WFM MyTime'; } },
    usernameLabel: { get: function () { return browser.element('.user-name'); } },
    usernameLink: { get: function () { return browser.element('.user-name-link'); } },
    signoutButton: { get: function () { return browser.element('#signout'); } },
	
    open: { value: function() {
        LoginPage.open.call(this, 'Mytime');
    } },
	
	signin: { value: function() {
		LoginPage.signin(this);
		this.usernameLabel.waitForExist(60 * 1000);
    } },
	
	signout: { value: function() {
		console.log('Signing out');
		this.usernameLink.click();
		this.signoutButton.waitForExist(10 * 1000);
		this.signoutButton.click();
    } },
});

module.exports = MytimePage