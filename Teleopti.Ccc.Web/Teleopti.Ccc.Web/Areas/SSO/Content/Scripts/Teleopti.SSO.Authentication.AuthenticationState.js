
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
					gotoChangePassword();
					return;
				}
				if (responseData.AlreadyExpired) {
					gotoMustChangePassword();
					return;
				}
				self.SetReturnUrlToCookie();
				self.GotoReturnUrl();
			}
		});

		$.ajax(options);
	};

	var setCookie = function(cname, cvalue) {
		var d = new Date();
		d.setTime(d.getTime() + (5 * 60 * 1000));
		var expires = "expires=" + d.toUTCString();
		document.cookie = cname + "=" + cvalue + "; " + expires + "; path=/";
	};

	this.SetReturnUrlToCookie = function () {
		if (window.location.hash)
			setCookie("returnHash", window.location.hash);
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


	var changeTenantPassword = function (options) {
		//not done!
		$.extend(options, {
			url: data.baseUrl + "ChangePassword/Modify",
			dataType: "json",
			type: 'POST',
			cache: false,
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status === 400 || jqXHR.Status === 403) {
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

	this.ApplyChangePassword = function (options) {
		options.data.UserName = authenticationModel.username;

		$.ajax({
			url: data.baseUrl + "ToggleHandler/IsEnabled?toggle=MultiTenancy_People_32113",
			async: false,
			success: function (data) {
				if (data.IsEnabled) {
					changeTenantPassword(options);
				}
				changePasswordAjax(options);
			}
		});
	};

	this.TryToSignIn = function (options) {
		authenticationModel = options.data;

		var error = function (jqXHR, textStatus, errorThrown) {
			if (jqXHR.status === 400) {
				var response = $.parseJSON(jqXHR.responseText);
				options.errormessage(response.Errors[0]);
				return;
			}
			try {
				var json = JSON.parse(jqXHR.responseText);
				$('#body-inner').html('<h2>Error: ' + jqXHR.status + '</h2>');
				$('#dialog-modal-header').text(errorThrown);
				$('#dialog-modal-body').html(json.Message);
				$('#dialog-modal').modal({
					show: true
				});
			}
			catch (e) {
				$('#body-inner').html('<h2>Error: ' + jqXHR.status + '</h2>');
				$('#dialog-modal-header').text(errorThrown);
				$('#dialog-modal-body').html(jqXHR.responseText);
				$('#dialog-modal').modal({
					show: true
				});
			}
		};

		$.extend(options, {
			error: error
		});

		checkPasswordAjax(options);
	};
};

