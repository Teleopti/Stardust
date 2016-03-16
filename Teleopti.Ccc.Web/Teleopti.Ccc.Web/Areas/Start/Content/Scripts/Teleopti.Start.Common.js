/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.Start) === 'undefined') {
		Teleopti.Start = {};
	}
}

Teleopti.Start.Common = (function ($) {
	function _log() {
		if (window.console && window.console.log)
			window.console.log(Array.prototype.join.call(arguments, ' '));
	}

	return {
		AjaxFailed: function (jqXHR, noIdea, title) {
			$('#dialog-modal').attr('title', 'Ajax error: ' + title);
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
			_log("Method Failed" + jqXHR + noIdea + title);
		},
		Log: function (logmessage) {
			_log(logmessage);
		}
	};

})(jQuery);

