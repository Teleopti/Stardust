var LoginPage = require('./Login.page')

var ASMWidgetPage = Object.create(LoginPage, {
	title: { get: function () { return 'Teleopti WFM ASMWidget'; } },

    widget: { get: function () { return browser.element('.cisco-widget'); } },
	
    open: { value: function() {
        LoginPage.open.call(this, 'Mytime/ASMWidget');
    } },
	
	signin: { value: function() {
		LoginPage.signin(this);
		this.widget.waitForExist(60 * 1000);
    } },
});

module.exports = ASMWidgetPage