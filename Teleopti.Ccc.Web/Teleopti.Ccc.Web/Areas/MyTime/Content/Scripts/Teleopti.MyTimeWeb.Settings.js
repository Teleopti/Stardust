﻿if (typeof (Teleopti) === 'undefined') {
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
		$("#selectors select").change(function () {
			var data = { LCID: $(this).val() };

			$.ajax({
				url: "Settings/UpdateCulture",
				dataType: "json",
				contentType: 'application/json; charset=utf-8',
				type: "POST",
				cache: false,
				data: JSON.stringify(data),
				success: function (data, textStatus, jqXHR) {
					alert("ok");
				},
				error: function (jqXHR, textStatus, errorThrown) {
					Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
				}
			});
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