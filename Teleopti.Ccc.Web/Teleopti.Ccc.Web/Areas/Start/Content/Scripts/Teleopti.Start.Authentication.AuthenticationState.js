
Teleopti.Start.Authentication.AuthenticationState = function (data) {

	var authenticationModel;
	var businessUnits;

	var loadBusinessUnits = function (options) {
		var error = function (jqXHR, textStatus, errorThrown) {
			options.error(jqXHR, textStatus, errorThrown);
		};

		var success = function (responseData, textStatus, jqXHR) {
			businessUnits = responseData;
			options.success(responseData, textStatus, jqXHR);
		};

		$.ajax({
			url: data.baseUrl + "Start/AuthenticationApi/BusinessUnits",
			dataType: "json",
			type: 'GET',
			data: authenticationModel,
			error: error,
			success: success
		});
	};

	this.ForceLoadBusinessUnits = function (options) {
		authenticationModel = options.data;
		loadBusinessUnits(options);
	};

	this.MightLoadBusinessUnits = function (options) {
		if (authenticationModel)
			$.extend(authenticationModel, options.data);
		else
			authenticationModel = options.data;

		if (businessUnits) {
			options.success(businessUnits);
			return;
		}

		loadBusinessUnits(options);
	};

	this.SignIn = function (options) {
		$.extend(authenticationModel, options.data);

		$.ajax({
			url: data.baseUrl + "Start/AuthenticationApi/Logon",
			dataType: "json",
			type: 'POST',
			data: authenticationModel,
			error: options.error,
			success: options.success
		});
	};

};

