var OktaPage = require('./pages/Okta.page');
var MytimePage = require('./pages/Mytime.page');

describe('okta signin should work', function() {
	
    it('should work', function () {
		OktaPage.open();
		OktaPage.signin();
		MytimePage.usernameLabel.waitForExist(60 * 1000);
    });
});