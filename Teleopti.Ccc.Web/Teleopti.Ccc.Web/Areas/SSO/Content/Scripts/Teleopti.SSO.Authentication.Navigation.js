
Teleopti.SSO.Authentication.NavigationConstructor = function () {
	this.GotoChangePassword = function (dataSourceName) {
		window.location.hash = 'changepassword/' + encodeURIComponent(dataSourceName);
	};
	this.GotoMustChangePassword = function (dataSourceName) {
		window.location.hash = 'mustchangepassword/' + encodeURIComponent(dataSourceName);
	};
	this.GotoBusinessUnits = function (returnUrl) {
		window.location = returnUrl;
	};
	this.GotoSignIn = function () {
		window.location.hash = '';
	};
};
Teleopti.SSO.Authentication.Navigation = new Teleopti.SSO.Authentication.NavigationConstructor();
