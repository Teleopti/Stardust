if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}


Teleopti.MyTimeWeb.Settings = (function ($) {

	function _init() {
		Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Settings/Index', Teleopti.MyTimeWeb.Settings.PartialInit);
	}

	function _partialInit() {
		_onSelectorChanged();
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