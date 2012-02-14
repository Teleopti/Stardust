/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Common = (function ($) {
	function _log() {
		if (window.console && window.console.log)
			window.console.log(Array.prototype.join.call(arguments, ' '));
	}

	function _isFixedDate(dateString) {
		return dateString.match(/^\d{4}-\d{2}-\d{2}$/);
	}

	function _parseFixedDateStringToDate(dateString) {
		if (!_isFixedDate(dateString)) {
			return new Date();
		}
		return $.datepicker.parseDate($.datepicker.ISO_8601, dateString);
	}

	function _fixedDateToPartsUrl(fixedDate) {
		return '/' + fixedDate.split("-").join("/");
	}

	function _openEditSection(editSectionId) {
		var editsection = $(editSectionId);

		$('#modal-disable').show();

		/* Need this to get edit-section height*/
		editsection.css({ 'visibility': 'hidden', 'display': 'block' });
		var editHeight = editsection.height();
		editsection.removeAttr('style');

		$('#page').animate({
			paddingTop: editHeight + 78
		}, 300, function () {
			editsection.show();
		});

		$(editsection).find('.cancel')
			.bind('click', function (e) {
				_closeEditSection(editSectionId);
			});

		$('.edit-section-content .ui-button').button("option", "disabled", false);
	}

	function _closeEditSection(editSectionId) {
		var editsection = $(editSectionId);
		editsection.hide();
		$('#page').animate({
			paddingTop: 77
		}, 300, function () {
			$('#modal-disable').hide();
		});

	};

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
		},
		ParseToDate: function (dateString) {
			return _parseFixedDateStringToDate(dateString);
		},
		OpenEditSection: function (editSectionId) {
			_openEditSection(editSectionId);
		},
		FixedDateToPartsUrl: function (fixedDate) {
			return _fixedDateToPartsUrl(fixedDate);
		},
		IsFixedDate: function (dateString) {
			return _isFixedDate(dateString);
		},
		CloseEditSection: function (editSectionId) {
			_closeEditSection(editSectionId);
		}
	};

})(jQuery);

Teleopti.MyTimeWeb.Common.Layout = (function ($) {
	return {
		// Disable navigation tabs
		DisableTabs: function () {
			$("#tabs-signin, #tabs").tabs({ disabled: [0, 1] });
		},

		// Activating navigation tabs
		ActivateTabs: function () {
			$("#tabs").tabs();
		},

		// Activating buttons
		ActivateStdButtons: function () {
			$(".button").button();
		},

		// Activate custom inputs
		ActivateCustomInput: function () {
			$('.edit-module input[type="checkbox"], .edit-module input[type="radio"]').customInput();
		},


		//Activating tooltip where available
		ActivateTooltip: function () {
			var _toolTip = '';
			$('.tooltip').hover(function (e) {
				_self = $(this);
				_tooltip = $('<div></div>').addClass('tooltip-container').appendTo(_self);
				_tooltipTop = $('<div></div>').addClass('tooltip-top').appendTo(_tooltip);
				if (($(_self).data('mytime-subject') !== "" && $(_self).data('mytime-subject') !== undefined) && ($(_self).data('mytime-location') !== undefined)) {
					_tooltipContent = $('<div><dl><dt>Subject: </dt><dd>' + $(_self).attr('data-mytime-subject') + '</dd><dt>Location: </dt><dd>' + $(_self).attr('data-mytime-location') + '</dd></dl></div>')
								.addClass('tooltip-content')
								.appendTo(_tooltip).parent().hide().fadeIn(500);
				}
				else if (($(_self).data('mytime-activity') !== "" && $(_self).data('mytime-activity') !== undefined) && ($(_self).data('mytime-start-time') !== "" && $(_self).data('mytime-start-time') !== undefined) && ($(_self).data('mytime-end-time') !== "" && $(_self).data('mytime-end-time') !== undefined)) {
					_tooltipContent = $('<div>' + $(_self).attr('data-mytime-activity') + '<br/>' + $(_self).attr('data-mytime-start-time') + ' - ' + $(_self).attr('data-mytime-end-time') + '</div>')
								.addClass('tooltip-content')
								.appendTo(_tooltip).parent().hide().fadeIn(500);
				}
				else if ($(_self).data('mytime-tooltip') !== "" && $(_self).data('mytime-tooltip') !== undefined) {
					_tooltipContent = $('<div>' + $(_self).attr('data-mytime-tooltip') + '</div>')
								.addClass('tooltip-content')
								.appendTo(_tooltip).parent().hide().fadeIn(500);
				}
				_tooltipBottom = $('<div></div>').addClass('tooltip-bottom').appendTo(_tooltip);
				e.preventDefault();

			}, function () {
				$(_tooltip).fadeOut(100).remove();
			});
		}

	};
})(jQuery);

Teleopti.MyTimeWeb.Common.LoadingOverlay = (function ($) {

	function _addOverlay(optionsOrElement) {
		var options = {};
		if (optionsOrElement.element) {
			options = optionsOrElement;
		} else {
			options.element = optionsOrElement;
		}
		options.loadingClass = options.loadingClass || 'loading';
		
		options.element.addClass('relative');
		$('<div>')
			.css({
				'height': '100%'
			})
			.addClass(options.loadingClass)
			.addClass('overlay')
			.appendTo(options.element)
			.show()
			;
	}

	function _removeOverlay(element) {
		$('.overlay', element).remove();
	}

	return {
		Add: function (optionsOrElement) {
			_addOverlay(optionsOrElement);
		},
		Remove: function (element) {
			_removeOverlay(element);
		}
	};

})(jQuery);
