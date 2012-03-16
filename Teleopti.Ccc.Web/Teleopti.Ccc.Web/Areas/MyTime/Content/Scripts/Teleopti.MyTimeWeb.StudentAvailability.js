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

Teleopti.MyTimeWeb.StudentAvailability = (function ($) {


	function _setEditError(message) {
		$('#StudentAvailability-edit-error').html(message || '');
	}

	function _layout() {
		Teleopti.MyTimeWeb.StudentAvailability.Layout.SetClassesFromDayState();
	}

	function _initPeriodSelection() {
		var rangeSelectorId = '#StudentAvailabilityDateRangeSelector';
		var periodData = $('#StudentAvailability-body').data('mytime-periodselection');
		Teleopti.MyTimeWeb.Portal.InitPeriodSelection(rangeSelectorId, periodData);
	}

	function _disableToolbarButtons() {
		_updateButtonState($('#StudentAvailability-edit-button'), false);
		_updateButtonState($('#StudentAvailability-delete-button'), false);
	}

	function _enableToolbarButtons(selectedDay) {
		var editable = selectedDay.hasClass('editable');
		var deletable = selectedDay.hasClass('deletable');
		_updateButtonState($('#StudentAvailability-edit-button'), editable);
		_updateButtonState($('#StudentAvailability-delete-button'), deletable);
	}

	function _updateButtonState(button, enabled) {
		if (enabled)
			button.removeAttr('disabled');
		else
			button.attr('disabled', 'disabled').removeClass('ajax-disabled');
	}

	function _xhr(type, successCallback, addressSuffix, reqData) {
		$.myTimeAjax({
				url: "StudentAvailability/StudentAvailability" + addressSuffix,
				type: type,
				data: reqData,
				success: successCallback,
				statusCode404: function() { }
			});
	}

	function _updateDayAndCloseEditSection(data) {
		// {"Errors":["The Date field is required."]}
		if (typeof data.Errors != 'undefined') {
			_setEditError(data.Errors.join(' '));
			return;
		}
		_markDayAsUpdated(data);
		Teleopti.MyTimeWeb.Common.CloseEditSection("#StudentAvailability-edit-section");
	}

	function _markDayAsUpdated(data) {
		var calendarDay = $('li[data-mytime-date="' + data.Date + '"]');
		if (data.AvailableTimeSpan == null) { // was deleted
			_handleDaySelected(calendarDay);
			_makeNotDeletable(calendarDay);
			_enableToolbarButtons(calendarDay);
		} else {
			calendarDay.addClass('deletable');
		}
		calendarDay.find('div.day-content').html('<span class="fullwidth displayblock mt15"></span>').find('span').text(data.AvailableTimeSpan);
		calendarDay.addClass('unvalidated');
	};

	function _bindDataToForm(data) {
		$('#StudentAvailability-edit-starttime').combobox('set', data.StartTime);
		$('#StudentAvailability-edit-endtime').combobox('set', data.EndTime);
		var nextDayCb = $('#StudentAvailability-edit-nextday-cb');
		if (data.NextDay) {
			nextDayCb.attr('checked', 'checked');
		} else {
			nextDayCb.removeAttr('checked');
		}
		nextDayCb.trigger('updateState');
	}

	function _extractDataFromForm(date) {
		// { Date:'2011-09-23', StartTime: '10:00', EndTime: '20:00', NextDay: false }
		var startTime = $('#StudentAvailability-edit-starttime-input').val();
		var endTime = $('#StudentAvailability-edit-endtime-input').val();
		var nextDay = $('#StudentAvailability-edit-nextday-cb').is(':checked');
		return { Date: date,
			StartTime: startTime,
			EndTime: endTime,
			NextDay: nextDay
		}; // Change attr to prop, jquery >= 1.6
	}

	function _initToolbarButtons() {
		var editButton = $('#StudentAvailability-edit-button');
		editButton.click(function () {
			_xhr('GET', function (data) {
				_bindDataToForm(data);
			},
				Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl($('#StudentAvailability-edit-section').data('mytime-selected-date')),
				null);
			var calendarDay = $('li[data-mytime-date="' + $('#StudentAvailability-edit-section').data('mytime-selected-date') + '"]');
			_enableToolbarButtons(calendarDay);
			_setEditError();
			Teleopti.MyTimeWeb.Common.OpenEditSection('#StudentAvailability-edit-section');
		});

		var deleteButton = $('#StudentAvailability-delete-button');
		deleteButton.click(function (event) {
			_xhr('DELETE',
				_updateDayAndCloseEditSection,
				Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl($('#StudentAvailability-edit-section').data('mytime-selected-date')),
				null);
		});

	}

	function _initEditSection() {
		$(".combobox").combobox();
		Teleopti.MyTimeWeb.Common.Layout.ActivateCustomInput();
		Teleopti.MyTimeWeb.Common.Layout.ActivateStdButtons();
		var submitButton = $('#StudentAvailability-edit-ok-button');
		submitButton.click(function () {
			_xhr('POST',
				_updateDayAndCloseEditSection,
				'',
				_extractDataFromForm($('#StudentAvailability-edit-section').data('mytime-selected-date')));
		});
		_initLabels();
	}

	function _initLabels() {
		$('#StudentAvailability-edit-section input[type=text]')
			.labeledinput()
			;
	}


	function _handleDaysSelected(dates) {
		if (dates.length !== 1) {
			_disableToolbarButtons();
			return;
		}
		var cell = $('li[data-mytime-date=' + dates[0] + ']');
		_handleDaySelected(cell);
	}

	function _handleDaySelected(day) {
		var date = day.data('mytime-date');
		_enableToolbarButtons(day);
		$('#StudentAvailability-edit-section').data('mytime-selected-date', date);
	}

	function _makeNotDeletable(day) {
		day.removeClass('deletable');
		Teleopti.MyTimeWeb.StudentAvailability.Layout.RemoveDeletableState(day);
	}

	function _activateSelectable() {
		$('#StudentAvailability-body-inner').calendarselectable({
			datesChanged: function (event, data) {
				_handleDaysSelected(data.dates);
			}
		});

	}
	return {
		Init: function () {
			_layout();
			_initToolbarButtons();
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('StudentAvailability/Index', Teleopti.MyTimeWeb.StudentAvailability.StudentAvailabilityPartialInit);
		},
		StudentAvailabilityPartialInit: function () {
			_layout();
			_initPeriodSelection();
			_disableToolbarButtons();
			_initEditSection();
			_activateSelectable();
		}
	};

})(jQuery);

$(function () { Teleopti.MyTimeWeb.StudentAvailability.Init(); });

Teleopti.MyTimeWeb.StudentAvailability.Layout = (function ($) {

	function _setDayState(week) {
		$('li[data-mytime-date]', week).each(function () {
			var curDay = $(this);
			var state = parseInt(curDay.data('mytime-state'));
			if (!state) {
				curDay.addClass('non-editable');
				return;
			}
			if (state & 1) {
				curDay.addClass('editable');
			}
			if (state & 2) {
				curDay.addClass('deletable');
			}
		});
	}

	function _removeState(day, stateToRemove) {
		var currentState = parseInt(day.data('mytime-state'));
		var newState = currentState ^ stateToRemove;
		day.data('mytime-state', newState);
	}

	return {
		SetClassesFromDayState: function () {
			var weeks = $('.calendarview-week');
			weeks.each(function () {
				_setDayState($(this));
			});
		}
		,
		RemoveDeletableState: function (day) {
			_removeState(day, 2);
		}
	};
})(jQuery);
