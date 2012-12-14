/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.StudentAvailability.EditStudentAvailabilityFormViewModel.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.StudentAvailability = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var studentAvailabilityToolTip = null;
	var editStudentAvailabilityFormViewModel = null;

	function _setEditError(message) {
		editStudentAvailabilityFormViewModel.ValidationError(message || '');
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
		if (enabled) {
			button.click(function () {
				button.qtip('hide');
			});
		}
		else
			button.removeClass('ajax-disabled');
	}

	function _xhr(type, successCallback, addressSuffix, reqData) {
		ajax.Ajax({
			url: "StudentAvailability/StudentAvailability" + addressSuffix,
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			type: type,
			data: JSON.stringify(reqData),
			success: successCallback,
			statusCode404: function () { }
		});
	}

	function _updateDay(data) {
		// {"Errors":["The Date field is required."]}
		if (typeof data.Errors != 'undefined') {
			_setEditError(data.Errors.join(' '));
			return;
		}
		_markDayAsUpdated(data);
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
		var span = calendarDay.find('div.day-content').html('<span class="fullwidth displayblock mt15"></span>').find('span');
		if (data.AvailableTimeSpan != null)
			span.text(data.AvailableTimeSpan);

		//		calendarDay.addClass('unvalidated');
	};

	function _initToolbarButtons() {
		var editButton = $('#StudentAvailability-edit-button');
		var template = $('#Student-availability-edit-form');

		editStudentAvailabilityFormViewModel = new Teleopti.MyTimeWeb.StudentAvailability.EditStudentAvailabilityFormViewModel();

		studentAvailabilityToolTip = $('<div/>')
			.qtip({
				id: "edit-student-availability",
				content: {
					text: template,
					title: {
						text: '&nbsp;',
						button: 'Close'
					}
				},
				position: {
					target: editButton,
					my: "left top",
					at: "left bottom",
					adjust: {
						x: 11,
						y: 0
					}
				},
				show: {
					target: editButton,
					event: 'click'
				},
				hide: {
					target: editButton,
					event: 'click'
				},
				style: {
					def: false,
					classes: 'ui-tooltip-custom ui-tooltip-rounded ui-tooltip-shadow',
					tip: {
						corner: "top left"
					}
				},
				events: {
					render: function () {
						$('#Student-availability-reset')
							.button()
							.click(function () {
								editStudentAvailabilityFormViewModel.reset();
							});
						$('#Student-availability-apply')
							.button()
							.click(function () {
								_setStudentAvailability(ko.toJS(editStudentAvailabilityFormViewModel));
							});
						ko.applyBindings(editStudentAvailabilityFormViewModel, template[0]);
					}
				}
			});
		editButton.removeAttr('disabled');

		var deleteButton = $('#StudentAvailability-delete-button');
		deleteButton.removeAttr('disabled');
		deleteButton.click(function () {
			_deleteStudentAvailability();
		});
	}

	function _deleteStudentAvailability() {
		$('#StudentAvailability-body-inner .ui-selected')
			.each(function (index, cell) {
				var date = $(cell).data('mytime-date');
				//				var promise = preferencesAndScheduleViewModel.DayViewModels[date].SetPreference(preference, validationErrorCallback);
				//				promises.push(promise);
				_xhr('DELETE',
					_updateDay,
					Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date),
					null);
			});
	}


	function _setStudentAvailability(studentAvailability) {
		//		var promises = [];

		editStudentAvailabilityFormViewModel.ValidationError('');

		var validationErrorCallback = function (data) {
			var message = data.Errors.join('</br>');
			editStudentAvailabilityFormViewModel.ValidationError(message);
		};
		$('#StudentAvailability-body-inner .ui-selected')
			.each(function (index, cell) {
				var date = $(cell).data('mytime-date');
				studentAvailability.Date = date;
				//				var promise = preferencesAndScheduleViewModel.DayViewModels[date].SetPreference(preference, validationErrorCallback);
				//				promises.push(promise);
				_xhr('POST', _updateDay, '', studentAvailability);
			});
		//		if (promises.length != 0) {
		//			$.when.apply(null, promises)
		//				.done(function () { periodFeedbackViewModel.LoadFeedback(); });
		//		}
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
			_activateSelectable();
		}
	};

})(jQuery);

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
