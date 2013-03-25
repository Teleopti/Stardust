/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
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
		
	}

	function _initViewModels() {
		dayViewModels = {};
		$('li[data-mytime-date]').each(function (index, element) {
			var dayViewModel = new Teleopti.MyTimeWeb.StudentAvailability.DayViewModel(ajax);
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

	function _initToolbarButtons() {
		var editButton = $('#StudentAvailability-edit-button');
		var template = $('#Student-availability-edit-form');

		editFormViewModel = new Teleopti.MyTimeWeb.StudentAvailability.EditFormViewModel();

		editButton.click(function (e) { e.preventDefault(); });
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
	                event: 'mousedown'
	            },
	            hide: {
	                target: editButton,
	                event: 'mousedown'
	            },
	            style: {
	                def: false,
	                classes: 'ui-tooltip-custom ui-tooltip-rounded ui-tooltip-shadow',
	                tip: {
	                    corner: "top left"
	                }
	            },
	            events: {
	                render: function() {
	                    ko.applyBindings(editFormViewModel, template[0]);
	                }
	            }
	        });
		editButton.removeAttr('disabled');

		var deleteButton = $('#StudentAvailability-delete-button');
		deleteButton.removeAttr('disabled');
		deleteButton.click(function (e) {
		    e.preventDefault();
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

	return {
		Init: function () {
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack(
				'StudentAvailability/Index',
				Teleopti.MyTimeWeb.StudentAvailability.StudentAvailabilityPartialInit,
				Teleopti.MyTimeWeb.StudentAvailability.StudentAvailabilityPartialDispose
			);
		},
		StudentAvailabilityPartialInit: function () {
			if (!$('#StudentAvailability-body').length) {
				return;
			}
			_initToolbarButtons();
			_initPeriodSelection();
			_initViewModels();
			_activateSelectable();
		},
		StudentAvailabilityPartialDispose: function () {
			studentAvailabilityToolTip.qtip('toggle', false);
			ajax.AbortAll();
		},
		SetStudentAvailability: function(studentAvailabilityViewModel) {
		    _setStudentAvailability(ko.toJS(studentAvailabilityViewModel));
		}
	};

})(jQuery);