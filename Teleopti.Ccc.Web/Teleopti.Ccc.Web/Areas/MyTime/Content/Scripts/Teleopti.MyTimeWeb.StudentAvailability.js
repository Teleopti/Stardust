﻿/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Portal.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.StudentAvailability.EditFormViewModel.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.StudentAvailability = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var dayViewModels = [];
	var studentAvailabilityToolTip = null;
	var editFormViewModel = null;

	function _layout() {
		Teleopti.MyTimeWeb.StudentAvailability.Layout.SetClassesFromDayState();
	}

	function _initPeriodSelection() {
		var rangeSelectorId = '#StudentAvailabilityDateRangeSelector';
		var periodData = $('#StudentAvailability-body').data('mytime-periodselection');
		Teleopti.MyTimeWeb.Portal.InitPeriodSelection(rangeSelectorId, periodData);
	}

	function _initViewModels() {
		dayViewModels = [];
		$('li[data-mytime-date].editable').each(function (index, element) {
			var dayViewModel = new Teleopti.MyTimeWeb.StudentAvailability.DayViewModel(ajax);
			dayViewModel.ReadElement(element);
			dayViewModels[dayViewModel.Date] = dayViewModel;
			ko.applyBindings(dayViewModel, element);
		});

		var from = $('li[data-mytime-date].editable').first().data('mytime-date');
		var to = $('li[data-mytime-date].editable').last().data('mytime-date');

		_loadStudentAvailabilityAndSchedules(from, to);
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

	function _loadStudentAvailabilityAndSchedules(from, to) {
		var deferred = $.Deferred();
		ajax.Ajax({
			url: "StudentAvailability/StudentAvailabilitiesAndSchedules",
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			type: 'GET',
			data: {
				From: from,
				To: to
			},
			beforeSend: function (jqXHR) {
				$.each(dayViewModels, function (index, day) {
					day.IsLoading(true);
				});
			},
			success: function (data, textStatus, jqXHR) {
				data = data || [];
				$.each(data, function (index, element) {
					var dayViewModel = dayViewModels[element.Date];
					if (element.StudentAvailability)
						dayViewModel.ReadStudentAvailability(element.StudentAvailability);
					dayViewModel.IsLoading(false);
				});
				deferred.resolve();
			}
		});
		return deferred.promise();
	};

	function _updateDay(data) {
		// {"Errors":["The Date field is required."]}
		if (typeof data.Errors != 'undefined') {
			editFormViewModel.ValidationError(data.Errors.join(' ') || '');
			return;
		}
		dayViewModels[data.Date].AvailableTimeSpan(data.AvailableTimeSpan);
	}

	function _initToolbarButtons() {
		var editButton = $('#StudentAvailability-edit-button');
		var template = $('#Student-availability-edit-form');

		editFormViewModel = new Teleopti.MyTimeWeb.StudentAvailability.EditFormViewModel();

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
								editFormViewModel.reset();
							});
						$('#Student-availability-apply')
							.button()
							.click(function () {
								_setStudentAvailability(ko.toJS(editFormViewModel));
							});
						ko.applyBindings(editFormViewModel, template[0]);
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

		editFormViewModel.ValidationError('');

		var validationErrorCallback = function (data) {
			var message = data.Errors.join('</br>');
			editFormViewModel.ValidationError(message);
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
			_initViewModels();
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
		});
	}

	return {
		SetClassesFromDayState: function () {
			var weeks = $('.calendarview-week');
			weeks.each(function () {
				_setDayState($(this));
			});
		}

	};
})(jQuery);
