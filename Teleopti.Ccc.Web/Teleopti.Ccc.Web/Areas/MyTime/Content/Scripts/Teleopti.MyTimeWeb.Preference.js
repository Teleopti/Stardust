/// <reference path="~/Content/Scripts/jquery-1.6.4-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.6.4.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Preference = (function ($) {

	var feedbackLoadingStarted = false;
	var periodFeedbackViewModel = null;
	var dayViewModels = {};

	function _initPeriodSelection() {
		var rangeSelectorId = '#PreferenceDateRangeSelector';
		var periodData = $('#Preference-body').data('mytime-periodselection');
		Teleopti.MyTimeWeb.Portal.InitPeriodSelection(rangeSelectorId, periodData);
	}

	function _initSplitButton() {
		$('#Preference-set-button')
			.parent()
			.splitbutton({
				clicked: function (event, item) {
					$('#Preference-body-inner .ui-selected')
						.each(function (index, cell) {
							var date = $(cell).data('mytime-date');
							_ajaxForDate({
								type: 'POST',
								data: {
									Date: date,
									Id: item.value
								},
								date: date,
								fillData: _fillPreference,
								complete: function () {
									dayViewModels[date].LoadFeedback();
									periodFeedbackViewModel.LoadFeedback();
								}
							});
						});
				}
			});
	}

	function _initDeleteButton() {
		$('#Preference-delete-button')
			.click(function () {
				$('#Preference-body-inner .ui-selected')
					.each(function (index, cell) {
						var date = $(cell).data('mytime-date');
						_ajaxForDate({
							type: 'DELETE',
							data: { Date: date },
							date: date,
							statusCode404: function () { },
							fillData: _fillPreference,
							complete: function () {
								dayViewModels[date].LoadFeedback();
								periodFeedbackViewModel.LoadFeedback();
							}
						});
					});
			})
			.removeAttr('disabled')
			;
	}

	function _fillPreference(cell, data) {
		$('li[data-mytime-date="' + data.Date + '"] .day-content').css("border-left-color", data.HexColor);

		var preference = $('.preference', cell);
		preference.text(data.PreferenceRestriction || "");
	}

	function _ajaxForDate(options) {

		var type = options.type || 'GET',
		    date = options.date || null, // required
		    data = options.data || {},
		    statusCode404 = options.statusCode404,
			url = options.url || "Preference/Preference",
		    fillData = options.fillData || function () { },
		    complete = options.complete || null
			;

		var cell = $('li[data-mytime-date="' + date + '"]');
		Teleopti.MyTimeWeb.Ajax.Ajax({
			url: url,
			dataType: "json",
			type: type,
			beforeSend: function (jqXHR) {

				var currentRequest = cell.data('request');
				if (currentRequest) {
					currentRequest.abort();
				}

				cell.data('request', jqXHR);

				$('#loading-small-gray-blue')
					.clone()
					.removeAttr('id')
					.removeClass('template')
					.addClass('loading-small-gray-blue')
					.addClass('temporary')
					.appendTo(cell)
					;

			},
			complete: function (jqXHR, textStatus) {

				cell.data('request', null);

				$('.temporary', cell)
					.remove();

				if (complete)
					complete(jqXHR, textStatus);
			},
			data: data,
			success: function (data, textStatus, jqXHR) {
				fillData(cell, data);
			},
			statusCode404: statusCode404,
			error: function (jqXHR, textStatus, errorThrown) {

				var cellHtml = $('<h2></h2>')
					.addClass('error');

				$('.preference', cell)
					.html(cellHtml);

				var error = "Error!";
				try {
					error = $.parseJSON(jqXHR.responseText);
				} catch (e) {
					cellHtml.append(error);
					return;
				}

				$('<a></a>')
					.append(error.ShortMessage + "!")
					.click(function () {
						errorInfo.toggle();
					})
					.appendTo(cellHtml)
					;
				var errorInfo = $('<div></div>')
					.hide()
					.width(300)
					.css({
						'position': 'absolute',
						'border': '1px solid #ccc',
						'background-color': 'white',
						'padding': '10px'
					})
					.append(error.Message)
					.insertAfter(cellHtml)
					;

			}
		});
	}

	function _activateSelectable() {
		$('#Preference-body-inner').calendarselectable();
	}

	function _initViewModels(feedbackLoader) {

		dayViewModels = {};
		$('li[data-mytime-date].inperiod').each(function (index, element) {
			var dayViewModel = new Teleopti.MyTimeWeb.Preference.DayViewModel();
			dayViewModel.ReadElement(element);
			dayViewModels[dayViewModel.Date] = dayViewModel;
			ko.applyBindings(dayViewModel, element);
		});

		var date = Teleopti.MyTimeWeb.Portal.CurrentFixedDate();
		periodFeedbackViewModel = new Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel(dayViewModels, date);

		var element = $('#Preference-period-feedback-view')[0];
		if (element)
			ko.applyBindings(periodFeedbackViewModel, element);

		feedbackLoader = feedbackLoader || function (call) { call(); };
		feedbackLoader(function () {
			periodFeedbackViewModel.LoadFeedback();
			$.each(dayViewModels, function (index, element) {
				element.LoadFeedback();
				feedbackLoadingStarted = true;
			});
		});
	}

	function _callWhenFeedbackIsLoaded(callback) {
		if (feedbackLoadingStarted && !Teleopti.MyTimeWeb.Ajax.IsRequesting())
			callback();
		else
			setTimeout(function () { _callWhenFeedbackIsLoaded(callback); }, 100);
	}

	function _soon(call) {
		setTimeout(function () { call(); }, 300);
	}

	return {
		Init: function () {
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Preference/Index', Teleopti.MyTimeWeb.Preference.PreferencePartialInit);
			_initSplitButton();
			_initDeleteButton();
		},
		InitViewModels: function () {
			_initViewModels();
		},
		PreferencePartialInit: function () {
			if (!$('#Preference-body').length)
				return;
			_initPeriodSelection();
			_initViewModels(_soon);
			_activateSelectable();
		},
		CallWhenFeedbackIsLoaded: function (callback) {
			_callWhenFeedbackIsLoaded(callback);
		},
		AjaxForDate: function (options) {
			return _ajaxForDate(options);
		}
	};

})(jQuery);


Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel = function (dayViewModels, date) {
	var self = this;

	this.LoadFeedback = function () {
		Teleopti.MyTimeWeb.Ajax.Ajax({
			url: "PreferenceFeedback/PeriodFeedback",
			dataType: "json",
			data: { Date: date },
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
				self.TargetDaysOffLower(data.TargetDaysOff.Lower);
				self.TargetDaysOffUpper(data.TargetDaysOff.Upper);
				self.PossibleResultDaysOff(data.PossibleResultDaysOff);
				self.TargetContractTimeLower(data.TargetHours.Lower);
				self.TargetContractTimeUpper(data.TargetHours.Upper);
			}
		});
	};

	this.TargetDaysOffLower = ko.observable();
	this.TargetDaysOffUpper = ko.observable();
	this.PossibleResultDaysOff = ko.observable();

	this.TargetContractTimeLower = ko.observable();
	this.TargetContractTimeUpper = ko.observable();

	this.PossibleResultContractTimeMinutesLower = ko.computed(function () {
		var sum = 0;
		$.each(dayViewModels, function (index, day) {
			var value = day.PossibleContractTimeMinutesLower();
			if (value)
				sum += parseInt(value);
			sum += day.ContractTimeMinutes;
		});
		return sum;
	});

	this.PossibleResultContractTimeMinutesUpper = ko.computed(function () {
		var sum = 0;
		$.each(dayViewModels, function (index, day) {
			var value = day.PossibleContractTimeMinutesUpper();
			if (value)
				sum += parseInt(value);
			sum += day.ContractTimeMinutes;
		});
		return sum;
	});

	this.PossibleResultContractTimeLower = ko.computed(function () {
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(self.PossibleResultContractTimeMinutesLower());
	});

	this.PossibleResultContractTimeUpper = ko.computed(function () {
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(self.PossibleResultContractTimeMinutesUpper());
	});

};

