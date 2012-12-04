
Teleopti.Start.Authentication.NavigationConstructor = function () {
	this.GotoChangePassword = function (dataSourceName) {
		window.location.hash = 'changepassword/' + dataSourceName;
	};
	this.GotoMustChangePassword = function (dataSourceName) {
		window.location.hash = 'mustchangepassword/' + dataSourceName;
	};
	this.GotoBusinessUnits = function (authenticationType, dataSourceName) {
		window.location.hash = 'businessunit/' + authenticationType + '/' + dataSourceName;
	};
	this.GotoSignIn = function (jqXHR, textStatus, errorThrown) {
		window.location.hash = '';
	};
	this.GotoMenu = function () {
		window.location.hash = 'menu';
	};
};
Teleopti.Start.Authentication.Navigation = new Teleopti.Start.Authentication.NavigationConstructor();
