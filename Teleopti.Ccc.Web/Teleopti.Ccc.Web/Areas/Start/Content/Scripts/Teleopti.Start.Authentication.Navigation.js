
Teleopti.Start.Authentication.NavigationConstructor = function () {
	this.GotoChangePassword = function (dataSourceName) {
		window.location.hash = 'changepassword/' + dataSourceName;
	};
	this.GotoMustChangePassword = function () {
		window.location.hash = 'mustchangepassword/' + dataSourceName;
	};
	this.GotoBusinessUnits = function (authenticationType, dataSourceName) {
		window.location.hash = 'businessunit/' + authenticationType + '/' + dataSourceName;
	};
	this.GotoSignIn = function (jqXHR, textStatus, errorThrown) {
		window.location.hash = '';
		if (jqXHR.status == 500) {
			var response = $.parseJSON(jqXHR.responseText);
			$('#Exception-message').text(response.Message);
			$('#Exception-div').show();
		}
	};
	this.GotoMenu = function () {
		window.location.hash = 'menu';
	};
};
Teleopti.Start.Authentication.Navigation = new Teleopti.Start.Authentication.NavigationConstructor();
