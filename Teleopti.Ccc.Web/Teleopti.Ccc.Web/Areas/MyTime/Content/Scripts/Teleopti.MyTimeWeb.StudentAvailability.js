/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Portal.js" />
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

	function _xhr(type, successCallback, addressSuffix, reqData) {
		var deferred = $.Deferred();
		ajax.Ajax({
			url: "StudentAvailability/StudentAvailability" + addressSuffix,
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			type: type,
			data: JSON.stringify(reqData),
			success: successCallback,
			statusCode404: function () { },
			complete: function () {
				deferred.resolve();
			}
		});
		return deferred.promise();
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
			_makeNotDeletable(calendarDay);
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
		var promises = [];
		$('#StudentAvailability-body-inner .ui-selected')
			.each(function (index, cell) {
				var date = $(cell).data('mytime-date');
				//				var promise = preferencesAndScheduleViewModel.DayViewModels[date].SetPreference(preference, validationErrorCallback);
				//				promises.push(promise);
				var promise = _xhr('DELETE',
					_updateDay,
					Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date),
					null);
				promises.push(promise);
			});
		if (promises.length != 0) {
			$.when.apply(null, promises)
				.done(function () {

				});
		}
	}


	function _setStudentAvailability(studentAvailability) {
		var promises = [];

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
				var promise = _xhr('POST', _updateDay, '', studentAvailability);
				promises.push(promise);
			});
		if (promises.length != 0) {
			$.when.apply(null, promises)
				.done(function () {
					
				});
		}
	}

	function _makeNotDeletable(day) {
		day.removeClass('deletable');
		Teleopti.MyTimeWeb.StudentAvailability.Layout.RemoveDeletableState(day);
	}

	function _activateSelectable() {
		$('#StudentAvailability-body-inner').calendarselectable();

	}
	return {
		Init: function () {
			_layout();
			_initToolbarButtons();
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack(
				'StudentAvailability/Index',
				Teleopti.MyTimeWeb.StudentAvailability.StudentAvailabilityPartialInit,
				Teleopti.MyTimeWeb.StudentAvailability.StudentAvailabilityPartialDispose
			);
		},
		StudentAvailabilityPartialInit: function () {
			_layout();

			_initPeriodSelection();
			_activateSelectable();
		},
		StudentAvailabilityPartialDispose: function () {
			studentAvailabilityToolTip.qtip('toggle', false);
			ajax.AbortAll();
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
