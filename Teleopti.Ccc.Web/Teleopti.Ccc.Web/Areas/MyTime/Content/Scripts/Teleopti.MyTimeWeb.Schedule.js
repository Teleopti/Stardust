/// <reference path="~/Content/Scripts/jquery-1.6.4.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/jquery-1.6.4-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Content/Scripts/date.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}


Teleopti.MyTimeWeb.Schedule = (function ($) {

	function _initPeriodSelection() {
		var rangeSelectorId = '#ScheduleDateRangeSelector';
		var periodData = $('#ScheduleWeek-body').data('mytime-periodselection');
		Teleopti.MyTimeWeb.Portal.InitPeriodSelection(rangeSelectorId, periodData);
	}

	return {
		Init: function () {
			if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Schedule/Week', Teleopti.MyTimeWeb.Schedule.PartialInit);
			}
			Teleopti.MyTimeWeb.Schedule.TextRequest.Init();
		},
		PartialInit: function () {
			Teleopti.MyTimeWeb.Common.Layout.ActivateTooltip();
			Teleopti.MyTimeWeb.Schedule.Layout.SetSchemaItemsHeights();
			_initPeriodSelection();
			Teleopti.MyTimeWeb.Common.Layout.ActivateCustomInput();
			Teleopti.MyTimeWeb.Common.Layout.ActivateStdButtons();
			Teleopti.MyTimeWeb.Schedule.TextRequest.PartialInit();
		}
	};

})(jQuery);

Teleopti.MyTimeWeb.Schedule.TextRequest = (function ($) {

	function _initEditSection() {
		_initButtons();
		_initControls();
		_initLabels();
		_clearFormData();
	}

	function _initLabels() {
		$('#Schedule-addRequest-section input[type=text], #Schedule-addRequest-section textarea')
			.labeledinput()
			;
	}

	function _initButtons() {
		$('#Schedule-addRequest-ok-button')
			.click(function () {
				_addTextRequest();
			});

		$('#Text-request-tab')
			.click(function () {
				_hideAbsenceTypes();
			});
		$('#Absence-request-tab')
			.click(function () {
				_showAbsenceTypes();
			});
	}

	function _initControls() {
		$('#Schedule-addRequest-section .date-input')
			.datepicker()
			;
		$("#Schedule-addRequest-section .combobox.time-input")
			.combobox()
			;
		$("#Schedule-addRequest-section .combobox.absence-input")
			.combobox()
			;
	}

	function _addTextRequest() {
		var formData = _getFormData();
		$.myTimeAjax({
			url: "Requests/TextRequest",
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: "POST",
			cache: false,
			data: JSON.stringify(formData),
			success: function (data, textStatus, jqXHR) {
				_displayTextRequest(formData.Period.StartDate);
				$('#Schedule-addRequest-section').parents(".qtip").hide();
				_clearFormData();
				_clearValidationError();
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var data = $.parseJSON(jqXHR.responseText);
					_displayValidationError(data);
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}

	function _showAbsenceTypes() {
		$('#Absence-type-element').show();
		$('#Absence-request-tab').css("color", "#15C");
		$('#Text-request-tab').css("color", "#000");
	}

	function _hideAbsenceTypes() {
		$('#Absence-type-element').hide();
		$('#Text-request-tab').css("color", "#15C");
		$('#Absence-request-tab').css("color", "#000");
	}

	function _displayValidationError(data) {
		var message = data.Errors.join(' ');
		$('#Schedule-addRequest-error').html(message || '');
	}

	function _clearValidationError() {
		$('#Schedule-addRequest-error').html('');
	}

	function _getFormData() {
		return {
			Subject: $('#Schedule-addRequest-subject-input').val(),
			Period: {
				StartDate: $('#Schedule-addRequest-fromDate-input').val(),
				StartTime: $('#Schedule-addRequest-fromTime-input-input').val(),
				EndDate: $('#Schedule-addRequest-toDate-input').val(),
				EndTime: $('#Schedule-addRequest-toTime-input-input').val()
			},
			Message: $('#Schedule-addRequest-message-input').val()
		};
	}

	function _clearFormData() {
		$('#Schedule-addRequest-section input, #Schedule-addRequest-section textarea, #Schedule-addRequest-section select')
			.not(':button, :submit, :reset')
			.reset()
			;
	}

	function _displayTextRequest(inputDate) {
		//		var date = Date.parse(inputDate);
		var date = $.datepicker.parseDate($.datepicker._defaults.dateFormat, inputDate);
		var formattedDate = $.datepicker.formatDate('yy-mm-dd', date);
		var textRequestCount = $('ul[data-mytime-date="' + formattedDate + '"] .text-request');
		//		var newTitle = textRequestCount.attr('title').replace(new RegExp('[0-9]|[1-9][0-9]'), parseInt(textRequestCount.text()) + 1);
		var title = textRequestCount.attr('title');
		if (title == undefined)
			return;
		var newTitle = title.replace(/[\d\.]+/g, parseInt(textRequestCount.text()) + 1);
		textRequestCount.attr('title', newTitle);
		textRequestCount
			.show()
			.children()
			.first()
			.text(parseInt(textRequestCount.text()) + 1)
			;
	}

	return {
		Init: function () {

		},
		PartialInit: function () {
			_initEditSection();
		}
	};

})(jQuery);

Teleopti.MyTimeWeb.Schedule.Layout = (function ($) {

	function _setDayState(curDay) {
		switch ($(curDay).data('mytime-state')) {
			case 1:
				break;
			case 2:
				$(curDay).addClass('today');
				break;
			case 3:
				$(curDay).addClass('editable');
				break;
			case 4:
				$(curDay).addClass('non-editable');
				break;
		}
	}

	return {
		SetSchemaItemsHeights: function () {
			var currentTallest = 0; // Tallest li per row
			var currentLength = 0;  // max amount of li's
			var currentHeight = 0;  // max height of ul
			var i = 0;
			$('.weekview-day').each(function () {
				if ($('li', this).length > currentLength) {
					currentLength = $('li', this).length;
				}

				_setDayState($(this));
			});
			for (i = 3; i <= currentLength; i++) {
				var currentLiRow = $('.weekview-day li:nth-child(' + i + ')');
				$(currentLiRow).each(function () {
					if ($(this).height() > currentTallest) {
						currentTallest = $(this).height();
					}

				});
				$('>div', $(currentLiRow)).css({ 'min-height': currentTallest - 20 }); // remove padding from height
				currentTallest = 0;
			}

			$('.weekview-day').each(function () {
				if ($(this).height() > currentHeight) {
					currentHeight = $(this).height();
				}
			});

			$('.weekview-day li:last-child').each(function () {
				var ulHeight = $(this).parent().height();
				var incBorders = (currentLength * 6);
				$(this).height((currentHeight - ulHeight) + incBorders);
			});
		}
	};

})(jQuery);

$(function () { Teleopti.MyTimeWeb.Schedule.Init(); });

