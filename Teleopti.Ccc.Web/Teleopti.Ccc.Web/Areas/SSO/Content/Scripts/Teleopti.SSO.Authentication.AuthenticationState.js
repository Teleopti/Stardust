﻿
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
	var businessUnits;
	var applications;

	var gotoSignInView = Teleopti.SSO.Authentication.Navigation.GotoSignIn;
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

	//this.AttemptGotoApplicationBySignIn = function (options) {

	//	$.extend(options, {
	//		success: function (responseData, textStatus, jqXHR) {

	//			if (responseData.length > 1) {
	//				gotoBusinessUnitsView(authenticationModel.type, authenticationModel.datasource);
	//				return;
	//			}

	//			if (responseData.length == 0) {
	//				options.nobusinessunit();
	//				return;
	//			}

	//			if (responseData.length == 1) {
	//				$.extend(options, {
	//					data: {
	//						businessUnitId: responseData[0].Id
	//					}
	//				});
	//				self.AttemptGotoApplicationBySelectingBusinessUnit(options);
	//				return;
	//			}

	//			options.errormessage("obscure amount of business units found");
	//		}
	//	});

	//	businessUnitsAjax(options);
	//};

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

	//var businessUnitsAjax = function (options) {

	//	var success = options.success;

	//	$.extend(options, {
	//		url: data.baseUrl + "SSO/AuthenticationApi/BusinessUnits",
	//		dataType: "json",
	//		type: 'GET',
	//		cache: false,
	//		data: authenticationModel,
	//		success: function (responseData, textStatus, jqXHR) {
	//			businessUnits = responseData;
	//			success(responseData, textStatus, jqXHR);
	//		}
	//	});

	//	$.ajax(options);
	//};

	//var logonAjax = function (options) {

	//	$.extend(options, {
	//		url: data.baseUrl + "SSO/AuthenticationApi/Logon",
	//		dataType: "json",
	//		type: 'POST',
	//		cache: false,
	//		data: authenticationModel
	//	});

	//	$.ajax(options);
	//};

	//var applicationsAjax = function (options) {

	//	var success = options.success;

	//	$.extend(options, {
	//		url: data.baseUrl + "SSO/Menu/Applications",
	//		dataType: "json",
	//		type: 'GET',
	//		cache: false,
	//		data: null,
	//		success: function (responseData, textStatus, jqXHR) {
	//			applications = responseData;
	//			success(responseData, textStatus, jqXHR);
	//		}
	//	});

	//	$.ajax(options);
	//};


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
		//if (!authenticationModel.isWindows) {
			checkPasswordAjax(options);
		//} else {
		//	self.GotoReturnUrl();
		//}
	};

	//this.AttemptGotoApplicationBySelectingBusinessUnit = function (options) {
	//	authenticationModel.businessUnitId = options.data.businessUnitId;

	//	var error = options.error || gotoSignInView;
	//	var errormessage = options.errormessage || gotoSignInView;

	//	$.extend(options, {
	//		success: function (logonData, textState, jqXHR) {

	//			$.extend(options, {
	//				success: function (applicationsData, textState, jqXHR) {

	//					var area;

	//					var inApplication = ko.utils.arrayFirst(applicationsData, function (a) {
	//						var url = "/" + a.Area + "/";
	//						return window.location.href.indexOf(url) !== -1;
	//					});
						
	//					var anywhereApplication = ko.utils.arrayFirst(applicationsData, function (a) {
	//						return a.Area === "Anywhere";
	//					});

	//					if (inApplication)
	//						area = inApplication.Area;
	//					else if (applicationsData.length == 1)
	//						area = applicationsData[0].Area;
	//					else if (anywhereApplication)
	//						area = anywhereApplication.Area;

	//					if (area) {
	//						window.location.href = data.baseUrl + area;
	//						return;
	//					}

	//					if (applicationsData.length > 1) {
	//						gotoMenuView();
	//						return;
	//					}

	//					errormessage("obscure amount of applications found");
	//				}
	//			});

	//			applicationsAjax(options);

	//		},
	//		error: error,
	//		errormessage: errormessage
	//	});

	//	logonAjax(options);
	//};





	//this.GetDataForBusinessUnitSelectionView = function (options) {
	//	if (authenticationModel)
	//		$.extend(authenticationModel, options.data);
	//	else
	//		authenticationModel = options.data;

	//	if (businessUnits) {
	//		options.businessunits(businessUnits);
	//		return;
	//	}

	//	$.extend(options, {
	//		success: function () {
	//			options.businessunits(businessUnits);
	//		},
	//		error: gotoSignInView
	//	});

	//	businessUnitsAjax(options);
	//};

	//this.GetDataForMenu = function (options) {

	//	if (applications) {
	//		options.applications(applications);
	//		return;
	//	}

	//	$.extend(options, {
	//		success: function () {
	//			options.applications(applications);
	//		},
	//		error: gotoSignInView
	//	});

	//	applicationsAjax(options);
	//};

};

