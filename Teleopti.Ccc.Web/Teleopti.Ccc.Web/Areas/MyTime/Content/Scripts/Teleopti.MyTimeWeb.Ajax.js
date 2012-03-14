/// <reference path="~/Content/Scripts/jquery-1.6.4.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.min.js" />
/// <reference path="~/Content/Scripts/jquery-1.6.4-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Content/Scripts/jquery.qtip.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Ajax = (function ($) {

	var _settings = {};

	function _ajax(options) {

		_setupUrl(options);
		_setupError(options);
		_setupCache(options);
		_setupGlobal(options);

		$.ajax(options);
	}

	function _setupUrl(options) {
		if (options.url.indexOf('http://') != -1)
			return;
		if (options.url.indexOf('/') == 0)
			return;
		options.url = _settings.baseUrl + options.url;
	}

	function _setupError(options) {
		
		options.statusCode401 = options.statusCode401 || function () { window.location.href = '/'; };

		var errorCallback = options.error;

		options.error = function (jqXHR, textStatus, errorThrown) {
			if (options.statusCode400 && jqXHR && jqXHR.status == 400) {
				options.statusCode400(jqXHR, textStatus, errorThrown);
				return;
			}
			if (options.statusCode401 && jqXHR && jqXHR.status == 401) {
				options.statusCode401(jqXHR, textStatus, errorThrown);
				return;
			}
			if (options.statusCode404 && jqXHR && jqXHR.status == 404) {
				options.statusCode404(jqXHR, textStatus, errorThrown);
				return;
			}
			if (errorCallback) {
				errorCallback(jqXHR, textStatus, errorThrown);
				return;
			}
			Teleopti.MyTimeWeb.Ajax.UI.ShowAjaxError(jqXHR, textStatus, errorThrown);
		};
	}

	function _setupCache(options) {
		options.cache = options.cache || false;
	}

	function _setupGlobal(options) {
		options.global = options.global || false;
	}

	return {
		Init: function (settings) {
			_settings = settings;
		},
		Ajax: function (options) {
			_ajax(options);
		}
	};

})(jQuery);

$.extend({
		myTimeAjax: function(options) {
			Teleopti.MyTimeWeb.Ajax.Ajax(options);
		}
	})
	;

Teleopti.MyTimeWeb.Ajax.UI = (function ($) {

	function _ajaxErrorBody(jqXHR, textStatus, errorThrown) {
		$('#body-inner').html('<h2>Error: ' + jqXHR.status + '</h2>');
	}

	function _ajaxErrorDialog(jqXHR, textStatus, errorThrown) {
		$('#dialog-modal').attr('title', 'Ajax error: ' + errorThrown);
		$('#dialog-modal').dialog({
			width: 800,
			height: 500,
			position: 'center',
			modal: true,
			create: function (event, ui) {
				var responseText = jqXHR.responseText;
				$(this).html(responseText);
			}
		});
	}

	return {
		ShowAjaxError: function (jqXHR, textStatus, errorThrown) {
			_ajaxErrorBody(jqXHR, textStatus, errorThrown);
			_ajaxErrorDialog(jqXHR, textStatus, errorThrown);
		}
	};

})(jQuery);
