
Teleopti.Start.Authentication.NavigationConstructor = function () {
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
