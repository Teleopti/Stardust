/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.1.custom.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Portal.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.StudentAvailability.DayViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.StudentAvailability.EditFormViewModel.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.StudentAvailability = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var dayViewModels = {};
	var studentAvailabilityToolTip = null;
	var editFormViewModel = null;

	function _initPeriodSelection() {
		var rangeSelectorId = '#AvailabilityDateRangeSelector';
		var periodData = $('#StudentAvailability-body').data('mytime-periodselection');
		Teleopti.MyTimeWeb.Portal.InitPeriodSelection(rangeSelectorId, periodData);
	}
    
	function _ajaxForDate(model, options) {
	    var type = options.type || 'GET',
		    data = options.data || {},
		    statusCode400 = options.statusCode400,
		    statusCode404 = options.statusCode404,
		    url = options.url || "Availability/StudentAvailability",
		    success = options.success || function () { },
		    complete = options.complete || null;

	    return ajax.Ajax({
	        url: url,
	        dataType: "json",
	        contentType: "application/json; charset=utf-8",
	        type: type,
	        beforeSend: function (jqXHR) {
	            model.AjaxError('');
	            model.IsLoading(true);
	        },
	        complete: function (jqXHR, textStatus) {
	            model.IsLoading(false);
	            if (complete)
	                complete(jqXHR, textStatus);
	        },
	        success: success,
	        data: data,
	        statusCode404: statusCode404,
	        statusCode400: statusCode400,
	        error: function (jqXHR, textStatus, errorThrown) {
	            var error = {
	                ShortMessage: "Error!"
	            };
	            try {
	                error = $.parseJSON(jqXHR.responseText);
	            } catch (e) {
	            }
	            model.AjaxError(error.ShortMessage);
	        }
	    });
	};
    
	function _initViewModels() {
		dayViewModels = {};
		$('li[data-mytime-date]').each(function (index, element) {
			var dayViewModel = new Teleopti.MyTimeWeb.StudentAvailability.DayViewModel(_ajaxForDate);
			dayViewModel.ReadElement(element);
			dayViewModels[dayViewModel.Date] = dayViewModel;
			ko.applyBindings(dayViewModel, element);
		});

		var from = $('li[data-mytime-date]').first().data('mytime-date');
		var to = $('li[data-mytime-date]').last().data('mytime-date');

		_loadStudentAvailabilityAndSchedules(from, to);
	}

	function _loadStudentAvailabilityAndSchedules(from, to) {
		var deferred = $.Deferred();
		ajax.Ajax({
			url: "Availability/StudentAvailabilitiesAndSchedules",
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

	function _initToolbarButtons() {
		var editButton = $('#Availability-edit-button');
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

		var deleteButton = $('#Availability-delete-button');
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
				var promise = dayViewModels[date].DeleteStudentAvailability();
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

		$('#StudentAvailability-body-inner .ui-selected')
			.each(function (index, cell) {
				var date = $(cell).data('mytime-date');
				studentAvailability.Date = date;
				var promise = dayViewModels[date].SetStudentAvailability(studentAvailability, editFormViewModel);
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

	function _cleanBindings() {
	    $('li[data-mytime-date]').each(function (index, element) {
	        ko.cleanNode(element);
	    });

	    dayViewModels = {};
	}

	return {
		Init: function () {
			_initToolbarButtons();
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack(
				'Availability/Index',
				Teleopti.MyTimeWeb.StudentAvailability.StudentAvailabilityPartialInit,
				Teleopti.MyTimeWeb.StudentAvailability.StudentAvailabilityPartialDispose
			);
		},
		StudentAvailabilityPartialInit: function () {
			if (!$('#StudentAvailability-body').length) {
				return;
			}
			_initPeriodSelection();
			_initViewModels();
			_activateSelectable();
		},
		StudentAvailabilityPartialDispose: function () {
			studentAvailabilityToolTip.qtip('toggle', false);
			ajax.AbortAll();
		    _cleanBindings();
		}
	};

})(jQuery);