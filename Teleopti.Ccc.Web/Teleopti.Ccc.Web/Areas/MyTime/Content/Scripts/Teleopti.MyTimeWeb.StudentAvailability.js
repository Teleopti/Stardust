Teleopti.MyTimeWeb.StudentAvailability = (function($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax(),
		dayViewModels = {},
		editFormViewModel = null,
		vm = null,
		periodFeedbackVM = null,
		completelyLoaded,
		isHostAMobile = Teleopti.MyTimeWeb.Common.IsHostAMobile(),
		isHostAniPad = Teleopti.MyTimeWeb.Common.IsHostAniPad();

	var selectedDateSelectorStr =
		isHostAMobile || isHostAniPad
			? '#StudentAvailability-body-inner .date-is-selected'
			: '#StudentAvailability-body-inner .ui-selected';

	function SelectionViewModel() {
		var self = this;

		self.minDate = ko.observable(moment());
		self.maxDate = ko.observable(moment());
		self.IsHostAMobile = Teleopti.MyTimeWeb.Common.IsHostAMobile();
		self.displayDate = ko.observable();
		self.nextPeriodDate = ko.observable(moment());
		self.previousPeriodDate = ko.observable(moment());

		self.selectedDate = ko.observable(moment().startOf('day'));

		self.setCurrentDate = function(date) {
			self.selectedDate(date);
			self.selectedDate.subscribe(function(d) {
				Teleopti.MyTimeWeb.Portal.NavigateTo(
					'Availability/Index' + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD'))
				);
			});
		};

		self.nextPeriod = function() {
			self.selectedDate(self.nextPeriodDate());
		};

		self.previousPeriod = function() {
			self.selectedDate(self.previousPeriodDate());
		};

		self.isInitFinished = ko.observable(false);
	}

	function _initPeriodSelection() {
		var periodData = $('#StudentAvailability-body').data('mytime-periodselection');
		vm = new SelectionViewModel();
		vm.displayDate(periodData.Display);
		vm.nextPeriodDate(moment(periodData.PeriodNavigation.NextPeriod));
		vm.previousPeriodDate(moment(periodData.PeriodNavigation.PrevPeriod));
		vm.setCurrentDate(moment(periodData.Date));

		// use Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function(data) {  instead, when SA bindings doesnt cause exceptions
		ajax.Ajax({
			url: 'UserInfo/Culture',
			dataType: 'json',
			type: 'GET',
			success: function(data) {
				$('.moment-datepicker').attr(
					'data-bind',
					'datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: ' +
						data.WeekStart +
						', format: "' +
						Teleopti.MyTimeWeb.Common.DateFormat +
						'" }'
				);
				ko.applyBindings(vm, $('.availability-toolbar')[0]);
			}
		});
	}

	function _ajaxForDate(model, options) {
		var type = options.type || 'GET',
			data = options.data || {},
			statusCode400 = options.statusCode400,
			statusCode404 = options.statusCode404,
			url = options.url || 'Availability/StudentAvailability',
			success = options.success || function() {},
			complete = options.complete || null;

		return ajax.Ajax({
			url: url,
			dataType: 'json',
			contentType: 'application/json; charset=utf-8',
			type: type,
			beforeSend: function(jqXHR) {
				model.AjaxError('');
				model.IsLoading(true);
			},
			complete: function(jqXHR, textStatus) {
				model.IsLoading(false);
				if (complete) complete(jqXHR, textStatus);
			},
			success: success,
			data: data,
			statusCode404: statusCode404,
			statusCode400: statusCode400,
			error: function(jqXHR, textStatus, errorThrown) {
				var error = {
					ShortMessage: 'Error!'
				};
				try {
					error = $.parseJSON(jqXHR.responseText);
				} catch (e) {}
				model.AjaxError(error.ShortMessage);
			}
		});
	}

	function _initViewModels() {
		dayViewModels = {};
		$('li[data-mytime-date]').each(function(index, element) {
			var dayViewModel = new Teleopti.MyTimeWeb.StudentAvailability.DayViewModel(_ajaxForDate);
			dayViewModel.ReadElement(element);
			dayViewModel.LoadFeedback();
			dayViewModels[dayViewModel.Date] = dayViewModel;
			ko.applyBindings(dayViewModel, element);
		});

		var from = $('li[data-mytime-date]')
			.first()
			.data('mytime-date');
		var to = $('li[data-mytime-date]')
			.last()
			.data('mytime-date');

		_loadStudentAvailabilityAndSchedules(from, to).then(function() {
			vm.isInitFinished(true);
		});

		var periodData = $('#StudentAvailability-body').data('mytime-periodselection');
		periodFeedbackVM = new Teleopti.MyTimeWeb.StudentAvailability.PeriodFeedbackViewModel(
			ajax,
			dayViewModels,
			periodData.Date
		);

		if (isHostAMobile) {
			function AlertViewModel(feedbackViewModel) {
				var self = this;
				self.StudentAvailabilityFeedbackClass = ko.computed(function () {
					return feedbackViewModel.StudentAvailabilityFeedbackClass();
				});

				self.WarningCount = ko.computed(function () {
					return feedbackViewModel.WarningCount();
				});

				self.toggleWarningDetail = function () {
					feedbackViewModel.toggleWarningDetail();
				};
				self.IsHostAMobile = true;
			}

			var alertViewModelBinding = new AlertViewModel(periodFeedbackVM);
			ko.applyBindings(alertViewModelBinding, $('.warning-indicator')[0]);
		}

		periodFeedbackVM.LoadFeedback(completelyLoaded);

		var template = $('#StudentAvailability-period');
		ko.applyBindings(periodFeedbackVM, template[0]);
	}

	function _loadStudentAvailabilityAndSchedules(from, to) {
		var deferred = $.Deferred();

		ajax.Ajax({
			url: 'Availability/StudentAvailabilitiesAndSchedules',
			dataType: 'json',
			contentType: 'application/json; charset=utf-8',
			type: 'GET',
			data: {
				From: from,
				To: to
			},
			beforeSend: function(jqXHR) {
				$.each(dayViewModels, function(index, day) {
					day.IsLoading(true);
				});
			},
			success: function(data, textStatus, jqXHR) {
				data = data || [];
				$.each(data, function(index, element) {
					var dayViewModel = dayViewModels[element.Date];
					dayViewModel.ReadStudentAvailability(element);
					dayViewModel.IsLoading(false);
				});
				deferred.resolve();
			}
		});
		return deferred.promise();
	}

	function _initToolbarButtons() {
		var editButton = $('#Availability-edit-button');
		var template = $('#Student-availability-edit-form');
		var showMeridian = $('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true';

		editFormViewModel = new Teleopti.MyTimeWeb.StudentAvailability.EditFormViewModel(ajax, showMeridian);

		editButton.click(function(e) {
			if (_isAnyDateSelected()) {
				editFormViewModel.ValidationError('');
			}
			editFormViewModel.ToggleAddAvailabilityFormVisible();
			e.preventDefault();
		});

		ko.applyBindings(editFormViewModel, template[0]);

		editButton.removeAttr('disabled');

		var deleteButton = $('#Availability-delete-button');
		deleteButton.removeAttr('disabled');
		deleteButton.click(function(e) {
			e.preventDefault();
			_deleteStudentAvailability();
		});
	}

	function _deleteStudentAvailability() {
		var promises = [];
		$(selectedDateSelectorStr).each(function(index, cell) {
			var date = $(cell).data('mytime-date');
			var promise = dayViewModels[date].DeleteStudentAvailability();
			promises.push(promise);
		});
		if (promises.length != 0) {
			$.when.apply(null, promises).done(function() {
				_clearSelectedDates();
			});
		}
	}

	function _setStudentAvailability(studentAvailabilityViewModel) {
		var promises = [];

		editFormViewModel.ValidationError('');

		var applyButton = $('#Student-availability-apply');

		if (!_isAnyDateSelected()) {
			editFormViewModel.ValidationError(applyButton.attr('data-DateErrorMessage'));
			return false;
		}

		if (!editFormViewModel.StartTime()) {
			editFormViewModel.ValidationError(applyButton.attr('data-StartTimeErrorMessage'));
			return false;
		} else if (!editFormViewModel.EndTime()) {
			editFormViewModel.ValidationError(applyButton.attr('data-EndTimeErrorMessage'));
			return false;
		}

		if (editFormViewModel.IsPostingData()) return false;
		editFormViewModel.IsPostingData(true);

		$(selectedDateSelectorStr).each(function(index, cell) {
			var date = $(cell).data('mytime-date');
			studentAvailabilityViewModel.Date = date;
			var promise = dayViewModels[date].SetStudentAvailability(studentAvailabilityViewModel, editFormViewModel);
			promises.push(promise);
		});
		$.when.apply(null, promises).done(function() {
			if (!editFormViewModel.ShowError() && studentAvailabilityViewModel.IsHostAMobile)
				studentAvailabilityViewModel.ToggleAddAvailabilityFormVisible();
			_clearSelectedDates();
			editFormViewModel.IsPostingData(false);
		});
	}

	function _isAnyDateSelected() {
		if (isHostAMobile || isHostAniPad) return $('#StudentAvailability-body-inner .date-is-selected').length > 0;
		else return $('#StudentAvailability-body-inner .ui-selected').length > 0;
	}

	function _activateSelectable() {
		if (isHostAMobile || isHostAniPad) {
			$('#StudentAvailability-body-inner li[data-mytime-editable="True"]').on('click', function() {
				if (!$(this).hasClass('date-is-selected')) $(this).addClass('date-is-selected');
				else $(this).removeClass('date-is-selected');
			});
		} else {
			$('#StudentAvailability-body-inner').calendarselectable();
		}
	}

	function _clearSelectedDates() {
		$('#StudentAvailability-body-inner .date-is-selected').removeClass('date-is-selected');
	}

	function _cleanBindings() {
		$('li[data-mytime-date]').each(function(index, element) {
			ko.cleanNode(element);
		});

		dayViewModels = {};
	}

	return {
		Init: function() {
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack(
				'Availability/Index',
				Teleopti.MyTimeWeb.StudentAvailability.StudentAvailabilityPartialInit,
				Teleopti.MyTimeWeb.StudentAvailability.StudentAvailabilityPartialDispose
			);
		},
		StudentAvailabilityPartialInit: function(readyForInteractionCallback, completelyLoadedCallback) {
			completelyLoaded = completelyLoadedCallback;
			if (!$('#StudentAvailability-body').length) {
				return;
			}
			_initPeriodSelection();
			_initToolbarButtons();
			_initViewModels();
			_activateSelectable();
		},
		StudentAvailabilityPartialDispose: function() {
			ajax.AbortAll();
			_cleanBindings();
		},
		SetStudentAvailability: function(studentAvailabilityViewModel) {
			_setStudentAvailability(ko.toJS(studentAvailabilityViewModel));
		}
	};
})(jQuery);
