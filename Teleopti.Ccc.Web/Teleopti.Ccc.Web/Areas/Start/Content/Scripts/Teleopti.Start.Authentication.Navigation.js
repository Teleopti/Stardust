
Teleopti.Start.Authentication.NavigationConstructor = function() {
	this.GotoBusinessUnits = function (authenticationType, dataSourceName) {
		window.location.hash = 'businessunit/' + authenticationType + '/' + dataSourceName;
	};
	this.GotoSignIn = function () {
		window.location.hash = '';
	};
};
Teleopti.Start.Authentication.Navigation = new Teleopti.Start.Authentication.NavigationConstructor();
