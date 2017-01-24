﻿
RetrieveDeviceInstanceToken = function(currentToken) {
	return {store: false,token: ''};
}


Teleopti.Start.Authentication.AuthenticationState = function (data) {

	var self = this;

	var authenticationModel;
	var businessUnits;
	var applications;

	var gotoSignInView = Teleopti.Start.Authentication.Navigation.GotoSignIn;
	var gotoBusinessUnitsView = Teleopti.Start.Authentication.Navigation.GotoBusinessUnits;
	var toggleNewWeb = false;

	this.AttemptGotoApplicationBySignIn = function (options) {
		$.extend(options, {
			success: function (responseData, textStatus, jqXHR) {

				if (responseData.length > 1) {
					gotoBusinessUnitsView();
					return;
				}

				if (responseData.length == 0) {
					options.nobusinessunit();
					return;
				}

				if (responseData.length == 1) {
					$.extend(options, {
						data: {
							businessUnitId: responseData[0].Id
						}
					});
					self.AttemptGotoApplicationBySelectingBusinessUnit(options);

					return;
				}

				options.errormessage("obscure amount of business units found");
			}
		});

		businessUnitsAjax(options);
	};

	this.CheckState = function () {
		if (authenticationModel) {
			return true;
		} else {
			return false;
		}
	};

	var businessUnitsAjax = function (options) {

		var success = options.success;

		var optionForBusinessUnits = {
			url: data.baseUrl + "Start/AuthenticationApi/BusinessUnits",
			dataType: "json",
			type: 'GET',
			cache: false,
			data: authenticationModel,
			success: function (responseData, textStatus, jqXHR) {
				businessUnits = responseData;
				success(responseData, textStatus, jqXHR);
			},
			error: function () {
				//TODO: tenant - a bit strange here. Just made the scenarios green
				options.nodatasource();
			}
		}

		$.ajax(optionForBusinessUnits);
	};

	this.storeDeviceInstanceToken = function(options, token) {
		var optionForBusinessUnits = {
			url: data.baseUrl + "Start/UserToken",
			dataType: "json",
			type: 'POST',
			cache: false,
			data: JSON.stringify({ token: token }),
			success: function(responseData, textStatus, jqXHR) {
			},
			error: function() {
			}
		}

		$.ajax(optionForBusinessUnits);
	};

	var logonAjax = function (options) {

		$.extend(options, {
			url: data.baseUrl + "Start/AuthenticationApi/Logon",
			dataType: "json",
			type: 'POST',
			cache: false,
			data: authenticationModel
		});

		$.ajax(options).then(function() {
			var result = RetrieveDeviceInstanceToken(null);
			if (result.store) {
				self.storeDeviceInstanceToken(options, result.token);
			}
		});


	};

	var applicationsAjax = function (options) {
		var success = options.success;

		$.extend(options, {
			url: data.baseUrl + "Start/Menu/Applications",
			dataType: "json",
			type: 'GET',
			cache: false,
			data: null,
			success: function (responseData, textStatus, jqXHR) {
				applications = responseData;
				success(responseData, textStatus, jqXHR);
			}
		});

		$.ajax(options);
	};

	var globalAreasAjax = function (options) {

		var success = options.success;
		$.extend(options, {
			url: data.baseUrl + "Global/Application/Areas",
			dataType: "json",
			type: 'GET',
			cache: false,
			data: null,
			success: function (responseData, textStatus, jqXHR) {
				success(responseData, textStatus, jqXHR);
			}
		});

		$.ajax(options);
	};

	var getCookie = function (cname) {
		var name = cname + "=";
		var ca = document.cookie.split(';');
		for (var i = 0; i < ca.length; i++) {
			var c = ca[i];
			while (c.charAt(0) == ' ') c = c.substring(1);
			if (c.indexOf(name) == 0) return c.substring(name.length, c.length);
		}
		return "";
	};

	var deleteCookie = function (name) {
		document.cookie = name + '=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
	};

	this.TryToSignIn = function (options) {
		authenticationModel = options.data;

		var error = function (jqXHR, textStatus, errorThrown) {
			if (jqXHR.status == 400) {
				var response = $.parseJSON(jqXHR.responseText);
				options.errormessage(response.ModelState.Error[0]);
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

		self.AttemptGotoApplicationBySignIn(options);

	};

	this.AttemptGotoApplicationBySelectingBusinessUnit = function (options) {
	    authenticationModel.businessUnitId = options.data.businessUnitId;

	    if (window.sessionStorage) {
	    	window.sessionStorage.setItem("buid", authenticationModel.businessUnitId);
	    }

		var error = options.error || gotoSignInView;
		var errormessage = options.errormessage || gotoSignInView;

		$.extend(options, {
			success: function (logonData, textState, jqXHR) {
				toggleNewWeb = logonData.WfmTeamSchedule_MakeNewMyTeamDefault_39744;
				
				$.extend(options, {
					success: function(applicationsData, textState, jqXHR) {
						keepUrlAfterLogon(applicationsData, textState, jqXHR);
					}
				});
				globalAreasAjax(options);
			},
			error: error,
			errormessage: errormessage
		});

		if (typeof fatClientWebLogin !== 'undefined') {
		    authenticationDetailsAjax(authenticationModel.businessUnitId);
		}

		if (options.businessUnitSelectionError) {
			$.extend(options, {
				error: function (jqXHR, textStatus, errorThrown) {
					if (jqXHR.status == 400) {
						var json = JSON.parse(jqXHR.responseText);
						options.businessUnitSelectionError(json.ModelState.Error[0]);
					}
				}
			});
		}

		logonAjax(options);
	};


	function keepUrlAfterLogon(applicationsData, textState, jqXHR) {

		var areaToGo;
		var returnHash = getCookie("returnHash");

		if (applicationsData.length > 1) {

			tryToGoToHash(returnHash);
			if (toggleNewWeb) {
				tryToGoToWfm();
			} else {
				tryToGoToAnyWhere();
			}

			tryToGoToFirstApplication();

		} else if (applicationsData.length == 1) {

			tryToGoToHash(returnHash);
			tryToGoToFirstApplication();

		} else {
			errormessage($('#Signin-error').data('nopermissiontext'));
			return;
		}

		if (typeof fatClientWebLogin !== 'undefined') {
			authenticationDetailsAjax(authenticationModel.businessUnitId);
		}

		deleteCookie("returnHash");
		window.location.href = data.baseUrl + areaToGo;


		function tryToGoToHash(hash) {
			if (areaToGo && areaToGo.length > 0) return;

			var hasPermissionToGoToHash = applicationsData.some(function(app) {
				return hash.indexOf(app.Name) === 0;
			});
			if (hasPermissionToGoToHash) {
				areaToGo = hash;
			}
		}

		function tryToGoToAnyWhere() {
			if (areaToGo && areaToGo.length > 0) return;

			var anywhereApplication = ko.utils.arrayFirst(applicationsData, function(a) {
				return a.Name === "Anywhere";
			});
			if (anywhereApplication) {
				areaToGo = anywhereApplication.Name;
			}
		}

		function tryToGoToWfm() {
			if (areaToGo && areaToGo.length > 0) return;

			var wfmApplication = ko.utils.arrayFirst(applicationsData, function(a) {
				return a.Name === "WFM";
			});
			if (wfmApplication) {
				areaToGo = wfmApplication.Name;
			}
		}

		function tryToGoToFirstApplication() {
			if (areaToGo && areaToGo.length > 0) return;

			areaToGo = applicationsData[0].Name;
		}
	}


	var authenticationDetailsAjax = function (buId) {
		$.ajax({
			type: "GET",
			url: data.baseUrl + "Start/Url/AuthenticationDetails",
			async: false,
			success: function (data) {
				fatClientWebLogin(buId, data.PersonId);
			}
		});
	};


	this.GetDataForBusinessUnitSelectionView = function (options) {
		if (authenticationModel)
			$.extend(authenticationModel, options.data);
		else
			authenticationModel = options.data;

		if (businessUnits) {
			options.businessunits(businessUnits);
			return;
		}

		$.extend(options, {
			success: function () {
				options.businessunits(businessUnits);
			},
			error: gotoSignInView
		});
		businessUnitsAjax(options);
	};

	this.GetDataForMenu = function (options) {

		if (applications) {
			options.applications(applications);
			return;
		}

		$.extend(options, {
			success: function () {
				options.applications(applications);
			},
			error: gotoSignInView
		});

		applicationsAjax(options);
	};

};

