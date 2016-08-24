var Page = require('./Page.page')

var IdentityProvidersPage = Object.create(Page, {
    azureadProvider: { get: function () { return browser.element('.azuread'); } }
});

module.exports = IdentityProvidersPage