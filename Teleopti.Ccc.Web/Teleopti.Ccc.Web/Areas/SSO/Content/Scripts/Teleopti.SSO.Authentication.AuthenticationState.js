
if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.SSO) === 'undefined') {
		Teleopti.SSO = {};
		if (typeof (Teleopti.SSO.Authentication) === 'undefined') {
			Teleopti.SSO.Authentication = {};
		}
	}
}

Teleopti.SSO.Authentication.AuthenticationState = function (data) {

	var self = this;

	var authenticationModel;

	var gotoReturnUrl = Teleopti.SSO.Authentication.Navigation.GotoReturnUrl;
	var gotoChangePassword = Teleopti.SSO.Authentication.Navigation.GotoChangePassword;
	var gotoMustChangePassword = Teleopti.SSO.Authentication.Navigation.GotoMustChangePassword;


	var checkPasswordAjax = function (options) {
		$.extend(options, {
			url: data.baseUrl + "SSO/ApplicationAuthenticationApi/Password",
			dataType: "json",
			type: 'POST',
			cache: false,
			data: authenticationModel,
			success: function (responseData, textStatus, jqXHR) {
				if (responseData.WillExpireSoon) {
					gotoChangePassword();
					return;
				}
				if (responseData.AlreadyExpired) {
					gotoMustChangePassword();
					return;
				}
				self.GotoReturnUrl(authenticationModel.rememberMe);
			}
		});

		$.ajax(options);
	};

	this.GotoReturnUrl = function(rememberMe) {
		gotoReturnUrl(Teleopti.SSO.Authentication.Settings.returnUrl, Teleopti.SSO.Authentication.Settings.pendingRequest, rememberMe);
	};

	this.CheckState = function () {
		if (authenticationModel) {
			return true;
		} else {
			return false;
		}
	};

	var changePasswordAjax = function (options) {
		$.extend(options, {
			url: data.baseUrl + "SSO/ApplicationAuthenticationApi/Password",
			dataType: "json",
			type: 'PUT',
			cache: false,
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var response = $.parseJSON(jqXHR.responseText);
					options.errormessage(response.ModelState.Error[0]);
					return;
				}
			},
			success: function (responseData, textStatus, jqXHR) {
				authenticationModel.password = options.data.newPassword;
				self.GotoReturnUrl(authenticationModel.rememberMe);
			}
		});

		$.ajax(options);
	};

	this.ApplyChangePassword = function (options) {
		options.data.UserName = authenticationModel.username;

		changePasswordAjax(options);
	};

	this.TryToSignIn = function (options) {
		authenticationModel = options.data;

		var error = function (jqXHR, textStatus, errorThrown) {
			if (jqXHR.status === 400) {
				var response = $.parseJSON(jqXHR.responseText);
				options.errormessage(response.ModelState.Error[0]);
				return;
			}

			options.errormessage("An error has occurred, please try again.");
			if (console && console.error)
				console.error("Method Failed: " + jqXHR.responseText + textStatus + errorThrown);
		};

		$.extend(options, {
			error: error
		});

		checkPasswordAjax(options);
	};
};