Teleopti.MyTimeWeb.Preference.DayViewModel = function () {
	var self = this;

	this.Date = "";
	this.ContractTimeMinutes = 0;
	this.HasFeedback = true;

	this.ReadElement = function (element) {
		var item = $(element);
		self.Date = item.attr('data-mytime-date');
		self.ContractTimeMinutes = parseInt($('[data-mytime-contract-time]', item).attr('data-mytime-contract-time')) || 0;
		self.HasFeedback = item.hasClass("feedback");
	};

	this.LoadFeedback = function () {
		if (!self.HasFeedback)
			return;
		Teleopti.MyTimeWeb.Preference.AjaxForDate({
			url: "PreferenceFeedback/Feedback",
			type: 'GET',
			data: { Date: self.Date },
			date: self.Date,
			fillData: function (cell, data) {
				self.FeedbackError(data.FeedbackError);
				self.PossibleStartTimes(data.PossibleStartTimes);
				self.PossibleEndTimes(data.PossibleEndTimes);
				self.PossibleContractTimeMinutesLower(data.PossibleContractTimeMinutesLower);
				self.PossibleContractTimeMinutesUpper(data.PossibleContractTimeMinutesUpper);
			}
		});
	};

	this.FeedbackError = ko.observable();
	this.PossibleStartTimes = ko.observable();
	this.PossibleEndTimes = ko.observable();

	this.PossibleContractTimeMinutesLower = ko.observable();
	this.PossibleContractTimeMinutesUpper = ko.observable();

	this.PossibleContractTimeLower = ko.computed(function () {
		var value = self.PossibleContractTimeMinutesLower();
		if (!value)
			return "";
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(value);
	});

	this.PossibleContractTimeUpper = ko.computed(function () {
		var value = self.PossibleContractTimeMinutesUpper();
		if (!value)
			return "";
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(value);
	});

	this.PossibleContractTimes = ko.computed(function () {
		var lower = self.PossibleContractTimeLower();
		var upper = self.PossibleContractTimeUpper();
		if (lower != "")
			return lower + "-" + upper;
		return "";
	});

};









Teleopti.MyTimeWeb.Preference.formatTimeSpan = function (totalMinutes) {
	if (!totalMinutes)
		return "0:00";
	var minutes = totalMinutes % 60;
	var hours = Math.floor(totalMinutes / 60);
	return hours + ":" + Teleopti.MyTimeWeb.Preference.rightPadNumber(minutes, "00");
};

Teleopti.MyTimeWeb.Preference.rightPadNumber = function (number, padding) {
	var formattedNumber = padding + number;
	var start = formattedNumber.length - padding.length;
	formattedNumber = formattedNumber.substring(start);
	return formattedNumber;
};
