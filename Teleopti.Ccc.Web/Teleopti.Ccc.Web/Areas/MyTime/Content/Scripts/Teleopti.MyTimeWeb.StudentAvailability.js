/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
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
	var editFormViewModel = null;
    var vm = null;

    var selectionViewModel = function() {
        var self = this;
        
        self.minDate = ko.observable(moment());
        self.maxDate = ko.observable(moment());

        self.displayDate = ko.observable();
        self.nextPeriodDate = ko.observable(moment());
        self.previousPeriodDate = ko.observable(moment());

        self.selectedDate = ko.observable(moment().startOf('day'));

        self.setCurrentDate = function (date) {
            self.selectedDate(date);
            self.selectedDate.subscribe(function (d) {
                Teleopti.MyTimeWeb.Portal.NavigateTo("Availability/Index" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD')));
            });
        };

        self.nextPeriod = function () {
            self.selectedDate(self.nextPeriodDate());
        };

        self.previousPeriod = function () {
            self.selectedDate(self.previousPeriodDate());
        };
    };

	function _initPeriodSelection() {
	    var periodData = $('#StudentAvailability-body').data('mytime-periodselection');
	    vm = new selectionViewModel();
	    vm.displayDate(periodData.Display);
	    vm.nextPeriodDate(moment(periodData.PeriodNavigation.NextPeriod));
	    vm.previousPeriodDate(moment(periodData.PeriodNavigation.PrevPeriod));
	    vm.setCurrentDate(moment(periodData.Date));
	    
		// use Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function(data) {  instead, when SA bindings doesnt cause exceptions
	    ajax.Ajax({
	    	url: 'UserInfo/Culture',
	    	dataType: "json",
	    	type: 'GET',
	    	success: function (data) {
	    		$('.moment-datepicker').attr('data-bind', 'datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: ' + data.WeekStart + ' }');
	    		ko.applyBindings(vm, $('div.subnavbar')[0]);
	    		ko.applyBindings(vm, $('div.hidden-sm')[0]);
	    	}
	    });
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
		var showMeridian = $('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true';

		editFormViewModel = new Teleopti.MyTimeWeb.StudentAvailability.EditFormViewModel(ajax, showMeridian);

		editButton.click(function (e) {
		    editFormViewModel.ToggleAddAvailabilityFormVisible();
		     e.preventDefault();
		});

		ko.applyBindings(editFormViewModel, template[0]);

		editButton.removeAttr('disabled');

		var deleteButton = $('#Availability-delete-button');
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

	function _cleanBindings() {
	    $('li[data-mytime-date]').each(function (index, element) {
	        ko.cleanNode(element);
	    });

	    dayViewModels = {};
	}

	return {
		Init: function () {
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
			_initToolbarButtons();
			_initViewModels();
			_activateSelectable();
		},
		StudentAvailabilityPartialDispose: function () {
			//studentAvailabilityToolTip.qtip('toggle', false);
			ajax.AbortAll();
			_cleanBindings();
		},
		SetStudentAvailability: function(studentAvailabilityViewModel) {
		    _setStudentAvailability(ko.toJS(studentAvailabilityViewModel));
		}
	};

})(jQuery);