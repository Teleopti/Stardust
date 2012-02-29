if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}


Teleopti.MyTimeWeb.Settings = (function ($) {

	function _init() {
		Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Settings/Index', Teleopti.MyTimeWeb.Settings.PartialInit);
		Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Settings/Password', Teleopti.MyTimeWeb.Settings.PartialInit);
	}

	function _partialInit() {
		_onSelectorChanged();
		_passwordEvents();
		_initButton();
	}

	function _initButton() {
		$("input#passwordButton").button()
				.click(function () {
					_updatePassword($("input#oldPassword").val(), $("input#password").val());
				});
	}

	function _passwordEvents() {
		$("input#password, input#passwordValidation").keyup(function () {
			var incorrectLabel = $("#nonMatchingPassword");
			var passwordButton = $("input#passwordButton");
			var pw = $("input#password").val();
			var pw2 = $("input#passwordValidation").val();
			if (pw != pw2) {
				incorrectLabel.show();
				passwordButton.button("disable");
			} else {
				incorrectLabel.hide();
				passwordButton.button("enable");
			}
		});

		$("input#oldPassword").keyup(function () {
			$("#incorrectOldPassword").hide();
		});
	}

	function _updatePassword(oldPassword, newPassword) {
		var data = { OldPassword: oldPassword, NewPassword: newPassword };
		$.ajax({
			url: "Settings/ChangePassword",
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: "PUT",
			cache: false,
			data: JSON.stringify(data),
			success: function (data, textStatus, jqXHR) {
				var updatedLabel = $("label#updated");
				updatedLabel.show();
				$("#passwordDiv input").reset();
				setTimeout(function () { updatedLabel.hide() }, 2000);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 401) {
					$("#incorrectOldPassword").show();
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}

	function _onSelectorChanged() {
		$("#selectors #cultureSelect").change(function () {
			_selectorChanged($(this).val(), "Settings/UpdateCulture");
		});
		$("#selectors #cultureUiSelect").change(function () {
			_selectorChanged($(this).val(), "Settings/UpdateUiCulture");
		});
	}

	function _selectorChanged(value, url) {
		$("#selectors label").hide();
		var data = { LCID: value };
		$.ajax({
			url: url,
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: "PUT",
			cache: false,
			data: JSON.stringify(data),
			success: function (data, textStatus, jqXHR) {
				location.reload();
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
		PartialInit: function () {
			_partialInit();
		}
	};
})(jQuery);

$(function () {
	Teleopti.MyTimeWeb.Settings.Init();
});