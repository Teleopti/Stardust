/// <reference path="~/Content/Scripts/jquery-1.6.4-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.6.4.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
}
if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
	Teleopti.MyTimeWeb = {};
}

Teleopti.MyTimeWeb.PreferenceInitializer = function (ajax, portal) {

	var loadingStarted = false;
	var periodFeedbackViewModel = null;
	var dayViewModels = {};

	function _initPeriodSelection() {
		var rangeSelectorId = '#PreferenceDateRangeSelector';
		var periodData = $('#Preference-body').data('mytime-periodselection');
		portal.InitPeriodSelection(rangeSelectorId, periodData);
	}

	function _initSplitButton() {
		$('#Preference-set-button')
			.parent()
			.splitbutton({
				clicked: function (event, item) {
					var promises = [];
					$('#Preference-body-inner .ui-selected')
						.each(function (index, cell) {
							var date = $(cell).data('mytime-date');
							var promise = dayViewModels[date].SetPreference(item.value);
							promises.push(promise);
						});
					$.when.apply(null, promises)
						.done(function () { periodFeedbackViewModel.LoadFeedback(); });
				}
			});
	}

	function _initDeleteButton() {
		$('#Preference-delete-button')
			.click(function () {
				var promises = [];
				$('#Preference-body-inner .ui-selected')
					.each(function (index, cell) {
						var date = $(cell).data('mytime-date');
						var promise = dayViewModels[date].DeletePreference();
						promises.push(promise);
					});
				$.when.apply(null, promises)
					.done(function () { periodFeedbackViewModel.LoadFeedback(); });
			})
			.removeAttr('disabled');
	}

	function _activateSelectable() {
		$('#Preference-body-inner').calendarselectable();
	}

	function _initViewModels(loader) {

		dayViewModels = {};
		$('li[data-mytime-date].inperiod').each(function (index, element) {
			var dayViewModel = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
			dayViewModel.ReadElement(element);
			dayViewModels[dayViewModel.Date] = dayViewModel;
			ko.applyBindings(dayViewModel, element);
		});

		var date = portal ? portal.CurrentFixedDate() : null;
		periodFeedbackViewModel = new Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel(ajax, dayViewModels, date);

		var element = $('#Preference-period-feedback-view')[0];
		if (element)
			ko.applyBindings(periodFeedbackViewModel, element);

		loader = loader || function (call) { call(); };
		loader(function () {
			periodFeedbackViewModel.LoadFeedback();
			$.each(dayViewModels, function (index, element) {
				element.LoadPreference(function () {
					element.LoadFeedback();
				});
				loadingStarted = true;
			});
		});
	}

	function _callWhenLoaded(callback) {
		if (loadingStarted && !ajax.IsRequesting())
			callback();
		else
			setTimeout(function () { _callWhenLoaded(callback); }, 100);
	}

	function _soon(call) {
		setTimeout(function () { call(); }, 300);
	}

	function _initExtendedPanels() {
		$('.preference .extended-indication')
			.each(function () {
				var date = $(this).closest("li[data-mytime-date]").attr("data-mytime-date");
				$(this)
					.qtip(
						{
							id: "extended-" + date,
							content: {
								text: $(this).next('.extended-panel')
							},
							position: {
								my: "top left",
								at: "top right",
								adjust: {
									x: 4,
									y: 5
								}
							},
							show: {
								event: 'click'
							},
							hide: {
								target: $("#page"),
								event: 'mousedown'
							},
							style: {
								def: false,
								classes: 'ui-tooltip-custom ui-tooltip-rounded ui-tooltip-shadow',
								tip: {
									corner: "left top"
								}
							}
						});
			});

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
			_initExtendedPanels();
		},
		CallWhenLoaded: function (callback) {
			_callWhenLoaded(callback);
		}
	};

};

