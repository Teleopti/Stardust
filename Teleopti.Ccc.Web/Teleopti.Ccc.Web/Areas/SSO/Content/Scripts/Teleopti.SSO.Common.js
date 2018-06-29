Teleopti.SSO.Common = (function ($) {
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

Teleopti.SSO.Common.Layout = (function ($) {
	return {
		ActivatePlaceHolderText: function () {
			$(':text, :password').placeholder();
		}
	};
})(jQuery);
