
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
			url: data.baseUrl + "SSO/ApplicationAuthenticationApi/CheckPassword",
			dataType: "json",
			type: 'GET',
			cache: false,
			data: authenticationModel,
			success: function (responseData, textStatus, jqXHR) {
				if (responseData.WillExpireSoon) {
					gotoChangePassword(authenticationModel.datasource);
					return;
				}
				if (responseData.AlreadyExpired) {
					gotoMustChangePassword(authenticationModel.datasource);
					return;
				}

				self.GotoReturnUrl();
			}
		});

		$.ajax(options);
	};

	this.GotoReturnUrl = function() {
		gotoReturnUrl(Teleopti.SSO.Authentication.Settings.returnUrl, Teleopti.SSO.Authentication.Settings.pendingRequest);
	};

	this.CheckState = function () {
		if (authenticationModel) {
			return true;
		} else {
			return false;
		}
	};

	this.ApplyChangePassword = function (options) {
		options.data.UserName = authenticationModel.username;
		changePasswordAjax(options);
	};

	var changePasswordAjax = function (options) {

		$.extend(options, {
			url: data.baseUrl + "SSO/ApplicationAuthenticationApi/ChangePassword",
			dataType: "json",
			type: 'POST',
			cache: false,
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var response = $.parseJSON(jqXHR.responseText);
					options.errormessage(response.Errors[0]);
					return;
				}
			},
			success: function (responseData, textStatus, jqXHR) {
				authenticationModel.password = options.data.newPassword;
				self.GotoReturnUrl();
			}
		});

		$.ajax(options);
	};

	this.TryToSignIn = function (options) {
		authenticationModel = options.data;

		var error = function (jqXHR, textStatus, errorThrown) {
			if (jqXHR.status == 400) {
				var response = $.parseJSON(jqXHR.responseText);
				options.errormessage(response.Errors[0]);
				return;
			}
			try {
				var json = JSON.parse(jqXHR.responseText);
				$('#Exception-message').text(json.Message);
				$('#Exception-div').show();
			}
			catch (e) {
				$('#body-inner').html('<h2>Error: ' + jqXHR.status + '</h2>');
				$('#dialog-modal-header').text(errorThrown);
				$('#dialog-modal-body').html(jqXHR.responseText);
				$('#dialog-modal').modal('show');
			}
		};

		$.extend(options, {
			error: error
		});

		checkPasswordAjax(options);
	};
};

