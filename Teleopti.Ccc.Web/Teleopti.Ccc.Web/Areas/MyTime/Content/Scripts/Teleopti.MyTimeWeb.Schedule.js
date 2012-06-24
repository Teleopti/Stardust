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

	function _initTooltip() {
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
					var date = $(event.originalEvent.target).closest('ul').attr('data-request-default-date');
					Teleopti.MyTimeWeb.Schedule.TextRequest.ClearFormData(date);
				}
			},
			show: {
				target: addTextRequest,
				event: 'click'
			},
			hide: {
				target: $(document.body).children().not('#ui-datepicker-div').not($(self)),
				event: 'mousedown'

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
			_initTooltip();
			Teleopti.MyTimeWeb.Schedule.Layout.SetSchemaItemsHeights();
			_initPeriodSelection();
			Teleopti.MyTimeWeb.Common.Layout.ActivateCustomInput();
			Teleopti.MyTimeWeb.Common.Layout.ActivateStdButtons();
			Teleopti.MyTimeWeb.Schedule.TextRequest.PartialInit();
		}
	};

})(jQuery);

Teleopti.MyTimeWeb.Schedule.RequestViewModel = (function RequestViewModel() {
	var self = this;
	this.IsFullDay = ko.observable(false);

	ko.computed(function () {
		if (self.IsFullDay()) {
			$('#Schedule-addRequest-fromTime-input-input').val($('#Schedule-addRequest-default-start-time').text());
			$('#Schedule-addRequest-toTime-input-input').val($('#Schedule-addRequest-default-end-time').text());
			_disableTimeinput();
		} else {
			$('#Schedule-addRequest-fromTime-input-input').reset();
			$('#Schedule-addRequest-toTime-input-input').reset();
			_enableTimeinput();
		}
		
	});

	function _enableTimeinput() {
		$('#Schedule-addRequest-fromTime button, #Schedule-addRequest-fromTime-input-input, #Schedule-addRequest-toTime button, #Schedule-addRequest-toTime-input-input')
			.removeAttr("disabled");
		$('#Schedule-addRequest-fromTime-input-input').css("color", "black");
		$('#Schedule-addRequest-toTime-input-input').css("color", "black");
	}

	function _disableTimeinput() {
		$('#Schedule-addRequest-fromTime button, #Schedule-addRequest-fromTime-input-input, #Schedule-addRequest-toTime button, #Schedule-addRequest-toTime-input-input')
			.attr("disabled", "disabled");
		$('#Schedule-addRequest-fromTime-input-input').css("color", "grey");
		$('#Schedule-addRequest-toTime-input-input').css("color", "grey");
	}
});

Teleopti.MyTimeWeb.Schedule.TextRequest = (function ($) {

	var requestViewModel = null;

	function _initEditSection() {
		_initButtons();
		_initControls();
		_initLabels();
	}

	function _initLabels() {
		$('#Schedule-addRequest-section input[type=text], #Schedule-addRequest-section textarea')
			.labeledinput()
			;
	}

	function _initButtons() {
		$('#Schedule-addRequest-ok-button')
			.click(function () {
				if ($('#Text-request-tab.selected-tab').length > 0) {
					_addRequest("Requests/TextRequest");
				} else {
					_addRequest("Requests/AbsenceRequest");
				}

			});

		$('#Text-request-tab')
			.click(function () {
				_clearValidationError();
				_hideAbsenceTypes();
				requestViewModel.IsFullDay(false);
			});
		$('#Absence-request-tab')
			.click(function () {
				_clearValidationError();
				_showAbsenceTypes();
				requestViewModel.IsFullDay(true);
			});
	}

	function _initControls() {
		$('#Schedule-addRequest-section .date-input')
			.datepicker()
			;
		$("#Schedule-addRequest-section .combobox.time-input").combobox();
		$("#Schedule-addRequest-section .combobox.absence-input").combobox();
		
		$("#Absence-type-input").attr('readonly', 'true');

		requestViewModel = new Teleopti.MyTimeWeb.Schedule.RequestViewModel();
		ko.applyBindings(requestViewModel, $('#Fullday-check')[0]);
	}

	function _addRequest(requestUrl) {
		var formData = _getFormData();
		Teleopti.MyTimeWeb.Ajax.Ajax({
			url: requestUrl,
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: "POST",
			cache: false,
			data: JSON.stringify(formData),
			success: function (data, textStatus, jqXHR) {
				_displayRequest(formData.Period.StartDate);
				$('#Schedule-addRequest-section').parents(".qtip").hide();
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
		$('#Absence-request-tab').addClass("selected-tab");
		$('#Text-request-tab').removeClass("selected-tab");
	}

	function _hideAbsenceTypes() {
		$('#Absence-type-element').hide();
		$('#Text-request-tab').addClass("selected-tab");
		$('#Absence-request-tab').removeClass("selected-tab");
	}

	function _displayValidationError(data) {
		var message = data.Errors.join('</br>');
		$('#Schedule-addRequest-error').html(message || '');
	}

	function _clearValidationError() {
		$('#Schedule-addRequest-error').html('');
	}

	function _getFormData() {
		var absenceId = $('#Absence-type').children(":selected").val();
		if (absenceId == undefined) {
			absenceId = null;
		}
		return {
			Subject: $('#Schedule-addRequest-subject-input').val(),
			AbsenceId: absenceId,
			Period: {
				StartDate: $('#Schedule-addRequest-fromDate-input').val(),
				StartTime: $('#Schedule-addRequest-fromTime-input-input').val(),
				EndDate: $('#Schedule-addRequest-toDate-input').val(),
				EndTime: $('#Schedule-addRequest-toTime-input-input').val()
			},
			Message: $('#Schedule-addRequest-message-input').val()
		};
	}

	function _clearFormData(date) {
		$('#Schedule-addRequest-section input, #Schedule-addRequest-section textarea, #Schedule-addRequest-section select')
			.not(':button, :submit, :reset')
			.reset()
			;
		$('#Absence-type').prop('selectedIndex', 0);
		$('#Schedule-addRequest-fromDate-input').val(date);
		$('#Schedule-addRequest-toDate-input').val(date);
		$('#Text-request-tab').click();
	}

	function _displayRequest(inputDate) {
		var date = $.datepicker.parseDate($.datepicker._defaults.dateFormat, inputDate);
		var formattedDate = $.datepicker.formatDate('yy-mm-dd', date);
		var textRequestCount = $('ul[data-mytime-date="' + formattedDate + '"] .text-request');
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
		},

		ClearFormData: function (date) {
			_clearFormData(date);
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
