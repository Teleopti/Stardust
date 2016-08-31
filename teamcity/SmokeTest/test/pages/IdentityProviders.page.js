var Page = require('./Page.page')

var IdentityProvidersPage = Object.create(Page, {
	title: { get: function () { return 'Teleopti Authentication Bridge'; } },
	
    azureadProvider: { get: function () { return browser.element('.azuread'); } },
	teleoptiProvider: { get: function () { return browser.element('.teleopti'); } },
	
	
});

module.exports = IdentityProvidersPage