﻿if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}


Teleopti.MyTimeWeb.Settings = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	function _init() {
		Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Settings/Index', Teleopti.MyTimeWeb.Settings.PartialInit);
		Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Settings/Password', Teleopti.MyTimeWeb.Settings.PartialInit);
	}

	function _partialInit() {
		_initSelectors();
		_passwordEvents();
		_initButton();
	}

	function _initButton() {
		$("#passwordButton")
			.click(function () {
				_updatePassword($("input#oldPassword").val(), $("input#password").val());
			});
	}

	function _passwordEvents() {
		$("input#password, input#passwordValidation").keyup(function () {
			var incorrectLabel = $("#nonMatchingPassword");
			var passwordButton = $("#passwordButton");
			var pw = $("input#password").val();
			var pw2 = $("input#passwordValidation").val();
			if (pw != pw2) {
				incorrectLabel.show();
				passwordButton.attr('disabled','disabled');
			} else {
				incorrectLabel.hide();
				passwordButton.removeAttr('disabled');
			}
		});

		$("input#oldPassword").keyup(function () {
			$("#incorrectOldPassword").hide();
		});

		$("input#password").keyup(function () {
			$("#invalidNewPassword").hide();
		});
		$("input#passwordValidation").keyup(function () {
			$("#invalidNewPassword").hide();
		});
	}

	function _updatePassword(oldPassword, newPassword) {
		var data = { OldPassword: oldPassword, NewPassword: newPassword };
		ajax.Ajax({
			url: "Settings/ChangePassword",
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: 'POST',
			data: JSON.stringify(data),
			success: function (data, textStatus, jqXHR) {
				var updatedLabel = $("label#updated");
				updatedLabel.show();
				$("#incorrectOldPassword").hide();
				$("#invalidNewPassword").hide();
				$("#settings input").reset();
				setTimeout(function () { updatedLabel.hide(); }, 2000);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var error = $.parseJSON(jqXHR.responseText);
					if (error.IsAuthenticationSuccessful) {
						$("#invalidNewPassword").show();
					} else {
						$("#incorrectOldPassword").show();
					}
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}

	function _initSelectors() {
		$('#cultureSelect')
			.select2({
			    width: 'resolve',
			    triggerChange: true,
			}).on('change', function () {
			    _selectorChanged($(this).val(), "Settings/UpdateCulture");
			})
			;
		$('#cultureUiSelect')
			.select2({
			    width: 'resolve',
			    triggerChange: true
			}).on('change', function () {
			    _selectorChanged($(this).val(), "Settings/UpdateUiCulture");
			})
			;
	}

	function _selectorChanged(value, url) {
		var data = { LCID: value };
		ajax.Ajax({
			url: url,
			contentType: 'application/json; charset=utf-8',
			type: "PUT",
			data: JSON.stringify(data),
			success: function (data, textStatus, jqXHR) {
				ajax.CallWhenAllAjaxCompleted(function () {
					location.reload();
				});
			},
			error: function (jqXHR, textStatus, errorThrown) {
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}

	return {
		Init: function () {
			_init();
		},
		PartialInit: function (readyForInteraction, completelyLoaded) {
			_partialInit();
			readyForInteraction();
			completelyLoaded();
		}
	};
})(jQuery);