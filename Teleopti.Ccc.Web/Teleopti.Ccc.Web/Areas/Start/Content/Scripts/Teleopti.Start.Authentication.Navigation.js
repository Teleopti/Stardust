
Teleopti.Start.Authentication.NavigationConstructor = function() {
	this.GotoBusinessUnits = function(authenticationType, dataSourceName) {
		window.location.hash = 'businessunit/' + authenticationType + '/' + dataSourceName;
	};
};
Teleopti.Start.Authentication.Navigation = new Teleopti.Start.Authentication.NavigationConstructor();
