
Teleopti.Start.Authentication.NavigationConstructor = function () {
	this.GotoBusinessUnits = function () {
		window.location.hash = 'businessunit';
	};
	this.GotoSignIn = function () {
		window.location.hash = '';
	};
	this.GotoMenu = function () {
		window.location.hash = 'menu';
	};
};
Teleopti.Start.Authentication.Navigation = new Teleopti.Start.Authentication.NavigationConstructor();
