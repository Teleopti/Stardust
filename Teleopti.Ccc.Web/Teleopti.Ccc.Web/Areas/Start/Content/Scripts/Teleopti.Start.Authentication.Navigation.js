
Teleopti.Start.Authentication.NavigationConstructor = function () {
	this.GotoChangePassword = function (dataSourceName) {
		window.location.hash = 'changepassword/' + encodeURIComponent(dataSourceName);
	};
	this.GotoMustChangePassword = function (dataSourceName) {
		window.location.hash = 'mustchangepassword/' + encodeURIComponent(dataSourceName);
	};
	this.GotoBusinessUnits = function (authenticationType, dataSourceName) {
		window.location.hash = 'businessunit/' + authenticationType + '/' + encodeURIComponent(dataSourceName);
	};
	this.GotoSignIn = function () {
		window.location.hash = '';
	};
	this.GotoMenu = function () {
		window.location.hash = 'menu';
	};
};
Teleopti.Start.Authentication.Navigation = new Teleopti.Start.Authentication.NavigationConstructor();
