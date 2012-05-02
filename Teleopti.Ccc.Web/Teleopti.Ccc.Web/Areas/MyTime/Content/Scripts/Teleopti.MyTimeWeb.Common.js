/// <reference path="~/Content/Scripts/jquery-1.6.4.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.min.js" />
/// <reference path="~/Content/Scripts/jquery-1.6.4-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Content/Scripts/jquery.qtip.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.js"/>


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Common = (function ($) {
	var _settings = {};

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
		Init: function (settings) {
			_settings = settings;
		},
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

			$('.tooltip').each(function () {
				$(this).qtip({
					content: {
						title: $(this).attr('tooltip-title'),
						text: $(this).attr('tooltip-text')
					},
					title: "test",
					style: {
						classes: 'ui-tooltip-custom ui-tooltip-rounded ui-tooltip-shadow',
						tip: true
					},
					position: {
						my: 'bottom left',
						at: 'top right',
						target: 'mouse'
					}
				});
			});

			$('[title]').each(function () {
				$(this).qtip({
					content: {
						text: $(this).attr('title')
					},
					title: "test",
					style: {
						classes: 'ui-tooltip-custom ui-tooltip-rounded ui-tooltip-shadow',
						tip: true
					},
					position: {
						my: 'bottom left',
						at: 'top right',
						target: 'mouse'
					}
				});
			});

			var addTextRequest = $('.add-text-request');
			$('<div/>').qtip({

				content: {
					text: $('#Schedule-addRequest-section'),
					title: {
						text: $('#Schedule-addRequest-title'),
						button: $('#Schedule-addRequest-cancel-button').text()
					}
				},
				position: {
					target: 'event',
					my: 'middle left',
					at: 'middle right',
					viewport: $(window),
					adjust: {
						x: 15
					}
				},
				events: {
					show: function (event, api) {
						$('#Schedule-addRequest-fromDate-input').val($(event.originalEvent.target).closest('ul').attr('data-request-default-date'));
						$('#Schedule-addRequest-toDate-input').val($(event.originalEvent.target).closest('ul').attr('data-request-default-date'));
					}
				},
				show: {
					target: addTextRequest,
					event: 'click'
				},
				hide: {
					target: addTextRequest,
					event: 'unfocus'
				},
				style: {
					classes: 'ui-tooltip-input ui-tooltip-rounded ui-tooltip-shadow',
					tip: true,
					border: {
						radius: 2
					}
				}
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