Teleopti.MyTimeWeb.Preference = Teleopti.MyTimeWeb.PreferenceInitializer(Teleopti.MyTimeWeb.Ajax, Teleopti.MyTimeWeb.Portal);

Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel = function (ajax, dayViewModels, date) {
	var self = this;

	this.LoadFeedback = function () {
		ajax.Ajax({
			url: "PreferenceFeedback/PeriodFeedback",
			dataType: "json",
			data: { Date: date },
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
				self.TargetDaysOffLower(data.TargetDaysOff.Lower);
				self.TargetDaysOffUpper(data.TargetDaysOff.Upper);
				self.PossibleResultDaysOff(data.PossibleResultDaysOff);
				self.TargetContractTimeLower(data.TargetContractTime.Lower);
				self.TargetContractTimeUpper(data.TargetContractTime.Upper);
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

Teleopti.MyTimeWeb.Preference.DayViewModel = function (ajax) {
	var self = this;

	// legacy.
	// should be refactored into using the viewmodel to update the ui etc
	// would result in less text...
	var ajaxForDate = function (options) {

		var type = options.type || 'GET',
		    date = options.date || null, // required
		    data = options.data || {},
		    statusCode404 = options.statusCode404,
		    url = options.url || "Preference/Preference",
		    success = options.success || function () {
		    },
		    complete = options.complete || null;

		var cell = $('li[data-mytime-date="' + date + '"]');
		return ajax.Ajax({
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
					.appendTo(cell);

			},
			complete: function (jqXHR, textStatus) {

				cell.data('request', null);

				$('.temporary', cell)
					.remove();

				if (complete)
					complete(jqXHR, textStatus);
			},
			success: success,
			data: data,
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
					.appendTo(cellHtml);
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
					.insertAfter(cellHtml);

			}
		});
	};



	this.Date = "";
	this.ContractTimeMinutes = 0;
	this.HasFeedback = true;
	this.HasPreference = true;
	this.Preference = ko.observable();
	this.Extended = ko.observable();
	this.ExtendedTitle = ko.observable();
	this.StartTimeLimitation = ko.observable();
	this.EndTimeLimitation = ko.observable();
	this.WorkTimeLimitation = ko.observable();
	this.Activity = ko.observable();
	this.ActivityStartTimeLimitation = ko.observable();
	this.ActivityEndTimeLimitation = ko.observable();
	this.ActivityTimeLimitation = ko.observable();
	this.Color = ko.observable();

	this.ReadElement = function (element) {
		var item = $(element);
		self.Date = item.attr('data-mytime-date');
		self.ContractTimeMinutes = parseInt($('[data-mytime-contract-time]', item).attr('data-mytime-contract-time')) || 0;
		self.HasFeedback = item.hasClass("feedback");
		self.HasPreference = item.hasClass("preference") || $(".preference", item).length > 0;
		self.Color($('.day-content', element).css("border-left-color"));
	};

	this.ReadPreference = function (data) {
		self.Color(data.Color);
		self.Preference(data.Preference);
		self.Extended(data.Extended);
		self.ExtendedTitle(data.ExtendedTitle);
		self.StartTimeLimitation(data.StartTimeLimitation);
		self.EndTimeLimitation(data.EndTimeLimitation);
		self.WorkTimeLimitation(data.WorkTimeLimitation);
		self.Activity(data.Activity);
		self.ActivityStartTimeLimitation(data.ActivityStartTimeLimitation);
		self.ActivityEndTimeLimitation(data.ActivityEndTimeLimitation);
		self.ActivityTimeLimitation(data.ActivityTimeLimitation);
	};

	this.LoadPreference = function (complete) {
		if (!self.HasPreference) {
			complete();
			return null;
		}
		return ajaxForDate({
			url: "Preference/Preference",
			type: 'GET',
			data: { Date: self.Date },
			date: self.Date,
			success: this.ReadPreference,
			complete: complete,
			statusCode404: function () { }
		});
	};

	this.LoadFeedback = function () {
		if (!self.HasFeedback)
			return null;
		return ajaxForDate({
			url: "PreferenceFeedback/Feedback",
			type: 'GET',
			data: { Date: self.Date },
			date: self.Date,
			success: function (data) {
				self.FeedbackError(data.FeedbackError);
				self.PossibleStartTimes(data.PossibleStartTimes);
				self.PossibleEndTimes(data.PossibleEndTimes);
				self.PossibleContractTimeMinutesLower(data.PossibleContractTimeMinutesLower);
				self.PossibleContractTimeMinutesUpper(data.PossibleContractTimeMinutesUpper);
			}
		});
	};

	this.SetPreference = function (value) {
		var deferred = $.Deferred();
		ajaxForDate({
			type: 'POST',
			data: {
				Date: self.Date,
				Id: value
			},
			date: self.Date,
			success: this.ReadPreference,
			complete: function () {
				deferred.resolve();
				self.LoadFeedback();
			}
		});
		return deferred.promise();
	};

	this.DeletePreference = function () {
		var deferred = $.Deferred();
		ajaxForDate({
			type: 'DELETE',
			data: { Date: self.Date },
			date: self.Date,
			statusCode404: function () { },
			success: this.ReadPreference,
			complete: function () {
				deferred.resolve();
				self.LoadFeedback();
			}
		});
		return deferred.promise();
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
