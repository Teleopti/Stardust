/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />


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

Teleopti.Start.Common.Layout = (function ($) {
	return {
		ActivateStdButtons: function () {
			$(".button").button();
		},
		ActivateCustomInput: function () {
			$('.edit-module input[type="checkbox"], .edit-module input[type="radio"]').customInput();
		},
		ActivatePlaceHolderText: function () {
			if (!Modernizr.input.placeholder) {
				var active = document.activeElement;
				$(':text, :password').focus(function() {
					if ($(this).attr('placeholder') != '' && $(this).val() == $(this).attr('placeholder')) {
						$(this).val('').removeClass('hasPlaceholder');
					}
				}).blur(function() {
					if ($(this).attr('placeholder') != '' && ($(this).val() == '' || $(this).val() == $(this).attr('placeholder'))) {
						$(this).val($(this).attr('placeholder')).addClass('hasPlaceholder');
					}
				});
				$(':text, :password').blur();
				$(active).focus();
				$('form').submit(function() {
					$(this).find('.hasPlaceholder').each(function() { $(this).val(''); });
				});
			}
		}
	};
})(jQuery);
