var Page = require('./Page.page')

var IdentityProvidersPage = Object.create(Page, {
	title: { get: function () { return 'Teleopti Authentication Bridge'; } },
	
    azureadProvider: { get: function () { return browser.element('.azuread'); } },
    adfs3: { get: function () { return browser.element('.adfs3'); } },
    googleProvider: { get: function () { return browser.element('.google'); } },
	teleoptiProvider: { get: function () { return browser.element('.teleopti'); } },
    oktaspProvider: { get: function () { return browser.element('.awfos'); } },
	
	
});

module.exports = IdentityProvidersPage